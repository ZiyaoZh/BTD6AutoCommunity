using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using BTD6AutoCommunity.Models;
using BTD6AutoCommunity.ScriptEngine;
using BTD6AutoCommunity.ScriptEngine.InstructionSystem;
using BTD6AutoCommunity.Services;
using BTD6AutoCommunity.Services.Interfaces;
using BTD6AutoCommunity.Strategies;
using BTD6AutoCommunity.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace BTD6AutoCommunity.ViewModels
{
    public class StartViewModel : INotifyPropertyChanged
    {
        private readonly IScriptService scriptService;
        private readonly IMessageBoxService messageBoxService;

        public StartViewModel(IScriptService scriptService, IMessageBoxService messageBoxService)
        {
            this.scriptService = scriptService;
            this.messageBoxService = messageBoxService;

            // 加载脚本设置
            scriptSettings = ScriptSettings.LoadJsonSettings();
            logHandler = new LogHandler() { EnableInfoLog = scriptSettings.EnableLogging };

            FunctionOptions = new ObservableCollection<FunctionDisplayItem>();
            foreach (FunctionTypes type in Constants.FunctionsList)
            {
                FunctionOptions.Add(new FunctionDisplayItem { Value = type });
            }
            // 默认选中第一个
            SelectedFunction = FunctionOptions[0];

            DifficultyOptions = new ObservableCollection<DifficultyDisplayItem>();
            foreach (LevelDifficulties difficulty in Constants.DifficultiesList)
            {
                DifficultyOptions.Add(new DifficultyDisplayItem { Value = difficulty });
            }
            // 默认选中第一个
            SelectedDifficulty = DifficultyOptions[0];

            MapOptions = new List<MapDisplayItem>();
            foreach (Maps map in Constants.MapsList)
            {
                MapOptions.Add(new MapDisplayItem { Value = map });
            }
            // 默认选中第一个
            SelectedMap = MapOptions[0];

            logHandler.OnLogMessage += HandleLogMessage;

            SelectScriptCommand = new RelayCommand<string>(SelectScript);
            StartOrStopCommand = new RelayCommand(ExecuteStartOrStop);
            PauseOrResumeCommand = new RelayCommand(ExecutePauseOrResume);
            ImportScriptCommand = new RelayCommand(ImportScript);
            OutputScriptCommand = new RelayCommand(OutputScript);
        }

        #region 我的脚本界面联动

        public ICommand SelectScriptCommand { get; }

        private void SelectScript(string filePath)
        {
            try
            {
                scriptService.LoadScript(filePath);
            }
            catch
            {
                messageBoxService.ShowError("脚本内容错误！");
                return;
            }

            var scriptMetadata = scriptService.GetMetadata();

            // 更新 VM 中选中项，会自动触发 UI 更新
            SelectedMap = MapOptions.FirstOrDefault(m => Equals(m.Value, scriptMetadata.SelectedMap));
            SelectedDifficulty = DifficultyOptions.FirstOrDefault(d => Equals(d.Value, scriptMetadata.SelectedDifficulty));
            SelectedScript = scriptMetadata.ScriptName;

            LoadSelectedScript();
        }

        #endregion

        #region 脚本编辑器联动

        public void UpdateSelectedScript()
        {
            LoadSelectedScript();
        }


        #endregion

        private int selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get => selectedTabIndex;
            set
            {
                if (selectedTabIndex != value)
                {
                    selectedTabIndex = value;
                    OnPropertyChanged(nameof(SelectedTabIndex));
                }
            }
        }

        public ObservableCollection<FunctionDisplayItem> FunctionOptions { get; }

        private FunctionDisplayItem selectedFunction;
        public FunctionDisplayItem SelectedFunction
        {
            get => selectedFunction;
            set
            {
                if (selectedFunction != value)
                {
                    selectedFunction = value;
                    OnPropertyChanged(nameof(SelectedFunction));
                    UpdateControlEnables();
                }
            }
        }

        public List<MapDisplayItem> MapOptions { get; }

        private MapDisplayItem selectedMap;
        public MapDisplayItem SelectedMap
        {
            get => selectedMap;
            set
            {
                if (selectedMap != value)
                {
                    selectedMap = value;
                    OnPropertyChanged(nameof(SelectedMap));
                    UpdateScriptList();
                    LoadSelectedScript();
                }
            }
        }

        public ObservableCollection<DifficultyDisplayItem> DifficultyOptions { get; }

        private DifficultyDisplayItem selectedDifficulty;
        public DifficultyDisplayItem SelectedDifficulty
        {
            get => selectedDifficulty;
            set
            {
                if (selectedDifficulty != value)
                {
                    selectedDifficulty = value;
                    OnPropertyChanged(nameof(SelectedDifficulty));
                    UpdateScriptList();
                    LoadSelectedScript();
                }
            }
        }

        private string selectedScript;
        public string SelectedScript
        {
            get => selectedScript;
            set
            {
                if (selectedScript != value)
                {
                    selectedScript = value;
                    OnPropertyChanged(nameof(SelectedScript));
                    LoadSelectedScript();
                }
            }
        }

        public string ScriptFullPath => scriptService.GetScriptPath(SelectedMap.Name, SelectedDifficulty.Name, SelectedScript);

        private List<string> previewInstructions = new List<string>();
        public List<string> PreviewInstructions
        {
            get => previewInstructions;
            set
            {
                if (previewInstructions != value)
                {
                    previewInstructions = value;
                    OnPropertyChanged(nameof(PreviewInstructions));
                }
            }
        }

        private List<string> scriptList = new List<string>();
        public List<string> ScriptList
        {
            get => scriptList;
            set
            {
                if (scriptList != value)
                {
                    scriptList = value;
                    OnPropertyChanged(nameof(ScriptList));
                }
            }
        }

        private bool _executeMapEnabled = true;
        public bool ExecuteMapEnabled
        {
            get => _executeMapEnabled;
            set
            {
                if (_executeMapEnabled != value)
                {
                    _executeMapEnabled = value;
                    OnPropertyChanged(nameof(ExecuteMapEnabled));
                }
            }
        }

        private bool _executeDifficultyEnabled = true;
        public bool ExecuteDifficultyEnabled
        {
            get => _executeDifficultyEnabled;
            set
            {
                if (_executeDifficultyEnabled != value)
                {
                    _executeDifficultyEnabled = value;
                    OnPropertyChanged(nameof(ExecuteDifficultyEnabled));
                }
            }
        }

        private bool _executeScriptEnabled = true;
        public bool ExecuteScriptEnabled
        {
            get => _executeScriptEnabled;
            set
            {
                if (_executeScriptEnabled != value)
                {
                    _executeScriptEnabled = value;
                    OnPropertyChanged(nameof(ExecuteScriptEnabled));
                }
            }
        }

        // 其他属性类似处理
        private void UpdateControlEnables()
        {
            bool disabled = SelectedFunction.Value == FunctionTypes.Collection
                         || SelectedFunction.Value == FunctionTypes.BlackBorder;

            ExecuteMapEnabled = !disabled;
            ExecuteScriptEnabled = !disabled;
            ExecuteDifficultyEnabled = !disabled;
        }

        private void UpdateScriptList()
        {
            if (SelectedMap == null || SelectedDifficulty == null)
            {
                ScriptList = new List<string>();
                return;
            }

            string fullPath = Path.Combine("data", "我的脚本", SelectedMap.Name, SelectedDifficulty.Name);
            fullPath = Path.GetFullPath(fullPath);

            if (Directory.Exists(fullPath))
            {
                ScriptList = Directory.GetFiles(fullPath)
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .ToList();
            }
            else
            {
                ScriptList = new List<string>();
            }
        }

        private void LoadSelectedScript()
        {
            if (SelectedMap == null || SelectedDifficulty == null)
            {
                PreviewInstructions = new List<string>();
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedMap.Name) ||
                SelectedDifficulty == null ||
                string.IsNullOrWhiteSpace(SelectedScript))
            {
                PreviewInstructions = new List<string>();
                return;
            }

            string path = scriptService.GetScriptPath(SelectedMap.Name, SelectedDifficulty.Name, SelectedScript);

            try
            {
                if (path != null && scriptService.LoadScript(path))
                {
                    PreviewInstructions = scriptService.GetPreview();
                }
                else
                {
                    PreviewInstructions = new List<string>();
                }
            }
            catch
            {
                PreviewInstructions = new List<string>();
            }
        }

        public ICommand ImportScriptCommand { get; }

        private void ImportScript()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "脚本文件 (*.btd6)|*.btd6";

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                string sourceFilePath = openFileDialog.FileName;

                if (Path.GetExtension(sourceFilePath) != ".btd6")
                {
                    messageBoxService.ShowError("脚本格式错误！");
                    return;
                }
                try
                {
                    scriptService.LoadScript(sourceFilePath);
                    scriptService.SaveScript();

                    var scriptMetadata = scriptService.GetMetadata();

                    // 更新 VM 中选中项，会自动触发 UI 更新
                    SelectedMap = MapOptions.FirstOrDefault(m => Equals(m.Value, scriptMetadata.SelectedMap));
                    SelectedDifficulty = DifficultyOptions.FirstOrDefault(d => Equals(d.Value, scriptMetadata.SelectedDifficulty));
                    SelectedScript = scriptMetadata.ScriptName;

                    LoadSelectedScript();
                }
                catch
                {
                    messageBoxService.ShowError("脚本内容错误！");
                    return;
                }
            }
        }

        public ICommand OutputScriptCommand { get; }

        private void OutputScript()
        {
            if (SelectedScript == "")
            {
                messageBoxService.ShowError("请先选择脚本！");
                return;
            }
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                string scriptName = SelectedScript;
                saveFileDialog.FileName = scriptName;
                saveFileDialog.Filter = "脚本文件 (*.btd6)|*.btd6";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string sourceFilePath = $@"data\我的脚本\{SelectedMap.Name}\{SelectedDifficulty.Name}\{SelectedScript}.btd6";
                    string destinationFilePath = saveFileDialog.FileName;
                    File.Copy(sourceFilePath, destinationFilePath, true);
                }
            }
        }


        private IStrategyExecutor currentStrategy;
        private readonly ScriptSettings scriptSettings;
        private readonly LogHandler logHandler = new LogHandler();


        private bool isRunning = false;

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
                OnPropertyChanged(nameof(StartButtonText));
            }
        }

        public string StartButtonText => IsRunning ? "终止" : "启动";

        public ICommand StartOrStopCommand { get; }

        private bool pauseButtonEnabled = false;

        public bool PauseButtonEnabled
        {
            get => pauseButtonEnabled;
            set
            {
                pauseButtonEnabled = value;
                OnPropertyChanged(nameof(PauseButtonEnabled));
            }
        }

        private bool isPaused = false;

        public bool IsPaused
        {
            get => isPaused;
            set
            {
                isPaused = value;
                OnPropertyChanged(nameof(IsPaused));
                OnPropertyChanged(nameof(PauseButtonText));
            }
        }

        public ICommand PauseOrResumeCommand { get; }

        public string PauseButtonText => IsRunning ? (IsPaused ? "继续" : "暂停") : "暂停";

        private int selectedPreviewIndex = -1;
        public int SelectedPreviewIndex
        {
            get => selectedPreviewIndex;
            set
            {
                if (selectedPreviewIndex != value)
                {
                    selectedPreviewIndex = value;
                    OnPropertyChanged(nameof(SelectedPreviewIndex));
                }
            }
        }

        private string currentInstructionText = "";
        public string CurrentInstructionText
        {
            get => currentInstructionText;
            set
            {
                if (currentInstructionText != value)
                {
                    currentInstructionText = value;
                    OnPropertyChanged(nameof(CurrentInstructionText));
                }
            }
        }

        private string currentTriggerText = "";
        public string CurrentTriggerText
        {
            get => currentTriggerText;
            set
            {
                if (currentTriggerText != value)
                {
                    currentTriggerText = value;
                    OnPropertyChanged(nameof(CurrentTriggerText));
                }
            }
        }

        private void ExecuteStartOrStop()
        {
            if (IsRunning)
            {
                currentStrategy.Stop();
                IsRunning = false;
                PauseButtonEnabled = false;
                return;
            }

            var selection = new UserSelection
            {
                selectedFunction = SelectedFunction.Value,
                selectedMap = SelectedMap.Value,
                selectedDifficulty = SelectedDifficulty.Value,
                selectedScript = SelectedScript,
                selectedIndex = SelectedPreviewIndex
            };

            switch (selection.selectedFunction)
            {
                case FunctionTypes.Custom:
                    currentStrategy = new CustomStrategy(logHandler, selection);
                    break;
                case FunctionTypes.Collection:
                    currentStrategy = new CollectionStrategy(logHandler);
                    break;
                case FunctionTypes.Circulation:
                    currentStrategy = new CirculationStrategy(logHandler, selection);
                    break;
                case FunctionTypes.Race:
                    currentStrategy = new RaceStrategy(logHandler, selection);
                    break;
                case FunctionTypes.BlackBorder:
                    currentStrategy = new BlackBorderStrategy(logHandler);
                    break;
            }

            if (currentStrategy == null || !currentStrategy.ReadyToStart) return;

            SubscribeToStrategyEvents(currentStrategy);

            IsRunning = true;
            PauseButtonEnabled = true;
            IsPaused = false;

            currentStrategy.Start();
        }

        private void ExecutePauseOrResume()
        {
            if (!IsRunning) return;
            if (IsPaused)
            {
                currentStrategy.Resume();
                IsPaused = false;
                return;
            }

            currentStrategy.Pause();
            IsPaused = true;

        }

        private string currentRound = "";
        public string CurrentRound
        {
            get => currentRound;
            set
            {
                if (currentRound != value)
                {
                    currentRound = value;
                    OnPropertyChanged(nameof(CurrentRound));
                }
            }
        }

        private string currentGold = "";
        public string CurrentGold
        {
            get => currentGold;
            set
            {
                if (currentGold != value)
                {
                    currentGold = value;
                    OnPropertyChanged(nameof(CurrentGold));
                }
            }
        }

        private string currentLife = "";
        public string CurrentLife
        {
            get => currentLife;
            set
            {
                if (currentLife != value)
                {
                    currentLife = value;
                    OnPropertyChanged(nameof(CurrentLife));
                }
            }
        }

        private void SubscribeToStrategyEvents(IStrategyExecutor strategy)
        {
            strategy.OnStopTriggered += () =>
            {
                IsRunning = false;
                PauseButtonEnabled = false;
            };

            strategy.OnGameDataUpdated += data =>
            {
                CurrentRound = data[0];
                CurrentGold = data[1];
                CurrentLife = data[2];
            };

            strategy.OnCurrentInstructionCompleted += UpdateInstructionDisplay;

            strategy.OnScriptLoaded += metadata =>
            {
                SelectedMap = MapOptions.FirstOrDefault(x => x.Value == metadata.SelectedMap);
                SelectedDifficulty = DifficultyOptions.FirstOrDefault(x => x.Value == metadata.SelectedDifficulty);
                SelectedScript = metadata.ScriptName;
            };
        }

        private bool isRoundTriggerMet;
        public bool IsRoundTriggerMet
        {
            get => isRoundTriggerMet;
            set
            {
                if (isRoundTriggerMet != value)
                {
                    isRoundTriggerMet = value;
                    OnPropertyChanged(nameof(IsRoundTriggerMet));
                }
            }
        }

        private bool isCoinTriggerMet;
        public bool IsCoinTriggerMet
        {
            get => isCoinTriggerMet;
            set
            {
                if (isCoinTriggerMet != value)
                {
                    isCoinTriggerMet = value;
                    OnPropertyChanged(nameof(IsCoinTriggerMet));
                }
            }
        }

        private void UpdateInstructionDisplay(ExecutableInstruction instruction)
        {
            // 更新当前选中项
            SelectedPreviewIndex = instruction.Index;

            // 设置触发条件是否满足（供 UI 绑定颜色等）
            IsRoundTriggerMet = instruction.IsRoundMet;
            IsCoinTriggerMet = instruction.IsCoinMet;

            // 构造文本显示
            var instructionBuilder = new StringBuilder();

            // 指令内容
            instructionBuilder.AppendLine("当前指令：");
            instructionBuilder.AppendLine(instruction.Content);

            var triggerBuilder = new StringBuilder();

            // 触发条件展示
            triggerBuilder.AppendLine("触发条件：");

            // 回合条件
            bool isRoundMet = instruction.IsRoundMet;
            string roundIcon = isRoundMet ? "✅" : "❌";
            // 金币条件
            bool isCashMet = instruction.IsCoinMet;
            string cashIcon = isCashMet ? "✅" : "❌";
            triggerBuilder.AppendLine($"第 {instruction.RoundTrigger} 回合后 {roundIcon} / {instruction.CoinTrigger} 金币 {cashIcon}");

            // 更新属性绑定的显示文本
            CurrentInstructionText = instructionBuilder.ToString();
            CurrentTriggerText = triggerBuilder.ToString();
        }


        private ObservableCollection<LogItem> logs = new ObservableCollection<LogItem>();

        public ObservableCollection<LogItem> Logs => logs;

        private LogItem currentLog = new LogItem();
        public LogItem CurrentLog
        {
            get => currentLog;
            set
            {
                if (currentLog != value)
                {
                    currentLog = value;
                    OnPropertyChanged(nameof(CurrentLog));
                }
            }
        }

        private void HandleLogMessage(string msg, LogLevel level)
        {
            CurrentLog = new LogItem { Message = msg, Level = level };
            logs.Add(CurrentLog);

            if (logs.Count > 1000)
                logs = new ObservableCollection<LogItem>(logs.Skip(logs.Count - 1000));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
