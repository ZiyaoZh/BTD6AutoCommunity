using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BTD6AutoCommunity.InputSimulator;
using static BTD6AutoCommunity.GameVisionRecognizer;
using static BTD6AutoCommunity.GameStateLocalization;
using static BTD6AutoCommunity.ScriptEditorSuite;
using static BTD6AutoCommunity.Constants;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Drawing;
using System.CodeDom.Compiler;
using OpenCvSharp.ML;

namespace BTD6AutoCommunity
{
    // 收集策略模式枚举
    public enum CollectionMode
    {
        SimpleCollection,    // 简单收集
        DoubleCashCollection, // 双金收集
        FastPathCollection   // 快速路径收集
    }

    public class CollectionStrategy
    {
        // 收集设置常量
        private const int CollectMapCount = 12;          // 每次运行收集地图数量
        private const int ExpertMapStartId = 90;        // 专家级地图起始ID
        private const int ExpertMapEndId = 101;           // 专家级地图终止ID

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

        private Dictionary<int, string> collectionScripts; // mapId -> 脚本路径
        private Dictionary<int, int> mapDifficulties; // mapId -> 难度
        private CollectionMode collectionMode;
        private int currentMapId;
        private bool IsHeroSelectionComplete;

        private int levelChallengingCount = 0;

        private ScriptEditorSuite ScriptEditorSuite;
        public event Action<ScriptEditorSuite> OnScriptLoaded;

        private LevelDataMonitor levelDataMonitor;
        private List<string> CurrentGameData; // 0: round, 1: cash, 2: life
        public event Action<List<string>> OnGameDataUpdated;

        private StrategyExecutor strategyExecutor;
        public event Action<ScriptInstructionInfo> OnCurrentStrategyCompleted;

        private System.Timers.Timer levelDataMonitorTimer;
        private System.Timers.Timer strategyExecutorTimer;
        private bool IsStrategyExecutionCompleted;

        private readonly LogHandler _logs;

        public bool ReadyToStart { get; private set; } = true;

        public CollectionStrategy(ScriptSettings settings, LogHandler logHandler)
        {
            _logs = logHandler;
            _context = new GameContext();
            if (_context.IsValid)
            {
                _logs.Log(_context.ToString(), LogLevel.Info);
                _settings = settings;

                stateMachine = new GameStateMachine(_context);

                InitializeHandlers();
                SetupGameStateTimer();
                currentMapId = 0;
            }
            else
            {
                ReadyToStart = false;
                _logs.Log("游戏窗口未找到，请确认游戏是否已启动", LogLevel.Error);
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
            MouseMoveAndLeftClick(_context, 960, 940);
        }

        private void HandleRaceResultsScreen()
        {
            MouseMoveAndLeftClick(_context, 960, 800);
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
            MouseMoveAndLeftClick(_context, 960, 880);
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
            MouseMoveAndLeftClick(_context, 80, 170);
            _logs.Log("已进入地图选择界面，开始选择收集额外地图", LogLevel.Info);
        }

        private void HandleChestCollection()
        {
            MouseMoveAndLeftClick(_context, 960, 680);
            _logs.Log("收集宝箱可打开，开始开箱", LogLevel.Info);
        }

        private void HandleLevelSearch()
        {
            MouseMoveAndLeftClick(_context, 1350, 45); // 收集
            //MouseMoveAndLeftClick(_context, 1275, 45); // 猴子小队
        }

        private void HandleLevelSearched()
        {
            currentMapId = RecognizeMapId(_context);
            //Debug.WriteLine("MapId: " + currentMapId);
            MouseMoveAndLeftClick(_context, 540, 650);
            _logs.Log($"已识别到地图：{GetTypeName((Maps)currentMapId)}", LogLevel.Info);
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
                    MouseMoveAndLeftClick(_context, 630, 400);
                    break;
                case 1:
                    MouseMoveAndLeftClick(_context, 970, 400);
                    break;
                case 2:
                    MouseMoveAndLeftClick(_context, 1300, 400);
                    break;
            }
            // 加载脚本
            ScriptEditorSuite = LoadScript(collectionScripts[currentMapId]);
            ScriptEditorSuite.Compile(_settings);
            OnScriptLoaded?.Invoke(ScriptEditorSuite);
            _logs.Log($"已加载脚本：{collectionScripts[currentMapId]}", LogLevel.Info);
        }

