using Newtonsoft.Json;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static OpenCvSharp.Stitcher;

namespace BTD6AutoCommunity
{
    internal class ExecuteDirectiveClass
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public InputUnion U;
            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const int INPUT_MOUSE = 0;
        private const int INPUT_KEYBOARD = 1;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        private List<List<string>> directive;
        private List<List<string>> selectMapDirective;
        private List<List<string>> completeDirective;
        private List<List<string>> findMapDirective;
        private List<List<string>> selectHeroDirective;
        private List<List<string>> restartDirective;

        public int currentIndex;
        public int currentMiniIndex;
        public int currentInstructionCount;
        public int lastYellowBlockCount;
        public int currentYellowBlockCount;
        public int circleTimes;
        private int findHeroretrys;
        public bool heroUnlocked;
        private int retry;
        private (int, int) currentCoords;
        private double windowDpi;
        private int gameDpi;
        private IntPtr hWnd;
        public bool stopFlag;
        public (int, int) currentTrigger;
        private GetGameData gameData;
        private int abiliityRgb;

        public bool findMapFlag;
        public bool selectHeroFlag;
        public bool selectMapFlag;
        public bool executeFlag;
        public bool completeFlag;
        public bool restartFlag;
        public bool ifContinueGame;

        public int currentMap; // 收集
        public int currentDifficulty;

        public ExecuteDirectiveClass(double mydpi, int mygameDpi, IntPtr myhWnd)
        {
            windowDpi = mydpi;
            gameDpi = mygameDpi;
            hWnd = myhWnd;

            currentIndex = 0;
            currentMiniIndex = 0;
            circleTimes = 0;
            retry = 0;
            findHeroretrys = 0;
            heroUnlocked = false;
            stopFlag = false;

            abiliityRgb = 0;

            selectMapFlag = false;
            executeFlag = false;
            completeFlag = false;
            restartFlag = false;

            currentTrigger = (0, 0);
            currentCoords = (0, 0);

            currentMap = -1;

            ifContinueGame = false;
        }


        public void LoadDirective(List<List<string>> dir)
        {
            directive = new List<List<string>>(dir);
            currentIndex = 0;
            currentMiniIndex = 0;
            stopFlag = false;
        }

        public void LoadSelectMapDirective(List<List<string>> dir)
        {
            selectMapDirective = new List<List<string>>(dir);
        }

        public void LoadCompleteDirctive(List<List<string>> dir)
        {
            completeDirective = new List<List<string>>(dir);
        }

        public void LoadFindMapDirctive(List<List<string>> dir)
        {
            findMapDirective = new List<List<string>>(dir);
        }

        public void LoadSelectHeroDirctive(List<List<string>> dir)
        {
            selectHeroDirective = new List<List<string>>(dir);
        }

        public void LoadRestartDirective(List<List<string>> dir)
        {
            restartDirective = new List<List<string>>(dir);
        }

        private void RunCode(List<int> arguments)
        {
            switch (arguments[0])
            {
                case 1:
                    if (gameDpi == 0)
                    {
                        currentCoords = (arguments[1], arguments[2]);
                        MoveTo(arguments[1], arguments[2]);
                    }
                    else
                    {
                        currentCoords = ((int)(arguments[1] / 1.5), (int)(arguments[2] / 1.5));
                        MoveTo((int)(arguments[1] / 1.5), (int)(arguments[2] / 1.5));
                    }
                    break;
                case 2:
                    LeftClick();
                    break;
                case 3:
                    LeftDown();
                    break;
                case 4:
                    LeftUp();
                    break;
                case 5:
                    MouseWheel(arguments[1]);
                    break;
                case 6:
                    KeyDown((uint)arguments[1]);
                    break;
                case 7:
                    KeyUp((uint)arguments[1]);
                    break;
                case 8:
                    KeyDown((uint)arguments[1]);
                    KeyUp((uint)arguments[1]);
                    break;
                case 10: // 空指令
                    break;
                case 11: // 移动+点击
                    if (gameDpi == 0)
                    {
                        currentCoords = (arguments[1], arguments[2]);
                        MoveTo(arguments[1], arguments[2]);
                    }
                    else
                    {
                        currentCoords = ((int)(arguments[1] / 1.5), (int)(arguments[2] / 1.5));
                        MoveTo((int)(arguments[1] / 1.5), (int)(arguments[2] / 1.5));
                    }
                    LeftClick();
                    break;
                // 16 找地图
                // 17 游戏胜利结束判定
                // 18 收集识别地图
                // 19 收集找英雄
                // 20 结束自由游戏
                // 21 开始自动回合
                // 22 找重开位置
                // 23 找英雄是否可放
                // 24 技能释放前rgb
                // 25 技能释放后rgb
            }
        }

