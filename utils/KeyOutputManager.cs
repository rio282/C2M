using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using WindowsInput;
using WindowsInput.Native;

namespace C2M.utils
{
	class KeyOutputManager
	{
		public KeyOutputManager()
		{

		}

		public static void PressKey(string key)
		{
			InputSimulator s = new InputSimulator();
			
			if (key.Length == 1 && !int.TryParse(key, out _))
				key = $"VK_{key}";
			s.Keyboard.KeyPress(Utilities.ParseEnum<VirtualKeyCode>(key));
		}

		public static void PressKeyByCode(int keycode)
		{
			InputSimulator s = new InputSimulator();
			s.Keyboard.KeyPress((VirtualKeyCode)keycode);
		}

		public static void PressKeyCombination(string combo)
		{
			InputSimulator s = new InputSimulator();

			// make array
			string[] keys = combo.Replace(" ", "").Split("+");
			// execute key down
			foreach (string key_str in keys) 
			{
				string key = key_str;
				if (key.Length == 1 && !int.TryParse(key, out _))
					key = $"VK_{key}";
				s.Keyboard.KeyDown(Utilities.ParseEnum<VirtualKeyCode>(key));
			}

			// reverse array
			Array.Reverse(keys);
			// execute key up
			foreach (string key_str in keys)
			{
				string key = key_str;
				if (key.Length == 1 && !int.TryParse(key, out _))
					key = $"VK_{key}";
				s.Keyboard.KeyUp(Utilities.ParseEnum<VirtualKeyCode>(key));
			}
		}

		public static void OpenOnScreenKeyboard()
		{
			// WARNING: this is SLOOOOWWW (not because of me, but because of osk.exe)
			const string progFiles = @"C:\Program Files\Common Files\Microsoft Shared\ink";
			string onScreenKeyboardPath = Path.Combine(progFiles, "TabTip.exe");

			Process process = new Process();
			process.StartInfo.UseShellExecute = true;
			process.StartInfo.FileName = onScreenKeyboardPath;
			process.Start();
		}
	}
}
