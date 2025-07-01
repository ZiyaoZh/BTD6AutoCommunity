using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static BTD6AutoCommunity.InputSimulator;
using static BTD6AutoCommunity.GameVisionRecognizer;
using static BTD6AutoCommunity.ScriptEditorSuite;
using System.Diagnostics;

namespace BTD6AutoCommunity
{
    internal class StrategyExecutor
    {
        private readonly GameContext _context;
        private readonly List<List<string>> scriptDirective;
        private readonly List<string> digitalScript;
        private readonly List<string> displayScript;

        private readonly object _checkExecutorLock = new object();
        private volatile bool _isExecuting = false;

        public ScriptInstructionInfo instructionInfo;
        private Dictionary<ActionTypes, Action> actionHandlers;

        public int currentFirstIndex;
        public int currentSecondIndex;

        // 放置失败处理变量
        private readonly List<int> reDeployList;
        private int currentReDeployIndex;
        private bool reDeployFlag;
        private int reDeployPace;

        private int gameRetryCount;
        public (int round, int cash) currentTrigger;
        private int abilityRgb;

        public bool IsStartFreePlay;
        public bool StartFreePlayFinished;

        public bool Finished;

        public StrategyExecutor(GameContext context, ScriptEditorSuite script)
        {
            _context = context;
            scriptDirective = script.compilerDirective;
            digitalScript = script.Digitalinstructions;
            displayScript = script.Displayinstructions;
            foreach (var dir in scriptDirective)
            {
                foreach (var arg in dir)
                {
                    Debug.WriteLine(arg);
                }
            }
            foreach (var dis in displayScript)
            {
                Debug.WriteLine(dis);
            }
            InitActionHandlers();
            currentFirstIndex = 0;
            currentSecondIndex = 0;
            gameRetryCount = 0;

            currentTrigger = (0, 0);

            // 1 上 2 下 3 左 4 右 5 左上 6 左下 7 右上 8 右下 
            reDeployList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
            currentReDeployIndex = -1;
            reDeployFlag = false;
            reDeployPace = 1;

            IsStartFreePlay = false;
            StartFreePlayFinished = false;

            Finished = false;
        }

        private void InitActionHandlers()
        {
            actionHandlers = new Dictionary<ActionTypes, Action>()
            {
                { ActionTypes.PlaceMonkey, HandlePlaceMonkey },
                { ActionTypes.UpgradeMonkey, HandleUpgradeMonkey },
                { ActionTypes.SwitchMonkeyTarget, HandleSwitchMonkeyTarget },
                { ActionTypes.UseAbility, HandleUseAbility },
                { ActionTypes.SwitchSpeed, HandleSwitchSpeed },
                { ActionTypes.SellMonkey, HandleSellMonkey },
                { ActionTypes.SetMonkeyFunction, HandleSetMonkeyFunction },
                { ActionTypes.PlaceHero, HandlePlaceHero },
                { ActionTypes.UpgradeHero, HandleUpgradeHero },
                { ActionTypes.PlaceHeroItem, HandlePlaceHeroItem },
                { ActionTypes.SwitchHeroTarget, HandleSwitchHeroTarget },
                { ActionTypes.SetHeroFunction, HandleSetHeroFunction },
                { ActionTypes.SellHero, HandleSellHero },
                { ActionTypes.MouseClick, HandleMouseClick },
                { ActionTypes.AdjustMonkeyCoordinates, HandleAdjustMonkeyCoordinates },
                { ActionTypes.WaitMilliseconds, HandleWaitMilliseconds },
                { ActionTypes.Jump, HandleJump },
                { ActionTypes.StartFreeplay, HandleStartFreeplay },
                { ActionTypes.EndFreeplay, HandleEndFreeplay }
            };
        }


