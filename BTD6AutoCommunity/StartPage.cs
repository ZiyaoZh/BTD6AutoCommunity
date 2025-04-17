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
using static BTD6AutoCommunity.Constants;

namespace BTD6AutoCommunity
{
    public partial class BTD6AutoCommunity
    {
        private ScriptEditorSuite ExecuteInstructions;
        public int dpi;

        CustomStrategy customStrategy;
        CollectionStrategy collectionStrategy;
        CirculationStrategy circulationStrategy;
        EventsStrategy eventsStrategy;
        RaceStrategy raceStrategy;
        //BlackBorderStrategy blackBorderStrategy;

        private LogHandler logHandler;
        private readonly object _logLock = new object();

        private void InitializeStartPage()
        {
            BindSelectedFunction();
            BindSelectedMap();
            BindSelectedDifficulty();
        }

        private void BindSelectedFunction()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", typeof(FunctionTypes));
            dt.Columns.Add("FunctionName", typeof(string));
            foreach (FunctionTypes item in Enum.GetValues(typeof(FunctionTypes)))
            {
                DataRow dr = dt.NewRow();
                dr["Value"] = item;
                dr["FunctionName"] = GetTypeName(item);
                dt.Rows.Add(dr);
            }
            ExecuteModeCB.DataSource = dt;
            ExecuteModeCB.ValueMember = "Value";
            ExecuteModeCB.DisplayMember = "FunctionName";
            ExecuteModeCB.SelectedValueChanged += ExecuteModeCB_SelectedValueChanged;

        }
        private void BindSelectedDifficulty()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", typeof(LevelDifficulties));
            dt.Columns.Add("DifficultyName", typeof(string));
            foreach (LevelDifficulties item in Enum.GetValues(typeof(LevelDifficulties)))
            {
                if (item == LevelDifficulties.Any) continue;
                DataRow dr = dt.NewRow();
                dr["Value"] = item;
                dr["DifficultyName"] = GetTypeName(item);
                dt.Rows.Add(dr);
            }
            ExecuteDifficultyCB.DataSource = dt;
            ExecuteDifficultyCB.ValueMember = "Value";
            ExecuteDifficultyCB.DisplayMember = "DifficultyName";
        }

        private void BindSelectedMap()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", typeof(Maps));
            dt.Columns.Add("MapName", typeof(string));
            foreach (Maps item in MapsToDisplay)
            {
                DataRow dr = dt.NewRow();
                dr["Value"] = item;
                dr["MapName"] = GetTypeName(item);
                dt.Rows.Add(dr);
            }
            ExecuteMapCB.DataSource = dt;
            ExecuteMapCB.ValueMember = "Value";
            ExecuteMapCB.DisplayMember = "MapName";
        }

        private void BindPreviewLB(List<string> lst)
        {
            BindingSource bs = new BindingSource
            {
                DataSource = lst
            };
            PreviewLB.DataSource = bs;
        }

        private void StartProgram_Click(object sender, EventArgs e)
        {
            UserSelection userSelection = new UserSelection()
            {
                selectedFunction = (FunctionTypes)ExecuteModeCB.SelectedValue,
                selectedMap = (Maps)ExecuteMapCB.SelectedValue,
                selectedDifficulty = (LevelDifficulties)ExecuteDifficultyCB.SelectedValue,
                selectedScript = ExecuteScriptCB.Text,
                selectedIndex = PreviewLB.SelectedIndex,
            };

            if (StartProgramBT.Text == "启动")
            {
                switch (userSelection.selectedFunction)
                { 
                    case FunctionTypes.Custom:
                        RunCustomMode(userSelection);
                        break;
                    case FunctionTypes.Collection:
                        RunCollectionMode();
                        break;
                    case FunctionTypes.Circulation:
                        RunCirculationMode(userSelection);
                        break;
                    case FunctionTypes.Race:
                        RunFasterMode(userSelection);
                        break;
                    case FunctionTypes.BlackBorder:
                        MessageBox.Show("敬请期待！");
                        RunBlackBorderMode();
                        break;
                    case FunctionTypes.Events:
                        RunEventsMode(userSelection);
                        break;
                }
            }
            else
            {
                customStrategy?.Stop();
                customStrategy = null;
                collectionStrategy?.Stop();
                collectionStrategy = null;
                circulationStrategy?.Stop();
                circulationStrategy = null;
                eventsStrategy?.Stop();
                eventsStrategy = null;
                raceStrategy?.Stop();
                raceStrategy = null;
                //blackBorderStrategy?.Stop();
                //blackBorderStrategy = null;
                ReleaseAllKeys();
                StartProgramBT.Text = "启动";
            }
        }

        private void RunCustomMode(UserSelection userSelection)
        {
            logHandler = new LogHandler() { EnableInfoLog = scriptSettings.EnableLogging };
            logHandler.OnLogMessage += HandleLogMessage;
            customStrategy = new CustomStrategy(scriptSettings, logHandler, userSelection);
            customStrategy.OnStopTriggered += HandleStopEvent;
            customStrategy.OnGameDataUpdated += HandleGameDataUpdated;
            customStrategy.OnCurrentStrategyCompleted += HandleCurrentStrategyCompleted;

            if (customStrategy.ReadyToStart)
            {
                StartProgramBT.Text = "终止";
                customStrategy.Start();
            }
        }

        private void RunCollectionMode()
        {
            logHandler = new LogHandler(){ EnableInfoLog = scriptSettings.EnableLogging};
            logHandler.OnLogMessage += HandleLogMessage;

            collectionStrategy = new CollectionStrategy(scriptSettings, logHandler);
            collectionStrategy.OnStopTriggered += HandleStopEvent;
            collectionStrategy.OnGameDataUpdated += HandleGameDataUpdated;
            collectionStrategy.OnCurrentStrategyCompleted += HandleCurrentStrategyCompleted;
            collectionStrategy.OnScriptLoaded += HandleScriptLoaded;

            if (collectionStrategy.ReadyToStart)
            {
                StartProgramBT.Text = "终止";
                collectionStrategy.Start();
            }
        }

        private void RunCirculationMode(UserSelection userSelection)
        {
            logHandler = new LogHandler() { EnableInfoLog = scriptSettings.EnableLogging };
            logHandler.OnLogMessage += HandleLogMessage;

            circulationStrategy = new CirculationStrategy(scriptSettings, logHandler, userSelection);
            circulationStrategy.OnStopTriggered += HandleStopEvent;
            circulationStrategy.OnGameDataUpdated += HandleGameDataUpdated;
            circulationStrategy.OnCurrentStrategyCompleted += HandleCurrentStrategyCompleted;

            if (circulationStrategy.ReadyToStart)
            {
                StartProgramBT.Text = "终止";
                circulationStrategy.Start();
            }
        }

        private void RunEventsMode(UserSelection userSelection)
        {
            logHandler = new LogHandler() { EnableInfoLog = scriptSettings.EnableLogging };
            logHandler.OnLogMessage += HandleLogMessage;

            eventsStrategy = new EventsStrategy(scriptSettings, logHandler, userSelection);
            eventsStrategy.OnStopTriggered += HandleStopEvent;
            eventsStrategy.OnGameDataUpdated += HandleGameDataUpdated;
            eventsStrategy.OnCurrentStrategyCompleted += HandleCurrentStrategyCompleted;

            if (eventsStrategy.ReadyToStart)
            {
                StartProgramBT.Text = "终止";
                eventsStrategy.Start();
            }
        }

        private void RunFasterMode(UserSelection userSelection)
        {
            logHandler = new LogHandler() { EnableInfoLog = scriptSettings.EnableLogging };
            logHandler.OnLogMessage += HandleLogMessage;

            raceStrategy = new RaceStrategy(scriptSettings, logHandler, userSelection);
            raceStrategy.OnStopTriggered += HandleStopEvent;
            raceStrategy.OnGameDataUpdated += HandleGameDataUpdated;
            raceStrategy.OnCurrentStrategyCompleted += HandleCurrentStrategyCompleted;

            if (raceStrategy.ReadyToStart)
            {
                StartProgramBT.Text = "终止";
                raceStrategy.Start();
            }
        }

        private void RunBlackBorderMode()
        {
            logHandler = new LogHandler() { EnableInfoLog = scriptSettings.EnableLogging };
            logHandler.OnLogMessage += HandleLogMessage;

        }
        private void HandleStopEvent()
        {
            if (StartProgramBT.InvokeRequired)
            {
                StartProgramBT.BeginInvoke(new Action(() =>
                {
                    //StartProgramBT.Enabled = false;
                    StartProgramBT.Text = "启动";
                }));
            }
            else
            {
                //StartProgramBT.Enabled = false;
                StartProgramBT.Text = "启动";
            }
        }

        private void HandleGameDataUpdated(List<string> gameData)
        {
            if (gameData[0] != "")
            {
                CurrentRoundLB.Invoke((MethodInvoker)delegate
                {
                    CurrentRoundLB.Text = gameData[0];
                });
            }
            if (gameData[1] != "")
            {
                CurrentGoldLB.Invoke((MethodInvoker)delegate
                {
                    CurrentGoldLB.Text = gameData[1];
                });
            }
            if (gameData[2] != "")
            {
                CurrentLifeLB.Invoke((MethodInvoker)delegate
                {
                    CurrentLifeLB.Text = gameData[2];
                });
            }
        }

        private void HandleCurrentStrategyCompleted(ScriptInstructionInfo instructionInfo)
        {
            CurrentInstructionLB.Invoke((MethodInvoker)delegate
            {
                CurrentInstructionLB.Text = "当前指令：" + instructionInfo.Content;
            });
            CurrentTriggerLB.Invoke((MethodInvoker)delegate
            {
                CurrentTriggerLB.Text = $"触发条件：第{instructionInfo.RoundTrigger}回合后 {instructionInfo.CashTrigger}金币";
            });
            PreviewLB.Invoke((MethodInvoker)delegate
            {
                PreviewLB.SelectedIndex = instructionInfo.Index;
            });
        }

        private void HandleScriptLoaded(ScriptEditorSuite scriptEditorSuite)
        {
            PreviewLB.Invoke((MethodInvoker)delegate
            {
                BindingSource bs = new BindingSource
                {
                    DataSource = scriptEditorSuite.Displayinstructions
                };
                PreviewLB.DataSource = bs;
            });
            ExecuteMapCB.Invoke((MethodInvoker)delegate
            {
                ExecuteMapCB.SelectedValue = scriptEditorSuite.SelectedMap;
            });
            ExecuteDifficultyCB.Invoke((MethodInvoker)delegate
            {
                ExecuteDifficultyCB.SelectedValue = scriptEditorSuite.SelectedDifficulty;
                ExecuteDifficultyCB_SelectedIndexChanged(ExecuteDifficultyCB, EventArgs.Empty);
            });
            ExecuteScriptCB.Invoke((MethodInvoker)delegate
            {
                ExecuteScriptCB.SelectedIndex = ExecuteScriptCB.FindString(scriptEditorSuite.ScriptName);
            });
        }

        private void HandleLogMessage(string msg, LogLevel level)
        {
            lock (_logLock)
            {
                var color = Color.Black;
                switch (level)
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

                if (logsRTB.InvokeRequired)
                {
                    logsRTB.Invoke(new Action(() =>
                    {
                        // 删除多余行数
                        if (logsRTB.Lines.Length >= 1000)
                        {
                            logsRTB.Text = string.Join("\n",
                                logsRTB.Lines.Skip(logsRTB.Lines.Length - 1000));
                        }
                        logsRTB.SelectionColor = color;
                        logsRTB.AppendText(msg + "\n");
                        logsRTB.ScrollToCaret();
                    }));
                }
                else
                {                // 删除多余行数
                    if (logsRTB.Lines.Length >= 1000)
                    {
                        logsRTB.Text = string.Join("\n",
                            logsRTB.Lines.Skip(logsRTB.Lines.Length - 1000));
                    }
                    logsRTB.SelectionColor = color;
                    logsRTB.AppendText(msg + "\n");
                    logsRTB.ScrollToCaret();
                }
            }
        }


        private void ExecuteDifficultyCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectDir1 = ExecuteMapCB.Text;
            string selectDir2 = ExecuteDifficultyCB.Text;
            //MessageBox.Show(selectDir1 + selectDir2);
            string fullPath = Path.GetFullPath(Path.Combine("data", "我的脚本", selectDir1, selectDir2));
            if (Directory.Exists(fullPath))
            {
                var files = Directory.GetFiles(fullPath).Select(file => Path.GetFileNameWithoutExtension(file)).ToList();
                ExecuteScriptCB.DataSource = new BindingSource(files, null);
            }
        }

        private void ExecuteScriptCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectDir1 = ExecuteMapCB.Text;
            string selectDir2 = ExecuteDifficultyCB.Text;
            string scriptName = ExecuteScriptCB.Text;
            string filePath1 = Path.GetFullPath(Path.Combine("data", "我的脚本", selectDir1, selectDir2, scriptName + ".btd6"));
            string filePath2 = Path.GetFullPath(Path.Combine("data", "我的脚本", selectDir1, selectDir2, scriptName + ".json"));

            if (File.Exists(filePath1) || File.Exists(filePath2))
            {
                string filePath = File.Exists(filePath1) ? filePath1 : filePath2;
                string jsonString = File.ReadAllText(filePath);
                ExecuteInstructions = JsonConvert.DeserializeObject<ScriptEditorSuite>(jsonString);
                BindPreviewLB(ExecuteInstructions.Displayinstructions);
            }
        }

        private void ExecuteModeCB_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!Enum.IsDefined(typeof(FunctionTypes), ExecuteModeCB.SelectedValue)) return;
            if ((FunctionTypes)ExecuteModeCB.SelectedValue == FunctionTypes.Collection ||
                (FunctionTypes)ExecuteModeCB.SelectedValue == FunctionTypes.BlackBorder)
            {
                ExecuteMapCB.Enabled = false;
                ExecuteDifficultyCB.Enabled = false;
                ExecuteScriptCB.Enabled = false;
            }
            else
            {
                ExecuteMapCB.Enabled = true;
                ExecuteDifficultyCB.Enabled = true;
                ExecuteScriptCB.Enabled = true;
            }
        }

        private void PreviewLB_DragDrop(object sender, DragEventArgs e)
        {
            
        }

        private void ImportScriptBT_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string sourceFilePath = openFileDialog.FileName;
                    if (Path.GetExtension(sourceFilePath) != ".btd6")
                    {
                        MessageBox.Show("脚本格式错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    ScriptEditorSuite scriptEditorSuite = new ScriptEditorSuite();
                    string jsonString = File.ReadAllText(sourceFilePath);
                    try
                    {
                        scriptEditorSuite = JsonConvert.DeserializeObject<ScriptEditorSuite>(jsonString);
                        scriptEditorSuite.ScriptName = Path.GetFileNameWithoutExtension(sourceFilePath);
                        scriptEditorSuite.RepairScript();
                    }
                    catch
                    {
                        MessageBox.Show("脚本内容错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    //File.Copy(sourceFilePath, destinationFilePath, true);
                    ExecuteMapCB.SelectedValue = scriptEditorSuite.SelectedMap;
                    ExecuteDifficultyCB.SelectedValue = scriptEditorSuite.SelectedDifficulty;
                    ExecuteDifficultyCB_SelectedIndexChanged(ExecuteDifficultyCB, EventArgs.Empty);
                    ExecuteScriptCB.SelectedIndex = ExecuteScriptCB.FindString(scriptEditorSuite.ScriptName);
                }
            }
        }
    }

    public enum FunctionTypes
    { 
        Custom = 0,
        Collection = 1,
        Circulation = 2,
        Race = 3,
        BlackBorder = 4,
        Events = 5
    }

    public struct UserSelection
    {
        public FunctionTypes selectedFunction;
        public Maps selectedMap;
        public LevelDifficulties selectedDifficulty;
        public string selectedScript;
        public int selectedIndex;
    }
}
