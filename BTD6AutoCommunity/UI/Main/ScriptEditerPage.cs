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
using BTD6AutoCommunity.ScriptEngine;
using System.Diagnostics;
using BTD6AutoCommunity.GameObjects;

namespace BTD6AutoCommunity.UI.Main
{
    // 脚本编辑器界面
    public partial class BTD6AutoUI
    {
        private ScriptEditorCore myScript;
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
            AnchorBTTT.SetToolTip(AnchorCoordsBT, "空白点即为完成升级点击的空白区域\n回车（Enter）自动输入坐标");
            
            myScript = new ScriptEditorCore();
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
                dr["ActionTypeName"] = Constants.GetTypeName(item);
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
            dt.Columns.Add("Value", typeof(int));
            dt.Columns.Add("TypeName", typeof(string));
            foreach (var item in Enum.GetValues(type))
            {
                DataRow dr = dt.NewRow();
                dr["Value"] = (int)item;
                dr["TypeName"] = Constants.GetTypeName(item, type);
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
            dt.Columns.Add(new DataColumn("id", typeof(int)));
            dt.Columns.Add(new DataColumn("object", typeof(string)));
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
            foreach (Maps item in Constants.MapsToDisplay)
            {
                DataRow dr = dt.NewRow();
                dr["Value"] = item;
                dr["MapName"] = Constants.GetTypeName(item);
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
                dr["HeroName"] = Constants.GetTypeName(item);
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
                dr["ModeName"] = Constants.GetTypeName(item);
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
                dr["DifficultyName"] = Constants.GetTypeName(item);
                dt.Rows.Add(dr);
            }
            DifficultyCB.DataSource = dt;
            DifficultyCB.ValueMember = "Value";
            DifficultyCB.DisplayMember = "DifficultyName";
        }

