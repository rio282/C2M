using C2M.utils;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;

namespace C2M
{
	internal class ControllerHandler
	{
		private const string KeymapFolder = @"C:\Users\rbere\Desktop\Programming\Local\Software\C2M\keymaps\";
		private const string KeymapFile = "default.keymap";

		private readonly C2M c2m;
		private readonly IMouseSimulator mouse;
		private readonly Controller controller;


		private const int CTMovementDivisionRate = 2_000;
		private const int CTScrollDivisionRate = 10_000;

		private readonly TaskScheduler ts;
		private IDictionary<GamepadButtonFlags, Task> keymap;
		private List<GamepadButtonFlags> lastKeysDown;

		public ControllerHandler(C2M c2m, Controller controller, IMouseSimulator mouse)
		{
			this.c2m = c2m;
			this.controller = controller;
			this.mouse = mouse;

			// create task scheduler
			SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
			ts = TaskScheduler.FromCurrentSynchronizationContext();

			lastKeysDown = new List<GamepadButtonFlags>();
			LoadKeymap();
		}

		private void LoadKeymap()
		{
			keymap = new Dictionary<GamepadButtonFlags, Task>();

			// file reader settings
			string fullFilePath = Path.Combine(KeymapFolder, KeymapFile);
			const Int32 BufferSize = 128;
			if (!File.Exists(fullFilePath))
			{
				Console.WriteLine("Keymap file doesn't exist.");
				//Console.Read();

				return;
				//Environment.Exit(1);
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
					keymap[gamepadButton] = action switch
					{
						// nameof(Actions.LeftClick) => TASK
						_ => null,
					};
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
			HandleTriggers(state);

			// hot AF key inputs and that
			foreach (KeyValuePair<GamepadButtonFlags, Task> input in keymap)
			{
				// check if key is valid
				bool isKeyDown = state.Gamepad.Buttons.HasFlag(input.Key);

				if (isKeyDown && !lastKeysDown.Contains(input.Key))
				{
					lastKeysDown.Add(input.Key);

					// normal operations
					if (input.Key == GamepadButtonFlags.A) mouse.LeftButtonDown();
					else if (input.Key == GamepadButtonFlags.B) KeyOutputManager.PressKey("escape");
					else if (input.Key == GamepadButtonFlags.X) mouse.RightButtonClick();
					else if (input.Key == GamepadButtonFlags.Y) KeyOutputManager.OpenOnScreenKeyboard();

					// movement arraows
					else if (input.Key == GamepadButtonFlags.DPadLeft) KeyOutputManager.PressKey("left");
					else if (input.Key == GamepadButtonFlags.DPadRight) KeyOutputManager.PressKey("right");
					else if (input.Key == GamepadButtonFlags.DPadUp) KeyOutputManager.PressKey("up");
					else if (input.Key == GamepadButtonFlags.DPadDown) KeyOutputManager.PressKey("down");

					// idk lol
					// else if (input.Key == GamepadButtonFlags.LeftShoulder) KeyOutputManager.PressKey("control");
					// else if (input.Key == GamepadButtonFlags.RightShoulder) KeyOutputManager.PressKeyCombination("ctrl+=");

					// volume
					else if (input.Key == GamepadButtonFlags.Back) SoundManager.VolumeDown();
					else if (input.Key == GamepadButtonFlags.Start) SoundManager.VolumeUp();
				}
				else if (!isKeyDown && lastKeysDown.Contains(input.Key))
				{
					mouse.LeftButtonUp();
					lastKeysDown.Remove(input.Key);
				}
			}
		}

		private ushort counter = 0;
		private void HandleTriggers(State state)
		{
			counter++;
			if (counter <= 5)
				return;

			if (state.Gamepad.LeftTrigger > Gamepad.TriggerThreshold && state.Gamepad.RightTrigger <= Gamepad.TriggerThreshold)
				KeyOutputManager.PressKeyCombination("control+subtract");

			else if (state.Gamepad.RightTrigger > Gamepad.TriggerThreshold && state.Gamepad.LeftTrigger <= Gamepad.TriggerThreshold)
				KeyOutputManager.PressKeyCombination("control+add");

			counter = 0;
		}
	}
}
