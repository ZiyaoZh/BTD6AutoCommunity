using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BTD6AutoCommunity.ScriptEngine
{
    public enum UserActions
    { 
        Add,
        Insert,
        Modify,
        Delete,
        MoveUp,
        MoveDown,
        Clear
    }

    public class InstructionSequence
    {
        private readonly List<Instruction> instructions = new List<Instruction>();

        public IReadOnlyList<Instruction> Instructions => instructions.AsReadOnly();

        // 依赖
        private List<int> monkeyCount = new List<int>(50);// 猴子的数量
        private List<(int monkeyIndex, int count)> monkeyId = new List<(int, int)>();// 猴子的ID
        private List<MonkeyTowerClass> monkeyList; // 每一个猴子对象
        private HeroClass hero = new HeroClass();

        // 非法指令索引列表
        private List<int> invalidIndexList = new List<int>();

        public void Add(Instruction inst)
        {
            if (inst.Type == ActionTypes.PlaceMonkey)
            {
                monkeyCount[inst.Arguments[0]]++;
                monkeyId.Add((inst.Arguments[0], monkeyCount[inst.Arguments[0]]));
                List<int> args = new List<int>(inst.AllArguments)
                {
                    [7] = monkeyId.Count - 1,
                    [6] = monkeyId.Last().count
                };
                inst = new Instruction(args);
            }
            instructions.Add(inst);
        }

        public void Insert(int index, Instruction inst)
        {
            if (inst == null || index < 0 || index > instructions.Count)
                return;
            if (inst.Type == ActionTypes.PlaceMonkey)
            {
                monkeyCount[inst.Arguments[0]]++;
                monkeyId.Add((inst.Arguments[0], monkeyCount[inst.Arguments[0]]));
                List<int> args = new List<int>(inst.AllArguments)
                {
                    [7] = monkeyId.Count - 1,
                    [6] = monkeyId.Last().count
                };
                inst = new Instruction(args);
            }
            instructions.Insert(index, inst);
        }

        public bool Modify(int index, Instruction newInstruction)
        {
            if (newInstruction == null || index < 0 || index >= instructions.Count)
                return false;
            if (IsSameType(instructions[index], newInstruction))
            {
                instructions[index] = newInstruction;
                return true;
            }
            return false;
        }

        private bool IsSameType(Instruction srcInst, Instruction destInst)
        {
            bool IsSame = false;
            if (srcInst.Type != destInst.Type) return false;
            switch (srcInst.Type)
            {
                case ActionTypes.PlaceMonkey:
                case ActionTypes.UpgradeMonkey:
                case ActionTypes.SwitchMonkeyTarget:
                case ActionTypes.SetMonkeyFunction:
                case ActionTypes.SellMonkey:
                case ActionTypes.AdjustMonkeyCoordinates:
                    if (srcInst.Arguments[0] == destInst.Arguments[0]) IsSame = true;
                    break;
                default:
                    IsSame = true;
                    break;
            }
            return IsSame;
        }

        public void Delete(int index)
        {
            if (!(index >= 0 && index < instructions.Count)) return;

            instructions.RemoveAt(index);
        }

        public void Move(int index, bool up)
        {
            int newIndex = up ? index - 1 : index + 1;
            if (index < 0 || index >= instructions.Count ||
                newIndex < 0 || newIndex >= instructions.Count)
                return;

            (instructions[index], instructions[newIndex]) = (instructions[newIndex], instructions[index]);
        }

        public List<int> GetAllArguments(int index)
        {
            if (index >= 0 && index < instructions.Count)
                return instructions[index].AllArguments;
            return new List<int>();
        }

        public void Clear()
        {
            instructions.Clear();
            ClearRely();
        }

        public void Build()
        {
            monkeyList = new List<MonkeyTowerClass>(monkeyId.Count);
            hero = new HeroClass();
            for (int i = 0; i < instructions.Count; i++)
            {
                Instruction inst = instructions[i];
                switch (instructions[i].Type)
                {
                    case ActionTypes.PlaceMonkey:
                        monkeyList[inst.Arguments[6]] = new MonkeyTowerClass((Monkeys)inst.Arguments[0], inst.Arguments[5], inst.Coordinates);
                        break;
                    case ActionTypes.UpgradeMonkey:
                        if (monkeyList[inst.Arguments[0]] == null) invalidIndexList.Add(i);
                        else
                        {
                            if (monkeyList[inst.Arguments[0]].Upgrade(inst.Arguments[1]))
                            {
                                inst.AllArguments[5] = monkeyList[inst.Arguments[0]].GetUpgradeInt();
                                inst.AllArguments[6] = monkeyList[inst.Arguments[0]].GetCoordinates().X;
                                inst.AllArguments[7] = monkeyList[inst.Arguments[0]].GetCoordinates().Y;
                                instructions[i] = new Instruction(inst.AllArguments);
                            }
                            else invalidIndexList.Add(i);
                        }
                        break;
                    case ActionTypes.SwitchMonkeyTarget:
                    case ActionTypes.SetMonkeyFunction:
                        if (monkeyList[inst.Arguments[0]] == null) invalidIndexList.Add(i);
                        else
                        {
                            if (monkeyList[inst.Arguments[0]].exsitence == false) invalidIndexList.Add(i);
                            else
                            {
                                inst.AllArguments[6] = monkeyList[inst.Arguments[0]].GetCoordinates().X;
                                inst.AllArguments[7] = monkeyList[inst.Arguments[0]].GetCoordinates().Y;
                                instructions[i] = new Instruction(inst.AllArguments);
                            }
                        }
                        break;
                    case ActionTypes.AdjustMonkeyCoordinates:
                        if (monkeyList[inst.Arguments[0]] == null) invalidIndexList.Add(i);
                        else
                        {
                            if (monkeyList[inst.Arguments[0]].exsitence == false) invalidIndexList.Add(i);
                            else
                            {
                                monkeyList[inst.Arguments[0]].SetCoordinates(inst.Coordinates);
                            }
                        }
                        break;
                    case ActionTypes.SellMonkey:
                        if (monkeyList[inst.Arguments[0]] == null) invalidIndexList.Add(i);
                        else
                        {
                            if (!monkeyList[inst.Arguments[0]].Sell()) invalidIndexList.Add(i);
                        }
                        break;
                    case ActionTypes.PlaceHero:
                        if (!hero.PlaceHero(inst.Coordinates)) invalidIndexList.Add(i);
                        break;
                    case ActionTypes.UpgradeHero:
                    case ActionTypes.PlaceHeroItem:
                    case ActionTypes.SwitchHeroTarget:
                    case ActionTypes.SetHeroFunction:
                        if (hero.exsitence == false) invalidIndexList.Add(i);
                        else
                        {
                            inst.AllArguments[6] = hero.GetCoordinates().X;
                            inst.AllArguments[7] = hero.GetCoordinates().Y;
                            instructions[i] = new Instruction(inst.AllArguments);
                        }
                        break;
                    case ActionTypes.SellHero:
                        if (!hero.Sell()) invalidIndexList.Add(i);
                        else
                        {
                            inst.AllArguments[6] = hero.GetCoordinates().X;
                            inst.AllArguments[7] = hero.GetCoordinates().Y;
                            instructions[i] = new Instruction(inst.AllArguments);
                        }
                        break;
                    default:
                        break;
                }
            }
            instructions.RemoveAll(x => invalidIndexList.Contains(instructions.IndexOf(x)));
            invalidIndexList.Clear();
        }

        private void ClearRely()
        {
            monkeyId.Clear();
            monkeyCount.Clear();
            monkeyList.Clear();
            hero = new HeroClass();
        }

        public Instruction this[int index] => (index >= 0 && index < instructions.Count) ? instructions[index] : null;

        public int Count => instructions.Count;
    }
}

