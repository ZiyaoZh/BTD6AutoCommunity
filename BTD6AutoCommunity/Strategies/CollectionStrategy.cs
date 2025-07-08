using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Drawing;
using System.CodeDom.Compiler;
using OpenCvSharp.ML;

namespace BTD6AutoCommunity.Strategies
{
    // 收集策略模式枚举
    public enum CollectionMode
    {
        SimpleCollection,    // 简单收集
        DoubleCashCollection, // 双金收集
        FastPathCollection   // 快速路径收集
    }

    public class CollectionStrategy : Base.BaseStrategy
    {
        // 收集设置常量
        private const int CollectMapCount = 12;          // 每次运行收集地图数量
        private const int ExpertMapStartId = 90;        // 专家级地图起始ID
        private const int ExpertMapEndId = 101;           // 专家级地图终止ID


        private Dictionary<int, string> collectionScripts; // mapId -> 脚本路径
        private Dictionary<int, int> mapDifficulties; // mapId -> 难度
        private CollectionMode collectionMode;
        private int currentMapId;
        private bool IsHeroSelectionComplete;

        private int levelChallengingCount = 0;

        private int returnableScreenCount = 0;

        public CollectionStrategy(ScriptSettings settings, LogHandler logHandler)
            : base(settings, logHandler)
        {
            InitializeStateHandlers();
            currentMapId = 0;
        }

        protected override void OnPreStart()
        {
            _logs.Log($"开始收集...", LogLevel.Info);
            //Thread.Sleep(3000);
            mapDifficulties = new Dictionary<int, int>();
            collectionScripts = new Dictionary<int, string>();

            if (_settings.EnableFastPath)
            {
                collectionMode = CollectionMode.FastPathCollection;
            }
            else if (_settings.EnableDoubleCoin)
            {
                collectionMode = CollectionMode.DoubleCashCollection;
            }
            else
            {
                collectionMode = CollectionMode.SimpleCollection;
            }
            _logs.Log($"当前收集模式：{Constants.CollectionScripts[collectionMode]}", LogLevel.Info);

            for (int mapId = ExpertMapStartId; mapId <= ExpertMapEndId; mapId++)
            {
                foreach (int dif in new int[] { 0, 1, 2 })
                {
                    string scriptPath = scriptFileManager.GetScriptFullPath(
                        Constants.GetTypeName((Maps)mapId),
                        Constants.GetTypeName((LevelDifficulties)dif),
                        Constants.CollectionScripts[collectionMode]
                        );
                    if (scriptPath != null)
                    {
                        mapDifficulties.Add(mapId, dif);
                        collectionScripts.Add(mapId, scriptPath);
                        break;
                    }
                }
            }
            if (collectionScripts.Count != CollectMapCount)
            {
                _logs.Log($"收集脚本不完整！", LogLevel.Error);
                return;
            }
        }

        protected override void InitializeStateHandlers()
        {
            stateHandlers = new Dictionary<GameState, Action>
            {
                { GameState.GameMainScreen, HandleMainScreen },
                { GameState.RaceResultsScreen, HandleRaceResultsScreen },
                { GameState.BossResultsScreen, HandleBossResultsScreen },
                { GameState.LevelSelectionScreen, HandleLevelSelection },
                { GameState.LevelSearchScreen, HandleLevelSearch },
                { GameState.LevelSearchedScreen, HandleLevelSearched },
                { GameState.LevelDifficultySelectionScreen, HandleLevelDifficultySelection },
                { GameState.LevelEasyModeSelectionScreen, HandleLevelEasyModeSelection },
                { GameState.LevelMediumModeSelectionScreen, HandleLevelMediumModeSelection },
                { GameState.LevelHardModeSelectionScreen, HandleLevelHardModeSelection },
                { GameState.HeroSelectionScreen, HandleHeroSelection },
                { GameState.LevelTipScreen, HandleLevelTipScreen },
                { GameState.LevelChallengingScreen, HandleLevelChallengingScreen },
                { GameState.LevelChallengingWithTipScreen, HandleLevelChallengingWithTipScreen },
                { GameState.LevelPassedScreen, HandleLevelPassScreen },
                { GameState.LevelSettlementScreen, HandleLevelSettlementScreen },
                { GameState.LevelFailedScreen,  HandleLevelFailedScreen },
                { GameState.LevelUpgradingScreen, HandleLevelUpgradingScreen },
                { GameState.ReturnableScreen, HandleReturnableScreen },
                { GameState.CollectionActivitiesAvailableScreen, HandleChestCollection },
                { GameState.CollectionActivitiesScreen, HandleReturnableScreen },
                { GameState.ThreeChestsScreen, HandleThreeChestsScreen },
                { GameState.TwoChestsScreen, HandleTwoChestsScreen },
                { GameState.InstaScreen, HandleInstaScreen },
                { GameState.ChestsOpenedScreen, HandleChestsOpenedScreen }
                //{ GameState.UnKnown, HandleReturnableScreen }
                // 添加更多状态处理...
            };
        }

