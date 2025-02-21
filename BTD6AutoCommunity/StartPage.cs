using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace BTD6AutoCommunity
{
    public partial class BTD6AutoCommunity
    {
        private InstructionsClass ExecuteInstructions;
        private GetGameData gameData;
        private ExecuteDirectiveClass execute;
        public int dpi;
        private int executeCount;
        private int currentExecuteMode;
        private int startMode;
        private int circleTimes;
        private bool firstStart;
        private bool timerFlag;
        private List<(string, string)> collectionScripts;

        private void InitializeStartPage()
        {
            BindSelectedMap();
            BindSelectedDifficulty();
            timerFlag = false;
            currentExecuteMode = -1;

            collectionScripts = new List<(string, string)>();
            firstStart = true;
        }
        private void BindSelectedDifficulty()
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("difficultyName");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            foreach (var item in Constants.Difficulty)
            {
                DataRow dr = dt.NewRow();
                dr["id"] = item.Key;
                dr["difficultyName"] = item.Value;
                dt.Rows.Add(dr);
            }
            ExecuteDifficultyCB.DataSource = dt;
            ExecuteDifficultyCB.ValueMember = "id";
            ExecuteDifficultyCB.DisplayMember = "difficultyName";
        }

        private void BindSelectedMap()
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("mapName");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            foreach (var item in Constants.Maps)
            {
                DataRow dr = dt.NewRow();
                dr["id"] = item.Key;
                dr["mapName"] = item.Value;
                dt.Rows.Add(dr);
            }
            ExecuteMapCB.DataSource = dt;
            ExecuteMapCB.ValueMember = "id";
            ExecuteMapCB.DisplayMember = "mapName";
        }

        private void BindPreviewLB(List<string> lst)
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = lst;
            PreviewLB.DataSource = bs;
        }

        private void StartProgram_Click(object sender, EventArgs e)
        {
            if (firstStart)
            {
                MessageBox.Show($"请确认打开游戏，并且游戏分辨为{GameDpiCB.SelectedItem}");
                firstStart = false;
            }

            if (StartProgramBT.Text == "启动")
            {
                currentExecuteMode = ExecuteModeCB.SelectedIndex;
                if (currentExecuteMode == 0)
                {
                    string selectDir1 = ExecuteMapCB.Text;
                    string selectDir2 = ExecuteDifficultyCB.Text;
                    string scriptName = ExecuteScriptCB.Text;
                    RunCustomMode(selectDir1, selectDir2, scriptName);
                }
                else if (currentExecuteMode == 1)
                {
                    int times = (int)ExecuteTimesUD.Value;
                    RunCollectionMode(times);
                }
                else if (currentExecuteMode == 2)
                {
                    int times = (int)ExecuteTimesUD.Value;
                    string selectDir1 = ExecuteMapCB.Text;
                    string selectDir2 = ExecuteDifficultyCB.Text;
                    string scriptName = ExecuteScriptCB.Text;
                    RunCircleMode(selectDir1, selectDir2, scriptName, times);
                }
                else if (currentExecuteMode == 3)
                {
                    string selectDir1 = ExecuteMapCB.Text;
                    string selectDir2 = ExecuteDifficultyCB.Text;
                    string scriptName = ExecuteScriptCB.Text;
                    RunFasterMode(selectDir1, selectDir2, scriptName);
                }
                else
                {
                    MessageBox.Show("请选择功能！");
                }
            }
            else
            {
                timerFlag = false;
                GetGameDataTM.Stop();
                ExecuteTM.Stop();
                ExecuteCircleTM.Stop();
                ExecuteCollectionTM.Stop();
                ExecuteFasterTM.Stop();
                StartProgramBT.Text = "启动";
            }
        }

        private string ExistScript(string selectDir1, string selectDir2, string scriptName, int mode)
        {
            string filePath1 = Path.GetFullPath(Path.Combine("data", "我的脚本", selectDir1, selectDir2, scriptName + ".btd6"));
            string filePath2 = Path.GetFullPath(Path.Combine("data", "我的脚本", selectDir1, selectDir2, scriptName + ".json"));

            if (File.Exists(filePath1) || File.Exists(filePath2))
            {
                return File.Exists(filePath1) ? filePath1 : filePath2;
            }
            else
            {
                if (mode == 0)
                {
                    GetGameDataTM.Stop();
                    ExecuteTM.Stop();
                    ExecuteCircleTM.Stop();
                    ExecuteCollectionTM.Stop();
                    ExecuteFasterTM.Stop();

                    MessageBox.Show("脚本不存在！");
                }
                return null;
            }
        }

        private InstructionsClass LoadInstruction(string filePath)
        {
            try
            {
                ExecuteInstructions = new InstructionsClass();
                string jsonString = File.ReadAllText(filePath);

                ExecuteInstructions = JsonConvert.DeserializeObject<InstructionsClass>(jsonString);
                ExecuteInstructions.BluidObjectList();
                return ExecuteInstructions;

            }
            catch
            {
                GetGameDataTM.Stop();
                ExecuteTM.Stop();
                ExecuteCircleTM.Stop();
                ExecuteCollectionTM.Stop();
                MessageBox.Show("脚本无法运行！");

                return null;
            }
        }

        private bool ExecuteInstruction()
        {
            ExecuteInstructions.Compile(GameDpiCB.SelectedIndex, (int)ExecuteTM.Interval, IfFastCB.Checked);
            ExecuteInstructions.SaveDirectiveToJson();
            execute.LoadDirective(ExecuteInstructions.compilerDirective);
            if (PreviewLB.SelectedIndex >= 0)
            {
                execute.currentIndex = PreviewLB.SelectedIndex;
            }
            BindPreviewLB(ExecuteInstructions.displayinstructions);

            StartProgramBT.Text = "终止";
            executeCount = 0;
            StartPrgramTC.SelectedIndex = 0;

            GetStartMode();
            GetGameDataTM.Interval = mySet.GetGameDataInterval;
            ExecuteTM.Interval = mySet.ExecuteInterval;

            IntPtr hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6");
            if (hWnd == IntPtr.Zero)
            {
                hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6-Epic");
                if (hWnd == IntPtr.Zero)
                {
                    MessageBox.Show("未找到游戏窗口");
                    return false;
                }
            }
            dpi = DisPlayMouseCoordinates.GetDpiForWindow(hWnd);
            gameData = new GetGameData(dpi / 96F, GameDpiCB.SelectedIndex, hWnd, 2);
            execute.executeFlag = true;
            execute.stopFlag = false;
            GetGameDataTM.Start();
            ExecuteTM.Start();
            return true;

        }

        private bool RunCustomMode(string selectDir1, string selectDir2, string scriptName)
        {
            string filePath = ExistScript(selectDir1, selectDir2, scriptName, 0);
            if (filePath != null)
            {
                ExecuteInstructions = LoadInstruction(filePath);
                IntPtr hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6");
                if (hWnd == IntPtr.Zero)
                {
                    hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6-Epic");
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("未找到游戏窗口");
                        return false;
                    }
                }
                dpi = DisPlayMouseCoordinates.GetDpiForWindow(hWnd);
                execute = new ExecuteDirectiveClass(dpi / 96F, GameDpiCB.SelectedIndex, hWnd);

                if (ExecuteInstruction())
                {
                    return true;
                }
            }
            return false;
        }

        private void RunCollectionMode(int times)
        {
            List<string> filePaths = new List<string>
            {
                "冰河之径",
                "黑暗地下城",
                "避难所",
                "峡谷",
                "水淹山谷",
                "炼狱",
                "血腥水坑",
                "工坊",
                "方院",
                "黑暗城堡",
                "泥泞的水坑",
                "#哎哟"
            };
            List<string> difficulty = new List<string>
            {
                "简单",
                "中级",
                "困难"
            };
            string scriptName = "";
            if (FastPathCB.Checked)
            {
                scriptName = "快速路径收集";
            }
            else if (DoubleCoinCB.Checked)
            {
                scriptName = "双金收集";
            }
            else
            {
                scriptName = "简单收集";
            }
            collectionScripts.Clear();
            foreach (string path in filePaths)
            {
                foreach (string dif in difficulty)
                {
                    if (ExistScript(path, dif, scriptName, 1) != null)
                    {
                        collectionScripts.Add((path, dif));
                        break;
                    }
                }
            }
            if (collectionScripts.Count != 12)
            {
                MessageBox.Show("收集脚本不完整！");
                return;
            }

            IntPtr hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6");
            if (hWnd == IntPtr.Zero)
            {
                hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6-Epic");
                if (hWnd == IntPtr.Zero)
                {
                    MessageBox.Show("未找到游戏窗口");
                    return;
                }
            }
            dpi = DisPlayMouseCoordinates.GetDpiForWindow(hWnd);
            execute = new ExecuteDirectiveClass(dpi / 96F, GameDpiCB.SelectedIndex, hWnd);
            execute.LoadFindMapDirctive(FindMapDirective());
            execute.LoadRestartDirective(RestartDirective());

            GetStartMode();
            GetGameDataTM.Interval = mySet.GetGameDataInterval;
            ExecuteTM.Interval = mySet.ExecuteInterval;

            StartProgramBT.Text = "终止";
            circleTimes = times;
            executeCount = 0;
            execute.findMapFlag = true;
            ExecuteCollectionTM.Start();
        }

        private void RunCircleMode(string selectDir1, string selectDir2, string scriptName, int times)
        {
            string filePath = ExistScript(selectDir1, selectDir2, scriptName, 0);
            if (filePath != null)
            {
                IntPtr hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6");
                if (hWnd == IntPtr.Zero)
                {
                    hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6-Epic");
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("未找到游戏窗口");
                        return;
                    }
                }
                dpi = DisPlayMouseCoordinates.GetDpiForWindow(hWnd);
                execute = new ExecuteDirectiveClass(dpi / 96F, GameDpiCB.SelectedIndex, hWnd);

                GetStartMode();
                GetGameDataTM.Interval = mySet.GetGameDataInterval;
                ExecuteTM.Interval = mySet.ExecuteInterval;

                ExecuteInstructions = LoadInstruction(filePath);
                ExecuteInstructions.Compile(GameDpiCB.SelectedIndex, mySet.ExecuteInterval, IfFastCB.Checked);
                execute.LoadSelectMapDirective(SelectMapDirctive(ExecuteInstructions.selectedMap, ExecuteInstructions.selectedDifficulty, ExecuteInstructions.selectedMode));
                execute.LoadDirective(ExecuteInstructions.compilerDirective);
                execute.LoadCompleteDirctive(CompleteDirective());
                execute.LoadRestartDirective(RestartDirective());

                StartProgramBT.Text = "终止";
                circleTimes = times;
                executeCount = 0;
                execute.selectMapFlag = true;
                ExecuteCircleTM.Start();
            }
        }

        private void RunFasterMode(string selectDir1, string selectDir2, string scriptName)
        {
            string filePath = ExistScript(selectDir1, selectDir2, scriptName, 0);
            if (filePath != null)
            {
                IntPtr hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6");
                if (hWnd == IntPtr.Zero)
                {
                    hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6-Epic");
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("未找到游戏窗口");
                        return;
                    }
                }
                dpi = DisPlayMouseCoordinates.GetDpiForWindow(hWnd);
                execute = new ExecuteDirectiveClass(dpi / 96F, GameDpiCB.SelectedIndex, hWnd);

                GetStartMode();
                GetGameDataTM.Interval = mySet.GetGameDataInterval;
                ExecuteTM.Interval = mySet.ExecuteInterval;

                ExecuteInstructions = LoadInstruction(filePath);
                ExecuteInstructions.Compile(GameDpiCB.SelectedIndex, (int)ExecuteTM.Interval, IfFastCB.Checked);
                execute.LoadDirective(ExecuteInstructions.compilerDirective);
                execute.LoadRestartDirective(RestartDirective());

                StartProgramBT.Text = "终止";
                executeCount = 0;
                execute.executeFlag = true;
                ExecuteFasterTM.Start();
            }
        }

        private List<List<string>> SelectMapDirctive(int mapKey, int difficulty, int mode)
        {
            int mapKind = mapKey / 30;
            Dictionary<int, (int, int)> modeToCoords = new Dictionary<int, (int, int)>
            {
                {0, (630, 590) },
                {1, (1300, 450) },
                {2, (1300, 450) },
                {3, (960, 750) },
                {4, (1600, 450) },
                {5, (1300, 450) },
                {6, (960, 750) },
                {7, (1300, 750) },
                {8, (1600, 750) },
                { 9, (960, 450) },
                { 10, (960, 450) },
                { 11, (960, 450) },
            };
            List<List<string>> command = new List<List<string>>
            {
                new List<string> {"10" ,"10" ,"10" ,"10" ,"10" , "10" ,"10" , "10" ,"10" ,"10", "1 960 940" , "2", $"1 {590 + 250 * mapKind} 980", "2", $"16 {mapKey}" ,
                    $"1 {630 + 335 * difficulty} 400" , "2", $"1 {modeToCoords[mode].Item1} {modeToCoords[mode].Item2}" ,"2",
                    "1 1140 730" ,"2", "10" ,"10" ,"10" ,"10" ,"10" , "10" ,"10" , "10" ,"10" ,"10" ,"10" ,"10" ,"10" , "1 960 750", "2"},
            };
            return command;
        }

        private List<List<string>> SelectHeroDirctive(int heroKey, int difficulty, int mode)
        {
            Dictionary<int, (int, int)> modeToCoords = new Dictionary<int, (int, int)>
            {
                { 0, (630, 590) },
                { 1, (1300, 450) },
                { 2, (1300, 450) },
                { 3, (960, 750) },
                { 4, (1600, 450) },
                { 5, (1300, 450) },
                { 6, (960, 750) },
                { 7, (1300, 750) },
                { 8, (1600, 750) },
                { 9, (960, 450) },
                { 10, (960, 450) },
                { 11, (960, 450) },
            };
            List<List<string>> command = new List<List<string>>
            {
                new List<string> {"10" ,"10" , "1 540 650" , "2", $"1 {630 + 335 * difficulty} 400", "2" , "1 100 990", "2",
                    "1 255 515", $"19 {heroKey}", "1 1115 615" , "2", "1 80 55" , "2", "10",
                    $"1 {modeToCoords[mode].Item1} {modeToCoords[mode].Item2}" ,"2","1 1140 730" , "2", "10",
                     "10", "10" ,"10" ,"10" ,"10" , "10" ,"10" , "10" ,"10" ,"10" ,"10" ,"10" ,"10" , "1 960 750", "2"},
            };
            return command;
        }

        private List<List<string>> FindMapDirective()
        {

            List<List<string>> command = new List<List<string>>
            {
                new List<string> {"10" ,"10" ,"10" ,"10" ,"10" , "10" ,"10" , "10" , "1 960 940" , "2", "10", "1 80 165" , "2", "1 1350 35", "2",
                "18", "10", "10"},
            };
            return command;
        }

        private List<List<string>> CompleteDirective()
        {
            List<List<string>> completeDirctive = new List<List<string>>
            {
                new List<string> {"17", "1 960 910", "2", "1 720 850" , "2"}
            };
            return completeDirctive;
        }

        private List<List<string>> RestartDirective()
        {
            List<List<string>> restartDirective = new List<List<string>>
            {
                new List<string> { "7 16", "22", "10", "10", "10", "10", "10", "2", "1 1140 730" , "2"}
            };
            return restartDirective;
        }

        private void ExecuteCollectionTM_Tick(object sender, EventArgs e)
        {
            if (execute.currentMap != -1)
            {
                Dictionary<string, int> difToKey = new Dictionary<string, int>()
                {
                    { "简单", 0},
                    { "中级", 1},
                    { "困难", 2 }
                };
                int index = execute.currentMap - 90; // 专家图
                ExecuteMapCB.SelectedValue = execute.currentMap;
                ExecuteDifficultyCB.SelectedIndex = difToKey[collectionScripts[index].Item2];
                string scriptName;

                if (FastPathCB.Checked)
                {
                    ExecuteScriptCB.SelectedItem = "快速路径收集";
                    scriptName = "快速路径收集";
                }
                else if (DoubleCoinCB.Checked)
                {
                    ExecuteScriptCB.SelectedItem = "双金收集";
                    scriptName = "双金收集";
                }
                else
                {
                    ExecuteScriptCB.SelectedItem = "简单收集";
                    scriptName = "简单收集";
                }


                string filePath = ExistScript(collectionScripts[index].Item1, collectionScripts[index].Item2, scriptName, 1);
                ExecuteInstructions = LoadInstruction(filePath);
                ExecuteInstructions.Compile(GameDpiCB.SelectedIndex, mySet.ExecuteInterval, IfFastCB.Checked);
                execute.LoadSelectHeroDirctive(SelectHeroDirctive(ExecuteInstructions.selectedHero, ExecuteInstructions.selectedDifficulty, ExecuteInstructions.selectedMode));
                execute.LoadDirective(ExecuteInstructions.compilerDirective);
                execute.LoadCompleteDirctive(CompleteDirective());
                execute.currentMap = -1;
                execute.findMapFlag = false;
                execute.selectHeroFlag = true;
            }
            if (execute.findMapFlag == true)
            {
                execute.ExecuteFindMap();
            }
            if (execute.selectHeroFlag == true)
            {
                if (execute.heroUnlocked)
                {
                    StartProgramBT.PerformClick();
                    MessageBox.Show("英雄未解锁");
                }
                try
                {
                    execute.ExecuteSelectHero();
                }
                catch
                {
                    StartProgramBT.PerformClick();
                    MessageBox.Show("英雄未解锁");
                }
            }
            if (execute.executeFlag == true)
            {
                if (!GetGameDataTM.Enabled)
                {
                    IntPtr hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6");
                    if (hWnd == IntPtr.Zero)
                    {
                        hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6-Epic");
                        if (hWnd == IntPtr.Zero)
                        {
                            StartProgramBT.PerformClick();
                            MessageBox.Show("未找到游戏窗口");
                        }
                    }
                    dpi = DisPlayMouseCoordinates.GetDpiForWindow(hWnd);
                    gameData = new GetGameData(dpi / 96F, GameDpiCB.SelectedIndex, hWnd, 2);
                    GetGameDataTM.Interval = mySet.GetGameDataInterval;
                    GetGameDataTM.Start();
                }
                if (!ExecuteTM.Enabled)
                {
                    GetStartMode();
                    executeCount = 0;
                    ExecuteTM.Interval = mySet.ExecuteInterval;
                    timerFlag = false;
                    ExecuteTM.Start();
                }
            }
            if (execute.completeFlag == true)
            {
                execute.ExecuteCompleteDirective(1);
            }
            if (execute.restartFlag == true)
            {
                execute.ExecuteRestartDirective();
            }
            if (circleTimes == execute.circleTimes)
            {
                ExecuteCircleTM.Stop();
                StartProgramBT.PerformClick();
            }
        }

        private void ExecuteCircleTM_Tick(object sender, EventArgs e)
        {
            //File.AppendAllText(@"D:\log.txt", execute.selectMapFlag.ToString() + " " + execute.executeFlag.ToString() + " " + execute.completeFlag.ToString() + "\n");
            if (execute.selectMapFlag == true)
            {
                execute.ExecuteSelectMap();
            }
            if (execute.executeFlag == true)
            {
                if (!GetGameDataTM.Enabled)
                {
                    IntPtr hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6");
                    if (hWnd == IntPtr.Zero)
                    {
                        hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6-Epic");
                        if (hWnd == IntPtr.Zero)
                        {
                            MessageBox.Show("未找到游戏窗口");
                            StartProgramBT.PerformClick();
                        }
                    }
                    dpi = DisPlayMouseCoordinates.GetDpiForWindow(hWnd);
                    gameData = new GetGameData(dpi / 96F, GameDpiCB.SelectedIndex, hWnd, 2);
                    GetGameDataTM.Interval = mySet.GetGameDataInterval;
                    GetGameDataTM.Start();
                }
                if (!ExecuteTM.Enabled)
                {
                    GetStartMode();
                    executeCount = 0;
                    ExecuteTM.Interval = mySet.ExecuteInterval;
                    timerFlag = false;
                    ExecuteTM.Start();
                }
            }
            if (execute.completeFlag == true)
            {
                execute.ExecuteCompleteDirective(0);
            }
            if (execute.restartFlag == true)
            {
                execute.ExecuteRestartDirective();
            }
            if (circleTimes == execute.circleTimes)
            {
                ExecuteCircleTM.Stop();
                StartProgramBT.PerformClick();
            }
        }

        private void RunFasterTM_Tick(object sender, EventArgs e)
        {
            if (execute.executeFlag == true)
            {
                if (!GetGameDataTM.Enabled)
                {
                    IntPtr hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6");
                    if (hWnd == IntPtr.Zero)
                    {
                        hWnd = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6-Epic");
                        if (hWnd == IntPtr.Zero)
                        {
                            MessageBox.Show("未找到游戏窗口");
                            StartProgramBT.PerformClick();
                        }
                    }
                    dpi = DisPlayMouseCoordinates.GetDpiForWindow(hWnd);
                    gameData = new GetGameData(dpi / 96F, GameDpiCB.SelectedIndex, hWnd, 2);
                    GetGameDataTM.Interval = mySet.GetGameDataInterval;
                    GetGameDataTM.Start();
                }
                if (!ExecuteTM.Enabled)
                {
                    GetStartMode();
                    executeCount = 0;
                    ExecuteTM.Interval = mySet.ExecuteInterval;
                    timerFlag = false;
                    ExecuteTM.Start();
                }
            }
            if (execute.restartFlag == true)
            {
                execute.ExecuteRestartDirective();
            }
        }

        private void GetGameDataTM_Tick(object sender, EventArgs e)
        {
            List<string> CurrentGameData = gameData.GetCurrentGameData(execute.ifContinueGame);
            if (CurrentGameData[0] != "")
            {
                CurrentRoundLB.Text = CurrentGameData[0];
            }
            if (CurrentGameData[1] != "")
            {
                CurrentGoldLB.Text = CurrentGameData[1];
            }
            if (CurrentGameData[2] != "")
            {
                CurrentLifeLB.Text = CurrentGameData[2];
            }
        }

        private void ExecuteTM_Tick(object sender, ElapsedEventArgs e)
        {
            if (execute.stopFlag || !execute.executeFlag)
            {
                execute.stopFlag = false;
                GetGameDataTM.Stop();
                ExecuteTM.Stop();
                if (ExecuteModeCB.SelectedIndex == 0)
                {
                    StartProgramBT.PerformClick();
                }
                return;
            }
            if (executeCount * ExecuteTM.Interval > 5000)
            {
                if (!Int32.TryParse(CurrentRoundLB.Text, out int roundTrigger))
                {
                    roundTrigger = 0;
                }
                if (!Int32.TryParse(CurrentGoldLB.Text, out int coinTrigger))
                {
                    coinTrigger = 0;
                }

                int currentIndex = execute.currentIndex;
                if (currentIndex < ExecuteInstructions.digitalinstructions.Count)
                {
                    List<int> arguments = ExecuteInstructions.GetArguments(currentIndex);
                    if (!timerFlag || arguments[0] == 15) // 等待指令直接执行，不等待上一个timer结束
                    {
                        timerFlag = true;
                        switch (startMode)
                        {
                            case 0:
                                execute.ExecuteDirective(roundTrigger, coinTrigger);
                                break;
                            case 1:
                                if (arguments[0] == 0) // 放置猴子指令
                                {
                                    execute.DeployMonkey(roundTrigger, coinTrigger, CheckUpgradeCB.Checked, currentExecuteMode);
                                }
                                else if (arguments[0] == 1) // 升级指令
                                {
                                    execute.ExecuteDirective(0, 99999999, ExecuteInstructions.upgradeCount, CheckUpgradeCB.Checked, currentExecuteMode);
                                }
                                else if (arguments[0] == 7) // 放置英雄指令
                                {
                                    execute.ExecuteDirective(0, 99999999, 0, CheckUpgradeCB.Checked, currentExecuteMode);
                                }
                                else if (arguments[0] == 13) // 鼠标点击
                                {
                                    execute.ExecuteDirective(roundTrigger, coinTrigger, 2, false, currentExecuteMode);
                                }
                                else if (arguments[0] == 15) // 等待指令
                                {
                                    execute.ExecuteDirective(roundTrigger, coinTrigger, 1, false, currentExecuteMode);
                                }
                                else if (arguments[0] == 16) // 开始自由游戏
                                {
                                    execute.ExecuteDirective(roundTrigger, coinTrigger, 1, false, currentExecuteMode);
                                }
                                else if (arguments[0] == 17) // 结束自由游戏
                                {
                                    execute.ExecuteDirective(roundTrigger, coinTrigger, 1, false, currentExecuteMode);
                                }
                                else
                                {
                                    execute.ExecuteDirective(roundTrigger, coinTrigger, 1, CheckUpgradeCB.Checked, currentExecuteMode);
                                }
                                break;
                            case 2:
                                if (arguments[0] == 0) // 放置猴子指令
                                {
                                    execute.DeployMonkey(roundTrigger, coinTrigger, CheckUpgradeCB.Checked, currentExecuteMode);
                                }
                                else if (arguments[0] == 1) // 升级指令
                                {
                                    execute.ExecuteDirective(roundTrigger, coinTrigger, ExecuteInstructions.upgradeCount, CheckUpgradeCB.Checked, currentExecuteMode);
                                }
                                else if (arguments[0] == 13) // 鼠标点击
                                {
                                    execute.ExecuteDirective(roundTrigger, coinTrigger, 2, CheckUpgradeCB.Checked, currentExecuteMode);
                                }
                                else
                                {
                                    execute.ExecuteDirective(roundTrigger, coinTrigger, 1, CheckUpgradeCB.Checked, currentExecuteMode);
                                }
                                break;
                        }
                        ShowInstructionPreView(currentIndex);
                        timerFlag = false;
                    }

                }
                else
                {
                    execute.currentIndex = 0;
                    execute.executeFlag = false;
                    execute.completeFlag = true;
                    execute.stopFlag = true;
                }

            }
            executeCount++;
        }

        private int GetStartMode()
        {
            if (BoostCB.Checked)
            {
                startMode = 1;
            }
            else
            {
                if (CheckCB.Checked)
                {
                    startMode = 2;
                }
                else
                {
                    startMode = 0;
                }
            }
            return startMode;
        }

        private void ShowInstructionPreView(int currentIndex)
        {
            if (PreviewLB.InvokeRequired)
            {
                PreviewLB.Invoke(new Action(() => ShowInstructionPreView(currentIndex)));
                return;
            }
            if (currentIndex < PreviewLB.Items.Count)
            {
                PreviewLB.SelectedIndex = currentIndex;
            }
            if (currentIndex - 1 > 0)
            {
                LastInstructionLB.Text = "上一指令：" + ExecuteInstructions.displayinstructions[currentIndex - 1];
            }
            else
            {
                LastInstructionLB.Text = "上一指令：无";

            }
            if (currentIndex < ExecuteInstructions.displayinstructions.Count)
            {
                CurrentInstructionLB.Text = "当前指令：" + ExecuteInstructions.displayinstructions[currentIndex];
                int rT = execute.currentTrigger.Item1, cT = execute.currentTrigger.Item2;
                CurrentTriggerLB.Text = $"触发条件：第{rT}回合后 {cT}金币";

            }
            else
            {
                CurrentInstructionLB.Text = "当前指令：无";
            }
            if (currentIndex + 1 < ExecuteInstructions.displayinstructions.Count)
            {

                NextInstructionLB.Text = "下一指令：" + ExecuteInstructions.displayinstructions[currentIndex + 1];
            }
            else
            {
                NextInstructionLB.Text = "下一指令：无";

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
                ExecuteInstructions = JsonConvert.DeserializeObject<InstructionsClass>(jsonString);
                BindPreviewLB(ExecuteInstructions.displayinstructions);
            }
        }

        private void ExecuteModeCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ExecuteModeCB.SelectedIndex == 0)
            {
                CheckUpgradeCB.Enabled = true;
                BoostCB.Enabled = true;
                CheckCB.Enabled = true;
            }
            if (ExecuteModeCB.SelectedIndex == 1)
            {
                CheckUpgradeCB.Enabled = false;
                CheckUpgradeCB.Checked = true;
                BoostCB.Enabled = false;
                BoostCB.Checked = true;
                CheckCB.Enabled = true;
            }
            if (ExecuteModeCB.SelectedIndex == 2)
            {
                CheckUpgradeCB.Enabled = false;
                CheckUpgradeCB.Checked = true;
                BoostCB.Enabled = false;
                BoostCB.Checked = true;
                CheckCB.Enabled = true;
            }
            if (ExecuteModeCB.SelectedIndex == 3)
            {
                CheckUpgradeCB.Enabled = false;
                CheckUpgradeCB.Checked = true;
                BoostCB.Enabled = false;
                BoostCB.Checked = true;
                CheckCB.Enabled = true;
            }
        }
    }
}
