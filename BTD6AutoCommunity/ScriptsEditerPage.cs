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
using static BTD6AutoCommunity.Constants;
using System.Diagnostics;

namespace BTD6AutoCommunity
{
    // 脚本编辑器界面
    public partial class BTD6AutoCommunity
    {
        private ScriptEditorSuite MyInstructions;
        private OverlayForm overlayForm;
        private WindowApiWrapper.POINT mousePos;
        private System.Timers.Timer DisplayMouseTimer;
        private void InitializeScriptsEditor()
        {
            BindActionClassCB();
            BindScriptMap();
            BindHero();
            BindScriptDifficulty();
            BindMode();
            ModeAndHero_SelectedIndexChanged(null, null);
            MyInstructions = new ScriptEditorSuite();
            AnchorBTTT.SetToolTip(AnchorCoordsBT, "空白点即为完成升级点击的空白区域\n回车（Enter）自动输入坐标");
        }

        private void BindActionClassCB()
        {
            List<ActionTypes> actionTypesToDisplay = new List<ActionTypes>
            {
                ActionTypes.PlaceMonkey,
                ActionTypes.UpgradeMonkey,
                ActionTypes.SwitchMonkeyTarget,
                ActionTypes.SetMonkeyFunction,
                ActionTypes.AdjustMonkeyCoordinates,
                ActionTypes.SellMonkey,
                ActionTypes.PlaceHero,
                ActionTypes.UpgradeHero,
                ActionTypes.SwitchHeroTarget,
                ActionTypes.SetHeroFunction,
                ActionTypes.PlaceHeroItem,
                ActionTypes.SellHero,
                ActionTypes.UseAbility,
                ActionTypes.SwitchSpeed,
                ActionTypes.StartFreeplay,
                ActionTypes.EndFreeplay,
                ActionTypes.MouseClick,
                ActionTypes.WaitMilliseconds,
                ActionTypes.Jump,
                ActionTypes.QuickCommandBundle
            };
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", typeof(ActionTypes));
            dt.Columns.Add("ActionTypeName", typeof(string));
            foreach (ActionTypes item in actionTypesToDisplay)
            {
                DataRow dr = dt.NewRow();
                dr["Value"] = item;
                dr["ActionTypeName"] = GetTypeName(item);
                dt.Rows.Add(dr);
            }
            InstructionClassCB.DataSource = dt;
            InstructionClassCB.ValueMember = "Value";
            InstructionClassCB.DisplayMember = "ActionTypeName";
            InstructionClassCB_SelectedIndexChanged(null, null);
            InstructionClassCB.SelectedIndexChanged += InstructionClassCB_SelectedIndexChanged;
        }

        private void BindArgument1CB(Type type)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", type);
            dt.Columns.Add("TypeName", typeof(string));
            foreach (var item in Enum.GetValues(type))
            {
                DataRow dr = dt.NewRow();
                dr["Value"] = item;
                dr["TypeName"] = GetTypeName(item, type);
                dt.Rows.Add(dr);
            }
            Argument1CB.DataSource = dt;
            Argument1CB.ValueMember = "Value";
            Argument1CB.DisplayMember = "TypeName";
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
            dt.Columns.Add("Value", typeof(Maps));
            dt.Columns.Add("MapName", typeof(string));
            foreach (Maps item in MapsToDisplay)
            {
                DataRow dr = dt.NewRow();
                dr["Value"] = item;
                dr["MapName"] = GetTypeName(item);
                dt.Rows.Add(dr);
            }
            MapCB.DataSource = dt;
            MapCB.ValueMember = "Value";
            MapCB.DisplayMember = "MapName";
        }

