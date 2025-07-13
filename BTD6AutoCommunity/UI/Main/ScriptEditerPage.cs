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

namespace BTD6AutoCommunity.UI.Main
{
    // 脚本编辑器界面
    public partial class BTD6AutoUI
    {
        private ScriptEditorCore myScript;
        private OverlayForm overlayForm;
        private WindowApiWrapper.POINT mousePos;
        private System.Timers.Timer DisplayMouseTimer;
        private Dictionary<ActionTypes, Action> actionControlsHandlers;
        private bool IsCheckInstruction;
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
            IsCheckInstruction = true;
            //myScript.Instructions.InstructionBuild += BindInstructionsViewLB;
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
                ActionTypes.InstructionsBundle
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

        private void BindArgumentCB(ComboBox comboBox, Type type)
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
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.DataSource = dt;
            comboBox.ValueMember = "Value";
            comboBox.DisplayMember = "TypeName";
            comboBox.Visible = true;
        }

        private void BindArgumentCB(ComboBox comboBox, string content)
        {
            comboBox.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox.DataSource = null;
            comboBox.Items.Clear();
            comboBox.Text = content;
            comboBox.Visible = true;
        }

        private void BindArgument1CB(List<string> list)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Value", typeof(string));
            dt.Columns.Add("TypeName", typeof(string));
            foreach (var item in list)
            {
                DataRow dr = dt.NewRow();
                dr["Value"] = item;
                dr["TypeName"] = item;
                dt.Rows.Add(dr);
            }
            Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;
            Argument1CB.DataSource = dt;
            Argument1CB.ValueMember = "Value";
            Argument1CB.DisplayMember = "TypeName";
            Argument1CB.Visible = true;
        }

        private void BindArgument1CB(List<int> monkeyIds)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("value", typeof(int)));
            dt.Columns.Add(new DataColumn("monkey", typeof(string)));
            foreach (var id in monkeyIds)
            {
                int monkeyIdNumber = id / 100;
                Monkeys monkeyType = (Monkeys)(id % 100);
                DataRow dr = dt.NewRow();
                dr["value"] = id;
                dr["monkey"] = Constants.GetTypeName(monkeyType) + monkeyIdNumber.ToString();
                dt.Rows.Add(dr);
            }
            Argument1CB.DropDownStyle = ComboBoxStyle.DropDownList;
            Argument1CB.DataSource = dt;
            Argument1CB.ValueMember = "value";
            Argument1CB.DisplayMember = "monkey";
            Argument1CB.Visible = true;
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

        private void BindInstructionsViewLB()
        {
            InstructionsViewLB.Items.Clear();
            foreach (var item in myScript.Instructions.Instructions)
            {
                InstructionsViewLB.Items.Add(item.ToString());
            }
        }

        private void Argument1CB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Argument1CB.Text.Contains("无坐标选择"))
            {
                HideCoordsChoosing();
            }
            if (Argument1CB.Text.Contains("有坐标选择"))
            {
                ShowCoordsChoosing();
            }
            SetModifyInstructionBT();
        }

        private void Argument2CB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Argument2CB.Text.Contains("无坐标选择"))
            {
                HideCoordsChoosing();
            }
            if (Argument2CB.Text.Contains("有坐标选择"))
            {
                ShowCoordsChoosing();
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

        private void InitInstructionControlsHandlers()
        {
            actionControlsHandlers = new Dictionary<ActionTypes, Action>()
            {
                { ActionTypes.PlaceMonkey, HandlePlaceMonkeyControls },
                { ActionTypes.UpgradeMonkey, HandleUpgradeMonkeyControls },
                { ActionTypes.SwitchMonkeyTarget, HandleSwitchMonkeyTargetControls },
                { ActionTypes.UseAbility, HandleUseAbilityControls },
                { ActionTypes.SwitchSpeed, HandleSwitchSpeedControls },
                { ActionTypes.SellMonkey, HandleSellMonkeyControls },
                { ActionTypes.SetMonkeyFunction, HandleSetMonkeyFunctionControls },
                { ActionTypes.PlaceHero, HandlePlaceHeroControls },
                { ActionTypes.UpgradeHero, HandleUpgradeHeroControls },
                { ActionTypes.PlaceHeroItem, HandlePlaceHeroObjectControls },
                { ActionTypes.SwitchHeroTarget, HandleSwitchHeroTargetControls },
                { ActionTypes.SetHeroFunction, HandleSetHeroFunctionControls },
                { ActionTypes.SellHero, HandleSellHeroControls },
                { ActionTypes.MouseClick, HandleMouseClickControls },
                { ActionTypes.AdjustMonkeyCoordinates, HandleAdjustMonkeyCoordinatesControls },
                { ActionTypes.WaitMilliseconds, HandleWaitMillisecondsControls },
                { ActionTypes.StartFreeplay, HandleStartFreeplayControls },
                { ActionTypes.EndFreeplay, HandleEndFreeplayControls },
                { ActionTypes.Jump, HandleJumpControls },
                { ActionTypes.InstructionsBundle, HandleInstructionsBundleControls }
            };
        }

        private void SetInstructionControls(ActionTypes actionType)
        {
            if (actionControlsHandlers == null) InitInstructionControlsHandlers();
            if (actionControlsHandlers.TryGetValue(actionType, out Action handler))
            {
                handler.Invoke();
            }
        }

        private void HandlePlaceMonkeyControls()
        {
            BindArgumentCB(Argument1CB, typeof(Monkeys));
            BindArgumentCB(Argument2CB, typeof(PlaceCheckTypes));
            ShowCoordsChoosing();
        }

        private void HandleUpgradeMonkeyControls()
        {
            BindArgument1CB(myScript.GetInstructionsMonkeyIds());
            BindArgumentCB(Argument2CB, typeof(UpgradeTypes));
            HideCoordsChoosing();
        }

        private void HandleSwitchMonkeyTargetControls()
        {
            BindArgument1CB(myScript.GetInstructionsMonkeyIds());
            BindArgumentCB(Argument2CB, typeof(TargetTypes));
            HideCoordsChoosing();
        }

        private void HandleUseAbilityControls()
        {
            BindArgumentCB(Argument1CB, typeof(SkillTypes));
            BindArgumentCB(Argument2CB, typeof(CoordinateTypes));
        }

        private void HandleSwitchSpeedControls()
        {
            BindArgumentCB(Argument1CB, typeof(SpeedTypes));
            Argument2CB.Visible = false;
            HideCoordsChoosing();
        }

        private void HandleSellMonkeyControls()
        {
            BindArgument1CB(myScript.GetInstructionsMonkeyIds());

            Argument2CB.Visible = false;
            HideCoordsChoosing();
        }

        private void HandleSetMonkeyFunctionControls()
        {
            BindArgument1CB(myScript.GetInstructionsMonkeyIds());
            BindArgumentCB(Argument2CB, typeof(MonkeyFunctionTypes));
            Argument1CB.Visible = true;
            Argument2CB.Visible = true;
        }

        private void HandlePlaceHeroControls()
        {
            Argument1CB.Visible = false;
            Argument2CB.Visible = false;
            ShowCoordsChoosing();
        }

        private void HandleUpgradeHeroControls()
        {
            Argument1CB.Visible = false;
            Argument2CB.Visible = false;
            HideCoordsChoosing();
        }

        private void HandlePlaceHeroObjectControls()
        {
            BindArgumentCB(Argument1CB, typeof(HeroObjectTypes));
            BindArgumentCB(Argument2CB, typeof(CoordinateTypes));
        }

        private void HandleSwitchHeroTargetControls()
        {
            BindArgumentCB(Argument1CB, typeof(TargetTypes));
            Argument2CB.Visible = false;
            HideCoordsChoosing();
        }

        private void HandleSetHeroFunctionControls()
        {
            BindArgumentCB(Argument1CB, typeof(MonkeyFunctionTypes));
            Argument2CB.Visible = false;
        }

        private void HandleSellHeroControls()
        {
            Argument1CB.Visible = false;
            Argument2CB.Visible = false;
            HideCoordsChoosing();
        }

        private void HandleMouseClickControls()
        {
            BindArgumentCB(Argument1CB, "输入点击次数");
            Argument2CB.Visible = false;
            ShowCoordsChoosing();
        }

        private void HandleAdjustMonkeyCoordinatesControls()
        {
            BindArgument1CB(myScript.GetInstructionsMonkeyIds());
            Argument2CB.Visible = false;
            ShowCoordsChoosing();
        }

        private void HandleWaitMillisecondsControls()
        {
            BindArgumentCB(Argument1CB, "输入等待时间(ms)");
            Argument2CB.Visible = false;
            HideCoordsChoosing();
        }

        private void HandleStartFreeplayControls()
        {
            Argument1CB.Visible = false;
            Argument2CB.Visible = false;
            HideCoordsChoosing();
        }

        private void HandleEndFreeplayControls()
        {
            Argument1CB.Visible = false;
            Argument2CB.Visible = false;
            HideCoordsChoosing();
        }

        private void HandleJumpControls()
        {
            BindArgumentCB(Argument1CB, "输入跳转指令行号");
            Argument2CB.Visible = false;
            HideCoordsChoosing();
        }

        private void HandleInstructionsBundleControls()
        {
            InstructionsBundle instructionsBundle = new InstructionsBundle();
            BindArgument1CB(instructionsBundle.BundleNames);
            BindArgumentCB(Argument2CB, "输入添加数量");
            HideCoordsChoosing();
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
            SetInstructionControls(selectedActionTypes);
        }

        private void SetModifyInstructionBT()
        {
            if (InstructionsViewLB.Items == null || InstructionsViewLB.Items.Count == 0) return;

            int index = InstructionsViewLB.SelectedIndex;
            ActionTypes instructionType = (ActionTypes)InstructionClassCB.SelectedValue;
            List<int> args = GetArguments();
            (int, int) triggers = GetTrigger();
            (int, int) coords = GetCoords();
            if (myScript.TryModifyInstruction(index, instructionType, args, triggers.Item1, triggers.Item2, coords))
            {
                ModifyInstructionBT.Enabled = true;
            }
            else
            {
                ModifyInstructionBT.Enabled = false;
            }
        }

        private List<int> GetArguments()
        {
            // 初始化为7个-1
            List<int> args = new List<int>();
            for (int i = 0; i < 7; i++) args.Add(-1);

            if (Argument1CB.Visible == true)
            {
                if (Argument1CB.DropDownStyle != ComboBoxStyle.DropDownList)
                {
                    if (Int32.TryParse(Argument1CB.Text, out int num)) args[0] = num;
                    else args[0] = 1;
                }
                else
                {
                    if (Argument1CB.SelectedValue != null && 
                            Int32.TryParse(Argument1CB.SelectedValue.ToString(), out int num)) args[0] = num;
                    else args[0] = Argument1CB.SelectedIndex;
                }
            }

            if (Argument2CB.Visible == true)
            {
                if (Argument2CB.DropDownStyle != ComboBoxStyle.DropDownList)
                {
                    if (Int32.TryParse(Argument2CB.Text, out int num)) args[1] = num;
                    else args[1] = 1;
                }
                else
                {
                    if (Argument2CB.SelectedValue != null &&
                            Int32.TryParse(Argument2CB.SelectedValue.ToString(), out int num)) args[1] = num;
                    else args[1] = Argument2CB.SelectedIndex;
                }
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
            int index = InstructionsViewLB.SelectedIndex;
            ActionTypes instructionType = (ActionTypes)InstructionClassCB.SelectedValue;
            if (instructionType != ActionTypes.InstructionsBundle) // 普通指令
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
                string bundleName = Argument1CB.SelectedValue.ToString();
                int times = 1;
                if (Int32.TryParse(Argument2CB.Text, out int num)) times = num;
                InstructionsBundle instructionsBundle = new InstructionsBundle();
                InstructionSequence sequence = instructionsBundle.GetInstructionSequence(bundleName);
                myScript.AddInstructionBundle(sequence, times);
                BindInstructionsViewLB();
            }
            if (index >= 0 && index < InstructionsViewLB.Items.Count) InstructionsViewLB.SetSelected(index, false);


            InstructionsViewLB.SelectedIndex = InstructionsViewLB.Items.Count - 1;
            
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
            }
            return;
        }

        private void InsertInstructionBT_Click(object sender, EventArgs e)
        {
            if (!Enum.IsDefined(typeof(ActionTypes), InstructionClassCB.SelectedValue)) return;
            
            int index = InstructionsViewLB.SelectedIndex;
            int count = 1;
            ActionTypes instructionType = (ActionTypes)InstructionClassCB.SelectedValue;

            if (instructionType != ActionTypes.InstructionsBundle) // 普通指令
            {
                //获取两个参数
                List<int> args = GetArguments();
                // 获取触发条件
                (int, int) triggering = GetTrigger();

                (int, int) coords = GetCoords();

                Instruction inst = myScript.InsertInstruction(InstructionsViewLB.SelectedIndex + 1, instructionType, args, triggering.Item1, triggering.Item2, coords);
                if (inst == null) return;
                InstructionsViewLB.Items.Insert(index + 1, inst.ToString());
            }
            else // 指令包
            {
                string bundleName = Argument1CB.SelectedValue.ToString();
                int times = 1;
                if (Int32.TryParse(Argument2CB.Text, out int num)) times = num;
                InstructionsBundle instructionsBundle = new InstructionsBundle();
                InstructionSequence sequence = instructionsBundle.GetInstructionSequence(bundleName);
                count = myScript.InsertInstructionBundle(index + 1, sequence, times);
                BindInstructionsViewLB();
            }
            if (index >= 0 && index < InstructionsViewLB.Items.Count)
            {
                InstructionsViewLB.SetSelected(index, false);
            }
            if (index + count < InstructionsViewLB.Items.Count)
            {
                InstructionsViewLB.SelectedIndex = index + count;
            }
            else
            {
                InstructionsViewLB.SelectedIndex = InstructionsViewLB.Items.Count - 1;
            }
        }

        private void DeleteInstructionBT_Click(object sender, EventArgs e)
        {
            if (InstructionsViewLB.SelectedIndices == null || InstructionsViewLB.SelectedIndices.Count == 0) return;
            int firstIndex = InstructionsViewLB.SelectedIndices[0];
            // 逆序删除
            for (int i = InstructionsViewLB.SelectedIndices.Count - 1; i >= 0; i--)
            {
                int index = InstructionsViewLB.SelectedIndices[i];
                myScript.DeleteInstruction(index);
                InstructionsViewLB.Items.RemoveAt(index);
            }
            if (firstIndex < InstructionsViewLB.Items.Count)
            {
                InstructionsViewLB.SelectedIndex = firstIndex;
            }
            else
            {
                InstructionsViewLB.SelectedIndex = firstIndex - 1;
            }
        }

        private void ClearInstructionsBT_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("清除所有指令？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                myScript.ClearInstructions();
                InstructionsViewLB.Items.Clear();
            }
            InstructionClassCB_SelectedIndexChanged(null, null);
        }

        private void UpBT_Click(object sender, EventArgs e)
        {
            if (InstructionsViewLB.SelectedIndices == null || InstructionsViewLB.SelectedIndices.Count == 0) return;
            List<int> selectedIndices = InstructionsViewLB.SelectedIndices.Cast<int>().ToList();
            List<int> updatedIndices = new List<int>();
            int currentIndex, lastIndex = -1;
            for (int i = 0; i < selectedIndices.Count; i++)
            {
                currentIndex = selectedIndices[i];
                if (currentIndex - 1 != lastIndex)
                {
                    myScript.MoveInstruction(currentIndex, true);
                    // 交换index - 1和index
                    (InstructionsViewLB.Items[currentIndex], InstructionsViewLB.Items[currentIndex - 1])
                        = (InstructionsViewLB.Items[currentIndex - 1], InstructionsViewLB.Items[currentIndex]);
                    updatedIndices.Add(currentIndex - 1);
                    lastIndex = currentIndex - 1;
                }
                else
                {
                    updatedIndices.Add(currentIndex);
                    lastIndex = currentIndex;
                }
            }
            IsCheckInstruction = false;
            //for (int i = 0; i < InstructionsViewLB.Items.Count; i++)
            //{
            //    InstructionsViewLB.SetSelected(i, false);
            //}

            foreach (int index in selectedIndices)
            {
                InstructionsViewLB.SetSelected(index, false);
            }
            foreach (int index in updatedIndices)
            {
                InstructionsViewLB.SetSelected(index, true);
            }
            IsCheckInstruction = true;
        }

        private void DownBT_Click(object sender, EventArgs e)
        {
            if (InstructionsViewLB.SelectedIndices == null || InstructionsViewLB.SelectedIndices.Count == 0) return;
            List<int> selectedIndices = InstructionsViewLB.SelectedIndices.Cast<int>().ToList();
            List<int> updatedIndices = new List<int>();
            int currentIndex, lastIndex = InstructionsViewLB.Items.Count;
            for (int i = selectedIndices.Count - 1; i >= 0; i--)
            {
                currentIndex = selectedIndices[i];
                if (currentIndex + 1 != lastIndex)
                {
                    myScript.MoveInstruction(currentIndex, false);
                    // 交换index + 1和index
                    (InstructionsViewLB.Items[currentIndex], InstructionsViewLB.Items[currentIndex + 1])
                        = (InstructionsViewLB.Items[currentIndex + 1], InstructionsViewLB.Items[currentIndex]);
                    updatedIndices.Add(currentIndex + 1);
                    lastIndex = currentIndex + 1;
                }
                else
                {
                    updatedIndices.Add(currentIndex);
                    lastIndex = currentIndex;
                }
            }
            IsCheckInstruction = false;
            foreach (int index in selectedIndices)
            {
                InstructionsViewLB.SetSelected(index, false);
            }
            foreach (int index in updatedIndices)
            {
                InstructionsViewLB.SetSelected(index, true);
            }
            IsCheckInstruction = true;
        }

        private void BuildInstructionsBT_Click(object sender, EventArgs e)
        {
            int index = InstructionsViewLB.SelectedIndex;
            myScript.BuildInstructions();
            BindInstructionsViewLB();
            if (index >= InstructionsViewLB.Items.Count) index = InstructionsViewLB.Items.Count - 1;
            InstructionsViewLB.SelectedIndex = index;

        }

        private void SaveInstructionBT_Click(object sender, EventArgs e)
        {
            try
            {
                string filePath;
                myScript.BuildInstructions();
                GetMetaData();
                filePath = myScript.SaveInstructionsToFile();
                myScript.ClearInstructions();
                ModeAndHero_SelectedIndexChanged(null, null);
                //ClearInstructionsBT_Click(null, null);

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


        private void InstructionsViewLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsCheckInstruction == false) return;
            if (InstructionsViewLB.SelectedIndices == null || 
                InstructionsViewLB.SelectedIndices.Count > 1 || 
                InstructionsViewLB.SelectedIndices.Count == 0
                ) return;

            List<int> allArguments = myScript.GetAllArguments(InstructionsViewLB.SelectedIndex);
            if (allArguments == null || allArguments.Count == 0) return;
            if (!Enum.IsDefined(typeof(ActionTypes), allArguments[0])) return;

            var actionType = (ActionTypes)allArguments[0];
            List<int> arguments = allArguments.GetRange(1, 7);
            (int X, int Y) coords = (allArguments[8], allArguments[9]);
            int roundTrigger = allArguments[10];
            int coinTrigger = allArguments[11];


            InstructionClassCB.SelectedValue = actionType;

            SetInstructionControls((ActionTypes)allArguments[0]);

            if (arguments[0] != -1)
            {
                if (Argument1CB.DropDownStyle != ComboBoxStyle.DropDownList)
                {
                    Argument1CB.Text = arguments[0].ToString();
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
                    Argument2CB.Text = arguments[1].ToString();
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
