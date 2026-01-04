using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine;
using BTD6AutoCommunity.GameObjects;
using BTD6AutoCommunity.ViewModels;
using BTD6AutoCommunity.Models;
using BTD6AutoCommunity.Services.Interfaces;

namespace BTD6AutoCommunity.Views.Main
{
    public partial class BTD6AutoUI
    {
        private StartViewModel startViewModel;

        private void InitializeStartPage()
        {
            IScriptService startPageScriptService = new ScriptService();
            startViewModel = new StartViewModel(startPageScriptService, messageBoxService);

            BindFunctionComboBox();
            BindMapComboBox();
            BindDifficultyComboBox();
            BindScriptComboBox();
            BindPreviewListBox();
            BindStartProgramButton();
            BindPauseButton();
            BindImportScriptButton();
            BindOutputScriptButton();
            BindEditScriptButton();
            BindCurrentRoundLabel();
            BindCurrentGoldLabel();
            BindCurrentLifeLabel();
            BindCurrentInstructionRichTextBox();
            BindCurrentTriggerRichTextBox();
            BindLogsRichTextBox();

        }

        private void BindFunctionComboBox()
        {
            // 绑定 ComboBox 数据源
            ExecuteModeCB.DataSource = startViewModel.FunctionOptions;
            ExecuteModeCB.DisplayMember = "Name";
            ExecuteModeCB.ValueMember = "Value";

            // 初始选中
            ExecuteModeCB.SelectedItem = startViewModel.SelectedFunction;

            // UI → VM
            ExecuteModeCB.SelectedIndexChanged += (s, e) =>
            {
                if (ExecuteModeCB.SelectedItem is FunctionDisplayItem item)
                    startViewModel.SelectedFunction = item;
            };
        }

