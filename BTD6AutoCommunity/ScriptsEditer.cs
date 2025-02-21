using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Timers;

namespace BTD6AutoCommunity
{
    // 脚本编辑器界面
    public partial class BTD6AutoCommunity
    {
        private InstructionsClass MyInstructions;
        private OverlayForm overlayForm;
        private DisPlayMouseCoordinates.POINT mousePos;
        private System.Timers.Timer DisplayMouseTimer;
        private void InitializeScriptsEditor()
        {
            BindInstructionClassCB();
            BindScriptMap();
            BindHero();
            BindScriptDifficulty();
            BindMode();
            InstructionClassCB_SelectedIndexChanged(null, null);
            MyInstructions = new InstructionsClass();
            AnchorBTTT.SetToolTip(AnchorCoordsBT, "空白点即为完成升级点击的空白区域\n回车（Enter）自动输入坐标");
        }

        private void BindInstructionClassCB()
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("class");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            foreach (var item in Constants.TypeToDisplay)
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

        private void BindScriptMap()
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
            MapCB.DataSource = dt;
            MapCB.ValueMember = "id";
            MapCB.DisplayMember = "mapName";
        }

        private void BindHero()
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("heroName");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            foreach (var item in Constants.Heros)
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
            foreach (var item in Constants.Mode)
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

        private void BindScriptDifficulty()
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
            DifficultyCB.DataSource = dt;
            DifficultyCB.ValueMember = "id";
            DifficultyCB.DisplayMember = "difficultyName";
        }

        private void BindArgument1CB(List<(int, int)> lst)
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("monkey");
            dt.Columns.Add(dc1);
            for (int i = 0; i < lst.Count; i++)
            {
                (int, int) item = lst[i];
                DataRow dr = dt.NewRow();
                dr["monkey"] = Constants.MonkeysToDisplay[item.Item1] + item.Item2.ToString();
                if (MyInstructions.objectList[i].ifDelete)
                {
                    dr["monkey"] += "(已删除)";
                }
                dt.Rows.Add(dr);
            }
            Argument1CB.DataSource = dt;
            Argument1CB.DisplayMember = "monkey";
        }

        private void BindInstructionsViewTL(List<string> lst)
        {
            BindingSource bs = new BindingSource
            {
                DataSource = lst
            };

            InstructionsViewTL.BeginUpdate();
            InstructionsViewTL.DataSource = null;
            InstructionsViewTL.DataSource = bs;
            InstructionsViewTL.EndUpdate();
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
                BindArgument1CB(Constants.MonkeysToDisplay);
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
                BindArgument2CB(Constants.TargetToChange);
                Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;

                Argument1CB.Visible = true;
                Argument2CB.Visible = true;

                SetCoinTriggeringCB(0);

                HideCoordsChoosing();
            }
            if (instructionString == "UseAbility")
            {
                BindArgument1CB(Constants.AbilityToDisplay);
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
                BindArgument2CB(Constants.TargetToChange);
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
                BindArgument1CB(Constants.InstructionPackages);
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

        private void InstructionClassCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = InstructionsViewTL.SelectedIndex;
            if (!Int32.TryParse(InstructionClassCB.SelectedValue.ToString(), out int type))
            {
                type = 0;
            }

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
            BindInstructionsViewTL(MyInstructions.displayinstructions);
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


            BindInstructionsViewTL(MyInstructions.displayinstructions);
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
                    BindInstructionsViewTL(MyInstructions.displayinstructions);
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

                BindInstructionsViewTL(MyInstructions.displayinstructions);
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
                BindInstructionsViewTL(MyInstructions.displayinstructions);
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
                BindInstructionsViewTL(MyInstructions.displayinstructions);
            }
        }

        private void UpBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewTL.SelectedIndex;
            if (index - 1 >= 0)
            {
                if (MyInstructions.ChangeInstructionPosition(index - 1))
                {
                    BindInstructionsViewTL(MyInstructions.displayinstructions);
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
                BindInstructionsViewTL(MyInstructions.displayinstructions);
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
                BindInstructionsViewTL(MyInstructions.displayinstructions);
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
                                if (AnchorCoordsBT.Text == "取消设置")
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

                if (AnchorCoordsBT.Text == "取消设置")
                {
                    AnchorCoordsBT.Text = "设置空白点";
                }

                if (overlayForm == null)
                {
                    overlayForm = new OverlayForm();
                    overlayForm.Show();
                }

                if (DisplayMouseTimer == null)
                {
                    DisplayMouseTimer = new System.Timers.Timer(100);
                    DisplayMouseTimer.Elapsed += new ElapsedEventHandler(DisplayMouseTimer_Elapsed);
                    DisplayMouseTimer.Start();
                }
            }
            else
            {
                CoordsChosingBT.Text = "选择坐标";
                KeyEvents.UnregisterHotKey(Handle, 13);

                if (DisplayMouseTimer != null)
                {
                    DisplayMouseTimer.Stop();
                    DisplayMouseTimer.Dispose();
                    DisplayMouseTimer = null;
                }

                if (overlayForm != null)
                {
                    overlayForm.Hide();
                    overlayForm.Dispose();
                    overlayForm = null;
                }
            }
        }


        private void AnchorCoordsBT_Click(object sender, EventArgs e)
        {
            if (AnchorCoordsBT.Text == "设置空白点")
            {
                AnchorCoordsBT.Text = "取消设置";
                KeyEvents.RegisterHotKey(Handle, 13, 0, Keys.Enter);

                if (CoordsChosingBT.Text == "取消选择")
                {
                    CoordsChosingBT.Text = "选择坐标";
                }

                if (overlayForm == null)
                {
                    overlayForm = new OverlayForm();
                    overlayForm.Show();
                }

                if (DisplayMouseTimer == null)
                {
                    DisplayMouseTimer = new System.Timers.Timer(100);
                    DisplayMouseTimer.Elapsed += new ElapsedEventHandler(DisplayMouseTimer_Elapsed);
                    DisplayMouseTimer.Start();
                }
            }
            else
            {
                AnchorCoordsBT.Text = "设置空白点";
                KeyEvents.UnregisterHotKey(Handle, 13);

                if (DisplayMouseTimer != null)
                {
                    DisplayMouseTimer.Stop();
                    DisplayMouseTimer.Dispose();
                    DisplayMouseTimer = null;
                }

                if (overlayForm != null)
                {
                    overlayForm.Hide();
                    overlayForm.Dispose();
                    overlayForm = null;
                }
            }
        }

        private void DisplayMouseTimer_Elapsed(object sender, ElapsedEventArgs e)
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
                        int currentDpi = DisPlayMouseCoordinates.GetDpiForWindow(hWnd);
                        //MessageBox.Show(dpiX.ToString() + " " + dpiY.ToString());
                        if (GameDpiCB.SelectedIndex == 0)
                        {
                            mousePos.X = (int)((double)mousePos.X * (double)currentDpi / 96.0 + 0.5);
                            mousePos.Y = (int)((double)mousePos.Y * (double)currentDpi / 96.0 + 0.5);
                        }
                        else
                        {
                            mousePos.X = (int)((1.5 * mousePos.X) * (currentDpi * 1.0 / 96.0) + 0.5);
                            mousePos.Y = (int)((1.5 * mousePos.Y) * (currentDpi * 1.0 / 96.0) + 0.5);
                        }
                    }
                    overlayForm.UpdateLabelPosition(Cursor.Position, $"X: {mousePos.X}, Y: {mousePos.Y}");
                }
            }
        }
    }
}
