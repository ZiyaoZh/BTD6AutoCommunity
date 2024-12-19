using Newtonsoft.Json;
using OpenCvSharp.Dnn;
using OpenCvSharp.Internal.Vectors;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Serialization;
using static OpenCvSharp.Stitcher;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BTD6AutoCommunity
{
    public partial class BTD6AutoCommunity : Form
    {
        private InstructionsClass MyInstructions;
        private InstructionsClass ExecuteInstructions;
        private OverlayForm overlayForm;
        private GetGameData gameData;
        private DisPlayMouseCoordinates.POINT mousePos;
        private string currentDirectory;
        private ExecuteDirectiveClass execute;
        public int dpi;
        private int executeCount;
        private int currentExecuteMode;
        private Settings mySet;
        private int startMode;
        private int circleTimes;
        private bool firstStart;
        private System.Timers.Timer ExecuteTM;
        private bool timerFlag;
        private List<(string, string)> collectionScripts;
        private Dictionary<System.Windows.Forms.Button, Keys> hotKeysMap;
        private System.Windows.Forms.Button currentButton;
        private bool isSettingHotKey;

        public BTD6AutoCommunity()
        {
            InitializeComponent();
            overlayForm = new OverlayForm();
            overlayForm.Show();
            MyInstructions = new InstructionsClass();
            mySet = new Settings();
            BindInstructionClassCB();
            InstructionClassCB_SelectedIndexChanged(null, null);
            BindMap(0);
            BindMap(1);
            BindHero();
            BindDifficulty(0);
            BindDifficulty(1);
            BindMode();
            LoadDirectoryTree();
            InitHotKeyMap();
            AnchorBTTT.SetToolTip(AnchorCoordsBT, "空白点即为完成升级点击的空白区域\n回车（Enter）自动输入坐标");
            ExecuteTM = new System.Timers.Timer(200);
            timerFlag = false;
            ExecuteTM.Elapsed += ExecuteTM_Tick;
            currentExecuteMode = -1;
            LoadSettings();

            collectionScripts = new List<(string, string)>();
            firstStart = true;
            currentButton = null;
            isSettingHotKey = false;
        }

        private void InitHotKeyMap()
        {
            hotKeysMap = new Dictionary<System.Windows.Forms.Button, Keys>
            {
                { hot0, Keys.None },
                { hot1, Keys.None },
                { hot2, Keys.None },
                { hot3, Keys.None },
                { hot4, Keys.None },
                { hot5, Keys.None },
                { hot10, Keys.None },
                { hot11, Keys.None },
                { hot12, Keys.None },
                { hot13, Keys.None },
                { hot14, Keys.None },
                { hot15, Keys.None },
                { hot16, Keys.None },
                { hot20, Keys.None },
                { hot21, Keys.None },
                { hot22, Keys.None },
                { hot23, Keys.None },
                { hot24, Keys.None },
                { hot25, Keys.None },
                { hot30, Keys.None },
                { hot31, Keys.None },
                { hot32, Keys.None },
                { hot33, Keys.None },
                { hot34, Keys.None }
            };
        }

        private void BindInstructionClassCB()
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("class");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            foreach (var item in MyInstructions.typeToDisplay)
            {
                DataRow dr = dt.NewRow();
                dr["id"] = item.Key;
                dr["class"] = item.Value;
                dt.Rows.Add(dr);
            }
            InstructionClassCB.DataSource = dt;
            InstructionClassCB.ValueMember = "id";
            InstructionClassCB.DisplayMember = "class";
        }

        private void BindArgument1CB(Dictionary<int, string> dic)
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("object");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            foreach (var item in dic)
            {
                DataRow dr = dt.NewRow();
                dr["id"] = item.Key;
                dr["object"] = item.Value;
                dt.Rows.Add(dr);
            }
            Argument1CB.DataSource = dt;
            Argument1CB.ValueMember = "id";
            Argument1CB.DisplayMember = "object";
        }

        private void BindArgument2CB(Dictionary<int, string> dic)
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("object");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            foreach (var item in dic)
            {
                DataRow dr = dt.NewRow();
                dr["id"] = item.Key;
                dr["object"] = item.Value;
                dt.Rows.Add(dr);
            }
            Argument2CB.DataSource = dt;
            Argument2CB.ValueMember = "id";
            Argument2CB.DisplayMember = "object";
        }

        private void BindMap(int index)
        {

            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("mapName");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            foreach (var item in MyInstructions.maps)
            {
                DataRow dr = dt.NewRow();
                dr["id"] = item.Key;
                dr["mapName"] = item.Value;
                dt.Rows.Add(dr);
            }
            if (index == 1)
            {
                MapCB.DataSource = dt;
                MapCB.ValueMember = "id";
                MapCB.DisplayMember = "mapName";
            }
            else if (index == 0)
            {
                ExecuteMapCB.DataSource = dt;
                ExecuteMapCB.ValueMember = "id";
                ExecuteMapCB.DisplayMember = "mapName";
            }
        }

        private void BindHero()
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("heroName");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            foreach (var item in MyInstructions.heros)
            {
                DataRow dr = dt.NewRow();
                dr["id"] = item.Key;
                dr["heroName"] = item.Value;
                dt.Rows.Add(dr);
            }
            HeroCB.DataSource = dt;
            HeroCB.ValueMember = "id";
            HeroCB.DisplayMember = "heroName";
        }

        private void BindMode()
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("modeName");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            foreach (var item in MyInstructions.mode)
            {
                DataRow dr = dt.NewRow();
                dr["id"] = item.Key;
                dr["modeName"] = item.Value;
                dt.Rows.Add(dr);
            }
            ModeCB.DataSource = dt;
            ModeCB.ValueMember = "id";
            ModeCB.DisplayMember = "modeName";
        }

        private void BindDifficulty(int index)
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("difficultyName");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            foreach (var item in MyInstructions.difficulty)
            {
                DataRow dr = dt.NewRow();
                dr["id"] = item.Key;
                dr["difficultyName"] = item.Value;
                dt.Rows.Add(dr);
            }
            if (index == 1)
            {
                DifficultyCB.DataSource = dt;
                DifficultyCB.ValueMember = "id";
                DifficultyCB.DisplayMember = "difficultyName";
            }
            else if (index == 0)
            {
                ExecuteDifficultyCB.DataSource = dt;
                ExecuteDifficultyCB.ValueMember = "id";
                ExecuteDifficultyCB.DisplayMember = "difficultyName";
            }

        }

        private void BindArgument1CB(List<(int, int)> lst)
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("monkey");
            dt.Columns.Add(dc1);
            for(int i = 0; i < lst.Count; i++)
            {
                (int, int) item = lst[i];
                DataRow dr = dt.NewRow();
                dr["monkey"] = MyInstructions.monkeysToDisplay[item.Item1] + item.Item2.ToString();
                if (MyInstructions.objectList[i].ifDelete)
                {
                    dr["monkey"] += "(已删除)";
                }
                dt.Rows.Add(dr);
            }
            Argument1CB.DataSource = dt;
            Argument1CB.DisplayMember = "monkey";
        }

        private void BindInstructionsViewTL(int index, List<string> lst)
        {

            if (index == 1)
            {
                
                BindingSource bs = new BindingSource();
                bs.DataSource = lst;

                InstructionsViewTL.BeginUpdate();
                InstructionsViewTL.DataSource = null;
                InstructionsViewTL.DataSource = bs;
                InstructionsViewTL.EndUpdate();
            }
            else
            {
                BindingSource bs = new BindingSource();
                bs.DataSource = lst;
                PreviewLB.DataSource = bs;
            }
        }

        private void HideCoordsChoosing()
        {
            CoordsXTB.Visible = false;
            CoordsXLB.Visible = false;
            CoordsYTB.Visible = false;
            CoordsYLB.Visible = false;
            CoordsChosingBT.Visible = false;
        }

        private void ShowCoordsChoosing()
        {
            CoordsXTB.Visible = true;
            CoordsXLB.Visible = true;
            CoordsYTB.Visible = true;
            CoordsYLB.Visible = true;
            CoordsChosingBT.Visible = true;
        }

        private void SetCoinTriggeringCB(int mode)
        {
            if (mode == 1)
            {
                if (CoinTriggeringCB.FindString("10%折扣") == -1)
                {
                    CoinTriggeringCB.Items.Add("10%折扣");
                }
                if (CoinTriggeringCB.FindString("15%折扣") == -1)
                {
                    CoinTriggeringCB.Items.Add("15%折扣");
                }
                if (CoinTriggeringCB.FindString("20%折扣") == -1)
                {
                    CoinTriggeringCB.Items.Add("20%折扣");
                }
                if (CoinTriggeringCB.FindString("25%折扣") == -1)
                {
                    CoinTriggeringCB.Items.Add("25%折扣");
                }
            }
            if (mode == 0)
            {
                if (CoinTriggeringCB.FindString("10%折扣") != -1)
                {
                    CoinTriggeringCB.Items.Remove("10%折扣");
                }
                if (CoinTriggeringCB.FindString("15%折扣") != -1)
                {
                    CoinTriggeringCB.Items.Remove("15%折扣");
                }
                if (CoinTriggeringCB.FindString("20%折扣") != -1)
                {
                    CoinTriggeringCB.Items.Remove("20%折扣");
                }
                if (CoinTriggeringCB.FindString("25%折扣") != -1)
                {
                    CoinTriggeringCB.Items.Remove("25%折扣");
                }
            }

        }

        private void SetInstructionVision(string instructionString)
        {
            if (instructionString == "DeployMonkey")
            {
                BindArgument1CB(MyInstructions.monkeysToDisplay);
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                Argument1CB.Visible = true;
                Argument2CB.Visible = false;

                SetCoinTriggeringCB(0);

                ShowCoordsChoosing();
            }
            if (instructionString == "UpgradeMonkey")
            {
                BindArgument1CB(MyInstructions.objectId);
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                Argument2CB.DataSource = null;
                Argument2CB.Items.Clear();
                Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;
                Argument2CB.Items.Add("上路");
                Argument2CB.Items.Add("中路");
                Argument2CB.Items.Add("下路");
                Argument2CB.SelectedIndex = 0;

                Argument1CB.Visible = true;
                Argument2CB.Visible = true;

                SetCoinTriggeringCB(1);

                HideCoordsChoosing();
            }
            if (instructionString == "ChangeMonkeyTarget")
            {
                BindArgument1CB(MyInstructions.objectId);
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;
                BindArgument2CB(MyInstructions.targetToChange);
                Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;

                Argument1CB.Visible = true;
                Argument2CB.Visible = true;

                SetCoinTriggeringCB(0);

                HideCoordsChoosing();
            }
            if (instructionString == "UseAbility")
            {
                BindArgument1CB(MyInstructions.abilityToDisplay);
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                Argument2CB.DataSource = null;
                Argument2CB.Items.Clear();
                Argument2CB.Items.Add("无坐标选择");
                Argument2CB.Items.Add("有坐标选择");
                Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;
                Argument2CB.SelectedIndex = 0;

                Argument1CB.Visible = true;
                Argument2CB.Visible = true;

                SetCoinTriggeringCB(0);
            }
            if (instructionString == "Fast")
            {
                Argument1CB.DataSource = null;
                Argument1CB.Items.Clear();
                Argument1CB.Items.Add("快/慢进");
                Argument1CB.Items.Add("竞速下一波");
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;
                Argument1CB.SelectedIndex = 0;

                Argument1CB.Visible = true;
                Argument2CB.Visible = false;
                HideCoordsChoosing();
            }
            if (instructionString == "SellMonkey")
            {
                BindArgument1CB(MyInstructions.objectId);

                Argument1CB.Visible = true;
                Argument2CB.Visible = false;
                HideCoordsChoosing();

                SetCoinTriggeringCB(0);
            }
            if (instructionString == "SetMonkeyMode")
            {
                BindArgument1CB(MyInstructions.objectId);
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                Argument2CB.DataSource = null;
                Argument2CB.Items.Clear();
                Argument2CB.Items.Add("无坐标选择");
                Argument2CB.Items.Add("有坐标选择");
                Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;
                Argument2CB.SelectedIndex = 0;

                Argument1CB.Visible = true;
                Argument2CB.Visible = true;

                SetCoinTriggeringCB(0);
            }
            if (instructionString == "DeployHero")
            {
                Argument1CB.Visible = false;
                Argument2CB.Visible = false;

                SetCoinTriggeringCB(0);

                ShowCoordsChoosing();
            }
            if (instructionString == "UpgradeHero")
            {

                Argument1CB.Visible = false;
                Argument2CB.Visible = false;

                SetCoinTriggeringCB(0);

                HideCoordsChoosing();
            }
            if (instructionString == "DeployHeroObject")
            {
                Argument1CB.DataSource = null;
                Argument1CB.Items.Clear();
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDown;
                Argument1CB.Text = "选择物品号";
                Argument1CB.Items.Add("1");
                Argument1CB.Items.Add("2");
                Argument1CB.Items.Add("3");
                Argument1CB.Items.Add("4");
                Argument1CB.Items.Add("5");
                Argument1CB.Items.Add("6");
                Argument1CB.Items.Add("7");
                Argument1CB.Items.Add("8");
                Argument1CB.Items.Add("9");
                Argument1CB.Items.Add("10");
                Argument1CB.Items.Add("11");
                Argument1CB.Items.Add("12");
                Argument1CB.Items.Add("13");
                Argument1CB.Items.Add("14");
                Argument1CB.Items.Add("15");
                Argument1CB.Items.Add("16");

                Argument2CB.DataSource = null;
                Argument2CB.Items.Clear();
                Argument2CB.Items.Add("无坐标选择");
                Argument2CB.Items.Add("有坐标选择");
                Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;
                Argument2CB.SelectedIndex = 0;

                Argument1CB.Visible = true;
                Argument2CB.Visible = true;

                SetCoinTriggeringCB(0);
            }
            if (instructionString == "ChangeHeroTarget")
            {
                BindArgument2CB(MyInstructions.targetToChange);
                Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;

                Argument1CB.Visible = false;
                Argument2CB.Visible = true;

                SetCoinTriggeringCB(0);

                HideCoordsChoosing();
            }
            if (instructionString == "SetHeroMode")
            {
                Argument1CB.DataSource = null;
                Argument1CB.Items.Clear();
                Argument1CB.Items.Add("功能1");
                Argument1CB.Items.Add("功能2");
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;
                Argument1CB.SelectedIndex = 0;

                Argument2CB.DataSource = null;
                Argument2CB.Items.Clear();
                Argument2CB.Items.Add("无坐标选择");
                Argument2CB.Items.Add("有坐标选择");
                Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;
                Argument2CB.SelectedIndex = 0;

                Argument1CB.Visible = true;
                Argument2CB.Visible = true;

                SetCoinTriggeringCB(0);
            }
            if (instructionString == "SellHero")
            {
                Argument1CB.Visible = false;
                Argument2CB.Visible = false;
                HideCoordsChoosing();

                SetCoinTriggeringCB(0);
            }
            if (instructionString == "Click")
            {
                Argument1CB.DataSource = null;
                Argument1CB.Items.Clear();
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDown;
                Argument1CB.Text = "选择点击次数";
                Argument1CB.Items.Add("1");
                Argument1CB.Items.Add("2");
                Argument1CB.Items.Add("3");
                Argument1CB.Items.Add("4");
                Argument1CB.Items.Add("5");
                Argument1CB.Items.Add("6");
                Argument1CB.Items.Add("7");
                Argument1CB.Items.Add("8");
                Argument1CB.Items.Add("9");
                Argument1CB.Items.Add("10");

                Argument1CB.Visible = true;
                Argument2CB.Visible = false;
                ShowCoordsChoosing();

                SetCoinTriggeringCB(0);
            }
            if (instructionString == "ChangeMonkeyCoords")
            {
                BindArgument1CB(MyInstructions.objectId);
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                Argument1CB.Visible = true;
                Argument2CB.Visible = false;

                SetCoinTriggeringCB(0);

                ShowCoordsChoosing();
            }
            if (instructionString == "Wait")
            {
                Argument1CB.DataSource = null;
                Argument1CB.Items.Clear();
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDown;
                Argument1CB.Text = "输入等待时间";

                Argument1CB.Visible = true;
                Argument2CB.Visible = false;

                SetCoinTriggeringCB(0);

                HideCoordsChoosing();
            }
            if (instructionString == "Continue")
            {
                Argument1CB.DataSource = null;
                Argument2CB.DataSource = null;

                Argument1CB.Visible = false;
                Argument2CB.Visible = false;

                SetCoinTriggeringCB(0);

                HideCoordsChoosing();
            }
            if (instructionString == "Stop")
            {
                Argument1CB.DataSource = null;
                Argument2CB.DataSource = null;

                Argument1CB.Visible = false;
                Argument2CB.Visible = false;

                SetCoinTriggeringCB(0);

                HideCoordsChoosing();
            }
            if (instructionString == "InstructionPackages")
            {
                Argument1CB.DataSource = null;
                Argument2CB.DataSource = null;
                BindArgument1CB(MyInstructions.instructionPackages);
                Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                Argument2CB.DropDownStyle = ComboBoxStyle.DropDown;
                Argument2CB.Text = "输入添加数量";

                Argument1CB.Visible = true;
                Argument2CB.Visible = true;
                SetCoinTriggeringCB(0);
                HideCoordsChoosing();
            }

        }

        public void GetGameInfo()
        {
            MyInstructions.selectedMap = Int32.Parse(MapCB.SelectedValue.ToString()); ;
            MyInstructions.selectedDifficulty = DifficultyCB.SelectedIndex;
            MyInstructions.selectedMode = ModeCB.SelectedIndex;
            MyInstructions.selectedHero = HeroCB.SelectedIndex;
            MyInstructions.anchorCoords = (Int32.Parse(AnchorXTB.Text), Int32.Parse(AnchorYTB.Text));
        }

        public void LoadGameInfo()
        {
            MapCB.SelectedValue = MyInstructions.selectedMap;
            DifficultyCB.SelectedIndex = MyInstructions.selectedDifficulty;
            ModeCB.SelectedIndex = MyInstructions.selectedMode;
            HeroCB.SelectedIndex = MyInstructions.selectedHero;
            AnchorXTB.Text = MyInstructions.anchorCoords.Item1.ToString();
            AnchorYTB.Text = MyInstructions.anchorCoords.Item2.ToString();
        }

        private void BTD6AutoCommunity_Activated(object sender, EventArgs e)
        {
            KeyEvents.RegisterHotKey(Handle, 101, 0, Keys.F1); //注册F1热键,根据id值101来判断需要执行哪个函数
        }

        private void BTD6AutoCommunity_Leave(object sender, EventArgs e)
        {
            KeyEvents.UnregisterHotKey(Handle, 101); //注册F1热键,根据id值101来判断需要执行哪个函数
        }

        private void InstructionClassCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = InstructionsViewTL.SelectedIndex;
            int type = Int32.Parse(InstructionClassCB.SelectedValue.ToString());

            if (index >= 0)
            {
                List<int> args = GetArguments();
                if(MyInstructions.IfSame(index, type, args))
                {
                    ChangeInstructionBT.Enabled = true;
                }
                else
                {
                    ChangeInstructionBT.Enabled = false;
                }
            }
            else
            {
                ChangeInstructionBT.Enabled = false;
            }

            CoinTriggeringCB.SelectedIndex = 0;
            RoundTriggeringCB.SelectedIndex = 0;

            AddInstructionBT.Enabled = true;
            InsertInstructionBT.Enabled = true;

            switch (type)
            {
                case 0: // 放置指令
                    SetInstructionVision("DeployMonkey");
                    break;
                case 1: // 升级指令
                    SetInstructionVision("UpgradeMonkey");

                    break;
                case 2: // 切换目标指令
                    SetInstructionVision("ChangeMonkeyTarget");

                    break;
                case 3: // 释放技能指令
                    SetInstructionVision("UseAbility");

                    break;
                case 4: // 倍速指令
                    SetInstructionVision("Fast");

                    break;
                case 5: // 出售指令
                    SetInstructionVision("SellMonkey");

                    break;
                case 6: // 设置猴子功能
                    SetInstructionVision("SetMonkeyMode");

                    break;
                case 7: // 放置英雄
                    SetInstructionVision("DeployHero");

                    break;
                case 8: // 升级英雄
                    SetInstructionVision("UpgradeHero");

                    break;
                case 9: // 放置英雄物品
                    SetInstructionVision("DeployHeroObject");

                    break;
                case 10: // 切换英雄目标
                    SetInstructionVision("ChangeHeroTarget");

                    break;
                case 11: // 设置英雄模式
                    SetInstructionVision("SetHeroMode");

                    break;
                case 12: // 出售英雄
                    SetInstructionVision("SellHero");

                    break;
                case 13: // 鼠标点击
                    SetInstructionVision("Click");

                    break;
                case 14: // 修改猴子坐标
                    SetInstructionVision("ChangeMonkeyCoords");

                    break;
                case 15:

                    SetInstructionVision("Wait");
                    break;
                case 16:

                    SetInstructionVision("Continue");
                    break;
                case 17:

                    SetInstructionVision("Stop");
                    break;
                case 25: // 批量添加指令包

                    SetInstructionVision("InstructionPackages");
                    break;
            }
        }

        private void Argument1CB_SelectedIndexChanged(object sender, EventArgs e)
        {

            int index = InstructionsViewTL.SelectedIndex;
            int type = InstructionClassCB.SelectedIndex;
            int argument1 = Argument1CB.SelectedIndex;
            if (index >= 0)
            {
                List<int> args = GetArguments();
                if (MyInstructions.IfSame(index, type, args))
                {
                    ChangeInstructionBT.Enabled = true;
                }
                else
                {
                    ChangeInstructionBT.Enabled = false;
                }
            }
            else
            {
                ChangeInstructionBT.Enabled = false;
            }

            AddInstructionBT.Enabled = true;
            InsertInstructionBT.Enabled = true;

            if (type == 1 || type == 2 || type == 5 || type == 6)
            {
                if (MyInstructions.objectList[argument1].ifDelete)
                {
                    ChangeInstructionBT.Enabled = false;
                    AddInstructionBT.Enabled = false;
                    InsertInstructionBT.Enabled = false;
                }
            }
        }

        private List<int> GetArguments()
        {
            List<int> args = new List<int>();
            if (Argument1CB.DropDownStyle != ComboBoxStyle.DropDownList)
            {
                if (Int32.TryParse(Argument1CB.Text, out int num))
                {
                    //MessageBox.Show(num.ToString());
                    args.Add(num);
                }
                else
                {
                    args.Add(Argument1CB.SelectedIndex);
                }
            }
            else
            {
                try
                {
                    if (Int32.TryParse(Argument1CB.SelectedValue.ToString(), out int num))
                    {
                        args.Add(num);
                    }
                    else
                    {
                        args.Add(Argument1CB.SelectedIndex);
                    }
                }
                catch
                {
                    args.Add(Argument1CB.SelectedIndex);
                }
            }
            if (Argument2CB.DropDownStyle != ComboBoxStyle.DropDownList)
            {
                if (Int32.TryParse(Argument2CB.Text, out int num))
                {
                    args.Add(num);
                }
                else
                {
                    args.Add(Argument2CB.SelectedIndex);
                }
            }
            else
            {
                args.Add(Argument2CB.SelectedIndex);
            }
            return args;
        }

        private (int, int) GetTrigger()
        {
            int cT, rT;
            if (RoundTriggeringCB.SelectedIndex <= 0)
            {
                if (Int32.TryParse(RoundTriggeringCB.Text, out int num1))
                {
                    rT = num1;
                }
                else
                {
                    rT = 0;
                }
            }
            else
            {
                rT = 0;
            }

            int difficulty = DifficultyCB.SelectedIndex + (ModeCB.SelectedIndex == 7 ? 1 : 0);


            if (InstructionClassCB.SelectedIndex == 1)
            {
                //MessageBox.Show("Coin:" + CoinTriggeringCB.SelectedIndex.ToString());
                if (CoinTriggeringCB.SelectedIndex <= 0)
                {
                    if (Int32.TryParse(CoinTriggeringCB.Text, out int num1))
                    {
                        cT = num1;
                    }
                    else
                    {
                        cT = 0;
                    }
                }
                else
                {
                    cT = MyInstructions.objectList[Argument1CB.SelectedIndex].GetCurrentUpgradeCost(Argument2CB.SelectedIndex, difficulty, CoinTriggeringCB.SelectedIndex - 1);
                }
            }
            else
            {
                //MessageBox.Show("Coin:" + CoinTriggeringCB.SelectedIndex.ToString());
                if (CoinTriggeringCB.SelectedIndex <= 0)
                {
                    if (Int32.TryParse(CoinTriggeringCB.Text, out int num1))
                    {
                        cT = num1;
                    }
                    else
                    {
                        cT = 0;
                    }
                }
                else if (CoinTriggeringCB.SelectedIndex == 1)
                {
                    cT = -1 * difficulty - 1;
                }
                else
                {
                    cT = 0;
                }
            }
            return (rT, cT);
        }

        private (int, int) GetCoords()
        {
            (int, int) coords;
            if (Int32.TryParse(CoordsXTB.Text, out int num1))
            {
                coords.Item1 = num1;
            }
            else
            {
                coords.Item1 = 0;
            }
            if (Int32.TryParse(CoordsYTB.Text, out int num2))
            {
                coords.Item2 = num2;
            }
            else
            {
                coords.Item2 = 0;
            }
            return coords;
        }

        private void AddInstructionBT_Click(object sender, EventArgs e)
        {
            InstructionsViewTL.Enabled = false;


            int instructionType = Int32.Parse(InstructionClassCB.SelectedValue.ToString());
            if (instructionType != 25) // 普通指令
            {
                //获取两个参数
                List<int> args = GetArguments();
                // 获取触发条件
                (int, int) triggering = GetTrigger();

                (int, int) coords = GetCoords();

                MyInstructions.AddInstruction(instructionType, args, triggering.Item1, triggering.Item2, coords);
            }
            else // 指令包
            {
                int packageIndex = Int32.Parse(Argument1CB.SelectedValue.ToString());
                Int32.TryParse(Argument2CB.Text, out int count);
                if (count <= 0)
                {
                    count = 1;
                }
                MyInstructions.InsertInstructionPackage(packageIndex, count, InstructionsViewTL.Items.Count - 1); // 0为添加到最后
            }

            // ListBox数据绑定
            InstructionsViewTL.Enabled = true;
            BindInstructionsViewTL(1, MyInstructions.displayinstructions);
            InstructionsViewTL.SelectedIndex = InstructionsViewTL.Items.Count - 1;
            //foreach (var item1 in MyInstructions.displayinstructions)
            //{
            //    MessageBox.Show(item1.ToString());
            //}
        }

        private void ChangeInstructionBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewTL.SelectedIndex;
            //获取两个参数
            List<int> args = GetArguments();
            // 获取触发条件
            (int, int) triggering = GetTrigger();

            (int, int) coords = GetCoords();

            // ListBox数据绑定
            MyInstructions.ModifyInstruction
                (index, InstructionClassCB.SelectedIndex, args, triggering.Item1, triggering.Item2, coords);


            BindInstructionsViewTL(1, MyInstructions.displayinstructions);
            InstructionsViewTL.SelectedIndex = index;

            return;
        }

        private void InsertInstructionBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewTL.SelectedIndex;

            int instructionType = Int32.Parse(InstructionClassCB.SelectedValue.ToString());
            if (instructionType != 25) // 普通指令
            {
                //获取两个参数
                List<int> args = GetArguments();
                // 获取触发条件
                (int, int) triggering = GetTrigger();

                (int, int) coords = GetCoords();

                if (MyInstructions.InsertInstruction(InstructionsViewTL.SelectedIndex, instructionType, args, triggering.Item1, triggering.Item2, coords))
                {
                    BindInstructionsViewTL(1, MyInstructions.displayinstructions);
                    if (index + 1 < InstructionsViewTL.Items.Count)
                    {
                        InstructionsViewTL.SelectedIndex = index + 1;
                    }
                    else
                    {
                        InstructionsViewTL.SelectedIndex = InstructionsViewTL.Items.Count - 1;
                    }
                }
            }
            else // 指令包
            {
                int packageIndex = Int32.Parse(Argument1CB.SelectedValue.ToString());
                Int32.TryParse(Argument2CB.Text, out int count);
                if (count <= 0)
                {
                    count = 1;
                }
                MyInstructions.InsertInstructionPackage(packageIndex, count, InstructionsViewTL.SelectedIndex);

                BindInstructionsViewTL(1, MyInstructions.displayinstructions);
                if (index + 1 < InstructionsViewTL.Items.Count)
                {
                    InstructionsViewTL.SelectedIndex = index + 1;
                }
                else
                {
                    InstructionsViewTL.SelectedIndex = InstructionsViewTL.Items.Count - 1;
                }
            }

        }

        private void DeleteInstructionBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewTL.SelectedIndex;
            if (index >= 0)
            {
                MyInstructions.DeleteInstruction(index);
                BindInstructionsViewTL(1, MyInstructions.displayinstructions);
                if (index < InstructionsViewTL.Items.Count)
                {
                    InstructionsViewTL.SelectedIndex = index;
                }
                else
                {
                    InstructionsViewTL.SelectedIndex = index - 1;
                }
            }
        }

        private void ClearInstructionBT_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("清除所有指令？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MyInstructions.Clear();
                BindInstructionsViewTL(1, MyInstructions.displayinstructions);
            }
        }


        private void UpBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewTL.SelectedIndex;
            if (index - 1 >= 0)
            {
                if(MyInstructions.ChangeInstructionPosition(index - 1))
                {
                    BindInstructionsViewTL(1, MyInstructions.displayinstructions);
                    InstructionsViewTL.SelectedIndex = index - 1;
                }
            }
            return;
        }

        private void DownBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewTL.SelectedIndex;
            if (MyInstructions.ChangeInstructionPosition(index))
            {
                BindInstructionsViewTL(1, MyInstructions.displayinstructions);
                InstructionsViewTL.SelectedIndex = index + 1;
            }
            return;
        }

        private void SaveInstructionBT_Click(object sender, EventArgs e)
        {
            string filePath;
            try
            {
                GetGameInfo();
                filePath = MyInstructions.SaveToJson();
                //MessageBox.Show(filePath);
                SelectPath(Path.GetFullPath(filePath));
                MyInstructions.Clear();
                BindInstructionsViewTL(1, MyInstructions.displayinstructions);
            }
            catch
            {
                MessageBox.Show("请选择空白点！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Argument2CB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Argument2CB.Text == "无坐标选择" && Argument2CB.SelectedIndex == 0)
            {
                HideCoordsChoosing();
            }
            if (Argument2CB.Text == "有坐标选择" && Argument2CB.SelectedIndex == 1)
            {
                ShowCoordsChoosing();
            }
        }

        private void RoundTriggeringCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RoundTriggeringCB.SelectedIndex == 1)
            {
                RoundTriggeringCB.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            if (RoundTriggeringCB.SelectedIndex == 0)
            {
                RoundTriggeringCB.DropDownStyle = ComboBoxStyle.DropDown;
            }
        }

        private void CoinTriggeringCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CoinTriggeringCB.SelectedIndex == 0)
            {
                CoinTriggeringCB.DropDownStyle = ComboBoxStyle.DropDown;
            }
            else
            {
                CoinTriggeringCB.DropDownStyle = ComboBoxStyle.DropDownList;
            }
        }

        private void InstructionsViewTL_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ChangeInstructionBT.Enabled = true;
                List<int> args = MyInstructions.GetArguments(InstructionsViewTL.SelectedIndex);
                InstructionClassCB.SelectedIndex = args[0];
                int triggerindex = 0;
                switch (args[0])
                {
                    case 0: // 放置指令
                        SetInstructionVision("DeployMonkey");
                        Argument1CB.SelectedValue = args[1];
                        CoordsXTB.Text = args[3].ToString();
                        CoordsYTB.Text = args[4].ToString();

                        triggerindex = 5;
                        break;
                    case 1: // 升级指令
                        SetInstructionVision("UpgradeMonkey");
                        Argument1CB.SelectedIndex = args[1];
                        Argument2CB.SelectedIndex = args[2];

                        triggerindex = 3;
                        break;
                    case 2: // 切换目标指令
                        SetInstructionVision("ChangeMonkeyTarget");
                        Argument1CB.SelectedIndex = args[1];
                        Argument2CB.SelectedIndex = args[2];

                        triggerindex = 3;
                        break;
                    case 3: // 释放技能指令
                        SetInstructionVision("UseAbility");
                        Argument1CB.SelectedValue = args[1];
                        if (args.Count > 4)
                        {
                            Argument2CB.SelectedIndex = 1;
                            CoordsXTB.Text = args[2].ToString();
                            CoordsYTB.Text = args[3].ToString();

                            triggerindex = 4;

                        }
                        else
                        {
                            triggerindex = 2;
                        }
                        break;
                    case 4: // 倍速指令
                        SetInstructionVision("Fast");
                        Argument1CB.SelectedIndex = args[1];

                        triggerindex = 2;
                        break;
                    case 5: // 出售指令
                        SetInstructionVision("SellMonkey");
                        Argument1CB.SelectedIndex = args[1];

                        triggerindex = 2;
                        break;
                    case 6: // 设置猴子功能
                        SetInstructionVision("SetMonkeyMode");
                        Argument1CB.SelectedIndex = args[1];
                        if (args.Count > 4)
                        {
                            Argument2CB.SelectedIndex = 1;
                            CoordsXTB.Text = args[2].ToString();
                            CoordsYTB.Text = args[3].ToString();

                            triggerindex = 4;

                        }
                        else
                        {
                            triggerindex = 2;
                        }
                        break;
                    case 7: // 放置英雄
                        SetInstructionVision("DeployHero");
                        CoordsXTB.Text = args[1].ToString();
                        CoordsYTB.Text = args[2].ToString();

                        triggerindex = 3;
                        break;
                    case 8: // 升级英雄
                        SetInstructionVision("UpgradeHero");
                        if (args.Count > 3)
                        {
                            Argument2CB.SelectedIndex = 1;
                            CoordsXTB.Text = args[1].ToString();
                            CoordsYTB.Text = args[2].ToString();

                            triggerindex = 3;

                        }
                        else
                        {
                            triggerindex = 1;
                        }
                        break;
                    case 9: // 放置英雄物品
                        SetInstructionVision("DeployHeroObject");
                        Argument1CB.SelectedIndex = args[1] - 1;
                        if (args.Count > 4)
                        {
                            Argument2CB.SelectedIndex = 1;
                            CoordsXTB.Text = args[2].ToString();
                            CoordsYTB.Text = args[3].ToString();

                            triggerindex = 4;

                        }
                        else
                        {
                            triggerindex = 2;
                        }
                        break;
                    case 10: // 切换英雄目标
                        SetInstructionVision("ChangeHeroTarget");
                        Argument2CB.SelectedIndex = args[1];

                        triggerindex = 2;
                        break;
                    case 11: // 设置英雄模式
                        SetInstructionVision("SetHeroMode");
                        Argument1CB.SelectedIndex = args[1];
                        if (args.Count > 4)
                        {
                            Argument2CB.SelectedIndex = 1;
                            CoordsXTB.Text = args[2].ToString();
                            CoordsYTB.Text = args[3].ToString();

                            triggerindex = 4;

                        }
                        else
                        {
                            triggerindex = 2;
                        }
                        break;
                    case 12: // 出售英雄
                        SetInstructionVision("SellHero");

                        triggerindex = 1;
                        break;
                    case 13: // 鼠标点击
                        SetInstructionVision("Click");
                        Argument1CB.SelectedIndex = args[1] - 1;

                        CoordsXTB.Text = args[2].ToString();
                        CoordsYTB.Text = args[3].ToString();
                        triggerindex = 4;
                        break;
                    case 14: // 修改猴子坐标
                        SetInstructionVision("ChangeMonkeyCoords");
                        Argument1CB.SelectedIndex = args[1];

                        CoordsXTB.Text = args[2].ToString();
                        CoordsYTB.Text = args[3].ToString();
                        triggerindex = 4;
                        break;
                    case 15: // 等待指令
                        SetInstructionVision("Wait");
                        Argument1CB.Text = args[1].ToString();
                        triggerindex = 2;
                        break;
                    case 16: // 继续指令
                        SetInstructionVision("Continue");
                        triggerindex = 1;
                        break;
                    case 17: // 停止指令
                        SetInstructionVision("Stop");
                        triggerindex = 1;
                        break;
                }
                RoundTriggeringCB.SelectedIndex = 0;
                RoundTriggeringCB.DropDownStyle = ComboBoxStyle.DropDown;
                RoundTriggeringCB.Text = args[triggerindex].ToString();
                CoinTriggeringCB.SelectedIndex = 0;
                CoinTriggeringCB.DropDownStyle = ComboBoxStyle.DropDown;
                CoinTriggeringCB.Text = args[triggerindex + 1].ToString();
            }
            catch
            {

            }
            
        }
        
        protected override void WndProc(ref Message m) // 重载WndProc函数
        {
            const int WM_HOTKEY = 0x0312;
            //按快捷键 
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                        case 13:    //Enter 设置
                            if (mousePos.X != -1)
                            {
                                if (CoordsChosingBT.Text == "取消选择")
                                {
                                    CoordsXTB.Text = mousePos.X.ToString();
                                    CoordsYTB.Text = mousePos.Y.ToString();
                                }
                                if (AnchorCoordsBT.Text == "取消选择")
                                {
                                    AnchorXTB.Text = mousePos.X.ToString();
                                    AnchorYTB.Text = mousePos.Y.ToString();
                                }
                            }
                            break;
                        case 101:
                            StartProgramBT.PerformClick(); 
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }
        
        private void CoordsChosingBT_Click(object sender, EventArgs e)
        {
            if (CoordsChosingBT.Text == "选择坐标")
            {
                CoordsChosingBT.Text = "取消选择";
                KeyEvents.RegisterHotKey(Handle, 13, 0, Keys.Enter);
                DisplayMouseTM.Start();
                overlayForm.Show();
            }
            else
            {
                CoordsChosingBT.Text = "选择坐标";
                KeyEvents.UnregisterHotKey(Handle, 13);
                DisplayMouseTM.Stop();
                overlayForm.Hide();
            }
        }
        
        private void AnchorCoordsBT_Click(object sender, EventArgs e)
        {
            if (AnchorCoordsBT.Text == "设置空白点")
            {
                AnchorCoordsBT.Text = "取消选择";
                KeyEvents.RegisterHotKey(Handle, 13, 0, Keys.Enter);
                DisplayMouseTM.Start();
                overlayForm.Show();
            }
            else
            {
                AnchorCoordsBT.Text = "设置空白点";
                KeyEvents.UnregisterHotKey(Handle, 13);
                DisplayMouseTM.Stop();
                overlayForm.Hide();
            }
        }
        
        private void DisplayMouseTM_Tick(object sender, EventArgs e)
        {
            IntPtr hWnd1 = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6");
            IntPtr hWnd2 = DisPlayMouseCoordinates.FindWindow(null, "BloonsTD6-Epic");
            IntPtr hWnd = IntPtr.Zero;
            if (hWnd1 != IntPtr.Zero)
            {
                hWnd = hWnd1;
            }
            else if (hWnd2 != IntPtr.Zero)
            {
                hWnd = hWnd2;
            }
            else
            {
                mousePos.X = -1;
                mousePos.Y = -1;
                overlayForm.UpdateLabelPosition(Cursor.Position, "未找到游戏窗口");
            }
            if (hWnd != IntPtr.Zero)
            {
                mousePos.X = Cursor.Position.X;
                mousePos.Y = Cursor.Position.Y;

                if (DisPlayMouseCoordinates.ScreenToClient(hWnd, ref mousePos))
                {
                    // 获取dpi
                    using (Graphics g = Graphics.FromHwnd(hWnd))
                    {
                        dpi = DisPlayMouseCoordinates.GetDpiForWindow(hWnd);
                        //MessageBox.Show(dpiX.ToString() + " " + dpiY.ToString());
                        if (GameDpiCB.SelectedIndex == 0)
                        {
                            mousePos.X = (int)((double)mousePos.X * (double)dpi / 96.0 + 0.5);
                            mousePos.Y = (int)((double)mousePos.Y * (double)dpi / 96.0 + 0.5);
                        }
                        else
                        {
                            mousePos.X = (int)((1.5 * mousePos.X) * (dpi * 1.0 / 96.0) + 0.5);
                            mousePos.Y = (int)((1.5 * mousePos.Y) * (dpi * 1.0 / 96.0) + 0.5);
                        }
                    }


                    overlayForm.UpdateLabelPosition(Cursor.Position, $"X: {mousePos.X}, Y: {mousePos.Y}");
                }
            }

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
            BindInstructionsViewTL(0, ExecuteInstructions.displayinstructions);

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
            gameData = new GetGameData(dpi / 96F, GameDpiCB.SelectedIndex, hWnd);
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
                    if ( hWnd == IntPtr.Zero)
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
                new List<string> {"10" ,"10" ,"10" ,"10" ,"10" , "10" ,"10" , "10" ,"10" ,"10", "1 835 940" , "2", $"1 {590 + 250 * mapKind} 980", "2", $"16 {mapKey}" , 
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
                new List<string> {"10" ,"10" ,"10" ,"10" ,"10" , "10" ,"10" , "10" , "1 835 940" , "2", "10", "1 80 165" , "2", "1 1350 35", "2",
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
                    gameData = new GetGameData(dpi / 96F, GameDpiCB.SelectedIndex, hWnd);
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
                    gameData = new GetGameData(dpi / 96F, GameDpiCB.SelectedIndex, hWnd);
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
                    gameData = new GetGameData(dpi / 96F, GameDpiCB.SelectedIndex, hWnd);
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

        private void InstructionsViewTL_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }
            e.DrawBackground();
            e.DrawFocusRectangle();
            StringFormat strFmt = new System.Drawing.StringFormat();
            strFmt.Alignment = StringAlignment.Center; //文本垂直居中
            strFmt.LineAlignment = StringAlignment.Center; //文本水平居中
            e.Graphics.DrawString(InstructionsViewTL.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds, strFmt);
            return;       
        }

        private void LoadDirectoryTree()
        {
            OperationsTV.Nodes.Clear();
            DirectoryInfo rootDir1 = new DirectoryInfo(@"data\我的脚本");
            DirectoryInfo rootDir2 = new DirectoryInfo(@"data\最近删除");
            TreeNode rootNode1 = new TreeNode(rootDir1.Name)
            {
                Tag = rootDir1
            };
            rootNode1.Nodes.Add("");
            OperationsTV.Nodes.Add(rootNode1);
            TreeNode rootNode2 = new TreeNode(rootDir2.Name)
            {
                Tag = rootDir2
            };
            rootNode2.Nodes.Add("");
            OperationsTV.Nodes.Add(rootNode2);
        }

        private void OperationsTV_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selectedNode = e.Node;
            if (selectedNode != null && selectedNode.Tag is DirectoryInfo info)
            {
                DirectoryInfo dirInfo = info;
                DisplayFiles(dirInfo.FullName);
                currentDirectory = dirInfo.FullName;
                //MessageBox.Show(dirInfo.FullName);
                LoadSubDirectories(selectedNode, dirInfo);
            }
        }

        private void LoadSubDirectories(TreeNode node, DirectoryInfo dirInfo)
        {
            //MessageBox.Show("正在加载目录：" + dirInfo.Name + node.Nodes.Count.ToString());
            if (node.Nodes.Count == 1 && node.Nodes[0].Text == "")
            {
                node.Nodes.Clear();
                try
                {

                    if (dirInfo.Name == "我的脚本")
                    {
                        //List<string> orderedSubDirectories = MyInstructions.maps
                        //        .OrderBy(kvp => kvp.Key)
                        //        .Select(kvp => kvp.Value)
                        //        .ToList();

                        foreach (var kvp in MyInstructions.maps)
                        {
                            string subDirName = kvp.Value;
                            DirectoryInfo subDir = new DirectoryInfo(Path.Combine(dirInfo.FullName, subDirName));
                            TreeNode subNode = new TreeNode(subDirName)
                            {
                                Tag = subDir
                            };
                            subNode.Nodes.Add(""); // Add dummy node to make it expandable
                            if (kvp.Key >= 90)
                            {
                                subNode.ForeColor = Color.Red;
                            }
                            else if (kvp.Key >= 60)
                            {
                                subNode.ForeColor = Color.Purple;
                            }
                            else if (kvp.Key >= 30)
                            {
                                subNode.ForeColor = Color.Blue;
                            }
                            else
                            {
                                subNode.ForeColor = Color.Green;
                            }
                            node.Nodes.Add(subNode);
                        }
                        return;
                    }
                    DirectoryInfo[] subDirs = dirInfo.GetDirectories();
                    foreach (DirectoryInfo subDir in subDirs)
                    {
                        TreeNode subNode = new TreeNode(subDir.Name)
                        {
                            Tag = subDir
                        };
                        subNode.Nodes.Add(""); // Add dummy node to make it expandable
                        node.Nodes.Add(subNode);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access denied to folder: " + dirInfo.FullName);
                }
            }
        }

        private void DisplayFiles(string path)
        {
            //MessageBox.Show("displayfile");
            OperationsLV.Items.Clear();
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                FileInfo[] files = dirInfo.GetFiles();
                //MessageBox.Show(files.Length.ToString());
                foreach (FileInfo file in files)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                    ListViewItem item = new ListViewItem(fileNameWithoutExtension);
                    item.SubItems.Add(file.Length.ToString());
                    item.SubItems.Add(file.Extension);
                    item.Tag = file.FullName;
                    OperationsLV.Items.Add(item);
                    //OperationsLV.Refresh();
                }
            }
            catch 
            {
                MessageBox.Show("无法打开目录: " + path);
            }
        }

        private void OperationsLV_ItemActivate(object sender, EventArgs e)
        {
            if (OperationsLV.SelectedItems.Count > 0)
            {
                string filePath = OperationsLV.SelectedItems[0].Tag as string;
                if (filePath != null)
                {
                    // 双击编辑
                    try
                    {
                        MyInstructions.Clear();

                        string jsonString = File.ReadAllText(filePath);
                        MyInstructions = JsonConvert.DeserializeObject<InstructionsClass>(jsonString);
                        MyInstructions.BluidObjectList();
                        LoadGameInfo();
                        BindInstructionsViewTL(1, MyInstructions.displayinstructions);
                        StartPrgramTC.SelectedIndex = 1;
                    }
                    catch
                    {
                        MessageBox.Show("请选择正确的脚本文件！");
                    }
                }
            }
        }

        private void ImportBT_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> eachDir = currentDirectory.Split('\\').ToList();
                string lastDir = eachDir[eachDir.Count - 1];
                if (lastDir == "简单" || lastDir == "中级" || lastDir == "困难")
                {
                    if (!string.IsNullOrEmpty(currentDirectory))
                    {
                        using (OpenFileDialog openFileDialog = new OpenFileDialog())
                        {
                            if (openFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                string sourceFilePath = openFileDialog.FileName;
                                string fileName = Path.GetFileName(sourceFilePath);
                                string destinationFilePath = Path.Combine(currentDirectory, fileName);
                                if (Path.GetExtension(fileName) == ".btd6")
                                {
                                    try
                                    {
                                        File.Copy(sourceFilePath, destinationFilePath, true);
                                        DisplayFiles(currentDirectory);
                                    }
                                    catch (IOException ex)
                                    {
                                        MessageBox.Show("脚本格式错误！" + ex.Message);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("脚本格式错误！");
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("请在左侧选择地图\\难度");
                    }
                }
                else
                {
                    MessageBox.Show("请在左侧选择地图\\难度");
                }
            }
            catch
            {
                MessageBox.Show("请在左侧选择地图\\难度");
            }

        }

        private void DeleteOperationBT_Click(object sender, EventArgs e)
        {
            if (OperationsLV.SelectedItems.Count > 0)
            {
                string filePath = OperationsLV.SelectedItems[0].Tag as string;
                if (filePath != null)
                {
                    try
                    {
                        // 判断是否是“最近删除”文件夹中的文件
                        if (filePath.Contains(@"data\最近删除\"))
                        {
                            // 直接删除文件
                            File.Delete(filePath);
                        }
                        else
                        {
                            string targetFolder = @"data\最近删除\";
                            string targetFileName = Path.GetFileName(filePath);
                            string targetFilePath = Path.Combine(targetFolder, targetFileName);
                            string newFileName = Path.GetFileNameWithoutExtension(targetFileName) +
                                                    "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") +
                            Path.GetExtension(targetFileName);
                            targetFilePath = Path.Combine(targetFolder, newFileName);
                            // 移动文件到“最近删除”文件夹
                            File.Move(filePath, targetFilePath);
                        }
                        DisplayFiles(currentDirectory);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("An error occurred while deleting the file: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择需要删除的脚本！");
            }
        }

        private void EditBT_Click(object sender, EventArgs e)
        {
            if (OperationsLV.SelectedItems.Count > 0)
            {
                string filePath = OperationsLV.SelectedItems[0].Tag as string;
                if (filePath != null)
                {
                    try
                    {
                        MyInstructions.Clear();
                        string jsonString = File.ReadAllText(filePath);

                        MyInstructions = JsonConvert.DeserializeObject<InstructionsClass>(jsonString);
                        MyInstructions.BluidObjectList();
                        LoadGameInfo();
                        BindInstructionsViewTL(1, MyInstructions.displayinstructions);
                        StartPrgramTC.SelectedIndex = 1;
                    }
                    catch
                    {
                        MessageBox.Show("文件打开失败！");
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择需要编辑的脚本！");
            }
        }

        private void SelectPath(string path)
        {
            TreeNode node = FindNodeByPath(OperationsTV.Nodes, path);
            if (node != null)
            {
                OperationsTV.SelectedNode = node;
                node.EnsureVisible();
                OperationsTV.Focus();
                DisplayFiles(path);
            }
            else
            {
                MessageBox.Show("Treenode not found");
            }
            StartPrgramTC.SelectedIndex = 3;
        }

        private TreeNode FindNodeByPath(TreeNodeCollection nodes, string path)
        {
            // 一次性加载完整目录
            foreach (TreeNode node in nodes)
            {
                // 确保仅在需要时或适当时加载
                if (node.Tag is DirectoryInfo dirInfo)
                {

                    // 显示当前节点路径和目标路径
                    //if (dirInfo != null)
                    //{
                    //    MessageBox.Show("当前节点路径: " + dirInfo.FullName + "\n目标路径: " + path);
                    //}
                    // 这样只在第一次调用时加载子目录，避免重复加载
                    LoadSubDirectories(node, dirInfo);

                    // 检查路径
                    if (dirInfo.FullName.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return node;
                    }
                }

                // 递归查找
                TreeNode foundNode = FindNodeByPath(node.Nodes, path);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            return null;
        }

        private void RunBT_Click(object sender, EventArgs e)
        {
            if (OperationsLV.SelectedItems.Count > 0)
            {
                string filePath = OperationsLV.SelectedItems[0].Tag as string;
                if (filePath != null)
                {
                    try
                    {
                        string[] directoryName = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        //foreach (string directory in directoryName)
                        //{
                        //    MessageBox.Show(directory);
                        //}
                        int lastIndex = directoryName.Length - 1;
                        ExecuteMapCB.SelectedIndex = ExecuteMapCB.FindString(directoryName[lastIndex - 2]);
                        ExecuteDifficultyCB.SelectedIndex = ExecuteDifficultyCB.FindString(directoryName[lastIndex - 1]);
                        ExecuteDifficultyCB_SelectedIndexChanged(ExecuteDifficultyCB, EventArgs.Empty);
                        //MessageBox.Show(directoryName[lastIndex]);
                        ExecuteScriptCB.SelectedIndex = ExecuteScriptCB.FindString(Path.GetFileNameWithoutExtension(directoryName[lastIndex]));

                        StartPrgramTC.SelectedIndex = 0;
                        ExecuteModeCB.SelectedIndex = 0;
                        StartProgramBT.PerformClick();
                        //ExecuteInstructions = new InstructionsClass(); 
                        //string jsonString = File.ReadAllText(filePath);

                        //ExecuteInstructions = JsonConvert.DeserializeObject<InstructionsClass>(jsonString);
                        //ExecuteInstructions.BluidObjectList();

                        //ExecuteInstructions.Compile();
                        //ExecuteInstructions.SaveDirectiveToJson();
                        //execute = new ExecuteDirective(ExecuteInstructions.compilerDirective);
                        //if (!execute.GetWindowsInfo())
                        //{
                        //    MessageBox.Show("未找到游戏窗口！");
                        //}
                        //else
                        //{
                        //    executeCount = 0;
                        //    StartPrgramTC.SelectedIndex = 0;
                        //    GetGameDataTM.Start();
                        //    ExecuteTM.Start();
                        //}
                    }
                    catch
                    {
                        MessageBox.Show("文件打开失败！");
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择需要运行的脚本！");
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
                                if (arguments[0] == 1) // 升级指令
                                {
                                    execute.ExecuteDirective(0, 99999999, ExecuteInstructions.upgradeCount, CheckUpgradeCB.Checked, currentExecuteMode);
                                }
                                else if (arguments[0] == 0 || arguments[0] == 7) // 放置指令
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
                                if (arguments[0] == 1) // 升级指令
                                {
                                    execute.ExecuteDirective(roundTrigger, coinTrigger, ExecuteInstructions.upgradeCount, CheckUpgradeCB.Checked, currentExecuteMode);
                                }
                                else if (arguments[0] == 0) // 放置指令
                                {
                                    execute.ExecuteDirective(roundTrigger, coinTrigger, 0, CheckUpgradeCB.Checked, currentExecuteMode);
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

        private void OperationsLV_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label != null && OperationsLV.Items[e.Item].Tag is string oldFilePath)
            {
                string fileExtension = Path.GetExtension(oldFilePath);
                //string oldFileName = Path.GetFileNameWithoutExtension(oldFilePath);
                string newFileName = e.Label + fileExtension;
                string newFilePath = Path.Combine(currentDirectory, newFileName);

                try
                {
                    File.Move(oldFilePath, newFilePath);
                    OperationsLV.Items[e.Item].Tag = newFilePath; // Update the tag to the new file path
                }
                catch (IOException ex)
                {
                    MessageBox.Show("An error occurred while renaming the file: " + ex.Message);
                    e.CancelEdit = true; // Cancel the edit operation
                }
            }
        }

        private void GetGameDataUD_ValueChanged(object sender, EventArgs e)
        {
            //if (GetGameDataUD.Value)
        }

        private void SaveSettingsBT_Click(object sender, EventArgs e)
        {
            mySet.GetGameDataInterval = (int)GetGameDataUD.Value;
            mySet.ExecuteInterval = (int)ExecuteUD.Value;
            mySet.GameDpi = GameDpiCB.SelectedIndex;
            mySet.doubleCoinsEnabled = DoubleCoinCB.Checked;
            mySet.fastPathEnabled = FastPathCB.Checked;
            foreach (var buttonKeyPairs in hotKeysMap)
            {
                mySet.HotKey[Int32.Parse(buttonKeyPairs.Key.Tag.ToString())] = buttonKeyPairs.Value;
            }
            mySet.Save();
        }

        private void LoadSettings()
        {
            string jsonString = File.ReadAllText(@"Settings.Json");
            mySet = JsonConvert.DeserializeObject<Settings>(jsonString);
            GetGameDataUD.Value = mySet.GetGameDataInterval;
            ExecuteUD.Value = mySet.ExecuteInterval;
            GameDpiCB.SelectedIndex = mySet.GameDpi;
            ExecuteTM.Interval = mySet.ExecuteInterval;

            GetGameDataTM.Interval = mySet.GetGameDataInterval;
            DoubleCoinCB.Checked = mySet.doubleCoinsEnabled;
            FastPathCB.Checked = mySet.fastPathEnabled;

            List<System.Windows.Forms.Button> btns = hotKeysMap.Keys.ToList();
            foreach (var buttonKeyPairs in btns)
            {
                Keys currentkey = mySet.HotKey[Int32.Parse(buttonKeyPairs.Tag.ToString())];
                buttonKeyPairs.Text = currentkey.ToString();
                hotKeysMap[buttonKeyPairs] = currentkey;
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
                BindInstructionsViewTL(0, ExecuteInstructions.displayinstructions);
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

        private void Hot_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button btn = sender as System.Windows.Forms.Button;
            btn.Text = "请按键...";
            isSettingHotKey = true;
            currentButton = btn;
        }

        private void StartPrgramTC_KeyDown(object sender, KeyEventArgs e)
        {
            if (StartPrgramTC.SelectedIndex == 2 && isSettingHotKey && currentButton != null)
            {
                Keys pressedKey = e.KeyCode;
                hotKeysMap[currentButton] = pressedKey;
                currentButton.Text = pressedKey.ToString();

                isSettingHotKey = false;
                currentButton = null;
            }
        }
    }
}