        private void BindArgument1CB(List<MonkeyTowerClass> lst)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("value", typeof(int)));
            dt.Columns.Add(new DataColumn("monkey", typeof(string)));
            for (int i = 0; i < lst.Count; i++)
            {
                Monkeys monkeyType = lst[i].Type;
                int monkeyId = lst[i].monkeyId;
                DataRow dr = dt.NewRow();
                dr["value"] = i;
                dr["monkey"] = Constants.GetTypeName(monkeyType) + monkeyId;
                if (lst[i].IsDelete)
                {
                    dr["monkey"] += "(已删除)";
                }
                dt.Rows.Add(dr);
            }
            Argument1CB.DataSource = dt;
            Argument1CB.ValueMember = "value";
            Argument1CB.DisplayMember = "monkey";
        }

        private void BindInstructionsViewLB()
        {
            InstructionsViewLB.Items.Clear();
            foreach (var item in myScript.Instructions.Instructions)
            {
                InstructionsViewLB.Items.Add(item.ToString());
            }
        }

        private void HideCoordsChoosing()
        {
            if (CoordsXTB.Visible == false) return;
            CoordsXTB.Visible = false;
            CoordsXLB.Visible = false;
            CoordsYTB.Visible = false;
            CoordsYLB.Visible = false;
            CoordsChosingBT.Visible = false;
        }

        private void ShowCoordsChoosing()
        {
            if (CoordsXTB.Visible == true) return;
            CoordsXTB.Visible = true;
            CoordsXLB.Visible = true;
            CoordsYTB.Visible = true;
            CoordsYLB.Visible = true;
            CoordsChosingBT.Visible = true;
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
                    ShowCoordsChoosing();
                    break;

                case ActionTypes.UpgradeMonkey:
                    BindArgument1CB(myScript.Instructions.GetMonkeyList());
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                    Argument2CB.DataSource = null;
                    Argument2CB.Items.Clear();
                    Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;
                    Argument2CB.Items.Add("上路");
                    Argument2CB.Items.Add("中路");
                    Argument2CB.Items.Add("下路");
                    // 设置value
                    Argument2CB.SelectedIndex = 0;

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = true;

                    HideCoordsChoosing();
                    break;

                case ActionTypes.SwitchMonkeyTarget:
                    BindArgument1CB(myScript.Instructions.GetMonkeyList());
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;
                    BindArgument2CB(Constants.TargetToChange);
                    Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = true;

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
                    BindArgument1CB(myScript.Instructions.GetMonkeyList());

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = false;
                    HideCoordsChoosing();

                    break;

                case ActionTypes.SetMonkeyFunction:
                    BindArgument1CB(myScript.Instructions.GetMonkeyList());
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                    Argument2CB.DataSource = null;
                    Argument2CB.Items.Clear();
                    Argument2CB.Items.Add("无坐标选择");
                    Argument2CB.Items.Add("有坐标选择");
                    Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;
                    Argument2CB.SelectedIndex = 0;

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = true;

                    break;
                case ActionTypes.PlaceHero:
                    Argument1CB.Visible = false;
                    Argument2CB.Visible = false;


                    ShowCoordsChoosing();
                    break;
                case ActionTypes.UpgradeHero:
                    Argument1CB.Visible = false;
                    Argument2CB.Visible = false;


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

                    break;

                case ActionTypes.SwitchHeroTarget:
                    BindArgument2CB(Constants.TargetToChange);
                    Argument2CB.DropDownStyle = ComboBoxStyle.DropDownList;

                    Argument1CB.Visible = false;
                    Argument2CB.Visible = true;

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

                    break;
                case ActionTypes.SellHero:
                    Argument1CB.Visible = false;
                    Argument2CB.Visible = false;
                    HideCoordsChoosing();

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

                    break;
                case ActionTypes.AdjustMonkeyCoordinates:
                    BindArgument1CB(myScript.Instructions.GetMonkeyList());
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = false;

                    ShowCoordsChoosing();
                    break;

                case ActionTypes.WaitMilliseconds:

                    Argument1CB.DataSource = null;
                    Argument1CB.Items.Clear();
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDown;
                    Argument1CB.Text = "输入等待时间";

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = false;

                    HideCoordsChoosing();
                    break;

                case ActionTypes.StartFreeplay:

                    Argument1CB.DataSource = null;
                    Argument2CB.DataSource = null;

                    Argument1CB.Visible = false;
                    Argument2CB.Visible = false;

                    HideCoordsChoosing();
                    break;

                case ActionTypes.EndFreeplay:
                    Argument1CB.DataSource = null;
                    Argument2CB.DataSource = null;

                    Argument1CB.Visible = false;
                    Argument2CB.Visible = false;

                    HideCoordsChoosing();
                    break;

                case ActionTypes.Jump:
                    Argument1CB.DataSource = null;
                    Argument1CB.Items.Clear();
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDown;
                    Argument1CB.Text = "输入跳转指令行号";

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = false;

                    HideCoordsChoosing();
                    break;

                case ActionTypes.QuickCommandBundle:
                    Argument1CB.DataSource = null;
                    Argument2CB.DataSource = null;
                    BindArgument1CB(Constants.InstructionPackages);
                    Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;

                    Argument2CB.DropDownStyle = ComboBoxStyle.DropDown;
                    Argument2CB.Text = "输入添加数量";

                    Argument1CB.Visible = true;
                    Argument2CB.Visible = true;
                    HideCoordsChoosing();
                    break;
            }
        }
        
        public void GetMetaData()
        {
            (int X, int Y) anchorCoords = (860, 540);
            if (int.TryParse(AnchorXTB.Text, out int x) && int.TryParse(AnchorYTB.Text, out int y))
            {
                anchorCoords = (x, y);
            }
            ScriptMetadata metadata = new ScriptMetadata(
                ScriptNameTB.Text,
                (Maps)MapCB.SelectedValue,
                (LevelDifficulties)DifficultyCB.SelectedValue,
                (LevelMode)ModeCB.SelectedValue,
                (Heroes)HeroCB.SelectedValue,
                anchorCoords
                );
            myScript.SetMetadata(metadata);
        }

        public void LoadScriptMetaData()
        {
            MapCB.SelectedValue = myScript.Metadata.SelectedMap;
            DifficultyCB.SelectedValue = myScript.Metadata.SelectedDifficulty;
            ModeCB.SelectedValue = myScript.Metadata.SelectedMode;
            HeroCB.SelectedValue = myScript.Metadata.SelectedHero;
            AnchorXTB.Text = myScript.Metadata.AnchorCoords.X.ToString();
            AnchorYTB.Text = myScript.Metadata.AnchorCoords.Y.ToString();
            ScriptNameTB.Text = myScript.Metadata.ScriptName;
        }

        private void ModeAndHero_SelectedIndexChanged(object sender, EventArgs e)
        {
            int intTime = DateTime.Now.GetHashCode() % 1000;
            string Time = (intTime > 0 ? intTime : -1 * intTime).ToString();

            if (!Enum.IsDefined(typeof(LevelMode), ModeCB.SelectedValue) ||
                !Enum.IsDefined(typeof(Heroes), HeroCB.SelectedValue)) 
                return;
            ScriptNameTB.Text = Constants.GetTypeName((LevelMode)ModeCB.SelectedValue) + "-" + 
                Constants.GetTypeName((Heroes)HeroCB.SelectedValue) + "-" + Time;

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

        private void InstructionClassCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (InstructionClassCB.SelectedIndex < 0) return;
            ActionTypes selectedActionTypes = (ActionTypes)InstructionClassCB.SelectedValue;

            AddInstructionBT.Enabled = true;
            InsertInstructionBT.Enabled = true;
            SetInstructionVision(selectedActionTypes);
        }

        private void Argument1CB_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 验证枚举是否合法
            if (!Enum.IsDefined(typeof(ActionTypes), InstructionClassCB.SelectedValue)) return;

            //int index = InstructionsViewTL.SelectedIndex;
            ActionTypes type = (ActionTypes)InstructionClassCB.SelectedValue;
            int argument1 = Argument1CB.SelectedIndex;

            AddInstructionBT.Enabled = true;
            InsertInstructionBT.Enabled = true;

            if (type == ActionTypes.UpgradeMonkey ||
                type == ActionTypes.SwitchMonkeyTarget ||
                type == ActionTypes.SetMonkeyFunction ||
                type == ActionTypes.SellMonkey)
            {
                if (myScript.Instructions.GetMonkeyList() == null || 
                    myScript.Instructions.GetMonkeyList()[argument1].IsDelete)
                {
                    ChangeInstructionBT.Enabled = false;
                    AddInstructionBT.Enabled = false;
                    InsertInstructionBT.Enabled = false;
                }
            }
        }

        private List<int> GetArguments()
        {
            // 初始化为7个-1
            List<int> args = new List<int>();
            for (int i = 0; i < 7; i++) args.Add(-1);

            if (Argument1CB.DropDownStyle != ComboBoxStyle.DropDownList)
            {
                if (Int32.TryParse(Argument1CB.Text, out int num)) args[0] = num;
                else args[0] = Argument1CB.SelectedIndex;
            }
            else
            {
                if (Argument1CB.SelectedValue != null && 
                        Int32.TryParse(Argument1CB.SelectedValue.ToString(), out int num)) args[0] = num;
                else args[0] = Argument1CB.SelectedIndex;
            }
            if (Argument2CB.DropDownStyle != ComboBoxStyle.DropDownList)
            {
                if (Int32.TryParse(Argument2CB.Text, out int num)) args[1] = num;
                else args[1] = Argument2CB.SelectedIndex;
            }
            else
            {
                args[1] = Argument2CB.SelectedIndex;
            }
            return args;
        }

        private (int, int) GetTrigger()
        {
            int cT, rT;
            if (Int32.TryParse(RoundTriggerTB.Text, out int num1)) rT = num1;
            else rT = 0;
            if (Int32.TryParse(CoinTriggerTB.Text, out int num2)) cT = num2;
            else cT = 0;
            return (rT, cT);
        }

        private (int X, int Y) GetCoords()
        {
            if (CoordsXTB.Visible == false || CoordsYTB.Visible == false) return (-1, -1);
            (int, int) coords; 
            if (Int32.TryParse(CoordsXTB.Text, out int num1)) coords.Item1 = num1;
            else coords.Item1 = -1;
            if (Int32.TryParse(CoordsYTB.Text, out int num2)) coords.Item2 = num2;
            else coords.Item2 = -1;
            return coords;
        }

        private void AddInstructionBT_Click(object sender, EventArgs e)
        {
            if (!Enum.IsDefined(typeof(ActionTypes), InstructionClassCB.SelectedValue)) return;

            InstructionsViewLB.Enabled = false;
            ActionTypes instructionType = (ActionTypes)InstructionClassCB.SelectedValue;
            if (instructionType != ActionTypes.QuickCommandBundle) // 普通指令
            {
                List<int> args = GetArguments();
                (int, int) triggers = GetTrigger();
                (int, int) coords = GetCoords();

                Instruction inst = myScript.AddInstruction(instructionType, args, triggers.Item1, triggers.Item2, coords);
                if (inst == null) return;
                InstructionsViewLB.Items.Add(inst.ToString());
            }
            else // 指令包
            {
                //int packageIndex = Int32.Parse(Argument1CB.SelectedValue.ToString());
                //Int32.TryParse(Argument2CB.Text, out int count);
                //if (count <= 0)
                //{
                //    count = 1;
                //}
                //myScript.InsertInstructionPackage(packageIndex, count, InstructionsViewTL.Items.Count - 1); // 0为添加到最后
            }

            InstructionsViewLB.Enabled = true;
            InstructionsViewLB.SelectedIndex = InstructionsViewLB.Items.Count - 1;
            //foreach (var item1 in MyInstructions.displayinstructions)
            //{
            //    MessageBox.Show(item1.ToString());
            //}
        }

        private void ModifyInstructionBT_Click(object sender, EventArgs e)
        {
            if (!Enum.IsDefined(typeof(ActionTypes), InstructionClassCB.SelectedValue)) return;

            int index = InstructionsViewLB.SelectedIndex;
            //获取两个参数
            List<int> args = GetArguments();
            // 获取触发条件
            (int, int) triggering = GetTrigger();

            (int, int) coords = GetCoords();

            Instruction inst = myScript.ModifyInstruction(index, (ActionTypes)InstructionClassCB.SelectedValue, args, triggering.Item1, triggering.Item2, coords);
            if (inst != null)  
            {
                InstructionsViewLB.Items[index] = inst.ToString();
                myScript.Instructions.Build();
                BindInstructionsViewLB();
            }

            if (index >= InstructionsViewLB.Items.Count) index = InstructionsViewLB.Items.Count - 1;
            InstructionsViewLB.SelectedIndex = index;

            return;
        }

        private void InsertInstructionBT_Click(object sender, EventArgs e)
        {
            if (!Enum.IsDefined(typeof(ActionTypes), InstructionClassCB.SelectedValue)) return;
            
            int index = InstructionsViewLB.SelectedIndex;

            ActionTypes instructionType = (ActionTypes)InstructionClassCB.SelectedValue;

            if (instructionType != ActionTypes.QuickCommandBundle) // 普通指令
            {
                //获取两个参数
                List<int> args = GetArguments();
                // 获取触发条件
                (int, int) triggering = GetTrigger();

                (int, int) coords = GetCoords();

                Instruction inst = myScript.InsertInstruction(InstructionsViewLB.SelectedIndex + 1, instructionType, args, triggering.Item1, triggering.Item2, coords);
                if (inst == null) return;
                InstructionsViewLB.Items.Insert(index + 1, inst.ToString());

                if (index + 1 < InstructionsViewLB.Items.Count)
                {
                    InstructionsViewLB.SelectedIndex = index + 1;
                }
                else
                {
                    InstructionsViewLB.SelectedIndex = InstructionsViewLB.Items.Count - 1;
                }
            }
            else // 指令包
            {
                //int packageIndex = Int32.Parse(Argument1CB.SelectedValue.ToString());
                //Int32.TryParse(Argument2CB.Text, out int count);
                //if (count <= 0)
                //{
                //    count = 1;
                //}
                //myScript.InsertInstructionPackage(packageIndex, count, InstructionsViewLB.SelectedIndex);

                //BindInstructionsViewTL(myScript.Displayinstructions);
                //if (index + 1 < InstructionsViewLB.Items.Count)
                //{
                //    InstructionsViewLB.SelectedIndex = index + 1;
                //}
                //else
                //{
                //    InstructionsViewLB.SelectedIndex = InstructionsViewLB.Items.Count - 1;
                //}
            }

        }

        private void DeleteInstructionBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewLB.SelectedIndex;
            if (index < 0) return;
            myScript.DeleteInstruction(index);
            InstructionsViewLB.Items.RemoveAt(index);
            if (index < InstructionsViewLB.Items.Count)
            {
                InstructionsViewLB.SelectedIndex = index;
            }
            else
            {
                InstructionsViewLB.SelectedIndex = index - 1;
            }
        }

        private void ClearInstructionsBT_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("清除所有指令？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                myScript.Instructions.Clear();
                InstructionsViewLB.Items.Clear();
            }
        }

        private void UpBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewLB.SelectedIndex;
            if (index - 1 >= 0)
            {
                myScript.MoveInstruction(index, true);
                // 交换index - 1和index
                (InstructionsViewLB.Items[index], InstructionsViewLB.Items[index - 1])
                    = (InstructionsViewLB.Items[index - 1], InstructionsViewLB.Items[index]);
                InstructionsViewLB.SelectedIndex = index - 1;
            }
            return;
        }

        private void DownBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewLB.SelectedIndex;
            if (index + 1 < InstructionsViewLB.Items.Count)
            {
                myScript.MoveInstruction(index, false);
                // 交换index + 1和index
                (InstructionsViewLB.Items[index], InstructionsViewLB.Items[index + 1])
                    = (InstructionsViewLB.Items[index + 1], InstructionsViewLB.Items[index]);
                InstructionsViewLB.SelectedIndex = index + 1;
            }
            return;
        }

        private void BuildInstructionsBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewLB.SelectedIndex;
            myScript.Instructions.Build();
            BindInstructionsViewLB();
            if (index >= InstructionsViewLB.Items.Count) index = InstructionsViewLB.Items.Count - 1;
            InstructionsViewLB.SelectedIndex = index;
        }

        private void SaveInstructionBT_Click(object sender, EventArgs e)
        {
            try
            {
                string filePath;
                myScript.Instructions.Build();
                GetMetaData();
                filePath = myScript.SaveInstructionsToFile();
                myScript.ClearInstructions();

                if (IsStartPageEditButtonClicked)
                {
                    StartPrgramTC.SelectedIndex = 0;
                    IsStartPageEditButtonClicked = false;
                    ExecuteScriptCB_SelectedIndexChanged(null, null);
                }
                else
                { 
                    SelectPath(Path.GetFullPath(Path.GetDirectoryName(filePath)));
                    StartPrgramTC.SelectedIndex = 3;
                }
            }
            catch
            {
                MessageBox.Show("脚本名不能包含特殊字符：\\/:*?\\");
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

        private void InstructionsViewLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<int> allArguments = myScript.GetAllArguments(InstructionsViewLB.SelectedIndex);
            if (allArguments == null || allArguments.Count == 0) return;
            if (!Enum.IsDefined(typeof(ActionTypes), allArguments[0])) return;

            var actionType = (ActionTypes)allArguments[0];
            List<int> arguments = allArguments.GetRange(1, 7);
            (int X, int Y) coords = (allArguments[8], allArguments[9]);
            int roundTrigger = allArguments[10];
            int coinTrigger = allArguments[11];


            InstructionClassCB.SelectedValue = actionType;

            SetInstructionVision((ActionTypes)allArguments[0]);

            if (arguments[0] != -1)
            {
                if (Argument1CB.DropDownStyle != ComboBoxStyle.DropDownList)
                {
                    if (Argument1CB.Items.Count <= arguments[0]) Argument1CB.Text = arguments[0].ToString();
                    else Argument1CB.SelectedIndex = arguments[0];
                }
                else
                {
                    if (Argument1CB.SelectedValue != null) Argument1CB.SelectedValue = arguments[0];
                    else Argument1CB.SelectedIndex = arguments[0];
                }
            }

            if (arguments[1] != -1)
            {
                if (Argument2CB.DropDownStyle != ComboBoxStyle.DropDownList)
                {
                    if (Argument2CB.Items.Count <= arguments[1]) Argument2CB.Text = arguments[1].ToString();
                    else Argument2CB.SelectedIndex = arguments[1];
                }
                else
                {
                    if (Argument2CB.SelectedValue != null) Argument2CB.SelectedValue = arguments[1];
                    else Argument2CB.SelectedIndex = arguments[1];
                }
            }

            CoordsXTB.Text = coords.X.ToString();
            CoordsYTB.Text = coords.Y.ToString();

            RoundTriggerTB.Text = roundTrigger.ToString();
            CoinTriggerTB.Text = coinTrigger.ToString();
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
            if (mousePos.X >= 16000 || mousePos.Y >= 9000)
            {
                overlayForm.UpdateLabelPosition(Cursor.Position, "无效的坐标");
            }
            else
            {
                overlayForm.UpdateLabelPosition(Cursor.Position, $"X: {mousePos.X}, Y: {mousePos.Y}");
            }
        }
    }
}