        public void ExecuteDirective(int round, int coin)
        {
            executeFlag = true;
            currentInstructionCount = directive[currentIndex].Count;
            List<int> arguments = directive[currentIndex][currentMiniIndex].Split(' ').Select(Int32.Parse).ToList();
            if (arguments[0] == 0)
            {
                currentTrigger = (arguments[1], arguments[2]);
                if (arguments[1] <= round && arguments[2] <= coin)
                {
                    currentMiniIndex++;
                    ExecuteDirective(round, coin);
                }
                else
                {
                    return;
                }
            }
            else
            {
                RunCode(arguments);
                currentMiniIndex++;
                if (currentMiniIndex == currentInstructionCount)
                {
                    currentMiniIndex = 0;
                    currentIndex++;
                }
                if (currentIndex == directive.Count)
                {
                    currentIndex = 0;
                    executeFlag = false;
                    completeFlag = true;
                    stopFlag = true;
                    return;
                }

            }
            return;
        }

        public void ExecuteDirective(int round, int coin, Dictionary<int, string> upgradeCount, bool checkUpgrade, int executeMode)
        { // 升级指令
            gameData = new GetGameData(windowDpi, gameDpi, hWnd);
            if (checkUpgrade)
            {
                if (!gameData.InGame())
                {
                    if (retry > 10)
                    {
                        retry = 0;
                        currentIndex = 0;
                        currentMiniIndex = 0;
                        while (executeFlag)
                        {
                            if (gameData.Complete())
                            {
                                executeFlag = false;
                                if (executeMode == 3)
                                {
                                    restartFlag = true;
                                    return;
                                }
                                completeFlag = true;
                                return;
                            }
                            if (gameData.ifFail())
                            {
                                executeFlag = false;
                                restartFlag = true;
                                return;
                            }
                        }
                        return;
                    }
                    retry++;
                    MoveTo(75, 60);
                    LeftClick();
                    return;
                }
                else
                {
                    retry = 0;
                }
            }
            executeFlag = true;

            currentInstructionCount = directive[currentIndex].Count;

            List<int> arguments = directive[currentIndex][currentMiniIndex].Split(' ').Select(Int32.Parse).ToList();

            if (arguments[0] == 0)
            {
                currentTrigger = (arguments[1], arguments[2]);
                if (arguments[1] <= round && arguments[2] <= coin)
                {
                    currentMiniIndex++;
                    ExecuteDirective(round, coin, upgradeCount, checkUpgrade, executeMode);
                }
                else
                {
                    return;
                }
            }
            else if (arguments[0] == 8 && (arguments[1] == 188 || arguments[1] == 190 || arguments[1] == 191))
            {
                //if (!upgradeFlag)
                //{
                //    //currentYellowBlockCount = gameData.GetYellowBlockCount();
                //    upgradeFlag = true;
                //}
                int colorIndex = GetColorIndex(arguments[1]);

                int route = arguments[1] == 188 ? 0 : (arguments[1] == 190 ? 1 : 2);
                int p = upgradeCount[currentIndex][route] - '0';
                if (p == 0)
                {
                    return;
                }
                //int reCount = 0;
                //while (currentYellowBlockCount == -1)
                //{
                //    if (reCount > 5)
                //    {
                //        reCount = 0;
                //        break;
                //    }
                //    currentYellowBlockCount = gameData.GetYellowBlockCount(colorIndex);
                //    reCount++;
                //}
                if (!gameData.GetYellowBlockCount(colorIndex, p))
                {
                    RunCode(arguments);
                    //System.IO.File.AppendAllText(@"answer.txt", currentYellowBlockCount.ToString() + " " + upgradeCount[currentIndex][route].ToString() + "\n");
                }
                else
                {
                    currentMiniIndex++;
                }
            }
            else if (arguments[0] == 8 && arguments[1] >= 65 && arguments[1] <= 90)
            {
                RunCode(arguments);
                if (gameData.IfDeploy())
                {
                    currentMiniIndex++;
                }
            }
            else
            {
                RunCode(arguments);
                currentMiniIndex++;
            }
            if (currentMiniIndex == currentInstructionCount)
            {
                currentMiniIndex = 0;
                currentIndex++;
            }
            if (currentIndex == directive.Count)
            {
                currentIndex = 0;
                executeFlag = false;
                if (executeMode == 3)
                {
                    restartFlag = true;
                    return;
                }
                completeFlag = true;
                stopFlag = true;
                return;
            }
            return;
        }

