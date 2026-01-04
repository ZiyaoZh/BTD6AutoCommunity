using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static BTD6AutoCommunity.Core.WindowApiWrapper;

namespace BTD6AutoCommunity.Core
{
    public class InputSimulator
    {
        // 鼠标移动
        public static void MouseMove(GameContext context, double x, double y)
        {
            Point absPoint = context.ConvertToAbsolute((x, y));
            int absX = absPoint.X;
            int absY = absPoint.Y;

            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = absX,
                        dy = absY,
                        dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE
                    }
                }
            };

            SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void MouseLeftDown()
        {
            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_LEFTDOWN
                    }
                }
            };

            SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void MouseLeftUp()
        {
            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_LEFTUP
                    }
                }
            };

            SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void MouseLeftClick()
        {
            INPUT[] inputs = new INPUT[2];

            // 按下左键
            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_LEFTDOWN
                    }
                }
            };

            // 松开左键
            inputs[1] = new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_LEFTUP
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        // 带绝对坐标的鼠标点击
        public static void MouseMoveAndLeftClick(GameContext context, double x, double y)
        {
            Point absPoint = context.ConvertToAbsolute((x, y));
            int absX = absPoint.X;
            int absY = absPoint.Y;

            INPUT[] inputs = new INPUT[3];

            // 移动鼠标
            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = absX,
                        dy = absY,
                        dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE
                    }
                }
            };

            // 按下左键
            inputs[1] = new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_LEFTDOWN
                    }
                }
            };

            // 松开左键
            inputs[2] = new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_LEFTUP
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        // 键盘按键封装
        public static void KeyboardPressAndRelease(ushort keyCode)
        {
            INPUT[] inputs = new INPUT[2];
            bool isExtendedKey = IsExtendedKey(keyCode);
            // 按下
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        wScan = (byte)MapVirtualKey(keyCode, 0),
                        dwFlags = isExtendedKey ? KEYEVENTF_EXTENDEDKEY : 0
                    }
                }
            };

            // 抬起
            inputs[1] = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        wScan = (byte)MapVirtualKey(keyCode, 0),
                        dwFlags = isExtendedKey ? (KEYEVENTF_KEYUP | KEYEVENTF_EXTENDEDKEY) : KEYEVENTF_KEYUP
                    }
                }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void ReleaseAllKeys()
        {
            for (int i = 0x08; i <= 0x87; i++)
            {
                KeyboardRelease((ushort)i);
            }
        }

        public static void KeyboardPress(ushort keyCode)
        {
            INPUT[] inputs = new INPUT[1];
            bool isExtendedKey = IsExtendedKey(keyCode);
            // 按下
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        wScan = (byte)MapVirtualKey(keyCode, 0),
                        dwFlags = isExtendedKey ? KEYEVENTF_EXTENDEDKEY : 0
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void KeyboardRelease(ushort keyCode)
        {
            INPUT[] inputs = new INPUT[1];
            bool isExtendedKey = IsExtendedKey(keyCode);
            // 抬起
            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        wScan = (byte)MapVirtualKey(keyCode, 0),
                        dwFlags = isExtendedKey ? (KEYEVENTF_KEYUP | KEYEVENTF_EXTENDEDKEY) : KEYEVENTF_KEYUP
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void MouseWheel(int arg)
        {
            var input = new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_WHEEL,
                        mouseData = (uint)arg
                    }
                }
            };
            SendInput(1, new[] { input }, INPUT.Size);
        }

        private static bool IsExtendedKey(ushort keyCode)
        {
            // 需要 KEYEVENTF_EXTENDEDKEY 标志的按键
            switch (keyCode)
            {
                case 0x23: // VK_END
                case 0x24: // VK_HOME
                case 0x25: // VK_LEFT (左箭头)
                case 0x26: // VK_UP (上箭头)
                case 0x27: // VK_RIGHT (右箭头)
                case 0x28: // VK_DOWN (下箭头)
                case 0x2D: // VK_INSERT
                case 0x2E: // VK_DELETE
                case 0x21: // VK_PRIOR (Page Up)
                case 0x22: // VK_NEXT (Page Down)
                case 0x2C: // VK_SNAPSHOT (Print Screen)
                case 0xA3: // VK_RCONTROL (右 Ctrl)
                case 0xA5: // VK_RMENU (右 Alt)
                case 0x6F: // VK_DIVIDE (小键盘 '/')
                case 0x0D: // VK_RETURN (数字小键盘 Enter)
                    return true;
                default:
                    return false;
            }
        }
    }
}
