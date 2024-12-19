using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTD6AutoCommunity
{
    internal class KeyEvents
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")] //申明API函数
        public static extern bool RegisterHotKey(
             IntPtr hWnd, // handle to window
             int id, // hot key identifier
             uint fsModifiers, // key-modifier options
             Keys vk // virtual-key code
        );
        [System.Runtime.InteropServices.DllImport("user32.dll")] //申明API函数
        public static extern bool UnregisterHotKey(
            IntPtr hWnd, // handle to window
            int id // hot key identifier
        );


    }
}
