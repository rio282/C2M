using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace C2M.utils
{
    class SoundManager
    {
        const byte K_VOLUME_UP = 175;
        const byte K_VOLUME_DOWN = 174;
        
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public static void VolumeUp()
        {
            keybd_event(K_VOLUME_UP, 0, 0, 0);
        }

        public static void VolumeDown()
        {
            keybd_event(K_VOLUME_DOWN, 0, 0, 0);
        }
    }
}