        private void HandleLevelEasyModeSelection()
        {
            if (ScriptEditorSuite == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法进入简单模式，返回", LogLevel.Error);
                return;
            }
            if (ScriptEditorSuite.SelectedMode != LevelMode.Standard &&
                LevelModeToDifficulty[ScriptEditorSuite.SelectedMode] != LevelDifficulties.Easy)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是简单模式，无法进入简单模式，返回", LogLevel.Error);
                return;
            }
            if (IsHeroSelectionComplete)
            {
                IsHeroSelectionComplete = false;

                Point point = GetLevelModePos(ScriptEditorSuite.SelectedMode);
                MouseMoveAndLeftClick(_context, point.X, point.Y);
            }
            else
            {
                MouseMoveAndLeftClick(_context, 100, 1000);
            }
            levelChallengingCount = 0;
        }

        private void HandleLevelMediumModeSelection()
        {
            if (ScriptEditorSuite == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法进入中级模式，返回", LogLevel.Error);
                return;
            }
            if (ScriptEditorSuite.SelectedMode != LevelMode.Standard &&
                LevelModeToDifficulty[ScriptEditorSuite.SelectedMode] != LevelDifficulties.Medium)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是中级模式，无法进入中级模式，返回", LogLevel.Error);
                return;
            }
            if (IsHeroSelectionComplete)
            {
                IsHeroSelectionComplete = false;
                Point point = GetLevelModePos(ScriptEditorSuite.SelectedMode);
                MouseMoveAndLeftClick(_context, point.X, point.Y);
            }
            else
            {
                MouseMoveAndLeftClick(_context, 100, 1000);
            }
            levelChallengingCount = 0;
        }

        private void HandleLevelHardModeSelection()
        {
            if (ScriptEditorSuite == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，无法进入困难模式，返回", LogLevel.Error);
                return;
            }
            if (ScriptEditorSuite.SelectedMode != LevelMode.Standard &&
                LevelModeToDifficulty[ScriptEditorSuite.SelectedMode] != LevelDifficulties.Hard)
            {
                HandleReturnableScreen();
                _logs.Log("当前模式不是困难模式，无法进入困难模式，返回", LogLevel.Error);
                return;
            }
            if (IsHeroSelectionComplete)
            {
                IsHeroSelectionComplete = false;
                Point point = GetLevelModePos(ScriptEditorSuite.SelectedMode);
                MouseMoveAndLeftClick(_context, point.X, point.Y);
            }
            else
            {
                MouseMoveAndLeftClick(_context, 100, 1000);
            }
            levelChallengingCount = 0;
        }

        private void HandleHeroSelection()
        {
            if (ScriptEditorSuite == null)
            {
                HandleReturnableScreen();
                _logs.Log("脚本未加载，返回", LogLevel.Error);
                return;
            }
            if (IsHeroSelectionComplete || ScriptEditorSuite == null)
            {
                HandleReturnableScreen();
                _logs.Log("英雄选择已完成，返回", LogLevel.Error);
                return;
            }
            Point heroPosition = GetHeroPosition(_context, ScriptEditorSuite.SelectedHero);
            
            for (int i = 0; i < 5 && heroPosition.X == -1; i++)
            {
                heroPosition = GetHeroPosition(_context, ScriptEditorSuite.SelectedHero);
                MouseWheel(-10);
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
            MouseMoveAndLeftClick(_context, heroPosition.X, heroPosition.Y);
            Thread.Sleep(500);
            MouseMoveAndLeftClick(_context, 1120, 620);
            Thread.Sleep(500);
            MouseMoveAndLeftClick(_context, 80, 55);
            IsHeroSelectionComplete = true;

            _logs.Log($"已选择英雄：{GetTypeName(ScriptEditorSuite.SelectedHero)}", LogLevel.Info);
        }

        private void HandleLevelTipScreen()
        {
            MouseMoveAndLeftClick(_context, 1140, 730);
        }

        private void HandleLevelChallengingScreen()
        {
            levelChallengingCount++;
            if (ScriptEditorSuite == null)
            {
                MouseMoveAndLeftClick(_context, 1600, 40);
                Thread.Sleep(500);
                MouseMoveAndLeftClick(_context, 850, 850);
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
                strategyExecutor = new StrategyExecutor( _context, ScriptEditorSuite);
                SetupStrategyExecutorTimer();
                strategyExecutorTimer.Start();
                _logs.Log("开始执行关卡策略...", LogLevel.Info);
            }
        }

        private void HandleLevelChallengingWithTipScreen()
        {
            MouseMoveAndLeftClick(_context, 960, 760);
        }

        private void HandleLevelPassScreen()
        {
            if (strategyExecutor != null && strategyExecutor.IsStartFreePlay)
            {
                MouseMoveAndLeftClick(_context, 1200, 850);
                strategyExecutor.StartFreePlayFinished = true;
                _logs.Log("自由游戏已开启，开始下一关", LogLevel.Info);
                return;
            }
            MouseMoveAndLeftClick(_context, 720, 850);
        }

        private void HandleLevelSettlementScreen()
        {
            MouseMoveAndLeftClick(_context, 960, 910);
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
            MouseMoveAndLeftClick(_context, 960, 980);
            _logs.Log("检测到升级界面，点击确认", LogLevel.Info);
        }

        private void HandleLevelFailedScreen()
        {
            StopLevelTimer();
            Point returnPos = GetFailedScreenReturnPosition(_context);
            MouseMoveAndLeftClick(_context, returnPos.X, returnPos.Y);
            _logs.Log("检测到关卡失败界面，回到主页", LogLevel.Info);
        }

        private void HandleReturnableScreen()
        {
            MouseMoveAndLeftClick(_context, 80, 55);
        }

        private void HandleThreeChestsScreen()
        {
            MouseMoveAndLeftClick(_context, 660, 540);
            Thread.Sleep(1000);
            MouseMoveAndLeftClick(_context, 660, 540);
            Thread.Sleep(1000);
            MouseMoveAndLeftClick(_context, 960, 540);
            Thread.Sleep(1000);
            MouseMoveAndLeftClick(_context, 960, 540);
            Thread.Sleep(1000);
            MouseMoveAndLeftClick(_context, 1260, 540);
            Thread.Sleep(1000);
            MouseMoveAndLeftClick(_context, 1260, 540);
            Thread.Sleep(1000);
            _logs.Log("获得3个insta", LogLevel.Info);
        }

        private void HandleTwoChestsScreen()
        {
            MouseMoveAndLeftClick(_context, 810, 540);
            Thread.Sleep(1000);
            MouseMoveAndLeftClick(_context, 810, 540);
            Thread.Sleep(1000);
            MouseMoveAndLeftClick(_context, 1110, 540); 
            Thread.Sleep(1000);
            MouseMoveAndLeftClick(_context, 1110, 540);
            Thread.Sleep(1000);
            _logs.Log("获得2个insta", LogLevel.Info);
        }

        private void HandleInstaScreen()
        {
            MouseMoveAndLeftClick(_context, 960, 540);
        }

        private void HandleChestsOpenedScreen()
        {
            MouseMoveAndLeftClick(_context, 960, 1000);
        }


        public void Start()
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
            _logs.Log($"当前收集模式：{CollectionScripts[collectionMode]}", LogLevel.Info);

            for (int mapId = ExpertMapStartId; mapId <= ExpertMapEndId; mapId++)
            {
                foreach (int dif in new int[] { 0, 1, 2 })
                {
                    string scriptPath = ExistScript(
                        GetTypeName((Maps)mapId), 
                        GetTypeName((LevelDifficulties)dif), 
                        CollectionScripts[collectionMode]
                        );
                    if (scriptPath!= null)
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
            _logs.Log("收集模式已停止...", LogLevel.Info);
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
                    _logs.Log($"当前状态：{GetChineseDescription(currentState)}", LogLevel.Info);
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
                return;
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
            levelDataMonitorTimer = new System.Timers.Timer(_settings.DataReadInterval);
            levelDataMonitorTimer.Elapsed += (s, e) => ReadGameData();
            levelDataMonitorTimer.AutoReset = true;
        }

        private void SetupStrategyExecutorTimer()
        {
            strategyExecutorTimer = new System.Timers.Timer(_settings.OperationInterval);
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
