using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine.InstructionSystem
{
    public class Instruction
    {
        public List<int> AllArguments { get; set; } = new List<int>(11);
        public ActionTypes Type { get => (ActionTypes)AllArguments[0]; set => AllArguments[0] = (int)value; }
        public List<int> Arguments { get => AllArguments.GetRange(1, 7); set => SetArguments(value); }
        public (int X, int Y) Coordinates { get => (AllArguments[8], AllArguments[9]); set => SetCoordinates(value.X, value.Y); }
        public int RoundTrigger { get => AllArguments[10]; set => AllArguments[10] = value; }
        public int CoinTrigger { get => AllArguments[11]; set => AllArguments[11] = value; }


        public Instruction(
            ActionTypes type,
            List<int> arguments,
            int roundTrigger,
            int coinTrigger,
            (int, int) coordinates
            )
        {
            AllArguments = new List<int>(11) { (int)type }
               .Concat(arguments)
               .Concat(new List<int> { roundTrigger, coinTrigger, coordinates.Item1, coordinates.Item2 })
               .ToList();
            Type = type;
            Arguments = arguments;
            RoundTrigger = roundTrigger;
            CoinTrigger = coinTrigger;
            Coordinates = coordinates;
            //AllArguments = new List<int>(11) { (int)Type }
            //   .Concat(Arguments)
            //   .Concat(new List<int> { RoundTrigger, CoinTrigger, Coordinates.X, Coordinates.Y })
            //   .ToList();
        }

        public Instruction(List<int> allArguments)
        {
            AllArguments = allArguments;
            //Type = (ActionTypes)allArguments[0];
            //Arguments = allArguments.GetRange(1, 7);
            //Coordinates = (allArguments[8], allArguments[9]);
            //RoundTrigger = allArguments[10];
            //CoinTrigger = allArguments[11];
        }

        public Instruction Clone()
        {
            return new Instruction(new List<int>(AllArguments));
        }

        public bool IsMonkeyInstruction()
        {
            if (
                Type == ActionTypes.UpgradeMonkey || 
                Type == ActionTypes.SwitchMonkeyTarget || 
                Type == ActionTypes.SellMonkey || 
                Type == ActionTypes.SetMonkeyFunction ||
                Type == ActionTypes.AdjustMonkeyCoordinates
                )
                return true;
            return false;
        }

        public bool IsHeroInstruction()
        {
            if (
                Type == ActionTypes.UpgradeHero ||
                Type == ActionTypes.PlaceHeroItem ||
                Type == ActionTypes.SwitchHeroTarget ||
                Type == ActionTypes.SetHeroFunction ||
                Type == ActionTypes.SellHero
                )
                return true;
            return false;
        }

        // 是否包含坐标参数
        public bool HasCoordinates()
        {
            if ( Coordinates.X != -1 && Coordinates.Y != -1) return true;
            return false;
        }

        // 参数n是否有效
        public bool IsValidArgument(int n)
        {
            if (n < 0 || n > 11) return false;
            if (AllArguments[n] == -1) return false;
            return true;
        }

        public  override string ToString()
        {
            string content = "";
            switch (Type)
            {
                case ActionTypes.PlaceMonkey: // 放置指令
                    content += Constants.GetTypeName((Monkeys)Arguments[0]) + (Arguments[6] / 100).ToString();
                    content += "放置";
                    content += "于" + Coordinates.ToString();
                    break;
                case ActionTypes.UpgradeMonkey: // 升级指令
                    content += Constants.GetTypeName((Monkeys)(Arguments[0] % 100)) + (Arguments[0] / 100).ToString();
                    if (Arguments[4] == -1)
                    {
                        if (Arguments[1] == 0 || Arguments[1] == 3) content += "升级上路";
                        if (Arguments[1] == 1 || Arguments[1] == 4) content += "升级中路";
                        if (Arguments[1] == 2 || Arguments[1] == 5) content += "升级下路";
                    }
                    else
                    {
                        content += "升级至";
                        content += Arguments[4].ToString().PadLeft(3, '0');
                    }
                    break;
                case ActionTypes.SwitchMonkeyTarget: // 切换目标指令
                    content += Constants.GetTypeName((Monkeys)(Arguments[0] % 100)) + (Arguments[0] / 100).ToString();
                    content += "目标";
                    content += Constants.GetTypeName((TargetTypes)Arguments[1]);
                    break;
                case ActionTypes.SetMonkeyFunction: // 设置猴子功能
                    content += Constants.GetTypeName((Monkeys)(Arguments[0] % 100)) + (Arguments[0] / 100).ToString();
                    if (Arguments[1] == 0 || Arguments[1] == 1) content += "更改功能1";
                    if (Arguments[1] == 2 || Arguments[1] == 3) content += "更改功能2";
                    if (Coordinates.X != -1)
                        content += "于" + Coordinates.ToString();
                    break;
                case ActionTypes.AdjustMonkeyCoordinates:
                    content += "修改" + Constants.GetTypeName((Monkeys)(Arguments[0] % 100)) + (Arguments[0] / 100).ToString() + "坐标";
                    content += "于" + Coordinates.ToString();
                    break;
                case ActionTypes.SellMonkey: // 出售指令
                    content += "出售";
                    content += Constants.GetTypeName((Monkeys)(Arguments[0] % 100)) + (Arguments[0] / 100).ToString();
                    break;
                case ActionTypes.PlaceHero:
                    content += "放置英雄";
                    content += "于" + Coordinates.ToString();
                    break;
                case ActionTypes.UpgradeHero:
                    content += "升级英雄";
                    break;
                case ActionTypes.PlaceHeroItem:
                    content += "放置英雄技能面板物品";
                    content += Arguments[0].ToString();
                    if (Coordinates.X != -1)
                        content += "于" + Coordinates.ToString();
                    break;
                case ActionTypes.SwitchHeroTarget:
                    content += "英雄目标";
                    content += Constants.GetTypeName((TargetTypes)Arguments[0]);
                    break;
                case ActionTypes.SetHeroFunction:
                    content += "更改英雄";
                    if (Arguments[0] / 2 == 1)
                        content += "功能2";
                    else
                        content += "功能1";
                    if (Coordinates.X != -1)
                        content += "于" + Coordinates.ToString();
                    break;
                case ActionTypes.SellHero:
                    content += "出售英雄";
                    break;
                case ActionTypes.UseAbility: // 释放技能指令
                    content += "释放";
                    content += Constants.GetTypeName((SkillTypes)Arguments[0]);
                    if (Coordinates.X != -1)
                        content += "于" + Coordinates.ToString();
                    break;
                case ActionTypes.SwitchSpeed: // 倍速指令
                    if (Arguments[0] == 0)
                        content += "切换倍速";
                    else if (Arguments[0] == 1)
                        content += "下一回合";
                    break;
                case ActionTypes.MouseClick:
                    content += "鼠标点击";
                    content += Coordinates.ToString();
                    content += Arguments[0].ToString() + "次";
                    break;
                case ActionTypes.WaitMilliseconds:
                    content += "等待" + Arguments[0].ToString() + "ms";
                    break;
                case ActionTypes.Jump:
                    content += "跳转到第" + Arguments[0].ToString() + "条指令";
                    break;
                case ActionTypes.StartFreeplay:
                    content += "开始自由游戏";
                    break;
                case ActionTypes.EndFreeplay:
                    content += "结束自由游戏";
                    break;
            }
            return content;
        }

        // 为Arguments实现set接口
        private void SetArguments(List<int> value)
        {
            //Arguments = value;
            AllArguments[1] = value[0];
            AllArguments[2] = value[1];
            AllArguments[3] = value[2];
            AllArguments[4] = value[3];
            AllArguments[5] = value[4];
            AllArguments[6] = value[5];
        }

        // 为Coordinates实现set接口
        private void SetCoordinates(int x, int y)
        {
            AllArguments[8] = x;
            AllArguments[9] = y;
        }

        public int this[int index] { get => AllArguments[index]; set => AllArguments[index] = value; }
    }
}

