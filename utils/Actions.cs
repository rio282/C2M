using System;

namespace C2M
{
	[Flags]
	public enum Actions : ushort
	{
		None = 0,
		LeftClick = 1,
		RightClick = 2,
		Movement = 3,
		Scroll = 4,
		Back = ushort.MaxValue - 1,
		Exit = ushort.MaxValue
	}
}
