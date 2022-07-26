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
			if (key.Length > 1 && !int.TryParse(key, out _))
				s.Keyboard.KeyPress(Utilities.ParseEnum<VirtualKeyCode>(key));
			else
				s.Keyboard.KeyPress(Utilities.ParseEnum<VirtualKeyCode>($"VK_{key}"));
		}

		public static void PressKeyByCode(int keycode)
		{
			InputSimulator s = new InputSimulator();
			s.Keyboard.KeyPress((VirtualKeyCode)keycode);
		}

		public static void PressKeyCombination(string combo)
		{
			// TODO: complete and clean this class up
			foreach (string ikey in combo.Replace(" ", "").Split("+"))
			{
				string key = ikey.Replace("+", "");

				Console.WriteLine(key);
			}
		}

		public static void OpenOnScreenKeyboard()
		{
			const string progFiles = @"C:\Program Files\Common Files\Microsoft Shared\ink";
			string onScreenKeyboardPath = Path.Combine(progFiles, "TabTip.exe");

			Process process = new Process();
			process.StartInfo.UseShellExecute = true;
			process.StartInfo.FileName = onScreenKeyboardPath;
			process.Start();

		}
	}
}