        private void BindHero()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", typeof(Heroes));
            dt.Columns.Add("HeroName", typeof(string));
            foreach (Heroes item in Enum.GetValues(typeof(Heroes)))
            {
                DataRow dr = dt.NewRow();
                dr["Value"] = item;
                dr["HeroName"] = GetTypeName(item);
                dt.Rows.Add(dr);
            }
            HeroCB.DataSource = dt;
            HeroCB.ValueMember = "Value";
            HeroCB.DisplayMember = "HeroName";
            HeroCB.SelectedValueChanged += ModeAndHero_SelectedIndexChanged;
        }

        private void BindMode()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", typeof(LevelMode));
            dt.Columns.Add("ModeName", typeof(string));
            foreach (LevelMode item in Enum.GetValues(typeof(LevelMode)))
            {
                DataRow dr = dt.NewRow();
                dr["Value"] = item;
                dr["ModeName"] = GetTypeName(item);
                dt.Rows.Add(dr);
            }
            ModeCB.DataSource = dt;
            ModeCB.ValueMember = "Value";
            ModeCB.DisplayMember = "ModeName";
            ModeCB.SelectedValueChanged += ModeAndHero_SelectedIndexChanged;
        }

        private void BindScriptDifficulty()
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
            DifficultyCB.DataSource = dt;
            DifficultyCB.ValueMember = "Value";
            DifficultyCB.DisplayMember = "DifficultyName";
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
                dr["monkey"] = GetTypeName((Monkeys)item.Item1) + item.Item2.ToString();
                if (MyInstructions.objectList[i].IsDelete)
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
        
        private void SetInstructionVision(ActionTypes actionType)
        {
            switch (actionType)
            {
                case ActionTypes.PlaceMonkey:
                    BindArgument1CB(typeof(Monkeys));
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;
                    Argument1CB.Visible = true;
                    Argument2CB.Visible = false;
                    SetCoinTriggeringCB(0);
                    ShowCoordsChoosing();
                    break;

                case ActionTypes.UpgradeMonkey:
                    BindArgument1CB(MyInstructions.ObjectId);
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
                    break;

                case ActionTypes.SwitchMonkeyTarget:
                    BindArgument1CB(MyInstructions.ObjectId);
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;
                    BindArgument2CB(Constants.TargetToChange);
                    Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = true;

                    SetCoinTriggeringCB(0);

                    HideCoordsChoosing();
                    break;

                case ActionTypes.UseAbility:
                    BindArgument1CB(typeof(SkillTypes));
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
                    break;

                case ActionTypes.SwitchSpeed:
                    Argument1CB.DataSource = null;
                    Argument1CB.Items.Clear();
                    Argument1CB.Items.Add("快/慢进");
                    Argument1CB.Items.Add("竞速下一波");
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;
                    Argument1CB.SelectedIndex = 0;

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = false;
                    HideCoordsChoosing();
                    break;

                case ActionTypes.SellMonkey:
                    BindArgument1CB(MyInstructions.ObjectId);

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = false;
                    HideCoordsChoosing();

                    SetCoinTriggeringCB(0);
                    break;

                case ActionTypes.SetMonkeyFunction:
                    BindArgument1CB(MyInstructions.ObjectId);
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
                    break;
                case ActionTypes.PlaceHero:
                    Argument1CB.Visible = false;
                    Argument2CB.Visible = false;

                    SetCoinTriggeringCB(0);

                    ShowCoordsChoosing();
                    break;
                case ActionTypes.UpgradeHero:
                    Argument1CB.Visible = false;
                    Argument2CB.Visible = false;

                    SetCoinTriggeringCB(0);

                    HideCoordsChoosing();
                    break;
                case ActionTypes.PlaceHeroItem:
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
                    break;

                case ActionTypes.SwitchHeroTarget:
                    BindArgument2CB(TargetToChange);
                    Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;

                    Argument1CB.Visible = false;
                    Argument2CB.Visible = true;

                    SetCoinTriggeringCB(0);

                    HideCoordsChoosing();
                    break;
                case ActionTypes.SetHeroFunction:
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
                    break;
                case ActionTypes.SellHero:
                    Argument1CB.Visible = false;
                    Argument2CB.Visible = false;
                    HideCoordsChoosing();

                    SetCoinTriggeringCB(0);
                    break;
                case ActionTypes.MouseClick:
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
                    break;
                case ActionTypes.AdjustMonkeyCoordinates:
                    BindArgument1CB(MyInstructions.ObjectId);
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = false;

                    SetCoinTriggeringCB(0);

                    ShowCoordsChoosing();
                    break;

                case ActionTypes.WaitMilliseconds:

                    Argument1CB.DataSource = null;
                    Argument1CB.Items.Clear();
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDown;
                    Argument1CB.Text = "输入等待时间";

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = false;

                    SetCoinTriggeringCB(0);

                    HideCoordsChoosing();
                    break;

                case ActionTypes.StartFreeplay:

                    Argument1CB.DataSource = null;
                    Argument2CB.DataSource = null;

                    Argument1CB.Visible = false;
                    Argument2CB.Visible = false;

                    SetCoinTriggeringCB(0);

                    HideCoordsChoosing();
                    break;

                case ActionTypes.EndFreeplay:
                    Argument1CB.DataSource = null;
                    Argument2CB.DataSource = null;

                    Argument1CB.Visible = false;
                    Argument2CB.Visible = false;

                    SetCoinTriggeringCB(0);

                    HideCoordsChoosing();
                    break;

                case ActionTypes.Jump:
                    Argument1CB.DataSource = null;
                    Argument1CB.Items.Clear();
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDown;
                    Argument1CB.Text = "输入跳转指令行号";

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = false;

                    SetCoinTriggeringCB(0);
                    HideCoordsChoosing();
                    break;

                case ActionTypes.QuickCommandBundle:
                    Argument1CB.DataSource = null;
                    Argument2CB.DataSource = null;
                    BindArgument1CB(InstructionPackages);
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                    Argument2CB.DropDownStyle = ComboBoxStyle.DropDown;
                    Argument2CB.Text = "输入添加数量";

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = true;
                    SetCoinTriggeringCB(0);
                    HideCoordsChoosing();
                    break;
            }
        }
        
        public void GetGameInfo()
        {
            MyInstructions.SelectedMap = (Maps)MapCB.SelectedValue;
            MyInstructions.SelectedDifficulty = (LevelDifficulties)DifficultyCB.SelectedValue;
            MyInstructions.SelectedMode = (LevelMode)ModeCB.SelectedValue;
            MyInstructions.SelectedHero = (Heroes)HeroCB.SelectedValue;
            try
            {
                MyInstructions.AnchorCoords = (Int32.Parse(AnchorXTB.Text), Int32.Parse(AnchorYTB.Text));
            }
            catch
            {
                MessageBox.Show("请设置空白点！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MyInstructions.ScriptName = ScriptNameTB.Text;
        }

        public void LoadScriptInfo()
        {
            MapCB.SelectedValue = MyInstructions.SelectedMap;
            DifficultyCB.SelectedValue = MyInstructions.SelectedDifficulty;
            ModeCB.SelectedValue = MyInstructions.SelectedMode;
            HeroCB.SelectedValue = MyInstructions.SelectedHero;
            AnchorXTB.Text = MyInstructions.AnchorCoords.Item1.ToString();
            AnchorYTB.Text = MyInstructions.AnchorCoords.Item2.ToString();
            ScriptNameTB.Text = MyInstructions.ScriptName;
        }

        private void ModeAndHero_SelectedIndexChanged(object sender, EventArgs e)
        {
            int intTime = DateTime.Now.GetHashCode() % 1000;
            string Time = (intTime > 0 ? intTime : -1 * intTime).ToString();

            if (!Enum.IsDefined(typeof(LevelMode), ModeCB.SelectedValue) ||
                !Enum.IsDefined(typeof(Heroes), HeroCB.SelectedValue)) 
                return;
            ScriptNameTB.Text = GetTypeName((LevelMode)ModeCB.SelectedValue) + "-" + 
                GetTypeName((Heroes)HeroCB.SelectedValue) + "-" + Time;

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
            e.Graphics.DrawString(InstructionsViewTL.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds, strFmt);
            return;
        }

        private void InstructionClassCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (InstructionClassCB.SelectedIndex < 0) return;
            int index = InstructionsViewTL.SelectedIndex;
            ActionTypes selectedActionTypes = (ActionTypes)InstructionClassCB.SelectedValue;

            if (index >= 0)
            {
                List<int> args = GetArguments();
                if (MyInstructions.IfSame(index, selectedActionTypes, args))
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
            SetInstructionVision(selectedActionTypes);
        }

        private void Argument1CB_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 验证枚举是否合法
            if (!Enum.IsDefined(typeof(ActionTypes), InstructionClassCB.SelectedValue)) return;

            int index = InstructionsViewTL.SelectedIndex;
            ActionTypes type = (ActionTypes)InstructionClassCB.SelectedValue;
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

            if (type == ActionTypes.UpgradeMonkey ||
                type == ActionTypes.SwitchMonkeyTarget ||
                type == ActionTypes.SetMonkeyFunction ||
                type == ActionTypes.SellMonkey)
            {
                if (MyInstructions.objectList[argument1].IsDelete)
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
            if (!Enum.IsDefined(typeof(ActionTypes), InstructionClassCB.SelectedValue)) return;

            InstructionsViewTL.Enabled = false;
            ActionTypes instructionType = (ActionTypes)InstructionClassCB.SelectedValue;
            if (instructionType != ActionTypes.QuickCommandBundle) // 普通指令
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
            BindInstructionsViewTL(MyInstructions.Displayinstructions);
            InstructionsViewTL.SelectedIndex = InstructionsViewTL.Items.Count - 1;
            //foreach (var item1 in MyInstructions.displayinstructions)
            //{
            //    MessageBox.Show(item1.ToString());
            //}
        }

        private void ModifyInstructionBT_Click(object sender, EventArgs e)
        {
            if (!Enum.IsDefined(typeof(ActionTypes), InstructionClassCB.SelectedValue)) return;

            int index = InstructionsViewTL.SelectedIndex;
            //获取两个参数
            List<int> args = GetArguments();
            // 获取触发条件
            (int, int) triggering = GetTrigger();

            (int, int) coords = GetCoords();

            // ListBox数据绑定
            MyInstructions.ModifyInstruction
                (index, (ActionTypes)InstructionClassCB.SelectedValue, args, triggering.Item1, triggering.Item2, coords);


            BindInstructionsViewTL(MyInstructions.Displayinstructions);
            InstructionsViewTL.SelectedIndex = index;

            return;
        }

        private void InsertInstructionBT_Click(object sender, EventArgs e)
        {
            if (!Enum.IsDefined(typeof(ActionTypes), InstructionClassCB.SelectedValue)) return;
            
            int index = InstructionsViewTL.SelectedIndex;

            ActionTypes instructionType = (ActionTypes)InstructionClassCB.SelectedValue;

            if (instructionType != ActionTypes.QuickCommandBundle) // 普通指令
            {
                //获取两个参数
                List<int> args = GetArguments();
                // 获取触发条件
                (int, int) triggering = GetTrigger();

                (int, int) coords = GetCoords();

                if (MyInstructions.InsertInstruction(InstructionsViewTL.SelectedIndex, instructionType, args, triggering.Item1, triggering.Item2, coords))
                {
                    BindInstructionsViewTL(MyInstructions.Displayinstructions);
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

                BindInstructionsViewTL(MyInstructions.Displayinstructions);
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
            if (index < 0) return;
            MyInstructions.DeleteInstruction(index);
            BindInstructionsViewTL(MyInstructions.Displayinstructions);
            if (index < InstructionsViewTL.Items.Count)
            {
                InstructionsViewTL.SelectedIndex = index;
            }
            else
            {
                InstructionsViewTL.SelectedIndex = index - 1;
            }
        }

        private void ClearInstructionBT_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("清除所有指令？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MyInstructions.Clear();
                BindInstructionsViewTL(MyInstructions.Displayinstructions);
            }
        }

        private void UpBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewTL.SelectedIndex;
            if (index - 1 >= 0)
            {
                if (MyInstructions.ChangeInstructionPosition(index - 1))
                {
                    BindInstructionsViewTL(MyInstructions.Displayinstructions);
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
                BindInstructionsViewTL(MyInstructions.Displayinstructions);
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
                filePath = MyInstructions.SaveToJson(ScriptNameTB.Text);
                //MessageBox.Show(filePath);
                SelectPath(Path.GetFullPath(filePath));
                MyInstructions.Clear();
                BindInstructionsViewTL(MyInstructions.Displayinstructions);
            }
            catch
            {
                MessageBox.Show("脚本名异常！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            ChangeInstructionBT.Enabled = true;
            List<int> args = MyInstructions.GetArguments(InstructionsViewTL.SelectedIndex);
            if (args == null || args.Count == 0) return;

            if (!Enum.IsDefined(typeof(ActionTypes), args[0])) return;
            var actionType = (ActionTypes)args[0];
            InstructionClassCB.SelectedValue = actionType;

            SetInstructionVision((ActionTypes)args[0]);

            int triggerindex = 0;

            switch (actionType)
            {
                case ActionTypes.PlaceMonkey: // 放置指令
                    Argument1CB.SelectedValue = args[1];
                    CoordsXTB.Text = args[3].ToString();
                    CoordsYTB.Text = args[4].ToString();

                    triggerindex = 5;
                    break;
                case ActionTypes.UpgradeMonkey: // 升级指令
                    Argument1CB.SelectedIndex = args[1];
                    Argument2CB.SelectedIndex = args[2];

                    triggerindex = 4;
                    break;
                case ActionTypes.SwitchMonkeyTarget: // 切换目标指令
                    Argument1CB.SelectedIndex = args[1];
                    Argument2CB.SelectedIndex = args[2];

                    triggerindex = 3;
                    break;
                case ActionTypes.UseAbility: // 释放技能指令
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
                case ActionTypes.SwitchSpeed: // 倍速指令
                    Argument1CB.SelectedIndex = args[1];

                    triggerindex = 2;
                    break;
                case ActionTypes.SellMonkey: // 出售指令
                    Argument1CB.SelectedIndex = args[1];

                    triggerindex = 2;
                    break;
                case ActionTypes.SetMonkeyFunction: // 设置猴子功能
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
                case ActionTypes.PlaceHero: // 放置英雄
                    CoordsXTB.Text = args[1].ToString();
                    CoordsYTB.Text = args[2].ToString();

                    triggerindex = 3;
                    break;
                case ActionTypes.UpgradeHero: // 升级英雄
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
                case ActionTypes.PlaceHeroItem: // 放置英雄物品
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
                case ActionTypes.SwitchHeroTarget: // 切换英雄目标
                    Argument2CB.SelectedIndex = args[1];

                    triggerindex = 2;
                    break;
                case ActionTypes.SetHeroFunction: // 设置英雄模式
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
                case ActionTypes.SellHero: // 出售英雄

                    triggerindex = 1;
                    break;
                case ActionTypes.MouseClick: // 鼠标点击
                    Argument1CB.SelectedIndex = args[1] - 1;

                    CoordsXTB.Text = args[2].ToString();
                    CoordsYTB.Text = args[3].ToString();
                    triggerindex = 4;
                    break;
                case ActionTypes.AdjustMonkeyCoordinates: // 修改猴子坐标
                    Argument1CB.SelectedIndex = args[1];

                    CoordsXTB.Text = args[2].ToString();
                    CoordsYTB.Text = args[3].ToString();
                    triggerindex = 4;
                    break;
                case ActionTypes.WaitMilliseconds: // 等待指令
                case ActionTypes.Jump:
                    Argument1CB.Text = args[1].ToString();
                    triggerindex = 2;
                    break;
                case ActionTypes.StartFreeplay: // 继续指令
                    triggerindex = 1;
                    break;
                case ActionTypes.EndFreeplay: // 停止指令
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
                        case 102:
                        case 103:
                        case 104:
                        case 105:
                        case 106:
                        case 107:
                        case 108:
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
                WindowApiWrapper.RegisterHotKey(Handle, 13, 0, Keys.Enter);

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
                WindowApiWrapper.UnregisterHotKey(Handle, 13);

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
                WindowApiWrapper.RegisterHotKey(Handle, 13, 0, Keys.Enter);

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
                WindowApiWrapper.UnregisterHotKey(Handle, 13);

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
            GameContext context = new GameContext();

            if (!context.IsValid)
            {
                mousePos.X = -1;
                mousePos.Y = -1;
                overlayForm.UpdateLabelPosition(Cursor.Position, "未找到游戏窗口");
                return;
            }
            Point mousePoint = WindowApiWrapper.GetCursorPosition();
            mousePoint.X = (int)(mousePoint.X * context.DpiScale);
            mousePoint.Y = (int)(mousePoint.Y * context.DpiScale);

            mousePoint = context.ConvertScreenPosition(mousePoint);
            mousePos.X = mousePoint.X;
            mousePos.Y = mousePoint.Y;
            
            overlayForm.UpdateLabelPosition(Cursor.Position, $"X: {mousePos.X}, Y: {mousePos.Y}");
        }
    }
}
