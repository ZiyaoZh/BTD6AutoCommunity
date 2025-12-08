using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Timers;
using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using BTD6AutoCommunity.ScriptEngine;
using BTD6AutoCommunity.ScriptEngine.ScriptSystem;
using BTD6AutoCommunity.ScriptEngine.InstructionSystem;
using System.Diagnostics;
using OpenCvSharp.Flann;
using System.Dynamic;
using BTD6AutoCommunity.Services.Interfaces;
using BTD6AutoCommunity.Services;
using BTD6AutoCommunity.ViewModels;
using BTD6AutoCommunity.Models;
using BTD6AutoCommunity.Models.Instruction;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Bson;

namespace BTD6AutoCommunity.UI.Main
{
    // 脚本编辑器界面
    public partial class BTD6AutoUI
    {
        private ScriptEditorViewModel scriptEditorViewModel;
        private MouseCoordinateDisplayService mouseCoordinateDisplayService;
        private ScriptService scriptEditerScriptService;
        private void InitializeScriptsEditor()
        {
            scriptEditerScriptService = new ScriptService();
            mouseCoordinateDisplayService = new MouseCoordinateDisplayService(Handle);
            scriptEditorViewModel = new ScriptEditorViewModel(scriptEditerScriptService, mouseCoordinateDisplayService, messageBoxService);
            BindControls();

        }

        private void BindControls()
        {
            BindScriptMapComboBox();
            BindScriptDifficultyComboBox();
            BindScriptModeComboBox();
            BindScriptHeroComboBox();
            BindScriptNameTextBox();
            BindAnchorCoordsControls();
            BindActionComboBox();
            BindArgument1ComboBox();
            BindArgument2ComboBox();
            BindInstructionTriggerControls();
            BindInstructionCoordinateControls();
            BindInstructionsViewListBox();
            BindAddInstructionButton();
            BindInsertInstructionButton();
            BindModifiyInstructionButton();
            BindRemoveInstructionButton();
            BindInstructionUpButton();
            BindInstructionDownButton();
            BindClearInstructionButton();
            BindBuildInstructionsButton();
            BindSaveInstructionsButton();
        }

