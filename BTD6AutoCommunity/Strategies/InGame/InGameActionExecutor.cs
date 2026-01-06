using BTD6AutoCommunity.ScriptEngine.InstructionSystem;
using BTD6AutoCommunity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using BTD6AutoCommunity.ScriptEngine;

namespace BTD6AutoCommunity.Strategies.InGame
{
    public class InGameActionExecutor
    {
        private readonly GameContext _context;
        private readonly List<ExecutableInstruction> ExecutableInstructions;

        private readonly object _checkExecutorLock = new object();
        private volatile bool _isExecuting = false;

        public ExecutableInstruction currentInstrucion;
        private Dictionary<ActionTypes, Action> actionHandlers;

        public int currentFirstIndex;
        public int currentSecondIndex;

        // 放置失败处理变量
        private readonly List<int> reDeployList;
        private int currentReDeployIndex;
        private bool reDeployFlag;
        private int reDeployPace;

        public (int round, int cash) currentTrigger;
        private int abilityRgb;

        public bool IsStartFreePlay;
        public bool StartFreePlayFinished;

        public bool Finished;

        public InGameActionExecutor(GameContext context, List<ExecutableInstruction> instructions)
        {
            _context = context;

            ExecutableInstructions = instructions;

            InitActionHandlers();
            currentFirstIndex = 0;
            currentSecondIndex = 0;

            currentTrigger = (0, 0);

            // 1 上 2 下 3 左 4 右 5 左上 6 左下 7 右上 8 右下 
            reDeployList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
            currentReDeployIndex = -1;
            reDeployFlag = false;
            reDeployPace = 10000;

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
                if (currentFirstIndex >= ExecutableInstructions.Count)
                {
                    Finished = true;
                    return;
                }
                if (currentSecondIndex == 0)
                {
                    currentInstrucion = ExecutableInstructions[currentFirstIndex];
                    currentTrigger = (currentInstrucion.RoundTrigger, currentInstrucion.CoinTrigger);

                    // 不满足触发条件
                    if (currentTrigger.round > currentRound || currentTrigger.cash > currentCash)
                    {
                        if (currentTrigger.round > currentRound)
                        {
                            currentInstrucion.IsRoundMet = false;
                        }
                        if (currentTrigger.cash > currentCash)
                        {
                            currentInstrucion.IsCoinMet = false;
                        }
                        return;
                    }
                    currentInstrucion.IsRoundMet = true;
                    currentInstrucion.IsCoinMet = true;
                }
                if (actionHandlers.TryGetValue(currentInstrucion.Type, out Action handler))
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
            int currentInstructionCount = currentInstrucion.Count;
            MicroInstruction micro = currentInstrucion[currentSecondIndex];

            if (micro.Type == MicroInstructionType.MouseMove) // 移动到猴子放置位置
            {
                if (reDeployFlag)
                {
                    if (currentReDeployIndex == reDeployList.Count)
                    {
                        currentReDeployIndex = 0;
                        reDeployPace++;
                    }
                    int x = micro[1];
                    int y = micro[2];
                    switch (reDeployList[currentReDeployIndex])
                    {
                        case 1: // 上
                            micro[2] += reDeployPace;
                            break;
                        case 2: // 下
                            micro[2] -= reDeployPace;
                            break;
                        case 3: // 左
                            micro[1] -= reDeployPace;
                            break;
                        case 4: // 右
                            micro[1] += reDeployPace;
                            break;
                        case 5: // 左上
                            micro[1] -= reDeployPace;
                            micro[2] += reDeployPace;
                            break;
                        case 6: // 左下
                            micro[1] -= reDeployPace;
                            micro[2] -= reDeployPace;
                            break;
                        case 7: // 右上
                            micro[1] += reDeployPace;
                            micro[2] += reDeployPace;
                            break;
                        case 8: // 右下
                            micro[1] += reDeployPace;
                            micro[2] -= reDeployPace;
                            break;
                    }
                    RunCode(micro);
                    currentSecondIndex = 2;
                    micro[1] = x;
                    micro[2] = y;
                }
                else
                {
                    RunCode(micro);
                    currentSecondIndex++;
                }
            }
            else if (micro.Type == MicroInstructionType.KeyboardPressAndRelease)
            {
                RunCode(micro);
                if (GameVisionRecognizer.IsMonkeyDeploy(_context))
                {
                    currentSecondIndex++;
                }
            }
            else if (micro[0] == 26) // 检测放置是否成功
            {
                Thread.Sleep(80);
                if (!GameVisionRecognizer.IsMonkeyDeploy(_context))
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
                    currentSecondIndex = 0;
                }
            }
            else
            {
                RunCode(micro);
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

            int currentInstructionCount = currentInstrucion.Count;
            MicroInstruction micro = currentInstrucion[currentSecondIndex];

            if (micro.Type == MicroInstructionType.CheckColorAndHitKey) // 升级猴子
            {
                //int route = instructionInfo.Arguments[2];

                int colorIndex = GetColorIndex(micro[2]);
                ////Debug.WriteLine("Upgrade " + colorIndex);
                //if (colorIndex == -1) return; // 未弹出升级界面

                //int p = instructionInfo.Arguments[3];
                //Debug.WriteLine("Upgrade " + colorIndex + " " + p);
                //if (p == 0) return;

                if (!GameVisionRecognizer.GetYellowBlockCount(_context, colorIndex, micro[3]))
                {
                    RunCode(micro);
                    return;
                }
                currentSecondIndex++;
            }
            else
            {
                RunCode(micro);
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
            HandleNormalAction();
            //int currentInstructionCount = currentInstrucion.Count;
            //MicroInstruction micro = currentInstrucion[currentSecondIndex];

            //if (micro[0] == 24) // 技能释放前rgb
            //{
            //    abilityRgb = GameVisionRecognizer.AbilityReady(_context, arguments[1]);
            //    List<int> keyarguments = new List<int> { 8, arguments[2] };
            //    RunCode(keyarguments);
            //    currentSecondIndex++;
            //}
            //else if (arguments[0] == 25) // 技能释放后rgb
            //{
            //    int currentRgb = GameVisionRecognizer.AbilityReady(_context, arguments[1]);
            //    if (Math.Abs(currentRgb - abilityRgb) < 15)
            //    {
            //        currentSecondIndex--;
            //    }
            //    else
            //    {
            //        currentSecondIndex++;
            //    }
            //}
            //else
            //{
            //    RunCode(arguments);
            //    currentSecondIndex++;
            //}
            //if (currentSecondIndex == currentInstructionCount)
            //{
            //    currentSecondIndex = 0;
            //    currentFirstIndex++;
            //}
            //return;
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
            int currentInstructionCount = currentInstrucion.Count;
            MicroInstruction micro = currentInstrucion[currentSecondIndex];

            if (micro.Type == MicroInstructionType.IsHeroCanPlace) // 找英雄是否可放
            {
                if (GameVisionRecognizer.IsHeroDeploy(_context))
                {
                    currentSecondIndex++;
                }
            }
            else if (micro.Type == MicroInstructionType.MouseMove && currentSecondIndex == 3) // 移动到英雄放置位置
            {
                if (reDeployFlag)
                {
                    if (currentReDeployIndex == reDeployList.Count)
                    {
                        currentReDeployIndex = 0;
                        reDeployPace++;
                    }
                    int x = micro[1];
                    int y = micro[2];
                    switch (reDeployList[currentReDeployIndex])
                    {
                        case 1: // 上
                            micro[2] += reDeployPace;
                            break;
                        case 2: // 下
                            micro[2] -= reDeployPace;
                            break;
                        case 3: // 左
                            micro[1] -= reDeployPace;
                            break;
                        case 4: // 右
                            micro[1] += reDeployPace;
                            break;
                        case 5: // 左上
                            micro[1] -= reDeployPace;
                            micro[2] += reDeployPace;
                            break;
                        case 6: // 左下
                            micro[1] -= reDeployPace;
                            micro[2] -= reDeployPace;
                            break;
                        case 7: // 右上
                            micro[1] += reDeployPace;
                            micro[2] += reDeployPace;
                            break;
                        case 8: // 右下
                            micro[1] += reDeployPace;
                            micro[2] -= reDeployPace;
                            break;
                    }
                    RunCode(micro);
                    currentSecondIndex = 4;
                    micro[1] = x;
                    micro[2] = y;
                }
                else
                {
                    RunCode(micro);
                    currentSecondIndex++;
                }
            }
            else if (micro.Type == MicroInstructionType.CheckPlaceSuccess) // 检测放置是否成功
            {
                Thread.Sleep(80);
                if (!GameVisionRecognizer.IsMonkeyDeploy(_context))
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
                    currentSecondIndex = 3;
                }
            }
            else
            {
                RunCode(micro);
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
            MicroInstruction micro = currentInstrucion[currentSecondIndex];

            currentFirstIndex = micro[1] - 1;
            currentSecondIndex = 0;
            return;
        }

        private void HandleStartFreeplay()
        {
            int currentInstructionCount = currentInstrucion.Count;
            MicroInstruction micro = currentInstrucion[currentSecondIndex];

            if (micro.Type == MicroInstructionType.StartAutoRound)
            {
                IsStartFreePlay = true;
                if (!StartFreePlayFinished) return;
                StartFreePlayFinished = false;
                IsStartFreePlay = false;
                currentSecondIndex++;
                return;
            }
            RunCode(micro);
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
            int currentInstructionCount = currentInstrucion.Count;
            MicroInstruction micro = currentInstrucion[currentSecondIndex];
            RunCode(micro);
            currentSecondIndex++;
            if (currentSecondIndex == currentInstructionCount)
            {
                currentSecondIndex = 0;
                currentFirstIndex++;
            }
        }

        private int GetColorIndex(int route)
        {
            if (GameVisionRecognizer.IsRightUpgrading(_context))
            {
                return route + 3;
            }
            if (GameVisionRecognizer.IsLeftUpgrading(_context))
            {
                return route;
            }
            return -1;
        }

        private void RunCode(MicroInstruction micro)
        {
        //        public enum MicroInstructionType
        //{
        //    MoveMouse = 1,
        //    LeftClick = 2,
        //    LeftDown = 3,
        //    LeftUp = 4,
        //    MouseWheel = 5,
        //    KeyboardPress = 6,
        //    KeyboardRelease = 7,
        //    KeyboardPressAndRelease = 8,
        //    Empty = 10,
        //    MoveMouseAndLeftClick = 11,
        //    JumpTo = 16,
        //    FindCollectMap = 18,
        //    FindHero = 19,
        //    EndFreeGame = 20,
        //    StartAutoRound = 21,
        //    FindReplayPosition = 22,
        //    IsHeroCanPlace = 23,
        //    SkillReleaseBeforeRgb = 24,
        //    SkillReleaseAfterRgb = 25,
        //    CheckPlaceSuccess = 26
        //}
            switch (micro.Type)
            {
                case MicroInstructionType.MouseMove:
                    InputSimulator.MouseMove(_context, micro[1] / 10000, micro[2] / 10000);
                    break;
                case MicroInstructionType.LeftClick:
                    InputSimulator.MouseLeftClick();
                    break;
                case MicroInstructionType.LeftDown:
                    InputSimulator.MouseLeftDown();
                    break;
                case MicroInstructionType.LeftUp:
                    InputSimulator.MouseLeftUp();
                    break;
                case MicroInstructionType.MouseWheel:
                    InputSimulator.MouseWheel(micro[1]);
                    break;
                case MicroInstructionType.KeyboardPress:
                    InputSimulator.KeyboardPress((ushort)micro[1]);
                    break;
                case MicroInstructionType.KeyboardRelease:
                    InputSimulator.KeyboardRelease((ushort)micro[1]);
                    break;
                case MicroInstructionType.KeyboardPressAndRelease:
                case MicroInstructionType.CheckColorAndHitKey:
                    InputSimulator.KeyboardPressAndRelease((ushort)micro[1]);
                    break;

                case MicroInstructionType.Empty: // 空指令
                    break;
                case MicroInstructionType.MoveMouseAndLeftClick: // 移动+点击
                    InputSimulator.MouseMoveAndLeftClick(_context, micro[1], micro[2]);
                    break;
                    // 9 检测升级并击键
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