        // 示例处理函数
        private void HandleMainScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 940);
        }

        private void HandleRaceResultsScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 800);
            Thread.Sleep(500);
            HandleReturnableScreen();
            Thread.Sleep(500);
            HandleReturnableScreen();
            Thread.Sleep(500);
            HandleReturnableScreen();
            Thread.Sleep(500);
            HandleReturnableScreen();
        }

        private void HandleBossResultsScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 880);
            Thread.Sleep(500);
            HandleReturnableScreen();
            Thread.Sleep(500);
            HandleReturnableScreen();
            Thread.Sleep(500);
            HandleReturnableScreen();
            Thread.Sleep(500);
            HandleReturnableScreen();
        }

        private void HandleLevelSelection()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 80, 170);
            _logs.Log("已进入地图选择界面，开始选择收集额外地图", LogLevel.Info);
        }

        private void HandleChestCollection()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 680);
            _logs.Log("收集宝箱可打开，开始开箱", LogLevel.Info);
        }

        private void HandleLevelSearch()
        {
            if (lastState == GameState.LevelSearchScreen)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 1275, 45); // 猴子小队
                return;
            }
            InputSimulator.MouseMoveAndLeftClick(_context, 1350, 45); // 收集
        }

        private void HandleLevelSearched()
        {
            currentMapId = GameVisionRecognizer.RecognizeMapId(_context);
            //Debug.WriteLine("MapId: " + currentMapId);
            InputSimulator.MouseMoveAndLeftClick(_context, 540, 650);
            _logs.Log($"已识别到地图：{Constants.GetTypeName((Maps)currentMapId)}", LogLevel.Info);
        }

        private void HandleLevelDifficultySelection()
        {
            if (currentMapId < ExpertMapStartId || currentMapId > ExpertMapEndId)
            {
                HandleReturnableScreen();
                _logs.Log("非专家级地图，无法进入收集模式，返回", LogLevel.Error);
                return;
            }
            switch (mapDifficulties[currentMapId])
            {
                case 0:
                    InputSimulator.MouseMoveAndLeftClick(_context, 630, 400);
                    break;
                case 1:
                    InputSimulator.MouseMoveAndLeftClick(_context, 970, 400);
                    break;
                case 2:
                    InputSimulator.MouseMoveAndLeftClick(_context, 1300, 400);
                    break;
            }
            // 加载脚本
            GetExecutableInstructions(collectionScripts[currentMapId]);
        }

        private void HandleLevelEasyModeSelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法进入简单模式，返回", LogLevel.Error);
                return;
            }
            if (scriptMetadata.SelectedMode != LevelMode.Standard &&
                Constants.LevelModeToDifficulty[scriptMetadata.SelectedMode] != LevelDifficulties.Easy)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是简单模式，无法进入简单模式，返回", LogLevel.Error);
                return;
            }
            if (IsHeroSelectionComplete)
            {
                IsHeroSelectionComplete = false;

                Point point = Constants.GetLevelModePos(scriptMetadata.SelectedMode);
                InputSimulator.MouseMoveAndLeftClick(_context, point.X, point.Y);
            }
            else
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 100, 1000);
            }
            levelChallengingCount = 0;
        }

        private void HandleLevelMediumModeSelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
                {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法进入中级模式，返回", LogLevel.Error);
                return;
            }
            if (scriptMetadata.SelectedMode != LevelMode.Standard &&
                Constants.LevelModeToDifficulty[scriptMetadata.SelectedMode] != LevelDifficulties.Medium)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是中级模式，无法进入中级模式，返回", LogLevel.Error);
                return;
            }
            if (IsHeroSelectionComplete)
            {
                IsHeroSelectionComplete = false;
                Point point = Constants.GetLevelModePos(scriptMetadata.SelectedMode);
                InputSimulator.MouseMoveAndLeftClick(_context, point.X, point.Y);
            }
            else
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 100, 1000);
            }
            levelChallengingCount = 0;
        }

        private void HandleLevelHardModeSelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
                {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法进入困难模式，返回", LogLevel.Error);
                return;
            }
            if (scriptMetadata.SelectedMode != LevelMode.Standard &&
                Constants.LevelModeToDifficulty[scriptMetadata.SelectedMode] != LevelDifficulties.Hard)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是困难模式，无法进入困难模式，返回", LogLevel.Error);
                return;
            }
            if (IsHeroSelectionComplete)
            {
                IsHeroSelectionComplete = false;
                Point point = Constants.GetLevelModePos(scriptMetadata.SelectedMode);
                InputSimulator.MouseMoveAndLeftClick(_context, point.X, point.Y);
            }
            else
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 100, 1000);
            }
            levelChallengingCount = 0;
        }

        private void HandleHeroSelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，返回", LogLevel.Error);
                return;
            }
            if (IsHeroSelectionComplete || scriptMetadata == null)
            {
                HandleReturnableScreen();
                _logs.Log("英雄选择已完成，返回", LogLevel.Error);
                return;
            }
            Point heroPosition = GameVisionRecognizer.GetHeroPosition(_context, scriptMetadata.SelectedHero);
            
            for (int i = 0; i < 5 && heroPosition.X == -1; i++)
            {
                heroPosition = GameVisionRecognizer.GetHeroPosition(_context, scriptMetadata.SelectedHero);
                InputSimulator.MouseWheel(-10);
                Thread.Sleep(500);
            }
            if (heroPosition.X == -1)
            {
                Stop();
                //MessageBox.Show("未找到英雄位置！");
                _logs.Log("未找到英雄位置！收集结束，请卸下英雄皮肤，重新开始", LogLevel.Error);
                return;
            }
            //Debug.WriteLine("HeroPosition: " + heroPosition.ToString());
            InputSimulator.MouseMoveAndLeftClick(_context, heroPosition.X, heroPosition.Y);
            Thread.Sleep(500);
            InputSimulator.MouseMoveAndLeftClick(_context, 1120, 620);
            Thread.Sleep(500);
            InputSimulator.MouseMoveAndLeftClick(_context, 80, 55);
            IsHeroSelectionComplete = true;

            _logs.Log($"已选择英雄：{Constants.GetTypeName(scriptMetadata.SelectedHero)}", LogLevel.Info);
        }

        private void HandleLevelTipScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 1140, 730);
        }

        private void HandleLevelChallengingScreen()
        {
            levelChallengingCount++;
            if (executableInstructions == null || scriptMetadata == null)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 1600, 40);
                Thread.Sleep(500);
                InputSimulator.MouseMoveAndLeftClick(_context, 850, 850);
                _logs.Log("脚本未加载，无法进入战斗，返回", LogLevel.Error);
                return;
            }
            if (IsStrategyExecutionCompleted)
            {
                StopLevelTimer();
                return;
            }
            if (levelChallengingCount < 2)
            {
                return;
            }
            StartLevelTimer(0, _settings.EnableRecommendInterval);
        }

        private void HandleLevelChallengingWithTipScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 760);
        }

        private void HandleLevelPassScreen()
        {
            if (InGameActionExecutor != null && InGameActionExecutor.IsStartFreePlay)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 1200, 850);
                InGameActionExecutor.StartFreePlayFinished = true;
                _logs.Log("自由游戏已开启，开始下一关", LogLevel.Info);
                return;
            }
            InputSimulator.MouseMoveAndLeftClick(_context, 720, 850);
        }

        private void HandleLevelSettlementScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 910);
            if (InGameActionExecutor != null && InGameActionExecutor.IsStartFreePlay) return;
            StopLevelTimer();
            if (IsStrategyExecutionCompleted)
            {
                IsStrategyExecutionCompleted = false;
            }
            _logs.Log($"进入关卡结算界面，挑战成功，停止策略执行, 本关用时：{(int)(levelChallengingCount * 1.5 / 60)}分{(int)(levelChallengingCount * 1.5 % 60)}秒", LogLevel.Info);
        }

        private void HandleLevelUpgradingScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 980);
            _logs.Log("检测到升级界面，点击确认", LogLevel.Info);
        }

        private void HandleLevelFailedScreen()
        {
            StopLevelTimer();
            Point returnPos = GameVisionRecognizer.GetFailedScreenReturnPosition(_context);
            InputSimulator.MouseMoveAndLeftClick(_context, returnPos.X, returnPos.Y);
            _logs.Log("检测到关卡失败界面，回到主页", LogLevel.Info);
        }

        private void HandleReturnableScreen()
        {
            returnableScreenCount++;
            if (returnableScreenCount < 2) return;
            InputSimulator.MouseMoveAndLeftClick(_context, 80, 55);
            returnableScreenCount = 0;
        }

        private void HandleThreeChestsScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 660, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 660, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 1260, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 1260, 540);
            Thread.Sleep(1000);
            _logs.Log("获得3个insta", LogLevel.Info);
        }

        private void HandleTwoChestsScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 810, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 810, 540);
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 1110, 540); 
            Thread.Sleep(1000);
            InputSimulator.MouseMoveAndLeftClick(_context, 1110, 540);
            Thread.Sleep(1000);
            _logs.Log("获得2个insta", LogLevel.Info);
        }

        private void HandleInstaScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 540);
        }

        private void HandleChestsOpenedScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 1000);
        }
    }
}
