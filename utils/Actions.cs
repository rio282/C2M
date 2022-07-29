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
		
		Left = 5,
		Right = 6,
		Up = 7,
		Down = 8,

		Back = 9,
		
		VolumeUp = 10,	
		VolumeDown = 11,
		
		ZoomIn = 12,
		ZoomOut = 13,

		OpenOSK = 14,
		AltTab = 15,
		Enter = 16
	}
}