        public void Tick(int currentRound, int currentCash)
        {
            if (_isExecuting) return;
            lock (_checkExecutorLock)
            {
                if (_isExecuting) return;
                _isExecuting = true;
            }
            try
            {
                //Debug.WriteLine($"currentFirstIndex: {currentFirstIndex} currentSecondIndex: {currentSecondIndex}");
                if (currentFirstIndex >= digitalScript.Count)
                {
                    Finished = true;
                    return;
                }
                if (currentSecondIndex == 0)
                {
                    instructionInfo = GetScriptInstructionInfo(digitalScript[currentFirstIndex]);
                    instructionInfo.Index = currentFirstIndex;
                    instructionInfo.Content = displayScript[currentFirstIndex];
                    List<int> arguments = scriptDirective[currentFirstIndex][0].Split(' ').Select(Int32.Parse).ToList();
                    currentTrigger = (arguments[1], arguments[2]);

                    // 不满足触发条件
                    if (arguments[1] > currentRound || arguments[2] > currentCash)
                    {
                        if (arguments[1] > currentRound)
                        {
                            instructionInfo.IsRoundMet = false;
                        }
                        if (arguments[2] > currentCash)
                        {
                            instructionInfo.IsCashMet = false;
                        }
                        return;
                    }

                    currentSecondIndex++;
                    Tick(currentRound, currentCash);
                }
                if (actionHandlers.TryGetValue(instructionInfo.Type, out Action handler))
                {
                    handler.Invoke();

                }
            }
            finally
            {
                _isExecuting = false;
            }

        }

        private void HandlePlaceMonkey()
        {
            //CheckInGame();

            int currentInstructionCount = scriptDirective[currentFirstIndex].Count;
            List<int> arguments = scriptDirective[currentFirstIndex][currentSecondIndex].Split(' ').Select(Int32.Parse).ToList();

            if (arguments[0] == 1) // 移动到猴子放置位置
            {
                if (reDeployFlag)
                {
                    if (currentReDeployIndex == reDeployList.Count)
                    {
                        currentReDeployIndex = 0;
                        reDeployPace++;
                    }
                    int x = arguments[1];
                    int y = arguments[2];
                    switch (reDeployList[currentReDeployIndex])
                    {
                        case 1: // 上
                            arguments[2] += reDeployPace;
                            break;
                        case 2: // 下
                            arguments[2] -= reDeployPace;
                            break;
                        case 3: // 左
                            arguments[1] -= reDeployPace;
                            break;
                        case 4: // 右
                            arguments[1] += reDeployPace;
                            break;
                        case 5: // 左上
                            arguments[1] -= reDeployPace;
                            arguments[2] += reDeployPace;
                            break;
                        case 6: // 左下
                            arguments[1] -= reDeployPace;
                            arguments[2] -= reDeployPace;
                            break;
                        case 7: // 右上
                            arguments[1] += reDeployPace;
                            arguments[2] += reDeployPace;
                            break;
                        case 8: // 右下
                            arguments[1] += reDeployPace;
                            arguments[2] -= reDeployPace;
                            break;
                    }
                    RunCode(arguments);
                    currentSecondIndex = currentInstructionCount - 2;
                    arguments[1] = x;
                    arguments[2] = y;
                }
                else
                {
                    RunCode(arguments);
                    currentSecondIndex++;
                }
            }
            else if (arguments[0] == 8)
            {
                RunCode(arguments);
                if (IsMonkeyDeploy(_context))
                {
                    currentSecondIndex++;
                }
            }
            else if (arguments[0] == 26) // 检测放置是否成功
            {
                Thread.Sleep(80);
                if (!IsMonkeyDeploy(_context))
                {
                    reDeployFlag = false;
                    currentReDeployIndex = -1;
                    reDeployPace = 1;
                    currentSecondIndex++;
                }
                else
                {
                    reDeployFlag = true;
                    currentReDeployIndex++;
                    currentSecondIndex = 1;
                }
            }
            else
            {
                RunCode(arguments);
                currentSecondIndex++;
            }
            if (currentSecondIndex == currentInstructionCount)
            {
                reDeployFlag = false;
                currentSecondIndex = 0;
                currentFirstIndex++;
            }
            return;
        }

        private void HandleUpgradeMonkey()
        { // 升级指令
            //CheckInGame();

            int currentInstructionCount = scriptDirective[currentFirstIndex].Count;

            List<int> arguments = scriptDirective[currentFirstIndex][currentSecondIndex].Split(' ').Select(Int32.Parse).ToList();

            if (arguments[0] == 8) // 升级猴子
            {
                int route = instructionInfo.Arguments[2];

                int colorIndex = GetColorIndex(route);
                //Debug.WriteLine("Upgrade " + colorIndex);
                if (colorIndex == -1) return; // 未弹出升级界面

                int p = instructionInfo.Arguments[3];
                Debug.WriteLine("Upgrade " + colorIndex + " " + p);
                if (p == 0) return;

                if (!GetYellowBlockCount(_context, colorIndex, p))
                {
                    RunCode(arguments);
                    return;
                }
                currentSecondIndex++;
            }
            else
            {
                RunCode(arguments);
                currentSecondIndex++;
            }
            if (currentSecondIndex == currentInstructionCount)
            {
                currentSecondIndex = 0;
                currentFirstIndex++;
            }
            return;
        }

