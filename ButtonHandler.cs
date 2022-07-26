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
	internal class ButtonHandler
	{
		private const string KeymapFolder = @"C:\Users\rbere\Desktop\Programming\Local\Software\C2M\";
		private const string KeymapFile = "layout.keymap";

		private readonly C2M c2m;
		private readonly IMouseSimulator mouse;
		private readonly Controller controller;


		private const int CTMovementDivisionRate = 2_000;
		private const int CTScrollDivisionRate = 10_000;

		private readonly TaskScheduler ts;
		private IDictionary<GamepadButtonFlags, Task> keymap;
		private List<GamepadButtonFlags> lastKeysDown;

		public ButtonHandler(C2M c2m, Controller controller, IMouseSimulator mouse)
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
				Console.Write("Keymap file doesn't exist.");
				Console.Read();
				Environment.Exit(1);
			}
			Console.Write("Found keymap file!");

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
					GamepadButtonFlags gamepadButton = Program.ParseEnum<GamepadButtonFlags>(input);
					keymap[gamepadButton] = action switch
					{
						nameof(Actions.LeftClick) => Task.Factory.StartNew(() => mouse.LeftButtonClick(), CancellationToken.None, TaskCreationOptions.None, ts),
						nameof(Actions.RightClick) => Task.Factory.StartNew(() => mouse.RightButtonClick(), CancellationToken.None, TaskCreationOptions.None, ts),
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
			mouse.VerticalScroll(y);
		}


		internal void Handle(State state)
		{
			Move(state);
			Scroll(state);
			foreach (KeyValuePair<GamepadButtonFlags, Task> input in keymap)
			{
				bool isKeyDown = state.Gamepad.Buttons.HasFlag(input.Key);
				if (isKeyDown && !lastKeysDown.Contains(input.Key))
					input.Value.Start(); // exec action bound to keymap
				else
					lastKeysDown.Remove(input.Key);
			}
		}
	}
}