        public void ExecuteDirective(int round, int coin, int mode, bool checkUpgrade, int executeMode)
        { // mode == 0 升级指令， mode == 2 鼠标点击指令
            gameData = new GetGameData(windowDpi, gameDpi, hWnd);

            executeFlag = true;

            currentInstructionCount = directive[currentIndex].Count;
            List<int> arguments = directive[currentIndex][currentMiniIndex].Split(' ').Select(Int32.Parse).ToList();
            if (checkUpgrade)
            {
                if (!gameData.InGame())
                {
                    if (retry > 10)
                    {
                        retry = 0;
                        currentIndex = 0;
                        currentMiniIndex = 0;
                        while (executeFlag)
                        {
                            if (gameData.Complete())
                            {
                                executeFlag = false;
                                if (executeMode == 3)
                                {
                                    restartFlag = true;
                                    return;
                                }
                                completeFlag = true;
                                return;
                            }
                            if (gameData.ifFail())
                            {
                                executeFlag = false;
                                restartFlag = true;
                                return;
                            }
                        }
                        return;
                    }
                    retry++;
                    MoveTo(75, 60);
                    LeftClick();
                    return;
                }
                else
                {
                    retry = 0;
                }
            }

            if (arguments[0] == 0)
            {
                currentTrigger = (arguments[1], arguments[2]);
                if (arguments[1] <= round && arguments[2] <= coin)
                {
                    currentMiniIndex++;
                    ExecuteDirective(round, coin, mode, checkUpgrade, executeMode);
                }
                else
                {
                    if (ifContinueGame)
                    {
                        MoveTo(75, 60);
                        LeftClick();
                    }
                    return;
                }
            }
            else if ((arguments[0] == 8 || arguments[0] == 6) && arguments[1] >= 65 && arguments[1] <= 90)
            { 
                if (mode == 0) // 放置指令
                {
                    RunCode(arguments);
                    if (gameData.IfDeploy())
                    {
                        currentMiniIndex++;
                    }
                }
                else
                {
                    RunCode(arguments);
                    currentMiniIndex++;
                }
            }
            else if (arguments[0] == 23) // 找英雄是否可放
            {
                if (gameData.IfHeroDeploy())
                {
                    currentMiniIndex++;
                }
            }
            else if (arguments[0] == 24) // 技能释放前rgb
            {
                abiliityRgb = gameData.AbilityReady(arguments[1]);
                List<int> keyarguments = new List<int> { 8, arguments[2] };
                RunCode(keyarguments);
                currentMiniIndex++;
            }
            else if (arguments[0] == 25) // 技能释放后rgb
            {
                int currentRgb = gameData.AbilityReady(arguments[1]);
                if (Math.Abs(currentRgb - abiliityRgb) < 15)
                {
                    currentMiniIndex--;
                }
                else
                {
                    currentMiniIndex++;
                }
            }
            else if (arguments[0] == 20) // 结束自由游戏
            {
                circleTimes++;
                currentIndex = 0;
                currentMiniIndex = 0;
                completeFlag = false;
                executeFlag = false;
                stopFlag = true;
                selectMapFlag = true;
                ifContinueGame = false;
            }
            else if (arguments[0] == 21) // 开始自由游戏
            {
                ifContinueGame = true;
                currentMiniIndex++;
            }
            else if (arguments[0] == 17) // 游戏胜利结束判定
            {
                gameData = new GetGameData(windowDpi, gameDpi, hWnd);
                if (gameData.Complete())
                {
                    currentMiniIndex++;
                }
            }
            else
            {
                RunCode(arguments);
                currentMiniIndex++;
            }
            if (currentMiniIndex == currentInstructionCount)
            {
                currentMiniIndex = 0;
                currentIndex++;
            }
            if (currentIndex == directive.Count)
            {
                currentIndex = 0;
                executeFlag = false;
                if (executeMode == 3)
                {
                    restartFlag = true;
                    return;
                }
                completeFlag = true;
                stopFlag = true;
            }
            return;
        }

