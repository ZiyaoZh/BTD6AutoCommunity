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
using System.Runtime.CompilerServices;
using BTD6AutoCommunity.GameObjects;

namespace BTD6AutoCommunity.Strategies
{
    public class CirculationStrategy : Base.BaseStrategy
    {
        private readonly Maps currentMap;
        private bool IsMapSelectionComplete;
        private bool IsHeroSelectionComplete;

        private int levelChallengingCount = 0;
        private int returnableScreenCount = 0;

        public CirculationStrategy(ScriptSettings settings, LogHandler logHandler, UserSelection userSelection)
            : base(settings, logHandler)
        {
            DefaultDataReadInterval = 1000;
            DefaultOperationInterval = 200;
            currentMap = userSelection.selectedMap;
            InitializeStateHandlers();
            GetExecutableInstructions(userSelection);
        }

        protected override void OnPreStart()
        {
            _logs.Log("开始循环刷关...", LogLevel.Info);
        }

        protected override void OnPostStop()
        {
            _logs.Log("循环刷关已停止!", LogLevel.Info);
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
                { GameState.LevelSettingScreen, HandleLevelSettingScreen },
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
            MapTypes mapType = Constants.GetMapType(currentMap);
            Point mapTypePos = Constants.GetMapTypePos(mapType);
            Point mapPos = GameVisionRecognizer.GetMapPos(_context, (int)currentMap);
            int reTryCount = 0;
            while (mapPos.X == -1)
            {
                if (reTryCount > 8)
                {
                    _logs.Log("未找到地图位置，请确认地图是否已解锁", LogLevel.Error);
                    Stop();
                    return;
                }
                reTryCount++;
                InputSimulator.MouseMoveAndLeftClick(_context, mapTypePos.X, mapTypePos.Y);
                Thread.Sleep(500);
                mapPos = GameVisionRecognizer.GetMapPos(_context, (int)currentMap);
            }
            InputSimulator.MouseMoveAndLeftClick(_context, mapPos.X, mapPos.Y);
            IsMapSelectionComplete = true;
            _logs.Log($"已选择地图：{Constants.GetTypeName(currentMap)}", LogLevel.Info);
        }

        private void HandleChestCollection()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 680);
            _logs.Log("收集宝箱可打开，开始开箱", LogLevel.Info);
        }

        private void HandleLevelSearch()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 80, 170);
        }

        private void HandleLevelSearched()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 80, 170);
        }

        private void HandleLevelDifficultySelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法选择难度，终止刷循环", LogLevel.Error);
                Stop();
                return;
            }
            if (!IsMapSelectionComplete)
            {
                HandleReturnableScreen();
                _logs.Log("地图选择未完成，无法选择难度，返回", LogLevel.Warning);
                return;
            }
            switch (scriptMetadata.SelectedDifficulty)
            {
                case LevelDifficulties.Easy:
                    InputSimulator.MouseMoveAndLeftClick(_context, 630, 400);
                    break;
                case LevelDifficulties.Medium:
                    InputSimulator.MouseMoveAndLeftClick(_context, 970, 400);
                    break;
                case LevelDifficulties.Hard:
                    InputSimulator.MouseMoveAndLeftClick(_context, 1300, 400);
                    break;
            }
        }

        private void HandleLevelEasyModeSelection()
        {
            if (executableInstructions == null || scriptMetadata == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法进入简单模式，终止刷循环", LogLevel.Error);
                Stop();
                return;
            }
            if (!IsMapSelectionComplete)
            {
                HandleReturnableScreen();
                _logs.Log("地图选择未完成，无法进入简单模式，返回", LogLevel.Warning);
                return;
            }
            if (scriptMetadata.SelectedMode != LevelMode.Standard && 
                Constants.LevelModeToDifficulty[scriptMetadata.SelectedMode] != LevelDifficulties.Easy)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是简单模式，无法进入简单模式，返回", LogLevel.Warning);
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
                _logs.Log("脚本未加载，无法进入中级模式，终止刷循环", LogLevel.Error);
                Stop();
                return;
            }
            if (!IsMapSelectionComplete)
            {
                HandleReturnableScreen();
                _logs.Log("地图选择未完成，无法进入中级模式，返回", LogLevel.Warning);
                return;
            }
            if (scriptMetadata.SelectedMode != LevelMode.Standard &&
                Constants.LevelModeToDifficulty[scriptMetadata.SelectedMode] != LevelDifficulties.Medium)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是中级模式，无法进入中级模式，返回", LogLevel.Warning);
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
                _logs.Log("脚本未加载，无法进入困难模式，终止刷循环", LogLevel.Error);
                Stop();
                return;
            }
            if (!IsMapSelectionComplete)
            {
                HandleReturnableScreen();
                _logs.Log("地图选择未完成，无法进入困难模式，返回", LogLevel.Warning);
                return;
            }
            if (scriptMetadata.SelectedMode != LevelMode.Standard &&
                Constants.LevelModeToDifficulty[scriptMetadata.SelectedMode] != LevelDifficulties.Hard)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是困难模式，无法进入困难模式，返回", LogLevel.Warning);
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
                _logs.Log("脚本未加载，无法选择英雄，终止刷循环", LogLevel.Error);
                Stop();
                return;
            }
            if (IsHeroSelectionComplete || scriptMetadata == null)
            {
                HandleReturnableScreen();
                _logs.Log("英雄选择已完成，返回", LogLevel.Warning);
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
            if (scriptMetadata == null)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 1600, 40);
                Thread.Sleep(500);
                InputSimulator.MouseMoveAndLeftClick(_context, 850, 850);
                _logs.Log("脚本未加载，无法进入战斗，终止刷循环", LogLevel.Error);
                Stop();
                return;
            }
            if (!IsMapSelectionComplete)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 1600, 40);
                Thread.Sleep(500);
                InputSimulator.MouseMoveAndLeftClick(_context, 850, 850);
                _logs.Log("地图选择未完成，无法进入战斗，返回", LogLevel.Warning);
                return;
            }
            if (levelChallengingCount < 2)
            {
                return;
            }
            if (IsStrategyExecutionCompleted)
            {
                StopLevelTimer();
                return;
            }
            StartLevelTimer(0, _settings.EnableRecommendInterval);
        }

        private void HandleLevelChallengingWithTipScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 760);
        }

        private void HandleLevelSettingScreen()
        {
            // 点重新开始
            InputSimulator.MouseMoveAndLeftClick(_context, 1080, 840);
            Thread.Sleep(500);
            InputSimulator.MouseMoveAndLeftClick(_context, 1135, 730);
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