        private void BindScriptMapComboBox()
        {
            MapCB.DataSource = scriptEditorViewModel.MapOptions;
            MapCB.ValueMember = "Value";
            MapCB.DisplayMember = "Name";

            MapCB.SelectedItem = scriptEditorViewModel.SelectedMap;

            MapCB.SelectedIndexChanged += (s, e) =>
            {
                if (MapCB.SelectedItem is MapDisplayItem item)
                {
                    scriptEditorViewModel.SelectedMap = item;
                }
            };

            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(scriptEditorViewModel.SelectedMap))
                {
                    MapCB.InvokeIfRequired(() =>
                    {
                        if (!Equals(MapCB.SelectedValue, scriptEditorViewModel.SelectedMap.Value))
                        {
                            MapCB.SelectedValue = scriptEditorViewModel.SelectedMap.Value;
                        }
                    });
                }
            };
        }

        private void BindScriptModeComboBox()
        {
            ModeCB.DataSource = scriptEditorViewModel.ModeOptions;
            ModeCB.ValueMember = "Value";
            ModeCB.DisplayMember = "Name";

            ModeCB.SelectedItem = scriptEditorViewModel.SelectedMode;

            ModeCB.SelectedIndexChanged += (s, e) =>
            {
                if (ModeCB.SelectedItem is ModeDisplayItem item)
                {
                    scriptEditorViewModel.SelectedMode = item;
                }
            };

            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(scriptEditorViewModel.SelectedMode))
                {
                    ModeCB.InvokeIfRequired(() =>
                    {
                        if (!Equals(ModeCB.SelectedValue, scriptEditorViewModel.SelectedMode.Value))
                        {
                            ModeCB.SelectedValue = scriptEditorViewModel.SelectedMode.Value;
                        }
                    });
                }
            };
        }

        private void BindScriptHeroComboBox()
        {
            HeroCB.DataSource = scriptEditorViewModel.HeroOptions;
            HeroCB.ValueMember = "Value";
            HeroCB.DisplayMember = "Name";

            HeroCB.SelectedItem = scriptEditorViewModel.SelectedHero;


            HeroCB.SelectedIndexChanged += (s, e) =>
            {
                if (HeroCB.SelectedItem is HeroDisplayItem item)
                {
                    scriptEditorViewModel.SelectedHero = item;
                }
            };

            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(scriptEditorViewModel.SelectedHero))
                {
                    HeroCB.InvokeIfRequired(() =>
                    {
                        if (!Equals(HeroCB.SelectedValue, scriptEditorViewModel.SelectedHero.Value))
                        {
                            HeroCB.SelectedValue = scriptEditorViewModel.SelectedHero.Value;
                        }
                    });
                }
            };
        }

        private void BindScriptDifficultyComboBox()
        {
            DifficultyCB.DataSource = scriptEditorViewModel.DifficultyOptions;
            DifficultyCB.ValueMember = "Value";
            DifficultyCB.DisplayMember = "Name";

            DifficultyCB.SelectedItem = scriptEditorViewModel.SelectedDifficulty;

            DifficultyCB.SelectedIndexChanged += (s, e) =>
            {
                if (DifficultyCB.SelectedItem is DifficultyDisplayItem item)
                {
                    scriptEditorViewModel.SelectedDifficulty = item;
                }
            };

            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(scriptEditorViewModel.SelectedDifficulty))
                {
                    DifficultyCB.InvokeIfRequired(() =>
                    {
                        if (!Equals(DifficultyCB.SelectedValue, scriptEditorViewModel.SelectedDifficulty.Value))
                        {
                            DifficultyCB.SelectedValue = scriptEditorViewModel.SelectedDifficulty.Value;
                        }
                    });
                }
            };
        }

        private void BindScriptNameTextBox()
        {
            ScriptNameTB.DataBindings.Add("Text", scriptEditorViewModel, "ScriptName");

            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(scriptEditorViewModel.ScriptName))
                {
                    if (!Equals(ScriptNameTB.Text, scriptEditorViewModel.ScriptName))
                    {
                        ScriptNameTB.Text = scriptEditorViewModel.ScriptName;
                    }
                }
            };
        }

        private void BindAnchorCoordsControls()
        {
            AnchorXTB.DataBindings.Add("Text", scriptEditorViewModel, "AnchorXText");
            AnchorYTB.DataBindings.Add("Text", scriptEditorViewModel, "AnchorYText");
            AnchorCoordsBT.DataBindings.Add("Text", scriptEditorViewModel, "AnchorButtonText");
            AnchorCoordsBT.Click += (s, e) => scriptEditorViewModel.ToggleAnchorModeCommand.Execute(null);

        }

        private void BindActionComboBox()
        {
            ActionsCB.DataSource = scriptEditorViewModel.ActionOptions;
            ActionsCB.ValueMember = "Value";
            ActionsCB.DisplayMember = "Name";

            ActionsCB.SelectedItem = scriptEditorViewModel.SelectedAction;

            ActionsCB.SelectedIndexChanged += (s, e) =>
            {
                if (ActionsCB.SelectedItem is ActionDisplayItem item)
                {
                    scriptEditorViewModel.SelectedAction = item;
                }
            };

            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(scriptEditorViewModel.SelectedAction))
                {
                    ActionsCB.InvokeIfRequired(() =>
                    {
                        if (!Equals(ActionsCB.SelectedValue, scriptEditorViewModel.SelectedAction.Value))
                        {
                            ActionsCB.SelectedValue = scriptEditorViewModel.SelectedAction.Value;
                        }
                    });
                }
            };
        }

        private void BindArgument1ComboBox()
        {
            if (scriptEditorViewModel.Argument1.IsVisible)
            {
                if (scriptEditorViewModel.Argument1.IsSelectable)
                {
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;
                    Argument1CB.DataSource = scriptEditorViewModel.Argument1.Options;
                    Argument1CB.ValueMember = "Value";
                    Argument1CB.DisplayMember = "Name";
                    Argument1CB.SelectedValue = scriptEditorViewModel.Argument1.SelectedItem.Value;
                }
                else
                {
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDown;
                    Argument1CB.Text = scriptEditorViewModel.Argument1.Placeholder;
                }
                Argument1CB.Visible = true;
            }
            else
            {
                Argument1CB.Visible = false;
            }
            bool _isUpdating = false;
            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(scriptEditorViewModel.Argument1))
                {
                    if (_isUpdating) return;

                    _isUpdating = true;
                    if (scriptEditorViewModel.Argument1.IsVisible)
                    {
                        if (scriptEditorViewModel.Argument1.IsSelectable)
                        {
                            Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;
                            Argument1CB.DataSource = scriptEditorViewModel.Argument1.Options;
                            Argument1CB.ValueMember = "Value";
                            Argument1CB.DisplayMember = "Name";
                            Argument1CB.SelectedValue = scriptEditorViewModel.Argument1.SelectedItem.Value ?? -1;
                        }
                        else
                        {
                            Argument1CB.DropDownStyle = ComboBoxStyle.DropDown;
                            if (scriptEditorViewModel.Argument1.IsPassive)
                            {
                                Argument1CB.Text = scriptEditorViewModel.Argument1.InputText;
                            }
                            else
                            {
                                Argument1CB.Text = scriptEditorViewModel.Argument1.Placeholder;
                            }
                        }
                        Argument1CB.Visible = true;
                    }
                    else
                    {
                        Argument1CB.Visible = false;
                    }

                    _isUpdating = false;
                }
            };
            Argument1CB.SelectedIndexChanged += (s, e) =>
            {
                if (_isUpdating) return;

                _isUpdating = true;

                scriptEditorViewModel.Argument1.SelectedItem = (IDisplayItem)Argument1CB.SelectedItem;
                scriptEditorViewModel.Argument1Value = Argument1CB.SelectedValue;

                _isUpdating = false;
            };

            Argument1CB.TextChanged += (s, e) =>
            {
                if (_isUpdating) return;

                _isUpdating = true;

                scriptEditorViewModel.Argument1.InputText = Argument1CB.Text;

                _isUpdating = false;
            };
        }

        private void BindArgument2ComboBox()
        {
            if (scriptEditorViewModel.Argument2.IsVisible)
            {
                if (scriptEditorViewModel.Argument2.IsSelectable)
                {
                    Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;
                    Argument2CB.DataSource = scriptEditorViewModel.Argument2.Options;
                    Argument2CB.ValueMember = "Value";
                    Argument2CB.DisplayMember = "Name";
                    Argument2CB.SelectedValue = scriptEditorViewModel.Argument2.SelectedItem.Value ?? -1;
                }
                else
                {
                    Argument2CB.DropDownStyle = ComboBoxStyle.DropDown;
                    Argument2CB.Text = scriptEditorViewModel.Argument2.Placeholder;
                }
                Argument2CB.Visible = true;
            }
            else
            {
                Argument2CB.Visible = false;
            }
            bool _isUpdating = false;
            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(scriptEditorViewModel.Argument2))
                {
                    if (_isUpdating) return;

                    _isUpdating = true;
                    if (scriptEditorViewModel.Argument2.IsVisible)
                    {
                        if (scriptEditorViewModel.Argument2.IsSelectable)
                        {
                            Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;
                            Argument2CB.DataSource = scriptEditorViewModel.Argument2.Options;
                            Argument2CB.ValueMember = "Value";
                            Argument2CB.DisplayMember = "Name";
                            Argument2CB.SelectedValue = scriptEditorViewModel.Argument2.SelectedItem.Value;
                        }
                        else
                        {
                            Argument2CB.DropDownStyle = ComboBoxStyle.DropDown;
                            if (scriptEditorViewModel.Argument2.IsPassive)
                            {
                                Argument2CB.Text = scriptEditorViewModel.Argument2.InputText;
                            }
                            else
                            {
                                Argument2CB.Text = scriptEditorViewModel.Argument2.Placeholder;
                            }
                        }
                        Argument2CB.Visible = true;
                    }
                    else
                    {
                        Argument2CB.Visible = false;
                    }

                    _isUpdating = false;
                }
            };

            Argument2CB.SelectedIndexChanged += (s, e) =>
            {
                if (_isUpdating) return;

                _isUpdating = true;
                scriptEditorViewModel.Argument2.SelectedItem = (IDisplayItem)Argument2CB.SelectedItem;
                scriptEditorViewModel.Argument2Value = Argument2CB.SelectedValue;

                _isUpdating = false;
            };

            Argument2CB.TextChanged += (s, e) =>
            {
                if (_isUpdating) return;

                _isUpdating = true;
                scriptEditorViewModel.Argument2.InputText = Argument2CB.Text;

                _isUpdating = false;
            };
        }

        private void BindInstructionTriggerControls()
        {
            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(scriptEditorViewModel.TriggerDefinition))
                {
                    RoundTriggerTB.Text = scriptEditorViewModel.TriggerDefinition.RoundTriggerText;
                    CoinTriggerTB.Text = scriptEditorViewModel.TriggerDefinition.CoinTriggerText;
                }
            };
            RoundTriggerTB.TextChanged += (s, e) =>
            {
                scriptEditorViewModel.TriggerDefinition.RoundTriggerInputText = RoundTriggerTB.Text;
            };
            CoinTriggerTB.TextChanged += (s, e) =>
            {
                scriptEditorViewModel.TriggerDefinition.CoinTriggerInputText = CoinTriggerTB.Text;
            };
        }

        private void BindInstructionCoordinateControls()
        {
            CoordsChosingBT.DataBindings.Add("Text", scriptEditorViewModel, "CoordinateButtonText");
            CoordsChosingBT.Click += (s, e) => scriptEditorViewModel.ToggleCoordinateCommand.Execute(null);
            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(scriptEditorViewModel.CoordinateDefinition))
                {
                    CoordsXTB.Visible = scriptEditorViewModel.CoordinateDefinition.IsVisible;
                    CoordsXLB.Visible = scriptEditorViewModel.CoordinateDefinition.IsVisible;
                    CoordsYTB.Visible = scriptEditorViewModel.CoordinateDefinition.IsVisible;
                    CoordsYLB.Visible = scriptEditorViewModel.CoordinateDefinition.IsVisible;
                    CoordsChosingBT.Visible = scriptEditorViewModel.CoordinateDefinition.IsVisible;
                    CoordsXTB.Text = scriptEditorViewModel.CoordinateDefinition.XText;
                    CoordsYTB.Text = scriptEditorViewModel.CoordinateDefinition.YText;
                }
            };

            CoordsXTB.TextChanged += (s, e) =>
            {
                scriptEditorViewModel.CoordinateDefinition.XInputText = CoordsXTB.Text;
            };
            CoordsYTB.TextChanged += (s, e) =>
            {
                scriptEditorViewModel.CoordinateDefinition.YInputText = CoordsYTB.Text;
            };
        }

        private void BindInstructionsViewListBox()
        {
            InstructionsViewLB.DataSource = scriptEditorViewModel.Instructions;
            InstructionsViewLB.DisplayMember = "InstructionString";

            bool _updatingSelection = false;

            InstructionsViewLB.SelectedIndexChanged += (s, e) =>
            {
                if (_updatingSelection) return;

                _updatingSelection = true;
                var selectedIndices = InstructionsViewLB.SelectedIndices.Cast<int>().ToList();
                scriptEditorViewModel.SelectedInstructionIndices = new List<int>(selectedIndices);
                _updatingSelection = false;
            };

            scriptEditorViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(scriptEditorViewModel.SelectedInstructionIndices))
                {
                    if (_updatingSelection) return;

                    _updatingSelection = true;
                    InstructionsViewLB.InvokeIfRequired(() =>
                    {
                        var selectedIndices = scriptEditorViewModel.SelectedInstructionIndices.ToList();
                        InstructionsViewLB.ClearSelected();
                        foreach (int index in selectedIndices)
                        {
                            if (index >= 0 && index < InstructionsViewLB.Items.Count)
                            {
                                InstructionsViewLB.SetSelected(index, true);
                            }
                        }
                    });
                    _updatingSelection = false;
                }
                if (e.PropertyName == nameof(scriptEditorViewModel.Instructions))
                {
                    if (_updatingSelection) return;

                    _updatingSelection = true;
                    InstructionsViewLB.DataSource = scriptEditorViewModel.Instructions;
                    InstructionsViewLB.DisplayMember = "InstructionString";

                    _updatingSelection = false;
                }
            };
        }

        private void BindAddInstructionButton()
        {
            AddInstructionBT.Click += (s, e) => scriptEditorViewModel.AddInstructionCommand.Execute(null);
        }

        private void BindInsertInstructionButton()
        {
            InsertInstructionBT.Click += (s, e) => scriptEditorViewModel.InsertInstructionCommand.Execute(null);
        }

        private void BindModifiyInstructionButton()
        {
            ModifyInstructionBT.DataBindings.Add("Enabled", scriptEditorViewModel, "IsModifyInstructionEnabled");
            ModifyInstructionBT.Click += (s, e) => scriptEditorViewModel.ModifyInstructionCommand.Execute(null);
        }

        private void BindRemoveInstructionButton()
        {
            RemoveInstructionBT.Click += (s, e) => scriptEditorViewModel.RemoveInstructionCommand.Execute(null);
        }

        private void BindClearInstructionButton()
        {
            ClearInstructionsBT.Click += (s, e) => scriptEditorViewModel.ClearInstructionCommand.Execute(null);
        }

        private void BindBuildInstructionsButton()
        {
            BuildInstructionsBT.Click += (s, e) => scriptEditorViewModel.BuildInstructionsCommand.Execute(null);
        }

        private void BindInstructionUpButton()
        {
            UpBT.Click += (s, e) => scriptEditorViewModel.InstructionUpCommand.Execute(null);
        }

        private void BindInstructionDownButton()
        {
            DownBT.Click += (s, e) => scriptEditorViewModel.InstructionDownCommand.Execute(null);
        }

        private void BindSaveInstructionsButton()
        {
            SaveInstructionsBT.Click += (s, e) =>
            {
                // TODO: 异常处理
                scriptEditorViewModel.SaveInstructionsCommand.Execute(IsStartPageEditButtonClicked);
                IsStartPageEditButtonClicked = false;
                string filePath = scriptEditerScriptService.GetScriptPath();
                SelectPath(Path.GetDirectoryName(filePath));
            };
        }

        private void InstructionsViewTL_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }
            e.DrawBackground();
            e.DrawFocusRectangle();
            StringFormat strFmt = new System.Drawing.StringFormat
            {
                Alignment = StringAlignment.Center, //文本垂直居中
                LineAlignment = StringAlignment.Center //文本水平居中
            };
            e.Graphics.DrawString(InstructionsViewLB.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds, strFmt);
            return;
        }

    }
}