        public void ExecuteFindMap()
        {
            findMapFlag = true;
            if (currentIndex == findMapDirective.Count)
            {
                currentIndex = 0;
                findMapFlag = false;
                selectHeroFlag = true;
                stopFlag = false;
                return;
            }
            List<int> arguments = findMapDirective[currentIndex][currentMiniIndex].Split(' ').Select(Int32.Parse).ToList();
            if (arguments[0] == 18)
            {
                gameData = new GetGameData(windowDpi, gameDpi, hWnd);
                currentMap = gameData.GetMapId();
                currentMiniIndex++;
            }
            else
            {
                RunCode(arguments);
                currentMiniIndex++;
            }
            if (currentMiniIndex == findMapDirective[currentIndex].Count)
            {
                currentIndex++;
                currentMiniIndex = 0;
            }
        }

        public void ExecuteSelectHero()
        {
            selectHeroFlag = true;
            if (currentIndex == selectHeroDirective.Count)
            {
                currentIndex = 0;
                selectHeroFlag = false;
                executeFlag = true;
                stopFlag = false;
                return;
            }
            List<int> arguments = selectHeroDirective[currentIndex][currentMiniIndex].Split(' ').Select(Int32.Parse).ToList();
            if (arguments[0] == 19)
            {
                gameData = new GetGameData(windowDpi, gameDpi, hWnd);
                (int, int) pos = gameData.GetHeroPos(arguments[1]);
                if (pos.Item1 != -1)
                {
                    MoveTo(pos.Item1, pos.Item2);
                    if (selectHeroDirective[currentIndex][currentMiniIndex + 1] != "2")
                    {
                        selectHeroDirective[currentIndex].Insert(currentMiniIndex + 1, "2");
                    }
                    currentMiniIndex++;
                }
                else
                {
                    MouseWheel(-2);
                    findHeroretrys++;
                    if (findHeroretrys > 10)
                    {
                        heroUnlocked = true;
                        return;
                    }
                }
            }
            else
            {
                RunCode(arguments);
                currentMiniIndex++;
            }
            if (currentMiniIndex == selectHeroDirective[currentIndex].Count)
            {
                currentIndex++;
                currentMiniIndex = 0;
            }
        }

        public void ExecuteSelectMap()
        {
            selectMapFlag = true;
            if (currentIndex == selectMapDirective.Count)
            {
                currentIndex = 0;
                selectMapFlag = false;
                executeFlag = true;
                stopFlag = false;
                return;
            }
            List<int> arguments = selectMapDirective[currentIndex][currentMiniIndex].Split(' ').Select(Int32.Parse).ToList();
            if (arguments[0] == 16)
            {
                gameData = new GetGameData(windowDpi, gameDpi, hWnd);
                (int, int) pos = gameData.GetMapPos(arguments[1]);
                if (pos.Item1 != -1)
                {
                    MoveTo(pos.Item1, pos.Item2);
                    if (selectMapDirective[currentIndex][currentMiniIndex + 1] != "2")
                    {
                        selectMapDirective[currentIndex].Insert(currentMiniIndex + 1, "2");
                    }
                    currentMiniIndex++;
                }
                else
                {
                    currentMiniIndex--;
                }
            }
            else
            {
                RunCode(arguments);
                currentMiniIndex++;
            }
            if (currentMiniIndex == selectMapDirective[currentIndex].Count)
            {
                currentIndex++;
                currentMiniIndex = 0;
            }
        }

        public void ExecuteCompleteDirective(int mode) // 0:刷循环 1: 刷收集
        {
            completeFlag = true;
            if (currentIndex == completeDirective.Count)
            {
                circleTimes++;
                currentIndex = 0;
                completeFlag = false;
                if (mode == 0)
                {
                    selectMapFlag = true;
                }
                if (mode == 1)
                {
                    findMapFlag = true;
                }
                return;
            }
            List<int> arguments = completeDirective[currentIndex][currentMiniIndex].Split(' ').Select(Int32.Parse).ToList();
            if (arguments[0] == 17)
            {
                if (!gameData.InGame())
                {
                    MoveTo(75, 60);
                    LeftClick();
                }
                if (gameData.ifFail())
                {
                    completeFlag = false;
                    restartFlag = true;
                    return;
                }
                if (gameData.Complete())
                {
                    //File.AppendAllText(@"D:\log.txt", "complete\n");
                    currentMiniIndex++;
                }
            }
            else
            {
                RunCode(arguments);
                currentMiniIndex++;
            }

            if (currentMiniIndex == completeDirective[currentIndex].Count)
            {
                currentIndex++;
                currentMiniIndex = 0;
            }
        }

