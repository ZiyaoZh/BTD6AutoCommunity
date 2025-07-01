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
    public class CirculationStrategy
    {
        private readonly GameContext _context;
        private readonly ScriptSettings _settings;
        public GameContext Context => _context;
        private readonly GameStateMachine stateMachine;

        private readonly object _checkStateTimerLock = new object();
        private volatile bool _isProcessing = false;
        private System.Timers.Timer checkGameStateTimer;
        public event Action OnStopTriggered;

        private Dictionary<GameState, Action> stateHandlers;
        private GameState lastState = GameState.UnKnown;

        private Maps currentMap;
        private bool IsMapSelectionComplete;
        private bool IsHeroSelectionComplete;

        private ScriptEditorSuite ScriptEditorSuite;
        public event Action<List<string>> OnScriptLoaded;

        private int levelChallengingCount = 0;

        private int returnableScreenCount = 0;


        private LevelDataMonitor levelDataMonitor;
        private List<string> CurrentGameData; // 0: round, 1: cash, 2: life
        public event Action<List<string>> OnGameDataUpdated;

        private InGame.InGameActionExecutor strategyExecutor;
        public event Action<ScriptInstructionInfo> OnCurrentStrategyCompleted;

        private System.Timers.Timer levelDataMonitorTimer;
        private const int RecommendDataReadInterval = 1000; 
        private System.Timers.Timer strategyExecutorTimer;
        private const int RecommendOperationInterval = 200;

        private bool IsStrategyExecutionCompleted;

        private readonly LogHandler _logs;

        public bool ReadyToStart { get; private set; } = true;

        public CirculationStrategy(ScriptSettings settings, LogHandler logHandler, UserSelection userSelection)
        {
            _logs = logHandler;
            _context = new GameContext();
            if (_context.IsValid)
            {
                _logs.Log(_context.ToString(), LogLevel.Info);
                _settings = settings;

                stateMachine = new GameStateMachine(_context);

                InitializeHandlers();
                LoadStrategyScript(userSelection);
                SetupGameStateTimer();
            }
            else
            {
                ReadyToStart = false;
                _logs.Log("游戏窗口未找到，请确认游戏是否已启动", LogLevel.Error);
            }
        }

        private void LoadStrategyScript(UserSelection userSelection)
        {
            string scriptPath = ScriptEditorSuite.ExistScript(
                    Constants.GetTypeName((Maps)userSelection.selectedMap),
                    Constants.GetTypeName((LevelDifficulties)userSelection.selectedDifficulty),
                    userSelection.selectedScript
                );
            //Debug.WriteLine($"map: {GetTypeName((Maps)userSelection.selectedMap)} diff: {GetTypeName((LevelDifficulties)userSelection.selectedDifficulty)} script: {userSelection.selectedScript}");
            if (scriptPath == null)
            {
                ReadyToStart = false;
                _logs.Log("脚本未选择，请确选择脚本", LogLevel.Error);
                return;
            }
            try
            {
                ScriptEditorSuite = ScriptEditorSuite.LoadScript(scriptPath);
                ScriptEditorSuite.Compile(_settings);
                OnScriptLoaded?.Invoke(ScriptEditorSuite.Displayinstructions);

                currentMap = (Maps)ScriptEditorSuite.SelectedMap;
                _logs.Log($"已加载脚本：{scriptPath}", LogLevel.Info);
            }
            catch
            {
                ReadyToStart = false;
                _logs.Log("脚本加载失败，请确认脚本是否正确", LogLevel.Error);
                return;
            }
        }

        private void InitializeHandlers()
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
            if (ScriptEditorSuite == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法选择难度，终止刷循环", LogLevel.Error);
                Stop();
                return;
            }
            if (!IsMapSelectionComplete)
            {
                HandleReturnableScreen();
                _logs.Log("地图选择未完成，无法选择难度，返回", LogLevel.Error);
                return;
            }
            switch (ScriptEditorSuite.SelectedDifficulty)
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
            if (ScriptEditorSuite == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法进入简单模式，终止刷循环", LogLevel.Error);
                Stop();
                return;
            }
            if (!IsMapSelectionComplete)
            {
                HandleReturnableScreen();
                _logs.Log("地图选择未完成，无法进入简单模式，返回", LogLevel.Error);
                return;
            }
            if (ScriptEditorSuite.SelectedMode != LevelMode.Standard && 
                Constants.LevelModeToDifficulty[ScriptEditorSuite.SelectedMode] != LevelDifficulties.Easy)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是简单模式，无法进入简单模式，返回", LogLevel.Error);
                return;
            }
            if (IsHeroSelectionComplete)
            {
                IsHeroSelectionComplete = false;

                Point point = Constants.GetLevelModePos(ScriptEditorSuite.SelectedMode);
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
            if (ScriptEditorSuite == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法进入中级模式，终止刷循环", LogLevel.Error);
                Stop();
                return;
            }
            if (!IsMapSelectionComplete)
            {
                HandleReturnableScreen();
                _logs.Log("地图选择未完成，无法进入中级模式，返回", LogLevel.Error);
                return;
            }
            if (ScriptEditorSuite.SelectedMode != LevelMode.Standard &&
                Constants.LevelModeToDifficulty[ScriptEditorSuite.SelectedMode] != LevelDifficulties.Medium)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是中级模式，无法进入中级模式，返回", LogLevel.Error);
                return;
            }
            if (IsHeroSelectionComplete)
            {
                IsHeroSelectionComplete = false;
                Point point = Constants.GetLevelModePos(ScriptEditorSuite.SelectedMode);
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
            if (ScriptEditorSuite == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法进入困难模式，终止刷循环", LogLevel.Error);
                Stop();
                return;
            }
            if (!IsMapSelectionComplete)
            {
                HandleReturnableScreen();
                _logs.Log("地图选择未完成，无法进入困难模式，返回", LogLevel.Error);
                return;
            }
            if (ScriptEditorSuite.SelectedMode != LevelMode.Standard &&
                Constants.LevelModeToDifficulty[ScriptEditorSuite.SelectedMode] != LevelDifficulties.Hard)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是困难模式，无法进入困难模式，返回", LogLevel.Error);
                return;
            }
            if (IsHeroSelectionComplete)
            {
                IsHeroSelectionComplete = false;
                Point point = Constants.GetLevelModePos(ScriptEditorSuite.SelectedMode);
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
            if (ScriptEditorSuite == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法选择英雄，终止刷循环", LogLevel.Error);
                Stop();
                return;
            }
            if (IsHeroSelectionComplete || ScriptEditorSuite == null)
            {
                HandleReturnableScreen();
                _logs.Log("英雄选择已完成，返回", LogLevel.Error);
                return;
            }
            Point heroPosition = GameVisionRecognizer.GetHeroPosition(_context, ScriptEditorSuite.SelectedHero);

            for (int i = 0; i < 5 && heroPosition.X == -1; i++)
            {
                heroPosition = GameVisionRecognizer.GetHeroPosition(_context, ScriptEditorSuite.SelectedHero);
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

            _logs.Log($"已选择英雄：{Constants.GetTypeName(ScriptEditorSuite.SelectedHero)}", LogLevel.Info);
        }

        private void HandleLevelTipScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 1140, 730);
        }

        private void HandleLevelChallengingScreen()
        {
            levelChallengingCount++;
            if (ScriptEditorSuite == null)
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
                _logs.Log("地图选择未完成，无法进入战斗，返回", LogLevel.Error);
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
            if (levelDataMonitorTimer == null)
            {
                CurrentGameData = new List<string>() { "0", "0", "0" };
                levelDataMonitor = new LevelDataMonitor(_context);
                SetupLevelDataMonitorTimer();
                levelDataMonitorTimer.Start();
                _logs.Log("已开启关卡数据识别", LogLevel.Info);
            }
            if (strategyExecutorTimer == null)
            {
                IsStrategyExecutionCompleted = false;
                strategyExecutor = new InGame.InGameActionExecutor(_context, ScriptEditorSuite);
                SetupStrategyExecutorTimer();
                strategyExecutorTimer.Start();
                _logs.Log("开始执行关卡策略...", LogLevel.Info);
            }
        }

        private void HandleLevelChallengingWithTipScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 760);
        }

        private void HandleLevelSettingScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 1080, 840);
            Thread.Sleep(500);
            InputSimulator.MouseMoveAndLeftClick(_context, 1135, 730);
        }

        private void HandleLevelPassScreen()
        {
            if (strategyExecutor != null && strategyExecutor.IsStartFreePlay)
            {
                InputSimulator.MouseMoveAndLeftClick(_context, 1200, 850);
                strategyExecutor.StartFreePlayFinished = true;
                _logs.Log("自由游戏已开启，开始下一关", LogLevel.Info);
                return;
            }
            InputSimulator.MouseMoveAndLeftClick(_context, 720, 850);
        }

        private void HandleLevelSettlementScreen()
        {
            InputSimulator.MouseMoveAndLeftClick(_context, 960, 910);
            if (strategyExecutor != null && strategyExecutor.IsStartFreePlay) return;
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

        public void Start()
        {
            _logs.Log($"开始循环刷关...", LogLevel.Info);
            checkGameStateTimer.Start();
        }

        public void Stop()
        {
            checkGameStateTimer?.Stop();
            StopLevelTimer();
            lock (_checkStateTimerLock)
            {
                _isProcessing = false;
            }
            OnStopTriggered?.Invoke();
            _logs.Log("循环刷关模式已停止...", LogLevel.Info);
        }

        private void CheckGameState()
        {
            if (_isProcessing) return;

            lock (_checkStateTimerLock)
            {
                if (_isProcessing) return;
                _isProcessing = true;
            }
            try
            {
                var currentState = stateMachine.GetCurrentState();
                //Debug.WriteLine("Current State: " + GetChineseDescription(currentState));
                if (currentState != lastState)
                {
                    lastState = currentState;
                    _logs.Log($"当前状态：{GameStateDescription.GetChineseDescription(currentState)}", LogLevel.Info);
                }

                if (stateHandlers.TryGetValue(currentState, out Action handler))
                {
                    handler.Invoke();
                }
            }
            finally
            {
                lock (_checkStateTimerLock)
                {
                    _isProcessing = false;
                }
            }
        }

        private void ExecuteStrategy()
        {
            int currentRound = Int32.TryParse(CurrentGameData[0], out int rt) ? rt : 0;
            int currentCash = Int32.TryParse(CurrentGameData[1], out int ct) ? ct : 0;

            if (strategyExecutor.Finished)
            {
                if (IsStrategyExecutionCompleted == false)
                {
                    IsStrategyExecutionCompleted = true;
                    _logs.Log("策略执行完毕!", LogLevel.Info);
                }
            }

            strategyExecutor.Tick(currentRound, currentCash);
            //Debug.WriteLine(strategyExecutor.instructionInfo.ToString());
            OnCurrentStrategyCompleted?.Invoke(strategyExecutor.instructionInfo);
        }

        private void ReadGameData()
        {
            CurrentGameData = levelDataMonitor.GetCurrentGameData();
            OnGameDataUpdated?.Invoke(CurrentGameData);
        }


        private void SetupGameStateTimer()
        {
            checkGameStateTimer = new System.Timers.Timer(1500); // 1.5秒间隔
            checkGameStateTimer.Elapsed += (s, e) => CheckGameState();
            checkGameStateTimer.AutoReset = true;
        }

        private void SetupLevelDataMonitorTimer()
        {
            int interval = _settings.DataReadInterval;
            if (_settings.EnableRecommendInterval)
            {
                interval = RecommendDataReadInterval;
                _logs.Log($"使用推荐数据读取间隔：{interval}ms", LogLevel.Info);
            }
            else
            {
                _logs.Log($"使用自定义数据读取间隔：{interval}ms", LogLevel.Info);
            }
            levelDataMonitorTimer = new System.Timers.Timer(interval);
            levelDataMonitorTimer.Elapsed += (s, e) => ReadGameData();
            levelDataMonitorTimer.AutoReset = true;
        }

        private void SetupStrategyExecutorTimer()
        {
            int interval = _settings.OperationInterval;
            if (_settings.EnableRecommendInterval)
            {
                interval = RecommendOperationInterval;
                _logs.Log($"使用推荐操作间隔：{interval}ms", LogLevel.Info);
            }
            else
            {
                _logs.Log($"使用自定义操作间隔：{interval}ms", LogLevel.Info);
            }
            strategyExecutorTimer = new System.Timers.Timer(interval);
            strategyExecutorTimer.Elapsed += (s, e) => ExecuteStrategy();
            strategyExecutorTimer.AutoReset = true;
        }

        private void StopLevelTimer()
        {
            if (levelDataMonitorTimer != null)
            {
                levelDataMonitorTimer.Stop();
                levelDataMonitorTimer.Dispose();
                levelDataMonitorTimer = null;
            }
            if (strategyExecutorTimer != null)
            {
                strategyExecutorTimer.Stop();
                strategyExecutorTimer.Dispose();
                strategyExecutorTimer = null;
            }
        }
    }
}