        private void HandleSwitchMonkeyTarget()
        {
            HandleNormalAction();
        }

        private void HandleUseAbility()
        {
            int currentInstructionCount = scriptDirective[currentFirstIndex].Count;

            List<int> arguments = scriptDirective[currentFirstIndex][currentSecondIndex].Split(' ').Select(Int32.Parse).ToList();


            if (arguments[0] == 24) // 技能释放前rgb
            {
                abilityRgb = AbilityReady(_context, arguments[1]);
                List<int> keyarguments = new List<int> { 8, arguments[2] };
                RunCode(keyarguments);
                currentSecondIndex++;
            }
            else if (arguments[0] == 25) // 技能释放后rgb
            {
                int currentRgb = AbilityReady(_context, arguments[1]);
                if (Math.Abs(currentRgb - abilityRgb) < 15)
                {
                    currentSecondIndex--;
                }
                else
                {
                    currentSecondIndex++;
                }
            }
            else
            {
                RunCode(arguments);
                currentSecondIndex++;
            }
            if (currentSecondIndex == currentInstructionCount)
            {
                currentSecondIndex = 0;
                currentFirstIndex++;
            }
            return;
        }

        private void HandleSwitchSpeed()
        {
            HandleNormalAction();
        }

        private void HandleSellMonkey()
        {
            HandleNormalAction();
        }

        private void HandleSetMonkeyFunction()
        {
            HandleNormalAction();
        }

        private void HandlePlaceHero()
        {
            int currentInstructionCount = scriptDirective[currentFirstIndex].Count;

            List<int> arguments = scriptDirective[currentFirstIndex][currentSecondIndex].Split(' ').Select(Int32.Parse).ToList();

            if (arguments[0] == 23) // 找英雄是否可放
            {
                if (IsHeroDeploy(_context))
                {
                    currentSecondIndex++;
                }
            }
            else if (arguments[0] == 1 && currentSecondIndex == 4) // 移动到英雄放置位置
            {
                if (reDeployFlag)
                {
                    if (currentReDeployIndex == reDeployList.Count)
                    {
                        currentReDeployIndex = 0;
                        reDeployPace++;
                    }
                    int x = arguments[1];
                    int y = arguments[2];
                    switch (reDeployList[currentReDeployIndex])
                    {
                        case 1: // 上
                            arguments[2] += reDeployPace;
                            break;
                        case 2: // 下
                            arguments[2] -= reDeployPace;
                            break;
                        case 3: // 左
                            arguments[1] -= reDeployPace;
                            break;
                        case 4: // 右
                            arguments[1] += reDeployPace;
                            break;
                        case 5: // 左上
                            arguments[1] -= reDeployPace;
                            arguments[2] += reDeployPace;
                            break;
                        case 6: // 左下
                            arguments[1] -= reDeployPace;
                            arguments[2] -= reDeployPace;
                            break;
                        case 7: // 右上
                            arguments[1] += reDeployPace;
                            arguments[2] += reDeployPace;
                            break;
                        case 8: // 右下
                            arguments[1] += reDeployPace;
                            arguments[2] -= reDeployPace;
                            break;
                    }
                    RunCode(arguments);
                    currentSecondIndex = currentInstructionCount - 2;
                    arguments[1] = x;
                    arguments[2] = y;
                }
                else
                {
                    RunCode(arguments);
                    currentSecondIndex++;
                }
            }
            else if (arguments[0] == 26) // 检测放置是否成功
            {
                Thread.Sleep(80);
                if (!IsMonkeyDeploy(_context))
                {
                    reDeployFlag = false;
                    currentReDeployIndex = -1;
                    reDeployPace = 1;
                    currentSecondIndex++;
                }
                else
                {
                    reDeployFlag = true;
                    currentReDeployIndex++;
                    currentSecondIndex = 4;
                }
            }
            else
            {
                RunCode(arguments);
                currentSecondIndex++;
            }
            if (currentSecondIndex == currentInstructionCount)
            {
                currentSecondIndex = 0;
                currentFirstIndex++;
            }
        }

