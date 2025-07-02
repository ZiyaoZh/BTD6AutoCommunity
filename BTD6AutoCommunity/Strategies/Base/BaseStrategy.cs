using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace BTD6AutoCommunity.Strategies.Base
{
    public abstract class BaseStrategy
    {
        // 核心依赖
        protected readonly GameContext _context;
        protected readonly ScriptSettings _settings;
        protected readonly LogHandler _logs;
        protected readonly GameStateMachine stateMachine;

        // 状态处理
        protected Dictionary<GameState, Action> stateHandlers;
        protected GameState lastState = GameState.UnKnown;

        // 控制
        protected readonly object _checkStateTimerLock = new object();
        protected volatile bool _isProcessing = false;
        protected System.Timers.Timer checkGameStateTimer;
        public bool ReadyToStart { get; protected set; } = true;

        // 事件
        public event Action OnStopTriggered;
        public event Action<ScriptEditorSuite> OnScriptLoaded;
        public event Action<List<string>> OnGameDataUpdated;
        public event Action<ScriptInstructionInfo> OnCurrentStrategyCompleted;

        // 脚本系统
        protected ScriptEditorSuite ScriptEditorSuite;

        // 执行引擎
        protected InGame.InGameActionExecutor InGameActionExecutor;
        protected bool IsStrategyExecutionCompleted;

        // 数据监控
        protected LevelDataMonitor LevelDataMonitor;
        protected List<string> CurrentGameData;

        protected System.Timers.Timer LevelDataMonitorTimer;
        protected System.Timers.Timer InGameActionExecutorTimer;

        protected const int DefaulCheckStateInterval = 1500;
        protected int DefaultDataReadInterval = 1000;
        protected int DefaultOperationInterval = 200;


        protected BaseStrategy(ScriptSettings settings, LogHandler logHandler)
        {
            _logs = logHandler;
            _context = new GameContext();
            _settings = settings;
            if (!_context.IsValid)
            {
                ReadyToStart = false;
                _logs.Log("游戏窗口未找到，请确认游戏是否已启动", LogLevel.Error);
                return;
            }

            _logs.Log(_context.ToString(), LogLevel.Info);
            stateMachine = new GameStateMachine(_context);

            //InitializeStateHandlers();
            SetupGameStateTimer();
        }

        protected void LoadStrategyScript(UserSelection userSelection)
        {
            string scriptPath = ScriptEditorSuite.ExistScript(
                    Constants.GetTypeName((Maps)userSelection.selectedMap),
                    Constants.GetTypeName((LevelDifficulties)userSelection.selectedDifficulty),
                    userSelection.selectedScript
                );
            if (scriptPath == null)
            {
                ReadyToStart = false;
                _logs.Log("脚本未选择，请选择脚本", LogLevel.Error);
                return;
            }
            try
            {
                ScriptEditorSuite = ScriptEditorSuite.LoadScript(scriptPath);
                ScriptEditorSuite.Compile(_settings);
                // OnScriptLoaded?.Invoke(ScriptEditorSuite);

                _logs.Log($"已加载脚本：{scriptPath}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                
                ReadyToStart = false;
                _logs.Log("脚本加载失败，请确认脚本是否正确\n错误信息: " + ex.Message, LogLevel.Error);
                return;
            }
        }

        protected void LoadStrategyScript(string scriptPath)
        {
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
                OnScriptLoaded?.Invoke(ScriptEditorSuite);

                _logs.Log($"已加载脚本：{scriptPath}", LogLevel.Info);
            }
            catch
            {
                ReadyToStart = false;
                _logs.Log("脚本加载失败，请确认脚本是否正确", LogLevel.Error);
                return;
            }
        }

        public virtual void Start()
        {
            // 1. 子类可预处理（如设置状态、构造字典等）
            OnPreStart();

            // 2. 启动状态检测
            checkGameStateTimer?.Start();

            // 3. 子类可添加额外行为，如开启数据监控、启动执行器等
            OnPostStart();
        }

        protected virtual void OnPreStart() { }

        protected virtual void OnPostStart() { }

        public virtual void Stop()
        {
            checkGameStateTimer?.Stop();
            StopLevelTimer();
            lock (_checkStateTimerLock)
            {
                _isProcessing = false;
            }
            OnStopTriggered?.Invoke();
            OnPostStop();
        }

        protected virtual void OnPostStop()
        {
            _logs.Log("策略已停止!", LogLevel.Info);
        }

        protected void SetupGameStateTimer()
        {
            checkGameStateTimer = new System.Timers.Timer(DefaulCheckStateInterval);
            checkGameStateTimer.Elapsed += (s, e) => CheckGameState();
            checkGameStateTimer.AutoReset = true;
        }

        protected void SetupLevelDataMonitorTimer(bool useRecommendInterval = false)
        {
            int interval = DefaultDataReadInterval;
            if (useRecommendInterval)
            {
                interval = _settings.DataReadInterval;
                _logs.Log($"使用推荐数据读取间隔：{interval}ms", LogLevel.Info);
            }
            else
            {
                _logs.Log($"使用自定义数据读取间隔：{interval}ms", LogLevel.Info);
            }
            LevelDataMonitorTimer = new System.Timers.Timer(interval);
            LevelDataMonitorTimer.Elapsed += (s, e) => ReadGameData();
            LevelDataMonitorTimer.AutoReset = true;
        }

        protected void SetupStrategyExecutorTimer(bool useRecommendInterval = false)
        {
            int interval = DefaultOperationInterval;
            if (useRecommendInterval)
            {
                interval = _settings.OperationInterval;
                _logs.Log($"使用推荐操作间隔：{interval}ms", LogLevel.Info);
            }
            else
            {
                _logs.Log($"使用自定义操作间隔：{interval}ms", LogLevel.Info);
            }
            InGameActionExecutorTimer = new System.Timers.Timer(interval);
            InGameActionExecutorTimer.Elapsed += (s, e) => ExecuteInGameAction();
            InGameActionExecutorTimer.AutoReset = true;
        }

        protected void StartLevelTimer(int startIndex = 0, bool useRecommendInterval = false)
        {
            if (LevelDataMonitorTimer == null)
            {
                CurrentGameData = new List<string>() { "0", "0", "0" };
                LevelDataMonitor = new LevelDataMonitor(_context);
                SetupLevelDataMonitorTimer(useRecommendInterval);
                LevelDataMonitorTimer.Start();
                _logs.Log("已开启关卡数据识别", LogLevel.Info);
            }
            if (InGameActionExecutorTimer == null)
            {
                IsStrategyExecutionCompleted = false;
                InGameActionExecutor = new InGame.InGameActionExecutor(_context, ScriptEditorSuite);
                SetupStrategyExecutorTimer(useRecommendInterval);
                InGameActionExecutor.currentFirstIndex = startIndex;
                InGameActionExecutorTimer.Start();
                _logs.Log("开始执行关卡策略...", LogLevel.Info);
            }
        }

        protected void StopLevelTimer()
        {
            LevelDataMonitorTimer?.Stop();
            InGameActionExecutorTimer?.Stop();
            LevelDataMonitorTimer?.Dispose();
            InGameActionExecutorTimer?.Dispose();
            LevelDataMonitorTimer = null;
            InGameActionExecutorTimer = null;
        }

        protected void CheckGameState()
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

        protected void ReadGameData()
        {
            CurrentGameData = LevelDataMonitor.GetCurrentGameData();
            OnGameDataUpdated?.Invoke(CurrentGameData);
        }

        protected void ExecuteInGameAction()
        {
            if (InGameActionExecutor.Finished)
            {
                OnGameActionFinished();
            }

            int round = int.TryParse(CurrentGameData[0], out var rt) ? rt : 0;
            int cash = int.TryParse(CurrentGameData[1], out var ct) ? ct : 0;
            InGameActionExecutor.Tick(round, cash);
            OnCurrentStrategyCompleted?.Invoke(InGameActionExecutor.instructionInfo);
        }

        protected virtual void OnGameActionFinished()
        {
            if (IsStrategyExecutionCompleted == false)
            {
                IsStrategyExecutionCompleted = true;
                _logs.Log("策略执行完毕!", LogLevel.Info);
            }
        }

        // 子类必须定义自己的状态处理器表
        protected abstract void InitializeStateHandlers();
    }
}

