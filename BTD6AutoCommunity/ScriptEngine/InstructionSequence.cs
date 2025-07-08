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
        private List<Instruction> instructions = new List<Instruction>();

        public List<Instruction> Instructions => instructions;

        // 依赖
        private List<int> monkeyCounts; // 猴子的数量

        private List<int> monkeyIds; // 猴子的ID
        private Dictionary<int, MonkeyTowerClass> monkeyList; // 每一个猴子对象
        private HeroClass hero = new HeroClass();

        // 非法指令索引列表
        private List<int> invalidIndexList = new List<int>();

        // 事件
        //public event Action InstructionBuild;


        public InstructionSequence()
        {
            InitRely();
        }

        public static InstructionSequence BuildByScriptModel(ScriptModel scriptModel)
        {
            InstructionSequence sequence = new InstructionSequence();
            sequence.SetInstructionList(scriptModel.InstructionsList);
            sequence.SetMonkeyCount(scriptModel.MonkeyCounts);
            sequence.SetMonkeyIds(scriptModel.MonkeyIds);
            sequence.Build();
            return sequence;
        }

        public void Add(Instruction inst)
        {
            if (inst.Type == ActionTypes.PlaceMonkey)
            {
                monkeyCounts[inst.Arguments[0]]++;
                int monkeyId = inst.Arguments[0] + monkeyCounts[inst.Arguments[0]] * 100;
                monkeyIds.Add(monkeyId);
                List<int> args = new List<int>(inst.AllArguments)
                {
                    [7] = monkeyIds.Last()
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
                int monkeyId = inst.Arguments[0] + monkeyCounts[inst.Arguments[0]] * 100;
                monkeyCounts[inst.Arguments[0]]++;
                monkeyIds.Add(monkeyId);
                List<int> args = new List<int>(inst.AllArguments)
                {
                    [7] = monkeyIds.Count - 1,
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
                Instruction oldInstruction = instructions[index];
                for (int i = 0; i < newInstruction.AllArguments.Count; i++)
                {
                    if (newInstruction[i] != -1) oldInstruction[i] = newInstruction[i];
                }
                instructions[index] = oldInstruction;
                return true;
            }
            return false;
        }

        public bool IsSameType(Instruction srcInst, Instruction destInst)
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
            if (instructions[index].Type == ActionTypes.PlaceMonkey)
            {
                int monkeyId = instructions[index].AllArguments[7];
                monkeyIds.Remove(monkeyId);
                instructions.RemoveAt(index);
                Build();
                return;
            }
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
            InitRely();
        }

        public void Build()
        {
            InitMonkeyList();
            hero = new HeroClass();
            for (int i = 0; i < instructions.Count; i++)
            {
                Instruction inst = instructions[i];
                switch (instructions[i].Type)
                {
                    case ActionTypes.PlaceMonkey:
                        monkeyList[inst.Arguments[6]] = new MonkeyTowerClass((Monkeys)inst.Arguments[0], inst.Arguments[6] / 100, inst.Coordinates);
                        break;
                    case ActionTypes.UpgradeMonkey:
                        if (!monkeyList.TryGetValue(inst.Arguments[0], out _)) invalidIndexList.Add(i);
                        else
                        {
                            if (monkeyList[inst.Arguments[0]].Upgrade(inst.Arguments[1] % 3))
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
                        if (!monkeyList.TryGetValue(inst.Arguments[0], out _)) invalidIndexList.Add(i);
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
                        if (!monkeyList.TryGetValue(inst.Arguments[0], out _)) invalidIndexList.Add(i);
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
                        if (!monkeyList.TryGetValue(inst.Arguments[0], out _)) invalidIndexList.Add(i);
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
            //InstructionBuild?.Invoke();
        }



        public List<List<int>> GetInstructionList()
        {
            List<List<int>> result = new List<List<int>>();
            foreach (Instruction inst in instructions)
            {
                result.Add(inst.AllArguments);
            }
            return result;
        }

        public void SetInstructionList(List<List<int>> instList)
        {
            instructions.Clear();
            foreach (List<int> inst in instList)
            {
                instructions.Add(new Instruction(inst));
            }
        }

        public List<int> GetMonkeyCount()
        {
            return monkeyCounts;
        }

        public void SetMonkeyCount(List<int> count)
        {
            monkeyCounts = count;
        }

        public List<int> GetMonkeyIds()
        {
            return monkeyIds;
        }

        public void SetMonkeyIds(List<int> id)
        {
            monkeyIds = id;
        }

        public Dictionary<int, MonkeyTowerClass> GetMonkeyList()
        {
            return monkeyList;
        }

        private void ClearRely()
        {
            monkeyIds.Clear();
            monkeyCounts.Clear();
            monkeyList.Clear();
            hero = new HeroClass();

        }

        private void InitRely()
        {
            monkeyCounts = new List<int>();

            for (int i = 0; i < 50; i++) monkeyCounts.Add(0);
            monkeyIds = new List<int>();
            monkeyList = new Dictionary<int, MonkeyTowerClass>();
        }

        private void InitMonkeyList()
        {
            monkeyList = new Dictionary<int, MonkeyTowerClass>();
            if (monkeyIds.Count == 0) return;
            foreach (int i in monkeyIds)
            {
                monkeyList.Add(i, null);
            }
        }

        public Instruction this[int index] => (index >= 0 && index < instructions.Count) ? instructions[index] : null;

        public int Count => instructions.Count;

        public Instruction Last => instructions.LastOrDefault();
    }
}