        private void HandleUpgradeHero()
        {
            HandleNormalAction();
        }

        private void HandlePlaceHeroItem()
        {
            HandleNormalAction();
        }

        private void HandleSwitchHeroTarget()
        {
            HandleNormalAction();
        }

        private void HandleSetHeroFunction()
        {
            HandleNormalAction();
        }

        private void HandleSellHero()
        {
            HandleNormalAction();
        }

        private void HandleMouseClick()
        {
            HandleNormalAction();
        }

        private void HandleAdjustMonkeyCoordinates()
        {
            HandleNormalAction();
        }

        private void HandleWaitMilliseconds()
        {
            HandleNormalAction();
        }

        private void HandleJump()
        {
            int currentInstructionCount = scriptDirective[currentFirstIndex].Count;

            List<int> arguments = scriptDirective[currentFirstIndex][currentSecondIndex].Split(' ').Select(Int32.Parse).ToList();

            currentFirstIndex = arguments[1] - 1;
            currentSecondIndex = 0;
            return;
        }

        private void HandleStartFreeplay()
        {
            int currentInstructionCount = scriptDirective[currentFirstIndex].Count;

            List<int> arguments = scriptDirective[currentFirstIndex][currentSecondIndex].Split(' ').Select(Int32.Parse).ToList();
            
            if (arguments[0] == 21)
            {
                IsStartFreePlay = true;
                if (!StartFreePlayFinished) return;
                StartFreePlayFinished = false;
                IsStartFreePlay = false;
                currentSecondIndex++;
                return;
            }
            RunCode(arguments);
            currentSecondIndex++;
            if (currentSecondIndex == currentInstructionCount)
            {
                currentSecondIndex = 0;
                currentFirstIndex++;
            }
        }

        private void HandleEndFreeplay()
        {
            HandleNormalAction();
        }
        
        private void HandleNormalAction()
        {
            //Debug.WriteLine($"NormalSecondIndex: {currentSecondIndex}");
            int currentInstructionCount = scriptDirective[currentFirstIndex].Count;

            List<int> arguments = scriptDirective[currentFirstIndex][currentSecondIndex].Split(' ').Select(Int32.Parse).ToList();
            RunCode(arguments);
            currentSecondIndex++;
            if (currentSecondIndex == currentInstructionCount)
            {
                currentSecondIndex = 0;
                currentFirstIndex++;
            }
        }

        private void CheckInGame()
        {
            if (!IsInGame(_context))
            {
                if (gameRetryCount > 8)
                {
                    gameRetryCount = 0;
                    currentFirstIndex = 0;
                    currentSecondIndex = 0;
                    return;
                }
                gameRetryCount++;
                MouseMove(_context, 75, 60);
                MouseLeftClick();
                return;
            }
            else
            {
                gameRetryCount = 0;
            }
        }

        private int GetColorIndex(int route)
        {
            if (IsRightUpgrading(_context))
            {
                return route + 3;
            }
            if (IsLeftUpgrading(_context))
            {
                return route;
            }
            return -1;
        }

        private void RunCode(List<int> arguments)
        {
            switch (arguments[0])
            {
                case 1:
                    MouseMove(_context, arguments[1], arguments[2]);
                    break;
                case 2:
                    MouseLeftClick();
                    break;
                case 3:
                    MouseLeftDown();
                    break;
                case 4:
                    MouseLeftUp();
                    break;
                case 5:
                    MouseWheel(arguments[1]);
                    break;
                case 6:
                    KeyboardPress((ushort)arguments[1]);
                    break;
                case 7:
                    KeyboardRelease((ushort)arguments[1]);
                    break;
                case 8:
                    KeyboardPressAndRelease((ushort)arguments[1]);
                    break;
                case 10: // 空指令
                    break;
                case 11: // 移动+点击
                    MouseMoveAndLeftClick(_context, arguments[1], arguments[2]);
                    break;
                    // 16 指令跳转
                    // 18 收集识别地图
                    // 19 收集找英雄
                    // 20 结束自由游戏
                    // 21 开始自动回合
                    // 22 找重开位置
                    // 23 找英雄是否可放
                    // 24 技能释放前rgb
                    // 25 技能释放后rgb
                    // 26 检测放置是否成功
            }
        }
    }
}
