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

namespace BTD6AutoCommunity.Strategies
{
    public class RaceStrategy : Base.BaseStrategy
    {
        private int levelChallengingCount = 0;


        public RaceStrategy(ScriptSettings settings, LogHandler logHandler, UserSelection userSelection)
            : base(settings, logHandler)
        {
            InitializeStateHandlers();
            GetExecutableInstructions(userSelection);
        }

        protected override void OnPreStart()
        {
            _logs.Log("开始凹竞速...", LogLevel.Info);
        }

        protected override void OnPostStop()
        {
            _logs.Log("凹竞速模式已停止!", LogLevel.Info);
        }

        protected override void InitializeStateHandlers()
        {
            stateHandlers = new Dictionary<GameState, Action>
            {
                { GameState.GameMainScreen, HandleMainScreen },
                { GameState.RaceResultsScreen, HandleRaceResultsScreen },
                { GameState.BossResultsScreen, HandleBossResultsScreen },
                { GameState.EventsScreen, HandleEventsScreen },
                { GameState.EventInfoScreen, HandleEventInfoScreen },
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
                { GameState.ChestsOpenedScreen, HandleChestsOpenedScreen },
                { GameState.UnKnown, HandleReturnableScreen }

                // 添加更多状态处理...
            };
        }

        private void HandleMainScreen()
        {
            HandleIrrelevantScreen();
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

        private void HandleEventsScreen()
        {
            HandleIrrelevantScreen();
        }

        private void HandleEventInfoScreen()
        {
            HandleIrrelevantScreen();
        }

        private void HandleLevelSelection()
        {
            HandleIrrelevantScreen();
        }

        private void HandleChestCollection()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 680);
            _logs.Log("收集宝箱可打开，开始开箱", LogLevel.Info);
        }

        private void HandleLevelSearch()
        {
            HandleIrrelevantScreen();
        }

        private void HandleLevelSearched()
        {
            HandleIrrelevantScreen();
        }

        private void HandleLevelDifficultySelection()
        {
            HandleIrrelevantScreen();
        }

        private void HandleLevelEasyModeSelection()
        {
            HandleIrrelevantScreen();
        }

        private void HandleLevelMediumModeSelection()
        {
            HandleIrrelevantScreen();
        }

        private void HandleLevelHardModeSelection()
        {
            HandleIrrelevantScreen();
        }

        private void HandleHeroSelection()
        {
            HandleIrrelevantScreen();
        }

        private void HandleLevelTipScreen()
        {
            HandleIrrelevantScreen();
        }

        private void HandleLevelChallengingScreen()
        {
            levelChallengingCount++;
            if (executableInstructions == null || scriptMetadata == null)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 1600, 40);
                Thread.Sleep(500);
                InputSimulator.MouseMoveAndLeftClick(_context, 850, 850);
                _logs.Log("脚本未加载，无法进入战斗，终止凹竞速", LogLevel.Error);
                Stop();
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
            StartLevelTimer();
        }

        private void HandleLevelChallengingWithTipScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 760);
        }

        private void HandleLevelPassScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 720, 850);
        }

        private void HandleLevelSettlementScreen()
        {
            StopLevelTimer();
            InputSimulator.MouseMoveAndLeftClick(_context, 1340, 860);
            Thread.Sleep(300);
            InputSimulator.MouseMoveAndLeftClick(_context, 1140, 730);
            if (IsStrategyExecutionCompleted)
            {
                IsStrategyExecutionCompleted = false;
            }
            _logs.Log($"进入关卡结算界面，挑战成功, 本关用时：{(int)(levelChallengingCount * 1.5 / 60)}分{(int)(levelChallengingCount * 1.5 % 60)}秒", LogLevel.Info);
        }

        private void HandleLevelUpgradingScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 980);
            _logs.Log("检测到升级界面，点击确认", LogLevel.Info);
        }

        private void HandleLevelFailedScreen()
        {
            StopLevelTimer();
            Point restartPos = GameVisionRecognizer.GetRestartPos(_context);
            InputSimulator.MouseMoveAndLeftClick(_context, restartPos.X, restartPos.Y);
            Thread.Sleep(300);
            InputSimulator.MouseMoveAndLeftClick(_context, 1140, 730);

            _logs.Log("检测到关卡失败界面，重新开始", LogLevel.Info);
        }

        private void HandleReturnableScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 80, 55);
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

        private void HandleIrrelevantScreen() // 无关的界面
        {
            _logs.Log("检测到无关的界面，请进入竞速关卡再启动", LogLevel.Warning);
            Stop();
        }
    }
}
