﻿using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using WindowsInput;
using WindowsInput.Native;

namespace C2M.utils
{
	class KeyOutputManager
	{

		private readonly IKeyboardSimulator keyboard;

		public KeyOutputManager(IKeyboardSimulator keyboard)
		{
			this.keyboard = keyboard;
		}

		public void PressKey(string key)
		{
			if (key.Length == 1 && !int.TryParse(key, out _))
				key = $"VK_{key}";
			keyboard.KeyPress(Utilities.ParseEnum<VirtualKeyCode>(key));
		}

		public void PressKeyByCode(int keycode)
		{
			keyboard.KeyPress((VirtualKeyCode)keycode);
		}

		public void PressKeyCombination(string combo)
		{
			// make array
			string[] keys = combo.Replace(" ", "").Split("+");
			// execute key down
			foreach (string key_str in keys) 
			{
				string key = key_str;
				if (key.Length == 1 && !int.TryParse(key, out _))
					key = $"VK_{key}";
				keyboard.KeyDown(Utilities.ParseEnum<VirtualKeyCode>(key));
			}

			// reverse array
			Array.Reverse(keys);
			// execute key up
			foreach (string key_str in keys)
			{
				string key = key_str;
				if (key.Length == 1 && !int.TryParse(key, out _))
					key = $"VK_{key}";
				keyboard.KeyUp(Utilities.ParseEnum<VirtualKeyCode>(key));
			}
		}

		public void OpenOnScreenKeyboard()
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
