using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using BTD6AutoCommunity.Models;
using BTD6AutoCommunity.Models.Instruction;
using BTD6AutoCommunity.ScriptEngine;
using BTD6AutoCommunity.ScriptEngine.InstructionSystem;
using BTD6AutoCommunity.ScriptEngine.ScriptSystem;
using BTD6AutoCommunity.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BTD6AutoCommunity.ViewModels
{
    public class ScriptEditorViewModel : INotifyPropertyChanged
    {
        private readonly IScriptService scriptService;
        private readonly IMouseCoordinateDisplayService coordinateDisplayService;
        private readonly IMessageBoxService messageBoxService;
        public ScriptEditorViewModel(IScriptService scriptService, IMouseCoordinateDisplayService coordinateDisplayService, IMessageBoxService messageBoxService)
        {
            this.scriptService = scriptService;
            this.coordinateDisplayService = coordinateDisplayService;
            this.messageBoxService = messageBoxService;

            MapOptions = new List<MapDisplayItem>();
            foreach (Maps map in Constants.MapsList)
            {
                MapOptions.Add(new MapDisplayItem { Value = map });
            }
            // 默认选中第一个
            SelectedMap = MapOptions[0];

            DifficultyOptions = new List<DifficultyDisplayItem>();
            foreach (LevelDifficulties difficulty in Constants.DifficultiesList)
            {
                DifficultyOptions.Add(new DifficultyDisplayItem { Value = difficulty });
            }
            // 默认选中第一个
            SelectedDifficulty = DifficultyOptions[0];

            ModeOptions = new List<ModeDisplayItem>();
            foreach (LevelModes mode in Constants.ModesList)
            {
                ModeOptions.Add(new ModeDisplayItem { Value = mode });
            }
            // 默认选中第一个
            SelectedMode = ModeOptions[0];

            HeroOptions = new List<HeroDisplayItem>();
            foreach (Heroes hero in Constants.HeroesList)
            {
                HeroOptions.Add(new HeroDisplayItem { Value = hero });
            }
            // 默认选中第一个
            SelectedHero = HeroOptions[0];

            EditScriptCommand = new RelayCommand<string>(EditScript);

            ToggleAnchorModeCommand = new RelayCommand(ToggleAnchorMode);
            ToggleCoordinateCommand = new RelayCommand(ToggleCoordinate);
            AddInstructionCommand = new RelayCommand(AddInstruction);
            InsertInstructionCommand = new RelayCommand(InsertInstruction);
            ModifyInstructionCommand = new RelayCommand(ModifyInstruction);
            RemoveInstructionCommand = new RelayCommand(RemoveInstruction);
            ClearInstructionCommand = new RelayCommand(ClearInstruction);
            InstructionUpCommand = new RelayCommand(InstructionUp);
            InstructionDownCommand = new RelayCommand(InstructionDown);
            BuildInstructionsCommand = new RelayCommand(BuildInstructions);
            SaveInstructionsCommand = new RelayCommand<bool>(SaveInstructions);
            InitializeInstructionEditor();
        }

        #region 开始界面联动

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

        public ICommand EditScriptCommand { get; }

        private void EditScript(string scriptPath)
        {
            try
            {
                scriptService.LoadScript(scriptPath);
            }
            catch
            {
                messageBoxService.ShowError("脚本内容错误！");
                return;
            }
            var metadata = scriptService.GetMetadata();
            SelectedMap = new MapDisplayItem { Value = metadata.SelectedMap };
            SelectedDifficulty = new DifficultyDisplayItem { Value = metadata.SelectedDifficulty };
            SelectedMode = new ModeDisplayItem { Value = metadata.SelectedMode };
            SelectedHero = new HeroDisplayItem { Value = metadata.SelectedHero };
            ScriptName = metadata.ScriptName;
            AnchorXText = metadata.AnchorCoords.X.ToString();
            AnchorYText = metadata.AnchorCoords.Y.ToString();
            Instructions = new BindingList<Instruction>(scriptService.GetInstructionsCopy().InstructionsList);
            SelectedTabIndex = 1;
        }

        #endregion

        #region 脚本分类相关

        public List<MapDisplayItem> MapOptions { get; }

        private MapDisplayItem selectedMap;
        public MapDisplayItem SelectedMap
        {
            get => selectedMap;
            set
            {
                selectedMap = value;
                OnPropertyChanged(nameof(SelectedMap));
            }
        }

        public List<DifficultyDisplayItem> DifficultyOptions { get; }

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
                }
            }
        }

        public List<HeroDisplayItem> HeroOptions { get; }

        private HeroDisplayItem selectedHero;
        public HeroDisplayItem SelectedHero
        {
            get => selectedHero;
            set
            {
                if (selectedHero != value)
                {
                    selectedHero = value;
                    OnPropertyChanged(nameof(SelectedHero));
                    UpdateScriptName();
                }
            }
        }

        public List<ModeDisplayItem> ModeOptions { get; }

        private ModeDisplayItem selectedMode;
        public ModeDisplayItem SelectedMode
        {
            get => selectedMode;
            set
            {
                if (selectedMode != value)
                {
                    selectedMode = value;
                    OnPropertyChanged(nameof(SelectedMode));
                    UpdateScriptName();
                }
            }
        }

        private string scriptName;

        public string ScriptName
        {
            get => scriptName;
            set
            {
                if (scriptName != value)
                {
                    scriptName = value;
                    OnPropertyChanged(nameof(ScriptName));
                }
            }
        }

        private void UpdateScriptName()
        {
            if (SelectedHero == null || SelectedMode == null) return;
            if (!Enum.IsDefined(typeof(Heroes), SelectedHero.Value) ||
                !Enum.IsDefined(typeof(LevelModes), SelectedMode.Value))
                return;
            int intTime = DateTime.Now.GetHashCode() % 1000;
            string Time = (intTime > 0 ? intTime : -1 * intTime).ToString();


            ScriptName = SelectedMode.Name + "-" + SelectedHero.Name + "-" + Time;
        }

        private string anchorXText;
        public string AnchorXText
        {
            get => anchorXText;
            set
            {
                if (anchorXText != value)
                {
                    anchorXText = value;
                    OnPropertyChanged(nameof(AnchorXText));
                }
            }
        }

        private string anchorYText;
        public string AnchorYText
        {
            get => anchorYText;
            set
            {
                if (anchorYText != value)
                {
                    anchorYText = value;
                    OnPropertyChanged(nameof(AnchorYText));
                }
            }
        }

        private string anchorButtonText = "设置空白点";
        public string AnchorButtonText
        {
            get => anchorButtonText;
            set
            {
                if (anchorButtonText != value)
                {
                    anchorButtonText = value;
                    OnPropertyChanged(nameof(AnchorButtonText));
                }
            }
        }

        private string coordinateButtonText = "选择坐标";
        public string CoordinateButtonText
        {
            get => coordinateButtonText;
            set
            {
                if (coordinateButtonText != value)
                {
                    coordinateButtonText = value;
                    OnPropertyChanged(nameof(CoordinateButtonText));
                }
            }
        }

        public ICommand ToggleAnchorModeCommand { get; }

        private void ToggleAnchorMode()
        {
            if (AnchorButtonText == "设置空白点")
            {
                AnchorButtonText = "取消设置";
                if (CoordinateButtonText == "选择坐标")
                {
                    coordinateDisplayService.StartDisplay(OnEnterPressed);
                }
                else
                {
                    CoordinateButtonText = "选择坐标";
                }
            }
            else
            {
                AnchorButtonText = "设置空白点";
                coordinateDisplayService.StopDisplay();
            }
        }

        public ICommand ToggleCoordinateCommand { get; }

        private void ToggleCoordinate()
        {
            if (CoordinateButtonText == "选择坐标")
            {
                CoordinateButtonText = "取消选择";
                if (AnchorButtonText == "设置空白点")
                {
                    coordinateDisplayService.StartDisplay(OnEnterPressed);
                }
                else
                {
                    AnchorButtonText = "设置空白点";
                }
            }
            else
            {
                CoordinateButtonText = "选择坐标";
                coordinateDisplayService.StopDisplay();
            }
        }

        private void OnEnterPressed(Point point)
        {
            if (AnchorButtonText == "取消设置")
            {
                AnchorXText = point.X.ToString();
                AnchorYText = point.Y.ToString();
            }
            if (CoordinateButtonText == "取消选择")
            {
                CoordinateDefinition.X = point.X;
                CoordinateDefinition.Y = point.Y;
                OnPropertyChanged(nameof(CoordinateDefinition));
            }
        }

        private ScriptMetadata GetMetadata()
        {
            return new ScriptMetadata
            {
                SelectedMap = SelectedMap.Value,
                SelectedDifficulty = SelectedDifficulty.Value,
                SelectedMode = SelectedMode.Value,
                SelectedHero = SelectedHero.Value,
                ScriptName = ScriptName,
                AnchorCoords = Int32.TryParse(AnchorXText, out int x) && Int32.TryParse(AnchorYText, out int y) ? (x, y) : (860, 540)
            };
        }

        #endregion

        #region 脚本指令相关

        private void InitializeInstructionEditor()
        {
            foreach (ActionTypes actionType in Constants.ActionsList)
            {
                ActionOptions.Add(new ActionDisplayItem { Value = actionType });
            }
            SelectedAction = ActionOptions[0];

        }

        public ObservableCollection<ActionDisplayItem> ActionOptions { get; } = new ObservableCollection<ActionDisplayItem>();

        // 指令编辑器是否可用, 选中指令时，无需刷新
        private bool _isSelectedInstruction = false;

        private ActionDisplayItem selectedAction;
        public ActionDisplayItem SelectedAction
        {
            get => selectedAction;
            set
            {
                if (selectedAction != value)
                {
                    selectedAction = value;
                    OnPropertyChanged(nameof(SelectedAction));
                    if (!_isSelectedInstruction)
                    {
                        UpdateInstructionEditor();
                        UpdateModifyInstructionEnabled();
                    }
                }
            }
        }

        private InstructionArgumentDefinition argument1;
        public InstructionArgumentDefinition Argument1
        {
            get => argument1;
            set
            {
                if (argument1 != value)
                {
                    argument1 = value;
                    OnPropertyChanged(nameof(Argument1));
                    argument1Value = argument1.GetValue();
                }
            }
        }

        private object argument1Value;
        public object Argument1Value
        {
            get => argument1Value;
            set
            {
                if (argument1Value != value)
                {
                    argument1Value = value;
                    UpdateCoordinate();
                    UpdateModifyInstructionEnabled();
                }
            }
        }

        private InstructionArgumentDefinition argument2;
        public InstructionArgumentDefinition Argument2
        {
            get => argument2;
            set
            {
                if (argument2 != value)
                {
                    argument2 = value;
                    OnPropertyChanged(nameof(Argument2));
                    Argument2Value = argument2.GetValue();
                }
            }
        }

        private object argument2Value;
        public object Argument2Value
        {
            get => argument2Value;
            set
            {
                if (argument2Value != value)
                {
                    argument2Value = value;
                    UpdateCoordinate();
                    UpdateModifyInstructionEnabled();
                }
            }
        }

        private InstructionTriggerDefinition triggerDefinition;
        public InstructionTriggerDefinition TriggerDefinition
        {
            get => triggerDefinition;
            set
            {
                if (triggerDefinition != value)
                {
                    triggerDefinition = value;
                    OnPropertyChanged(nameof(TriggerDefinition));
                }
            }
        }

        private InstructionCoordinateDefinition coordinateDefinition;
        public InstructionCoordinateDefinition CoordinateDefinition
        {
            get => coordinateDefinition;
            set
            {
                if (coordinateDefinition != value)
                {
                    coordinateDefinition = value;
                    OnPropertyChanged(nameof(CoordinateDefinition));
                }
            }
        }

        private BindingList<Instruction> instructions = new BindingList<Instruction>();
        public BindingList<Instruction> Instructions
        {
            get => instructions;
            set
            {
                if (instructions != value)
                {
                    instructions = value;
                    OnPropertyChanged(nameof(Instructions));
                }
            }
        }

        private Instruction SelectedInstruction => selectedInstructionIndices.Count > 0 && selectedInstructionIndices[0] >= 0 && selectedInstructionIndices[0] < Instructions.Count
            ? Instructions[selectedInstructionIndices[0]] : null;

        // 上下移动指令时是否禁用指令编辑器
        private bool _suppressSelectionChanged = false;

        private List<int> selectedInstructionIndices = new List<int>();
        public List<int> SelectedInstructionIndices
        {
            get => selectedInstructionIndices;
            set
            {
                if (!selectedInstructionIndices.SequenceEqual(value))
                {
                    selectedInstructionIndices = value;
                    OnPropertyChanged(nameof(SelectedInstructionIndices));
                    if (!_suppressSelectionChanged)
                    {
                        _isSelectedInstruction = true;
                        UpdateInstructionEditor(SelectedInstruction);
                        IsModifyInstructionEnabled = true;
                        _isSelectedInstruction = false;
                    }
                }
            }
        }

        private void UpdateInstructionEditor(Instruction inst = null)
        {
            if (SelectedAction == null) return;
            if (inst != null)
            {
                SelectedAction = new ActionDisplayItem { Value = inst.Type };
            }
            InstructionArgumentHandler argumentHandler = new InstructionArgumentHandler(scriptService);
            InstructionDefinition instructionDefinition = argumentHandler.GetDefinition(SelectedAction.Value, inst);
            Argument1 = instructionDefinition.Arguments[0];
            Argument2 = instructionDefinition.Arguments[1];
            TriggerDefinition = instructionDefinition.Trigger;
            CoordinateDefinition = instructionDefinition.Coordinate;
        }

        private void UpdateCoordinate()
        {
            if (Argument1 == null || Argument2 == null) return;
            if (Argument1.SelectedItem == null || Argument2.SelectedItem == null) return;
            if (Argument1.SelectedItem.Name.Contains("有坐标") || Argument2.SelectedItem.Name.Contains("有坐标"))
            {
                CoordinateDefinition.IsVisible = true;
                OnPropertyChanged(nameof(CoordinateDefinition));
            }
            if (Argument1.SelectedItem.Name.Contains("无坐标") || Argument2.SelectedItem.Name.Contains("无坐标"))
            {
                CoordinateDefinition.IsVisible = false;
                OnPropertyChanged(nameof(CoordinateDefinition));
            }
        }

        public ICommand AddInstructionCommand { get; }
        private void AddInstruction()
        {
            ActionTypes actionType = SelectedAction.Value;

            if (actionType != ActionTypes.InstructionsBundle)
            {
                List<int> arguments = GetArguments();
                int roundTrigger = TriggerDefinition.GetRoundTrigger();
                int coinTrigger = TriggerDefinition.GetCoinTrigger();
                (int, int) coordinate = CoordinateDefinition.GetCoordinate();
                Instructions.Add(scriptService.AddInstruction(actionType, arguments, roundTrigger, coinTrigger, coordinate));
            }
            else
            {
                scriptService.AddInstructionBundle((string)Argument1Value, (int)Argument2.GetValue());
                Instructions.Clear();
                Instructions = new BindingList<Instruction>(scriptService.GetInstructionsCopy().InstructionsList);
            }
            List<int> indices = new List<int>
            {
                Instructions.Count - 1
            };
            SelectedInstructionIndices = new List<int>(indices);
        }

        public ICommand InsertInstructionCommand { get; }

        private void InsertInstruction()
        {
            if (SelectedInstructionIndices.Count == 0) return;
            if (SelectedInstructionIndices[0] == -1) return;
            int index = SelectedInstructionIndices[0];
            int count = 1;
            ActionTypes actionType = SelectedAction.Value;

            if (actionType != ActionTypes.InstructionsBundle)
            {
                List<int> arguments = GetArguments();
                int roundTrigger = TriggerDefinition.GetRoundTrigger();
                int coinTrigger = TriggerDefinition.GetCoinTrigger();
                (int, int) coordinate = CoordinateDefinition.GetCoordinate();
                Instructions.Insert(index + 1, scriptService.InsertInstruction(index + 1, actionType, arguments, roundTrigger, coinTrigger, coordinate));
            }
            else
            {
                count = scriptService.InsertInstructionBundle(index + 1, (string)Argument1Value, (int)Argument2.GetValue());
                Instructions.Clear();
                Instructions = new BindingList<Instruction>(scriptService.GetInstructionsCopy().InstructionsList);
            }
            List<int> indices = new List<int>
            {
                index + count
            };
            SelectedInstructionIndices = new List<int>(indices);
        }

        // 修改按钮是否可用
        private bool isModifyInstructionEnabled = true;
        public bool IsModifyInstructionEnabled
        {
            get => isModifyInstructionEnabled;
            set
            {
                if (isModifyInstructionEnabled != value)
                {
                    isModifyInstructionEnabled = value;
                    OnPropertyChanged(nameof(IsModifyInstructionEnabled));
                }
            }
        }

        private void UpdateModifyInstructionEnabled()
        {
            if (SelectedInstructionIndices.Count == 0)
            {
                IsModifyInstructionEnabled = false;
                return;
            }
            if (SelectedInstructionIndices[0] == -1)
            {
                IsModifyInstructionEnabled = false;
                return;
            }
            int index = SelectedInstructionIndices[0];
            ActionTypes instructionType = SelectedAction.Value;
            if (instructionType == ActionTypes.InstructionsBundle)
            {
                IsModifyInstructionEnabled = false;
                return;
            }
            List<int> args = GetArguments();
            int RoundTrigger = TriggerDefinition.GetRoundTrigger();
            int CoinTrigger = TriggerDefinition.GetCoinTrigger();
            (int, int) coords = CoordinateDefinition.GetCoordinate();
            if (scriptService.TryModifyInstruction(index, instructionType, args, RoundTrigger, CoinTrigger, coords))
            {
                IsModifyInstructionEnabled = true;
            }
            else
            {
                IsModifyInstructionEnabled = false;
            }

        }

        public ICommand ModifyInstructionCommand { get; }
        private void ModifyInstruction()
        {
            if (SelectedInstructionIndices.Count == 0) return;
            if (SelectedInstructionIndices[0] == -1) return;
            int index = SelectedInstructionIndices[0];
            ActionTypes actionType = SelectedAction.Value;
            List<int> arguments = GetArguments();
            int roundTrigger = TriggerDefinition.GetRoundTrigger();
            int coinTrigger = TriggerDefinition.GetCoinTrigger();
            (int, int) coordinate = CoordinateDefinition.GetCoordinate();
            if (actionType != ActionTypes.InstructionsBundle)
            {
                Instructions[index] = scriptService.ModifyInstruction(index, actionType, arguments, roundTrigger, coinTrigger, coordinate);
            }
            List<int> indices = new List<int>
            {
                index
            };
            SelectedInstructionIndices = new List<int>(indices);
        }

        public ICommand RemoveInstructionCommand { get; }

        private void RemoveInstruction()
        {
            if (SelectedInstructionIndices.Count == 0) return;
            int firstIndex = SelectedInstructionIndices[0];
            var indices = SelectedInstructionIndices.OrderByDescending(i => i).ToList();
            foreach (int index in indices)
            {
                if (index < 0 || index >= Instructions.Count) continue;
                scriptService.RemoveInstruction(index);
                Instructions.RemoveAt(index);
            }
            if (firstIndex < Instructions.Count)
            {
                SelectedInstructionIndices = new List<int> { firstIndex };
            }
            else
            {
                SelectedInstructionIndices = new List<int> { firstIndex - 1 };
            }
        }

        public ICommand ClearInstructionCommand { get; }

        private void ClearInstruction()
        {
            // TODO: 确认是否要清除脚本

            scriptService.ClearInstructions();
            Instructions.Clear();
            SelectedInstructionIndices = new List<int>();
        }

        public ICommand BuildInstructionsCommand { get; }

        private void BuildInstructions()
        {
            int index = 0;
            if (SelectedInstructionIndices.Count > 0 && SelectedInstructionIndices[0] != -1)
            {
                index = SelectedInstructionIndices[0];
            }
            scriptService.BuildInstructions();
            Instructions.Clear();
            Instructions = new BindingList<Instruction>(scriptService.GetInstructionsCopy().InstructionsList);
            if (index > 0 && index < Instructions.Count)
            {
                SelectedInstructionIndices = new List<int> { index };
            }
            else
            {
                SelectedInstructionIndices = new List<int> { Instructions.Count - 1 };
            }
        }

        public ICommand InstructionUpCommand { get; }

        private void InstructionUp()
        {
            if (SelectedInstructionIndices.Count == 0) return;
            if (SelectedInstructionIndices[0] == -1) return;
            _suppressSelectionChanged = true;

            List<int> selectedIndices = SelectedInstructionIndices.OrderBy(i => i).ToList();
            List<int> updatedIndices = new List<int>();
            int currentIndex, lastIndex = -1;
            for (int i = 0; i < selectedIndices.Count; i++)
            {
                currentIndex = selectedIndices[i];
                if (currentIndex - 1 != lastIndex)
                {
                    scriptService.MoveInstruction(currentIndex, true);
                    (Instructions[currentIndex], Instructions[currentIndex - 1])
                        = (Instructions[currentIndex - 1], Instructions[currentIndex]);
                    updatedIndices.Add(currentIndex - 1);
                    lastIndex = currentIndex - 1;
                }
                else
                {
                    updatedIndices.Add(currentIndex);
                    lastIndex = currentIndex;
                }
            }
            SelectedInstructionIndices = new List<int>(updatedIndices);

            _suppressSelectionChanged = false;
        }

        public ICommand InstructionDownCommand { get; }

        private void InstructionDown()
        {
            if (SelectedInstructionIndices.Count == 0) return;
            if (SelectedInstructionIndices[0] == -1) return;
            _suppressSelectionChanged = true;

            List<int> selectedIndices = SelectedInstructionIndices.OrderByDescending(i => i).ToList();
            List<int> updatedIndices = new List<int>();
            int currentIndex, lastIndex = Instructions.Count;
            for (int i = 0; i < selectedIndices.Count; i++)
            {
                currentIndex = selectedIndices[i];
                if (currentIndex + 1 != lastIndex)
                {
                    scriptService.MoveInstruction(currentIndex, false);
                    (Instructions[currentIndex], Instructions[currentIndex + 1])
                        = (Instructions[currentIndex + 1], Instructions[currentIndex]);
                    updatedIndices.Add(currentIndex + 1);
                    lastIndex = currentIndex + 1;
                }
                else
                {
                    updatedIndices.Add(currentIndex);
                    lastIndex = currentIndex;
                }
            }
            SelectedInstructionIndices = new List<int>(updatedIndices);
            _suppressSelectionChanged = false;
        }

        public ICommand SaveInstructionsCommand { get; }

        private void SaveInstructions(bool toStart)
        {
            var metadata = GetMetadata();
            scriptService.SetMetadata(metadata);
            try
            {
                scriptService.SaveScript();
            }
            catch
            {
                messageBoxService.ShowError("保存失败，请检查脚本名称是否含有特殊字符，例如：/\\:*?\"<>|");
                return;
            }
            ClearInstruction();
            UpdateScriptName();
            if (toStart)
            {
                SelectedTabIndex = 0;
            }
            else
            {
                SelectedTabIndex = 3;
            }
        }


        private List<int> GetArguments()
        {
            List<int> arguments = new List<int>();
            for (int i = 0; i < 7; i++) arguments.Add(-1);
            arguments[0] = (int)Argument1.GetValue();
            arguments[1] = (int)Argument2.GetValue();
            return arguments;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
