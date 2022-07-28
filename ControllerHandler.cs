using C2M.utils;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;

namespace C2M
{
	internal class ControllerHandler
	{
		private const string KeymapFolder = @"..\..\..\keymaps\";
		private const string KeymapFile = "default.keymap";

		private readonly C2M c2m;
		private readonly IMouseSimulator mouse;
		private readonly Controller controller;
		private readonly KeyOutputManager keyOutputManager;

		private const int CTMovementDivisionRate = 2_000;
		private const int CTScrollDivisionRate = 10_000;

		private readonly TaskScheduler ts;
		private IDictionary<GamepadButtonFlags, Action> keymap;
		private List<GamepadButtonFlags> lastKeysDown;

		private ushort framecounter = 0;

		public ControllerHandler(C2M c2m, Controller controller, IMouseSimulator mouse)
		{
			this.c2m = c2m;
			this.controller = controller;
			this.mouse = mouse;
			keyOutputManager = new KeyOutputManager();

			// create task scheduler
			SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
			ts = TaskScheduler.FromCurrentSynchronizationContext();

			lastKeysDown = new List<GamepadButtonFlags>();
			LoadKeymap();
		}

		private void LoadKeymap()
		{
			keymap = new Dictionary<GamepadButtonFlags, Action>();

			// file reader settings
			string fullFilePath = Path.Combine(KeymapFolder, KeymapFile);
			const Int32 BufferSize = 128;
			if (!File.Exists(fullFilePath))
			{
				Console.WriteLine("Keymap file doesn't exist.");
				Console.Read();

				Environment.Exit(1);
			}
			Console.WriteLine("Found keymap file!");

			using var fileStream = File.OpenRead(fullFilePath);
			using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize);

			// process lines
			String line;
			while ((line = streamReader.ReadLine()) != null)
			{
				line = line.Replace(" ", "");
				if (line.Split("=").Length < 2)
					continue;

				string input = line.Split("=")[0];
				string action = line.Split("=")[1].Replace("=", "");

				try
				{
					// check if button exists
					GamepadButtonFlags gamepadButton = Utilities.ParseEnum<GamepadButtonFlags>(input);
					// assign correct action
					Action assignedAction = Utilities.ParseEnum<Actions>(action) switch
					{
						Actions.LeftClick => () => mouse.LeftButtonDown(),
						Actions.RightClick => () => mouse.RightButtonClick(),

						Actions.Left => () => keyOutputManager.PressKey("left"),
						Actions.Right => () => keyOutputManager.PressKey("right"),
						Actions.Up => () => keyOutputManager.PressKey("up"),
						Actions.Down => () => keyOutputManager.PressKey("down"),

						Actions.Back => () => keyOutputManager.PressKey("escape"),

						Actions.VolumeUp => () => SoundManager.VolumeUp(),
						Actions.VolumeDown => () => SoundManager.VolumeDown(),

						Actions.OpenOSK => () => keyOutputManager.OpenOnScreenKeyboard(),

						Actions.None => null,
						_ => null
					};

					keymap.Add(gamepadButton, assignedAction); // add to dict
				}
				catch (System.ArgumentException)
				{
					continue;
				}
			};
		}

		private void Move(State state)
		{
			var x = state.Gamepad.LeftThumbX / CTMovementDivisionRate;
			var y = state.Gamepad.LeftThumbY / CTMovementDivisionRate;

			mouse.MoveMouseBy(x, -y);
		}

		private void Scroll(State state)
		{
			var x = state.Gamepad.RightThumbX / CTScrollDivisionRate;
			var y = state.Gamepad.RightThumbY / CTScrollDivisionRate;

			mouse.HorizontalScroll(x);
			mouse.VerticalScroll(y >> 1);
		}

		internal void Handle(State state)
		{
			// handle generic stuff
			Move(state);
			Scroll(state);
			if (framecounter % 5 == 0)
            {
				HandleTriggers(state);
			}
			

			// loop over keymap, if key in keymap is down -> execute action
			foreach (KeyValuePair<GamepadButtonFlags, Action> control in keymap)
			{
				var input = control.Key;
				var action = control.Value;
				bool isKeyDown = state.Gamepad.Buttons.HasFlag(input);

				if (isKeyDown && !lastKeysDown.Contains(input))
				{
					lastKeysDown.Add(input);
					action.Invoke();
                }
				else if (!isKeyDown && lastKeysDown.Contains(input))
				{
					lastKeysDown.Remove(input);
					mouse.LeftButtonUp();
				}
			}

			framecounter++;
		}

		private void HandleTriggers(State state)
		{
			if (state.Gamepad.LeftTrigger > Gamepad.TriggerThreshold && state.Gamepad.RightTrigger <= Gamepad.TriggerThreshold)
				keyOutputManager.PressKeyCombination("control+subtract");

			else if (state.Gamepad.RightTrigger > Gamepad.TriggerThreshold && state.Gamepad.LeftTrigger <= Gamepad.TriggerThreshold)
				keyOutputManager.PressKeyCombination("control+add");
		}
	}
}