        private void BindMapComboBox()
        {
            ExecuteMapCB.DataBindings.Add("Enabled", startViewModel, "ExecuteMapEnabled");
            ExecuteMapCB.DataSource = startViewModel.MapOptions;
            ExecuteMapCB.DisplayMember = "Name";
            ExecuteMapCB.ValueMember = "Value";

            ExecuteMapCB.SelectedItem = startViewModel.SelectedMap;

            ExecuteMapCB.SelectedIndexChanged += (s, e) =>
            {
                if (ExecuteMapCB.SelectedItem is MapDisplayItem map)
                {
                    startViewModel.SelectedMap = map;
                }
            };

            startViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(startViewModel.SelectedMap))
                {
                    ExecuteMapCB.InvokeIfRequired(() =>
                    {
                        if (!Equals(ExecuteMapCB.SelectedItem, startViewModel.SelectedMap))
                        {
                            ExecuteMapCB.SelectedItem = startViewModel.SelectedMap;
                        }
                    });
                }
            };
        }

        private void BindDifficultyComboBox()
        {
            ExecuteDifficultyCB.DataBindings.Add("Enabled", startViewModel, "ExecuteDifficultyEnabled");
            // 绑定难度下拉框
            ExecuteDifficultyCB.DisplayMember = "Name";
            ExecuteDifficultyCB.ValueMember = "Value";
            ExecuteDifficultyCB.DataSource = startViewModel.DifficultyOptions;

            // 初始同步选中项
            ExecuteDifficultyCB.SelectedItem = startViewModel.SelectedDifficulty;

            // UI → ViewModel
            ExecuteDifficultyCB.SelectedIndexChanged += (s, e) =>
            {
                if (ExecuteDifficultyCB.SelectedItem is DifficultyDisplayItem item)
                {
                    startViewModel.SelectedDifficulty = item;
                }
            };

            // ViewModel → UI（绑定脚本列表）
            startViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(startViewModel.SelectedDifficulty))
                {
                    ExecuteDifficultyCB.InvokeIfRequired(() =>
                    {
                        if (!Equals(ExecuteDifficultyCB.SelectedItem, startViewModel.SelectedDifficulty))
                        {
                            ExecuteDifficultyCB.SelectedItem = startViewModel.SelectedDifficulty;
                        }
                    });
                }
            };
        }

        private void BindScriptComboBox()
        {
            ExecuteScriptCB.DataSource = new BindingSource(startViewModel.ScriptList, null);
            ExecuteScriptCB.DataBindings.Add("Enabled", startViewModel, "ExecuteScriptEnabled");
            // 绑定脚本下拉项
            startViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(startViewModel.ScriptList))
                {
                    ExecuteScriptCB.InvokeIfRequired(() =>
                    {
                        ExecuteScriptCB.DataSource = new BindingSource(startViewModel.ScriptList, null);
                    });
                }
                if (e.PropertyName == nameof(startViewModel.SelectedScript))
                {
                    ExecuteScriptCB.InvokeIfRequired(() =>
                    {
                        ExecuteScriptCB.SelectedItem = startViewModel.SelectedScript;
                    });
                }
            };

            // 用户选择脚本时 → VM
            ExecuteScriptCB.SelectedIndexChanged += (s, e) =>
            {
                startViewModel.SelectedScript = ExecuteScriptCB.Text;
            };
        }

        private void BindPreviewListBox()
        {
            PreviewLB.SelectedIndexChanged += (s, e) =>
            {
                startViewModel.SelectedPreviewIndex = PreviewLB.SelectedIndex;
            };

            startViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(startViewModel.PreviewInstructions))
                {
                    PreviewLB.InvokeIfRequired(() =>
                    {
                        PreviewLB.DataSource = new BindingSource(startViewModel.PreviewInstructions, null);
                    });
                }
                if (e.PropertyName == nameof(startViewModel.SelectedPreviewIndex))
                {
                    PreviewLB.InvokeIfRequired(() =>
                    {
                        PreviewLB.SelectedIndex = startViewModel.SelectedPreviewIndex;
                    });
                }
            };
        }

        private void BindCurrentRoundLabel()
        {
            startViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(startViewModel.CurrentRound))
                {
                    CurrentRoundLB.InvokeIfRequired(() =>
                    {
                        CurrentRoundLB.Text = startViewModel.CurrentRound;
                    });
                }
            };
        }

        private void BindCurrentGoldLabel()
        {
            startViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(startViewModel.CurrentGold))
                {
                    CurrentGoldLB.InvokeIfRequired(() =>
                    {
                        CurrentGoldLB.Text = startViewModel.CurrentGold;
                    });
                }
            };
        }

        private void BindCurrentLifeLabel()
        {
            startViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(startViewModel.CurrentLife))
                {
                    CurrentLifeLB.InvokeIfRequired(() =>
                    {
                        CurrentLifeLB.Text = startViewModel.CurrentLife;
                    });
                }
            };
        }

        private void BindCurrentInstructionRichTextBox()
        {
            startViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(startViewModel.CurrentInstructionText))
                {
                     CurrentInstructionRTB.InvokeIfRequired(() =>
                    {
                        CurrentInstructionRTB.Text = startViewModel.CurrentInstructionText;
                    });
                }
            };
        }

        private void BindCurrentTriggerRichTextBox()
        {
            startViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(startViewModel.CurrentTriggerText))
                {
                    CurrentTriggerRTB.InvokeIfRequired(() =>
                    {
                        CurrentTriggerRTB.Text = startViewModel.CurrentTriggerText;
                    });
                }
            };
        }

        private void BindLogsRichTextBox()
        {
            startViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(startViewModel.CurrentLog))
                {
                    logsRTB.InvokeIfRequired(() =>
                    {
                        var color = Color.Black;
                        switch (startViewModel.CurrentLog.Level)
                        {
                            case LogLevel.Warning:
                                color = Color.Blue;
                                break;
                            case LogLevel.Error:
                                color = Color.Red;
                                break;
                            case LogLevel.Info:
                                color = Color.Gray;
                                break;
                        }

                        // 当行数超过1000时，移除顶部的行
                        if (logsRTB.Lines.Length >= 1000)
                        {
                            logsRTB.SelectionStart = 0;
                            logsRTB.SelectionLength = logsRTB.GetFirstCharIndexFromLine(1);
                            logsRTB.SelectedText = "";
                        }

                        // 将光标移到末尾并以指定颜色追加新日志
                        logsRTB.SelectionStart = logsRTB.TextLength;
                        logsRTB.SelectionLength = 0;
                        logsRTB.SelectionColor = color;
                        logsRTB.AppendText(startViewModel.CurrentLog.Message + "\n");
                        logsRTB.ScrollToCaret();
                    });
                }
            };
        }

        private void BindStartProgramButton()
        {
            StartProgramBT.DataBindings.Add("Text", startViewModel, "StartButtonText");
            StartProgramBT.Click += (s, e) => startViewModel.StartOrStopCommand.Execute(null);
        }

        private void BindPauseButton()
        {
            PauseBT.DataBindings.Add("Enabled", startViewModel, "PauseButtonEnabled");
            PauseBT.DataBindings.Add("Text", startViewModel, "PauseButtonText");
            PauseBT.Click += (s, e) => startViewModel.PauseOrResumeCommand.Execute(null);
        }

        private void BindImportScriptButton()
        {
            ImportScriptBT.Click += (s, e) => startViewModel.ImportScriptCommand.Execute(null);
        }

        private void BindOutputScriptButton()
        {
            OutputScriptBT.Click += (s, e) => startViewModel.OutputScriptCommand.Execute(null);
        }

        private void BindEditScriptButton()
        {
            EditScriptBT.Click += (s, e) =>
            {
                IsStartPageEditButtonClicked = true;
                scriptEditorViewModel.EditScriptCommand.Execute(startViewModel.ScriptFullPath);
            };
        }
    }
}
