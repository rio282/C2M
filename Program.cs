using System;

namespace C2M
{
	class Program
	{
		public static T ParseEnum<T>(string value)
		{
			return (T)Enum.Parse(typeof(T), value, true);
		}

		static void Main(string[] args)
		{
			Console.Title = "C2M (Not Connected.)";
			C2M c2m = new C2M();
			c2m.Start();
		}
	}
}
