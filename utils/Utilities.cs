using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace C2M.utils
{
	class Utilities
	{

		public static bool IsKeyInputAllowed()
		{

			return false;
		}

		public static T ParseEnum<T>(string value)
		{
			return (T)Enum.Parse(typeof(T), value, true);
		}
	}
}