        public void ExecuteRestartDirective()
        {
            restartFlag = true;
            if (currentIndex == restartDirective.Count)
            {
                currentIndex = 0;
                restartFlag = false;
                executeFlag = true;
                return;
            }


            List<int> arguments = restartDirective[currentIndex][currentMiniIndex].Split(' ').Select(Int32.Parse).ToList();
            if (arguments[0] == 22)
            {
                if (gameData.ifFail() || gameData.Complete())
                {
                    int cur_x = gameData.GetRestartX();
                    if (cur_x == -1)
                    {
                        return;
                    }
                    if (gameDpi == 0)
                    {
                        MoveTo(cur_x, 810);
                    }
                    else
                    {
                        MoveTo((int)(cur_x / 1.5), (int)(810 / 1.5));
                    }
                    currentMiniIndex++;
                    return;
                }
                else
                {
                    if (!gameData.InGame())
                    {
                        MoveTo(75, 60);
                        LeftClick();
                    }
                }
            }
            else
            {
                RunCode(arguments);
                currentMiniIndex++;
            }


            if (currentMiniIndex == restartDirective[currentIndex].Count)
            {
                currentIndex++;
                currentMiniIndex = 0;
            }
        }

        private int GetColorIndex(int route)
        {
            if (currentCoords.Item1 >= (gameDpi == 0 ? 837 : 558) )
            {
                if (route == 188) return 0;
                if (route == 190) return 1;
                if (route == 191) return 2;
            }
            if (currentCoords.Item1 <= (gameDpi == 0 ? 836 : 557))
            {
                if (route == 188) return 3;
                if (route == 190) return 4;
                if (route == 191) return 5;
            }
            return 0;
        }

        private byte ScanCode(uint key)
        {
            return (byte)MapVirtualKey(key, 0);
        }

        public void MoveTo(int x, int y)
        {
            DisPlayMouseCoordinates.POINT mousePos;
            mousePos.X = (int)(x / windowDpi);
            mousePos.Y = (int)(y / windowDpi);
            DisPlayMouseCoordinates.ClientToScreen(hWnd, ref mousePos);

            int screenX = mousePos.X * 65536 / DisPlayMouseCoordinates.GetSystemMetrics(0);
            int screenY = mousePos.Y * 65536 / DisPlayMouseCoordinates.GetSystemMetrics(1);

            var input = new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE,
                        dx = screenX,
                        dy = screenY
                    }
                }
            };
            SendInput(1, new[] { input }, INPUT.Size);
            //IntPtr lParam = (IntPtr)((y << 16) | (x & 0xffff));

            //PostMessage(hWnd, 0x0200, IntPtr.Zero, lParam);
        }

        public void LeftDown()
        {
            SendMouseInput(MOUSEEVENTF_LEFTDOWN);
        }

        public void LeftUp()
        {
            SendMouseInput(MOUSEEVENTF_LEFTUP);
        }

        public void KeyDown(uint key)
        {
            if (key == 34 || key == 33)
            {
                SendKeyboardInput(key, KEYEVENTF_EXTENDEDKEY);
            }
            else
            {
                SendKeyboardInput(key, 0);
            }
            //if (SetForegroundWindow(hWnd))
            //{
            //    SendKeys.Send("{U}");
            //}
        }

        public void KeyUp(uint key)
        {
            if (key == 34 || key == 33)
            {
                SendKeyboardInput(key, KEYEVENTF_KEYUP | KEYEVENTF_EXTENDEDKEY);
            }
            else
            {
                //MessageBox.Show("keyup");
                SendKeyboardInput(key, KEYEVENTF_KEYUP);
            }
        }

        public void MouseWheel(int arg)
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

        public void LeftClick()
        {
            LeftDown();
            LeftUp();
        }

        private void SendMouseInput(uint dwFlags)
        {
            //var input = new INPUT
            //{
            //    type = INPUT_MOUSE,
            //    U = new InputUnion
            //    {
            //        mi = new MOUSEINPUT
            //        {
            //            dwFlags = dwFlags
            //        }
            //    }
            //};
            //SendInput(1, new[] { input }, INPUT.Size);
            int lParam = (currentCoords.Item1 << 16) | (currentCoords.Item2 & 0xffff);
            if (dwFlags == MOUSEEVENTF_LEFTDOWN)
            {
                SendMessage(hWnd, 0x0201, 0, lParam);
            }
            else
            {
                SendMessage(hWnd, 0x0202, 0, lParam);
            }
        }

        private void SendKeyboardInput(uint key, uint dwFlags)
        {
            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)key,
                        wScan = ScanCode(key),
                        dwFlags = dwFlags
                    }
                }
            };
            SendInput(1, new[] { input }, INPUT.Size);
        }
    }
}
