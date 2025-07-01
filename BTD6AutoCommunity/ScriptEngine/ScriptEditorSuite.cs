using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.AccessControl;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.InteropServices;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Forms;
using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using System.Diagnostics;

namespace BTD6AutoCommunity.ScriptEngine
{


    public struct ScriptInstructionInfo
    {
        public int Index { get; set; }  // 指令索引
        public ActionTypes Type { get; set; }  // 指令类型

        public string Content { get; set; }  // 指令内容
        public List<int> Arguments { get; set; }    // 指令内容
        public int RoundTrigger { get; set; }  // 回合触发条件
        public int CashTrigger { get; set; }   // Cash触发条件

        public bool IsRoundMet { get; set; }  // 是否达到回合触发条件

        public bool IsCashMet { get; set; }   // 是否达到Cash触发条件

        public ScriptInstructionInfo(int index, ActionTypes type, List<int> arguments, string content, int round, int cash)
        {
            Index = index;
            Type = type;
            Arguments = arguments;
            Content = content;
            RoundTrigger = round;
            CashTrigger = cash;
            IsRoundMet = true;
            IsCashMet = true;
        }

        public override string ToString()
        {
            return Content;
        }
    }

    public class ScriptEditorSuite
    {
        public string ScriptName;

        public Maps SelectedMap { get; set; }
        public LevelDifficulties SelectedDifficulty { get; set; }
        public LevelMode SelectedMode { get; set; }
        public Heroes SelectedHero { get; set; }
        public (int, int) AnchorCoords { get; set; }
        public List<int> ObjectCount { get; set; } // 猴子的数量

        public Dictionary<int, string> upgradeCount;
        public List<(int, int)> ObjectId { get; set; } // 猴子的ID
        public List<MonkeyTowerClass> objectList; // 每一个猴子对象
        public HeroClass hero;
        public List<(int, int)> triggering;
        public List<List<string>> compilerDirective;

        public List<string> Displayinstructions { get; set; }
        public List<string> Digitalinstructions { get; set; }
        public ScriptEditorSuite() 
        {
            Displayinstructions = new List<string>();
            Digitalinstructions = new List<string>();

            // 初始化计数列表
            ObjectCount = new List<int>();

            ObjectId = new List<(int, int)>();
            objectList = new List<MonkeyTowerClass>();
            hero = new HeroClass();
            triggering = new List<(int, int)>();
        }

        public static ScriptEditorSuite LoadScript(string path)
        {
            ScriptEditorSuite script;
            string jsonString = File.ReadAllText(path);
            script = JsonConvert.DeserializeObject<ScriptEditorSuite>(jsonString);
            script.ScriptName = Path.GetFileNameWithoutExtension(path);
            script.RepairScript();
            return script;
        }

        private (string, string) MakeInstruction(ActionTypes type, List<int> args, int roundTriggering, int coinTriggering, (int, int) coords)
        {
            if (ObjectCount.Count == 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    ObjectCount.Add(0);
                }
            }
            bool succeeded = true;
            List<int> arguments = new List<int>
            {
                (int)type
            };

            switch (type)
            {
                case ActionTypes.PlaceMonkey: // 放置指令
                    ObjectCount[args[0]]++;
                    ObjectId.Add((args[0], ObjectCount[args[0]]));

                    string jsonString = File.ReadAllText("Monkey.json");
                    MonkeyTowerClass newObject = JsonConvert.DeserializeObject<MonkeyTowerClass>(jsonString);
                    newObject.IsDelete = false;
                    newObject.SetCoordinates(coords);
                    objectList.Add(newObject);

                    arguments.Add(args[0]);
                    arguments.Add(objectList.Count() - 1);
                    arguments.Add(coords.Item1);
                    arguments.Add(coords.Item2);
                    break;
                case ActionTypes.UpgradeMonkey: // 升级指令
                    //if (objectList[args[0]].exsitence == false)
                    //{
                    //    succeeded = false;
                    //    break;
                    //}
                    succeeded = objectList[args[0]].Upgrade(args[1]);

                    arguments.Add(args[0]);
                    arguments.Add(args[1]);
                    arguments.Add(objectList[args[0]].GetUpgradeLevel(args[1]));
                    break;
                case ActionTypes.SwitchMonkeyTarget: // 切换目标指令
                    //if (objectList[args[0]].exsitence == false)
                    //{
                    //    succeeded = false;
                    //    break;
                    //}

                    arguments.Add(args[0]);
                    arguments.Add(args[1]);
                    break;
                case ActionTypes.UseAbility: // 释放技能指令
                    arguments.Add(args[0]);
                    if (args[1] == 1)
                    {
                        arguments.Add(coords.Item1);
                        arguments.Add(coords.Item2);
                    }
                    break;
                case ActionTypes.SwitchSpeed: // 倍速指令
                    arguments.Add(args[0]);

                    break;
                case ActionTypes.SellMonkey: // 出售指令
                    succeeded = objectList[args[0]].Sale();
                    arguments.Add(args[0]);

                    break;
                case ActionTypes.SetMonkeyFunction: // 设置猴子功能
                    //if (objectList[args[0]].exsitence == false)
                    //{
                    //    succeeded = false;
                    //    break;
                    //}
                    arguments.Add(args[0]);
                    if (args[1] == 1)
                    {
                        arguments.Add(coords.Item1);
                        arguments.Add(coords.Item2);
                    }

                    break;
                case ActionTypes.PlaceHero:
                    arguments.Add(coords.Item1);
                    arguments.Add(coords.Item2);

                    break;
                case ActionTypes.UpgradeHero:

                    break;
                case ActionTypes.PlaceHeroItem:

                    if (args[0] < 1)
                    {
                        arguments.Add(1);
                    }
                    else
                    {
                        arguments.Add(args[0]);
                    }
                    if (args[1] == 1)
                    {
                        arguments.Add(coords.Item1);
                        arguments.Add(coords.Item2);
                    }
                    break;
                case ActionTypes.SwitchHeroTarget:

                    arguments.Add(args[1]);

                    break;
                case ActionTypes.SetHeroFunction:
                    arguments.Add(args[0]);

                    if (args[1] == 1)
                    {
                        arguments.Add(coords.Item1);
                        arguments.Add(coords.Item2);
                    }

                    break;
                case ActionTypes.SellHero:

                    break;
                case ActionTypes.MouseClick:
                    if (args[0] < 1)
                    {
                        arguments.Add(1);
                    }
                    else 
                    {
                        arguments.Add(args[0]);
                    }
                    arguments.Add(coords.Item1);
                    arguments.Add(coords.Item2);

                    break;
                case ActionTypes.AdjustMonkeyCoordinates:
                    arguments.Add(args[0]);
                    arguments.Add(coords.Item1);
                    arguments.Add(coords.Item2);

                    break;
                case ActionTypes.WaitMilliseconds:
                    if (args[0] < 1)
                    {
                        arguments.Add(100);
                    }
                    else
                    {
                        arguments.Add(args[0]);
                    }
                    break;
                case ActionTypes.Jump:
                    if (args[0] > Digitalinstructions.Count) args[0] = Digitalinstructions.Count;
                    if (args[0] < 1 || Digitalinstructions.Count() == 0) args[0] = 1;
                    arguments.Add(args[0]);
                    break;
                case ActionTypes.StartFreeplay:
                    break;
                case ActionTypes.EndFreeplay:
                    break;
            }
            arguments.Add(roundTriggering);
            if (coinTriggering < 0)
            {
                arguments.Add(objectList[objectList.Count - 1].GetCurrentDeployCost(-1 * coinTriggering - 1));
            }
            else
            {
                arguments.Add(coinTriggering);
            }
            if (succeeded == true)
            {
                return ArgumentsToInstruction(arguments);
            }
            else
            {
                return ("", "");
            }
        }
        
        public bool AddInstruction(ActionTypes type, List<int> args, int roundTriggering, int coinTriggering, (int, int) coords)
        {
            try
            {
                (string, string) instructions = MakeInstruction(type, args, roundTriggering, coinTriggering, coords);
                if (instructions.Item1 != "")
                {
                    Displayinstructions.Add(instructions.Item1);
                    Digitalinstructions.Add(instructions.Item2);
                    // 猴子、英雄的相关指令需要刷新显示
                    if (type == ActionTypes.UpgradeMonkey ||
                        type == ActionTypes.SwitchMonkeyTarget ||
                        type == ActionTypes.SetMonkeyFunction ||
                        type == ActionTypes.SellMonkey)
                    {
                        RefreshDisplayInstructions(args[0]);
                    }
                    else if (type == ActionTypes.PlaceHero ||
                        type == ActionTypes.UpgradeHero ||
                        type == ActionTypes.PlaceHeroItem ||
                        type == ActionTypes.SwitchHeroTarget ||
                        type == ActionTypes.SetHeroFunction ||
                        type == ActionTypes.SellHero)
                    {
                        RefreshDisplayInstructions(-1);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("指令生成失败！");
                return false;
            }
        }

        public bool InsertInstructionPackage(int packageIndex, int count, int pos)
        {
            try
            {
                for (int i = 0; i < count; i++)
                {
                    switch (packageIndex)
                    {
                        case 0: // 022飞镖猴(模范)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 0, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 1: // 022回旋镖猴(模范)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 5: // 022海盗(模范)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 12, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 6: // 220潜艇(模范)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 11, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 7: // 022王牌飞行员(模范)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 13, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;

                        case 10: // 022法师猴(模范)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 20, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 11: // 022忍者猴(模范)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 22, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 15: // 022工程师(模范)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 33, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;

                        case 20: // 032飞镖猴(强力)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 0, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SwitchMonkeyTarget, new List<int> { ObjectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 21: // 204回旋镖(强力)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SwitchMonkeyTarget, new List<int> { ObjectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 22: // 024回旋镖(强力)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SwitchMonkeyTarget, new List<int> { ObjectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 23: // 031大炮(强力)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 2, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SwitchMonkeyTarget, new List<int> { ObjectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 24: // 050大炮(强力+出售)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 2, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SwitchMonkeyTarget, new List<int> { ObjectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UseAbility, new List<int> { 0, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SellMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 25: // 420冰猴
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 4, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 26: // 204图钉塔
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 3, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 27: // 420火锅
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 3, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;

                        case 30: // 302狙击猴(强力)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 10, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SwitchMonkeyTarget, new List<int> { ObjectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));

                            break;
                        case 31: // 204狙击猴(强力)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 10, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SwitchMonkeyTarget, new List<int> { ObjectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 32: // 042空投狙
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 10, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 33: // 004商船
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 12, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 34: // 042炮船
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 12, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 35: // 420驱逐舰(强力)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 12, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SwitchMonkeyTarget, new List<int> { ObjectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 36: // 050沙皇炸弹
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 13, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UseAbility, new List<int> { 0, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SellMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 37: // 204科曼奇
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 14, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 38: // 420长空
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 13, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SwitchMonkeyTarget, new List<int> { ObjectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 39: // 204潜艇
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 11, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 40: // 130德鲁伊
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 24, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 41: // 302超猴
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 21, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SwitchMonkeyTarget, new List<int> { ObjectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 42: // 203超猴
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 21, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 43: // 300炼金(出售)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 23, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SellMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 44: // 320炼金术士(出售)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 23, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SellMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 45: // 401人鱼猴
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 25, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 50: // 023香蕉农场
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 30, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 51: // 420香蕉农场
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 30, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 52: // 240刺钉工厂
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 31, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 53: // 032刺钉工厂(靠近)
                            InsertInstruction(pos++, ActionTypes.PlaceMonkey, new List<int> { 31, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.SwitchMonkeyTarget, new List<int> { ObjectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, ActionTypes.UpgradeMonkey, new List<int> { ObjectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;

                        default:
                            // 处理未定义的情况
                            break;
                    }
                }
            }
            catch
            {

            }
            return true;
        }

        public bool InsertInstruction(int index, ActionTypes type, List<int> args, int roundTriggering, int coinTriggering, (int, int) coords)
        {
            try
            {
                (string, string) instructions = MakeInstruction(type, args, roundTriggering, coinTriggering, coords);
                if (instructions.Item1 != "")
                {
                    Displayinstructions.Insert(index + 1, instructions.Item1);
                    Digitalinstructions.Insert(index + 1, instructions.Item2);
                    if (type == ActionTypes.UpgradeMonkey ||
                        type == ActionTypes.SwitchMonkeyTarget ||
                        type == ActionTypes.SetMonkeyFunction ||
                        type == ActionTypes.SellMonkey)
                    {
                        RefreshDisplayInstructions(args[0]);
                    }
                    else if (type == ActionTypes.PlaceHero ||
                        type == ActionTypes.UpgradeHero ||
                        type == ActionTypes.PlaceHeroItem ||
                        type == ActionTypes.SwitchHeroTarget ||
                        type == ActionTypes.SetHeroFunction ||
                        type == ActionTypes.SellHero)
                    {
                        RefreshDisplayInstructions(-1); // 刷新英雄
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("指令生成失败！");
                return false;
            }
        }

        public bool IfSame(int index, ActionTypes srcType, List<int> args)
        {
            bool same = false;
            if (index < 0) return false;
            if (index >= Digitalinstructions.Count) return false;
            List<int> arguments = GetArguments(index);
            ActionTypes destType = (ActionTypes)arguments[0];
            if (srcType != destType) return false;
            switch (srcType)
            {
                case ActionTypes.PlaceMonkey:
                case ActionTypes.UpgradeMonkey:
                case ActionTypes.SwitchMonkeyTarget:
                case ActionTypes.SetMonkeyFunction:
                case ActionTypes.SellMonkey:
                case ActionTypes.AdjustMonkeyCoordinates:
                    if (arguments[1] == args[0]) same = true;
                    break;
                case ActionTypes.UseAbility:
                case ActionTypes.SwitchSpeed:
                case ActionTypes.PlaceHero:
                case ActionTypes.UpgradeHero:
                case ActionTypes.PlaceHeroItem:
                case ActionTypes.SwitchHeroTarget:
                case ActionTypes.SetHeroFunction:
                case ActionTypes.SellHero:
                case ActionTypes.MouseClick:
                case ActionTypes.WaitMilliseconds:
                case ActionTypes.Jump:
                case ActionTypes.StartFreeplay:
                case ActionTypes.EndFreeplay:
                    same = true;
                    break;
            }
            return same;
        }

        private (string DisplayInstruction, string DigitalInstruction) ArgumentsToInstruction(List<int> arguments)
        {
            string digitalInstruction = "";
            string displayInstruction = "";
            for (int i = 0; i < arguments.Count; i++)
            {
                if (i != arguments.Count - 1)
                {
                    digitalInstruction += arguments[i].ToString() + " ";
                }
                else
                {
                    digitalInstruction += arguments[i].ToString();
                }
            }
            ActionTypes type = (ActionTypes)arguments[0];
        //PlaceMonkey = 0,                // 放置猴子
        //UpgradeMonkey = 1,              // 升级猴子
        //SwitchMonkeyTarget = 2,         // 切换猴子目标
        //UseAbility = 3,                 // 使用技能
        //SwitchSpeed = 4,                // 切换倍速
        //SellMonkey = 5,                 // 出售猴子
        //SetMonkeyFunction = 6,          // 设置猴子功能
        //PlaceHero = 7,                  // 放置英雄
        //UpgradeHero = 8,                // 升级英雄
        //PlaceHeroItem = 9,              // 英雄物品放置
        //SwitchHeroTarget = 10,          // 切换英雄目标
        //SetHeroFunction = 11,           // 设置英雄功能
        //SellHero = 12,                  // 出售英雄
        //MouseClick = 13,                // 鼠标点击
        //AdjustMonkeyCoordinates = 14,   // 修改猴子坐标
        //WaitMilliseconds = 15,          // 等待(ms)
        //StartFreeplay = 16,             // 开启自由游戏
        //EndFreeplay = 17,               // 结束自由游戏
        //Jump = 18,                      // 指令跳转 
        //QuickCommandBundle = 25         // 快捷指令包
            switch (type)
            {
                case ActionTypes.PlaceMonkey: // 放置指令
                    displayInstruction += Constants.GetTypeName((Monkeys)ObjectId[arguments[2]].Item1) + ObjectId[arguments[2]].Item2.ToString();
                    displayInstruction += "放置";
                    displayInstruction += "于(" + arguments[3].ToString() + ", " + arguments[4].ToString() + ")";

                    break;
                case ActionTypes.UpgradeMonkey: // 升级指令
                    displayInstruction += Constants.GetTypeName((Monkeys)ObjectId[arguments[1]].Item1) + ObjectId[arguments[1]].Item2.ToString();
                    displayInstruction += "升级至";
                    displayInstruction += objectList[arguments[1]].GetAllUpgradeLevel();

                    break;
                case ActionTypes.SwitchMonkeyTarget: // 切换目标指令
                    displayInstruction += Constants.GetTypeName((Monkeys)ObjectId[arguments[1]].Item1) + ObjectId[arguments[1]].Item2.ToString();
                    displayInstruction += "目标";
                    displayInstruction += Constants.TargetToChange[arguments[2]];
                    break;
                case ActionTypes.UseAbility: // 释放技能指令
                    displayInstruction += "释放技能";
                    displayInstruction += Constants.AbilityToDisplay[arguments[1]];
                    if (arguments.Count > 4)
                    {
                        displayInstruction += "于(" + arguments[2].ToString() + ", " + arguments[3].ToString() + ")";
                    }
                    break;
                case ActionTypes.SwitchSpeed: // 倍速指令
                    if (arguments[1] == 0)
                    {
                        displayInstruction += "切换倍速";
                    }
                    else if (arguments[1] == 1)
                    {
                        displayInstruction += "下一回合";
                    }

                    break;
                case ActionTypes.SellMonkey: // 出售指令
                    displayInstruction += "出售";
                    displayInstruction += Constants.GetTypeName((Monkeys)ObjectId[arguments[1]].Item1) + ObjectId[arguments[1]].Item2.ToString();

                    break;
                case ActionTypes.SetMonkeyFunction: // 设置猴子功能
                    displayInstruction += Constants.GetTypeName((Monkeys)ObjectId[arguments[1]].Item1) + ObjectId[arguments[1]].Item2.ToString();
                    displayInstruction += "更改功能";

                    if (arguments.Count > 4)
                    {
                        displayInstruction += "于(" + arguments[2].ToString() + ", " + arguments[3].ToString() + ")";
                    }

                    break;
                case ActionTypes.PlaceHero:
                    displayInstruction += "放置英雄";
                    displayInstruction += "于(" + arguments[1].ToString() + ", " + arguments[2].ToString() + ")";


                    break;
                case ActionTypes.UpgradeHero:
                    displayInstruction += "升级英雄";

                    break;
                case ActionTypes.PlaceHeroItem:
                    displayInstruction += "放置英雄技能面板物品";
                    displayInstruction += arguments[1].ToString();
                    if (arguments.Count > 4)
                    {
                        displayInstruction += "于(" + arguments[2].ToString() + ", " + arguments[3].ToString() + ")";
                    }

                    break;
                case ActionTypes.SwitchHeroTarget:
                    displayInstruction += "英雄目标";
                    displayInstruction += Constants.TargetToChange[arguments[1]];

                    break;
                case ActionTypes.SetHeroFunction:
                    displayInstruction += "更改英雄";
                    if (arguments[1] == 1)
                    {
                        displayInstruction += "功能2";
                    }
                    else
                    {
                        displayInstruction += "功能1";
                    }
                    if (arguments.Count > 4)
                    {
                        displayInstruction += "于(" + arguments[2].ToString() + ", " + arguments[3].ToString() + ")";
                    }

                    break;
                case ActionTypes.SellHero:
                    displayInstruction += "出售英雄";

                    break;
                case ActionTypes.MouseClick:
                    displayInstruction += "鼠标点击";
                    displayInstruction += "(" + arguments[2].ToString() + ", " + arguments[3].ToString() + ")";
                    displayInstruction += arguments[1].ToString() + "次";

                    break;
                case ActionTypes.AdjustMonkeyCoordinates:
                    displayInstruction += "修改" + Constants.GetTypeName((Monkeys)ObjectId[arguments[1]].Item1) + ObjectId[arguments[1]].Item2.ToString() + "坐标";
                    displayInstruction += "于(" + arguments[2].ToString() + ", " + arguments[3].ToString() + ")";

                    break;
                case ActionTypes.WaitMilliseconds:
                    displayInstruction += "等待" + arguments[1].ToString() + "ms";

                    break;
                case ActionTypes.Jump:
                    displayInstruction += "跳转到第" + arguments[1].ToString() + "条指令";

                    break;
                case ActionTypes.StartFreeplay:
                    displayInstruction += "开始自由游戏";

                    break;
                case ActionTypes.EndFreeplay:
                    displayInstruction += "结束自由游戏";
                    
                    break;
            }
            return (displayInstruction, digitalInstruction);
        }

        public bool ModifyInstruction(int index, ActionTypes type, List<int> args, int roundTriggering, int coinTriggering, (int, int) coords)
        {
            if (IfSame(index, type, args))
            {
                List<int> arguments = GetArguments(index);

                switch (type)
                {
                    case ActionTypes.PlaceMonkey:
                        arguments[3] = coords.Item1;
                        arguments[4] = coords.Item2;
                        arguments[5] = roundTriggering;
                        arguments[6] = coinTriggering;
                        break;
                    case ActionTypes.UpgradeMonkey:
                        arguments[2] = args[1];
                        arguments[4] = roundTriggering;
                        arguments[5] = coinTriggering;
                        break;
                    case ActionTypes.SwitchMonkeyTarget:
                        arguments[2] = args[1];
                        arguments[3] = roundTriggering;
                        arguments[4] = coinTriggering;
                        break;
                    case ActionTypes.UseAbility:
                        arguments[1] = args[0];
                        if (args[1] == 0)
                        {
                            if (arguments.Count == 4)
                            {
                                arguments[2] = roundTriggering;
                                arguments[3] = coinTriggering;
                            }
                            else
                            {
                                arguments.RemoveAt(2);
                                arguments.RemoveAt(2);
                            }
                        }
                        else
                        {
                            if (arguments.Count == 4)
                            {
                                arguments[2] = coords.Item1;
                                arguments[3] = coords.Item2;
                                arguments.Add(roundTriggering);
                                arguments.Add(coinTriggering);
                            }
                            else
                            {
                                arguments[2] = coords.Item1;
                                arguments[3] = coords.Item2;
                                arguments[4] = roundTriggering;
                                arguments[5] = coinTriggering;
                            }
                        }
                        break;
                    case ActionTypes.SwitchSpeed: // 倍速指令
                        arguments[1] = args[0];
                        arguments[2] = roundTriggering;
                        arguments[3] = coinTriggering;
                        break;
                    case ActionTypes.SellMonkey: // 出售指令
                        arguments[2] = roundTriggering;
                        arguments[3] = coinTriggering;
                        break;
                    case ActionTypes.SetMonkeyFunction: // 设置猴子功能
                        if (args[1] == 0)
                        {
                            if (arguments.Count == 4)
                            {
                                arguments[2] = roundTriggering;
                                arguments[3] = coinTriggering;
                            }
                            else
                            {
                                arguments.RemoveAt(2);
                                arguments.RemoveAt(2);
                            }
                        }
                        else
                        {
                            if (arguments.Count == 4)
                            {
                                arguments[2] = coords.Item1;
                                arguments[3] = coords.Item2;
                                arguments.Add(roundTriggering);
                                arguments.Add(coinTriggering);
                            }
                            else
                            {
                                arguments[2] = coords.Item1;
                                arguments[3] = coords.Item2;
                                arguments[4] = roundTriggering;
                                arguments[5] = coinTriggering;
                            }
                        }
                        break;
                    case ActionTypes.PlaceHero:  // 放置英雄
                    case ActionTypes.UpgradeHero:  // 升级英雄
                        arguments[1] = coords.Item1;
                        arguments[2] = coords.Item2;
                        arguments[3] = roundTriggering;
                        arguments[4] = coinTriggering;
                        break;
                    case ActionTypes.PlaceHeroItem: // 英雄物品放置
                        arguments[1] = args[0];
                        if (args[1] == 0)
                        {
                            if (arguments.Count == 4)
                            {
                                arguments[2] = roundTriggering;
                                arguments[3] = coinTriggering;
                            }
                            else
                            {
                                arguments.RemoveAt(5);
                                arguments.RemoveAt(4);
                            }
                        }
                        else
                        {
                            arguments[3] = coords.Item1;
                            arguments[4] = coords.Item2;
                            arguments.Add(roundTriggering);
                            arguments.Add(coinTriggering);
                        }
                        break;
                    case ActionTypes.SwitchHeroTarget: // 切换英雄目标
                        arguments[1] = args[1];
                        arguments[2] = roundTriggering;
                        arguments[3] = coinTriggering;
                        break;
                    case ActionTypes.SetHeroFunction: // 设置英雄功能
                        arguments[1] = args[0];
                        if (args[1] == 0)
                        {
                            if (arguments.Count == 4)
                            {
                                arguments[2] = roundTriggering;
                                arguments[3] = coinTriggering;
                            }
                            else
                            {
                                arguments[2] = roundTriggering;
                                arguments[3] = coinTriggering;
                                arguments.RemoveAt(5);
                                arguments.RemoveAt(4);
                            }
                        }
                        else
                        {
                            if (arguments.Count == 4)
                            {
                                arguments[2] = coords.Item1;
                                arguments[3] = coords.Item2;
                                arguments.Add(roundTriggering);
                                arguments.Add(coinTriggering);
                            }
                            else
                            {
                                arguments[2] = coords.Item1;
                                arguments[3] = coords.Item2;
                                arguments[4] = roundTriggering;
                                arguments[5] = coinTriggering;
                            }
                        }
                        break;
                    case ActionTypes.SellHero: // 出售英雄
                        arguments[1] = roundTriggering;
                        arguments[2] = coinTriggering;
                        break;
                    case ActionTypes.MouseClick: // 鼠标点击
                        arguments[1] = args[0];
                        arguments[2] = coords.Item1;
                        arguments[3] = coords.Item2;
                        arguments[4] = roundTriggering;
                        arguments[5] = coinTriggering;

                        break;
                    case ActionTypes.AdjustMonkeyCoordinates: // 修改猴子坐标
                        arguments[2] = coords.Item1;
                        arguments[3] = coords.Item2;
                        arguments[4] = roundTriggering;
                        arguments[5] = coinTriggering;
                        break;
                    case ActionTypes.WaitMilliseconds: // 等待(ms)
                        arguments[1] = args[0];
                        arguments[2] = roundTriggering;
                        arguments[3] = coinTriggering;
                        break;
                    case ActionTypes.Jump: // 跳转指令
                        if (args[0] < 1) args[0] = 1;
                        arguments[1] = args[0];
                        arguments[2] = roundTriggering;
                        arguments[3] = coinTriggering;
                        break;
                    case ActionTypes.StartFreeplay: // 开启自由游戏
                        arguments[1] = roundTriggering;
                        arguments[2] = coinTriggering;
                        break;
                    case ActionTypes.EndFreeplay: // 结束自由游戏
                        arguments[1] = roundTriggering;
                        arguments[2] = coinTriggering;
                        break;
                }
                (string, string) Instruction = ArgumentsToInstruction(arguments);
                if (Instruction.Item1 != "")
                {
                    Displayinstructions[index] = (Instruction.Item1);
                    Digitalinstructions[index] = (Instruction.Item2);
                    if (type == ActionTypes.UpgradeMonkey|| 
                        type == ActionTypes.SwitchMonkeyTarget|| 
                        type == ActionTypes.SetMonkeyFunction || 
                        type == ActionTypes.SellMonkey)
                    {
                        RefreshDisplayInstructions(args[0]);
                    }
                    if (type == ActionTypes.PlaceHero || 
                        type == ActionTypes.UpgradeHero || 
                        type == ActionTypes.PlaceHeroItem || 
                        type == ActionTypes.SwitchHeroTarget || 
                        type == ActionTypes.SetHeroFunction || 
                        type == ActionTypes.SellHero)
                    {
                        RefreshDisplayInstructions(-1);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void DeleteInstruction(int index)
        {
            List<int> args = GetArguments(index);

            if (!Enum.IsDefined(typeof(ActionTypes), args[0])) return;
            ActionTypes type = (ActionTypes)args[0];
            int monkeyIndex;
            int count;
            List<int> currentArguments;
            ActionTypes currentType;
            switch (type)
            {
                case ActionTypes.PlaceMonkey:
                    monkeyIndex = args[2];
                    objectList[monkeyIndex].IsDelete = true;
                    for (count = Digitalinstructions.Count() - 1; count >= index; count--)
                    {
                        currentArguments = GetArguments(count);
                        currentType = (ActionTypes)currentArguments[0];
                        if (currentType == ActionTypes.UpgradeMonkey ||
                            currentType == ActionTypes.SwitchMonkeyTarget ||
                            currentType == ActionTypes.SetMonkeyFunction ||
                            currentType == ActionTypes.SellMonkey)
                        {
                            if (currentArguments[1] == monkeyIndex)
                            {
                                Digitalinstructions.RemoveAt(count);
                                Displayinstructions.RemoveAt(count);
                            }
                        }
                    }
                    Digitalinstructions.RemoveAt(index);
                    Displayinstructions.RemoveAt(index);
                    break;
                case ActionTypes.UpgradeMonkey:
                    Digitalinstructions.RemoveAt(index);
                    Displayinstructions.RemoveAt(index);
                    RefreshDisplayInstructions(args[1]);
                    break;
                case ActionTypes.SellMonkey: // 出售指令
                    objectList[args[1]].exsitence = true;
                    Digitalinstructions.RemoveAt(index);
                    Displayinstructions.RemoveAt(index);
                    break;
                case ActionTypes.PlaceHero:  // 放置英雄
                    hero.exsitence = false;
                    for (count = index; count < Digitalinstructions.Count(); count++)
                    {
                        currentArguments = GetArguments(count);
                        currentType = (ActionTypes)currentArguments[0];
                        if (currentType == ActionTypes.PlaceHero ||
                            currentType == ActionTypes.UpgradeHero ||
                            currentType == ActionTypes.PlaceHeroItem ||
                            currentType == ActionTypes.SwitchHeroTarget ||
                            currentType == ActionTypes.SetHeroFunction ||
                            currentType == ActionTypes.SellHero)
                        {
                            Digitalinstructions.RemoveAt(count);
                            Displayinstructions.RemoveAt(count);
                            count--;
                            if (currentType == ActionTypes.SellHero) break;
                        }
                    }
                    break;
                case ActionTypes.SwitchHeroTarget:  // 切换英雄目标
                    hero.exsitence = true;
                    Digitalinstructions.RemoveAt(index);
                    Displayinstructions.RemoveAt(index);
                    break;
                default:
                    Digitalinstructions.RemoveAt(index);
                    Displayinstructions.RemoveAt(index);
                    break;
            }
        }

        public bool ChangeInstructionPosition(int index)
        {
            bool IsChange = true;
            if (index == Digitalinstructions.Count() - 1) return false;

            List<int> args1 = GetArguments(index);
            List<int> args2 = GetArguments(index + 1);

            if (!Enum.IsDefined(typeof(ActionTypes), args1[0]) ||!Enum.IsDefined(typeof(ActionTypes), args2[0])) return false;

            ActionTypes type1 = (ActionTypes)args1[0];
            ActionTypes type2 = (ActionTypes)args2[0];

            if (type1 == ActionTypes.PlaceMonkey && 
                type2 == ActionTypes.UpgradeMonkey && 
                args1[2] == args2[1])  // 放置和升级不可交换
            {
                IsChange = false;
            }
            else if (type1 == ActionTypes.UpgradeMonkey && 
                type2 == ActionTypes.UpgradeMonkey && 
                args1[1] == args2[1])
            {
                (Digitalinstructions[index + 1], Digitalinstructions[index]) = (Digitalinstructions[index], Digitalinstructions[index + 1]);
                RefreshDisplayInstructions(args1[1]);
            }
            else if (type1 == ActionTypes.PlaceMonkey &&
                type2 == ActionTypes.SellMonkey && 
                args1[2] == args2[1])
            {
                IsChange = false;
            }
            else if (type1 == ActionTypes.UpgradeMonkey &&
                type2 == ActionTypes.SellMonkey
                && args1[1] == args2[1])
            {
                IsChange = false;
            }
            else if (type1 == ActionTypes.PlaceHero && (
                type2 == ActionTypes.UpgradeHero || 
                type2 == ActionTypes.PlaceHeroItem || 
                type2 == ActionTypes.SwitchHeroTarget || 
                type2 == ActionTypes.SetHeroFunction || 
                type2 == ActionTypes.SellHero))
            {
                IsChange = false;
            }
            else if ((type1 == ActionTypes.PlaceHero || 
                type1 == ActionTypes.UpgradeHero || 
                type1 == ActionTypes.PlaceHeroItem || 
                type1 == ActionTypes.SwitchHeroTarget || 
                type1 == ActionTypes.SetHeroFunction) &&
                type2 == ActionTypes.SellHero)
            {
                IsChange = false;
            }
            else if (type1 == ActionTypes.StartFreeplay && 
                type2 == ActionTypes.EndFreeplay)
            {
                IsChange = false;
            }
            else
            {
                (Digitalinstructions[index + 1], Digitalinstructions[index]) = (Digitalinstructions[index], Digitalinstructions[index + 1]);
                (Displayinstructions[index + 1], Displayinstructions[index]) = (Displayinstructions[index], Displayinstructions[index + 1]);
            }
            return IsChange;
        }

        public void RefreshDisplayInstructions(int monkeyId)
        {
            List<int> deleteQue = new List<int>();
            if (monkeyId == -1)
            {
                hero.exsitence = false;
                for (int i = 0; i < Digitalinstructions.Count(); i++)
                {
                    List<int> arguments = GetArguments(i);
                    ActionTypes type = (ActionTypes)arguments[0];
                    if (type == ActionTypes.PlaceHero)
                    {
                        if (hero.exsitence == false)
                        {
                            hero.exsitence = true;
                        }
                        else
                        {
                            deleteQue.Add(i);
                        }
                    }
                    if ((type == ActionTypes.UpgradeHero ||
                        type == ActionTypes.PlaceHeroItem ||
                        type == ActionTypes.SwitchHeroTarget ||
                        type == ActionTypes.SetHeroFunction) && 
                        hero.exsitence == false)
                    {
                        deleteQue.Add(i);
                    }
                    if (type == ActionTypes.SellHero)
                    {
                        if (hero.exsitence == false)
                        {
                            deleteQue.Add(i);
                        }
                        else
                        {
                            hero.exsitence = false;
                        }
                    }
                }
            }
            else
            {
                objectList[monkeyId].ClearUpgrade();
                objectList[monkeyId].exsitence = true;
                for (int i = 0; i < Digitalinstructions.Count(); i++)
                {
                    List<int> arguments = GetArguments(i);
                    ActionTypes type = (ActionTypes)arguments[0];
                    if (type == ActionTypes.UpgradeMonkey && 
                        monkeyId == arguments[1])
                    {
                        if (objectList[monkeyId].exsitence == false)
                        {
                            deleteQue.Add(i);
                        }
                        else
                        {
                            objectList[monkeyId].Upgrade(arguments[2]);
                            arguments[3] = objectList[monkeyId].GetUpgradeLevel(arguments[2]);
                            Displayinstructions[i] = ArgumentsToInstruction(arguments).DisplayInstruction;
                            Digitalinstructions[i] = ArgumentsToInstruction(arguments).DigitalInstruction;
                        }
                    }
                    else if (type == ActionTypes.SwitchMonkeyTarget &&
                        monkeyId == arguments[1])
                    {
                        if (objectList[monkeyId].exsitence == false)
                        {
                            deleteQue.Add(i);
                        }
                    }
                    else if (type == ActionTypes.SetMonkeyFunction && 
                        monkeyId == arguments[1])
                    {
                        if (objectList[monkeyId].exsitence == false)
                        {
                            deleteQue.Add(i);
                        }
                    }
                    else if (type == ActionTypes.SellMonkey && 
                        monkeyId == arguments[1])
                    {
                        bool succeeded = objectList[monkeyId].Sale();
                        if (succeeded == false)
                        {
                            deleteQue.Add(i);
                        }
                    }
                }
            }
            for (int i = deleteQue.Count() - 1; i >= 0; i--)
            {
                Digitalinstructions.RemoveAt(deleteQue[i]);
                Displayinstructions.RemoveAt(deleteQue[i]);
            }
        }

        public List<int> GetArguments(int index) 
        {
            if (index < 0 || index >= Digitalinstructions.Count) return null;
            return Digitalinstructions[index].Split(' ').Select(Int32.Parse).ToList();
        }

        public static ScriptInstructionInfo GetScriptInstructionInfo(string digitalInstruction)
        {
            List<int> arguments = digitalInstruction.Split(' ').Select(Int32.Parse).ToList();
            ActionTypes type = (ActionTypes)arguments[0];
            return new ScriptInstructionInfo
            {
                Type = type,
                Arguments = arguments,
                CashTrigger = arguments[arguments.Count() - 1],
                RoundTrigger = arguments[arguments.Count() - 2],
                IsRoundMet = true,
                IsCashMet = true
            };
        }

        public string SaveToJson(string scriptName)
        {
            string filePath = $@"data\我的脚本\{Constants.GetTypeName(SelectedMap)}\{Constants.GetTypeName(SelectedDifficulty)}\" + scriptName + ".btd6";
            string directoryPath = Path.GetDirectoryName(filePath);
            //string filePath = "你好.json";
            var partialInfo = new
            {
                SelectedMap,
                SelectedDifficulty,
                SelectedMode,
                SelectedHero,
                AnchorCoords,
                ObjectCount,
                ObjectId,
                Displayinstructions,
                Digitalinstructions
            };

            string jsonString = JsonConvert.SerializeObject(partialInfo, Formatting.Indented);
            if (!Directory.Exists(directoryPath))
            {

                Directory.CreateDirectory(directoryPath);
            }
            File.WriteAllText(filePath, jsonString);
            return directoryPath;
        }

        public void RepairScript()
        {
            upgradeCount = new Dictionary<int, string>();
            for (int i = 0; i < ObjectId.Count; i++)
            {
                string jsonString = File.ReadAllText("Monkey.json");
                MonkeyTowerClass newObject = JsonConvert.DeserializeObject<MonkeyTowerClass>(jsonString);
                objectList.Add(newObject);
            }
            for (int i = 0; i < Digitalinstructions.Count; i++) 
            {
                List<int> args = GetArguments(i);
                ActionTypes type = (ActionTypes)args[0];
                switch (type)
                {
                    case ActionTypes.PlaceMonkey:
                        objectList[args[2]].IsDelete = false;
                        objectList[args[2]].SetCoordinates((args[3], args[4]));
                        break;
                    case ActionTypes.UpgradeMonkey:
                        objectList[args[1]].Upgrade(args[2]);
                        upgradeCount.Add(i, objectList[args[1]].GetAllUpgradeLevel());

                        // 修复升级指令，添加具体升级的等级参数
                        if (args.Count() == 5) // 旧版本没有升级等级参数
                        {
                            args.Insert(3, objectList[args[1]].GetUpgradeLevel(args[2]));
                            Displayinstructions[i] = ArgumentsToInstruction(args).DisplayInstruction;
                            Digitalinstructions[i] = ArgumentsToInstruction(args).DigitalInstruction;
                        }
                        else // 新版本有升级等级参数
                        {
                            args[3] = objectList[args[1]].GetUpgradeLevel(args[2]);
                            Displayinstructions[i] = ArgumentsToInstruction(args).DisplayInstruction;
                            Digitalinstructions[i] = ArgumentsToInstruction(args).DigitalInstruction;
                        }
                        break;
                    case ActionTypes.SellMonkey:
                        objectList[args[1]].Sale();
                        break;
                    case ActionTypes.PlaceHero:
                        hero.exsitence = true;
                        break;
                    case ActionTypes.SellHero:
                        hero.exsitence = false;
                        break;
                }
            }
            SaveToJson(ScriptName);
        }

        private string GetMoveInstruction(int argument1, int argument2, bool ifClick)
        {
            if (!ifClick)
            {
                return "1 " + argument1.ToString() + " " + argument2.ToString();
            }
            else
            {
                return "11 " + argument1.ToString() + " " + argument2.ToString();
            }
        }

        public void Compile(ScriptSettings settings)
        {
            bool ifFast = false;
            HotKey hotKey1, hotKey2, hotKey3;
            int count = 0;
            compilerDirective = new List<List<string>>();
            for (int i = 0; i < Digitalinstructions.Count; i++)
            {
                string digitalInstruction = Digitalinstructions[i];
                List<string> miniDirective = new List<string>();
                List<int> arguments = digitalInstruction.Split(' ').Select(Int32.Parse).ToList();
                
                int coinTrigger = arguments.Last();
                arguments.RemoveAt(arguments.Count() - 1);
                int roundTrigger = arguments.Last();
                arguments.RemoveAt(arguments.Count() - 1);
                miniDirective.Add("0 " + roundTrigger.ToString() + " " + coinTrigger.ToString());
                ActionTypes type = (ActionTypes)arguments[0];
                switch (type)
                {
                    case ActionTypes.PlaceMonkey: // 放置猴子
                        hotKey1 = settings.GetHotKey((Monkeys)arguments[1]);
                        // move
                        miniDirective.Add("1 " + arguments[3].ToString() + " " + arguments[4].ToString());
                        if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
                        miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                        if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

                        // click
                        miniDirective.Add("2");

                        miniDirective.Add("26");

                        break;
                    case ActionTypes.UpgradeMonkey: // 升级猴子
                        if (!IfLast(i))
                        {
                            // move
                            miniDirective.Add(GetMoveInstruction(objectList[arguments[1]].coordinates.Item1, objectList[arguments[1]].coordinates.Item2, ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }

                        if (arguments[2] == 0)
                        {
                            hotKey1 = settings.GetHotKey(HotkeyAction.UpgradeTopPath);
                            miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                        }
                        if (arguments[2] == 1)
                        {
                            hotKey1 = settings.GetHotKey(HotkeyAction.UpgradeMiddlePath);
                            miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                        }
                        if (arguments[2] == 2)
                        {
                            hotKey1 = settings.GetHotKey(HotkeyAction.UpgradeBottomPath);
                            miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                        }
                        if (!IfNext(i))
                        {
                            // move 
                            miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }

                        break;
                    case ActionTypes.SwitchMonkeyTarget: // 切换目标
                        hotKey1 = settings.GetHotKey(HotkeyAction.SwitchTarget);
                        hotKey2 = settings.GetHotKey(HotkeyAction.ReverseSwitchTarget);
                        // move
                        miniDirective.Add(GetMoveInstruction(objectList[arguments[1]].coordinates.Item1, objectList[arguments[1]].coordinates.Item2, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }
                        if (arguments[2] >= 0 && arguments[2] <= 2) // 右改1，2，3次
                        {
                            if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
                            if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
                            if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
                            for (count = 0; count <= arguments[2]; count++)
                            {
                                miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                            }
                            if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
                            if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
                            if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }
                        }
                        else // 左改1，2，3次
                        {
                            if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
                            if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
                            if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
                            for (count = 0; count <= arguments[2] - 3; count++)
                            miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
                            if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
                            if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
                            if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }
                        }

                        // move
                        miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }
                        break;
                    case ActionTypes.UseAbility: // 使用技能
                        if (arguments[1] < 9) 
                        {
                            miniDirective.Add("24 " + (arguments[1]).ToString() + " " + (arguments[1] + 49).ToString());
                            miniDirective.Add("25 " + (arguments[1]).ToString());
                        }
                        else if (arguments[1] == 9)
                        {
                            miniDirective.Add("24 9 48");
                            miniDirective.Add("25 9");
                        }
                        else if (arguments[1] == 10)
                        {
                            miniDirective.Add("24 10 189");
                            miniDirective.Add("25 10");
                        }
                        else if (arguments[1] == 11)
                        {
                            miniDirective.Add("24 11 187");
                            miniDirective.Add("25 11");
                        }
                        // 选择释放坐标
                        if (arguments.Count > 2)
                        {
                            // move
                            miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }
                        break;
                    case ActionTypes.SwitchSpeed: // 切换倍速
                        hotKey1 = settings.GetHotKey(HotkeyAction.ChangeSpeed);
                        hotKey2 = settings.GetHotKey(HotkeyAction.NextRound);
                        if (arguments[1] == 0) // 快/慢切换
                        {
                            if (!IfLast(i))
                            {
                                if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
                                if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
                                if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
                            }
                            miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                            if (!IfNext(i))
                            {
                                if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
                                if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
                                if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }
                            }
                        }
                        if (arguments[1] == 1) // 竞速下一回合
                        {
                            if (!IfLast(i))
                            {
                                if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
                                if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
                                if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
                            }
                            miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
                            if (!IfNext(i))
                            {
                                if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
                                if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
                                if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }
                            }
                        }

                        break;
                    case ActionTypes.SellMonkey: // 出售猴子
                        hotKey1 = settings.GetHotKey(HotkeyAction.Sell);
                        // move
                        miniDirective.Add(GetMoveInstruction(objectList[arguments[1]].coordinates.Item1, objectList[arguments[1]].coordinates.Item2, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }

                        if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
                        miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                        if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

                        break;
                    case ActionTypes.SetMonkeyFunction: // 设置猴子功能
                        hotKey1 = settings.GetHotKey(HotkeyAction.SetFunction1);
                        // move
                        miniDirective.Add(GetMoveInstruction(objectList[arguments[1]].coordinates.Item1, objectList[arguments[1]].coordinates.Item2, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }

                        if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
                        miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                        if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); };
                        if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

                        if (arguments.Count > 2)
                        {
                            // move
                            miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }
                        // move
                        miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }

                        break;
                    case ActionTypes.PlaceHero: // 放置英雄
                        miniDirective.Add("23");

                        miniDirective.Add(GetMoveInstruction(1715, 230, false));

                        miniDirective.Add("2");

                        // move
                        miniDirective.Add(GetMoveInstruction(arguments[1], arguments[2], false));

                        // click
                        miniDirective.Add("2");

                        miniDirective.Add("26");


                        break;
                    case ActionTypes.UpgradeHero: // 升级英雄
                        hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
                        hotKey2 = settings.GetHotKey(HotkeyAction.UpgradeTopPath);
                        if (!IfLast(i))
                        {
                            if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
                            if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
                            if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
                            miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                            if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
                            if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
                            if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }
                        }

                        if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
                        if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
                        if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
                        miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
                        if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
                        if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
                        if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }

                        if (!IfNext(i))
                        {
                            // move
                            miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }

                        break;
                    case ActionTypes.PlaceHeroItem: // 放置英雄物品
                        // 点击英雄
                        hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
                        if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
                        miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                        if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

                        hotKey2 = settings.GetHotKey((HeroObjectTypes)arguments[1]);
                        if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
                        if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
                        if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
                        miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
                        if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
                        if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
                        if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }

                        if (arguments.Count >  2)
                        {
                            // move
                            miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }
                        // move
                        miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }
                        break;
                    case ActionTypes.SwitchHeroTarget: // 切换英雄目标
                        // 点击英雄
                        hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
                        hotKey2 = settings.GetHotKey(HotkeyAction.SwitchTarget);
                        hotKey3 = settings.GetHotKey(HotkeyAction.ReverseSwitchTarget);
                        if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
                        miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                        if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

                        if (arguments[1] >= 0 && arguments[1] <= 2) // 右改1，2，3次
                        {
                            if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
                            if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
                            if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
                            for (count = 0; count <= arguments[1]; count++)
                            {
                                miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
                            }
                            if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
                            if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
                            if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }
                        }
                        else // 左改1，2，3次
                        {
                            if (hotKey3.Alt) { miniDirective.Add("6 " + "18"); }
                            if (hotKey3.Control) { miniDirective.Add("6 " + "17"); }
                            if (hotKey3.Shift) { miniDirective.Add("6 " + "16"); }
                            for (count = 0; count <= arguments[1] - 3; count++)
                                miniDirective.Add("8 " + ((int)hotKey3.MainKey).ToString());
                            if (hotKey3.Alt) { miniDirective.Add("7 " + "18"); }
                            if (hotKey3.Control) { miniDirective.Add("7 " + "17"); }
                            if (hotKey3.Shift) { miniDirective.Add("7 " + "16"); }
                        }

                        // move
                        miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }
                        break;
                    case ActionTypes.SetHeroFunction: // 设置英雄功能
                        // 点击英雄
                        hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
                        hotKey2 = settings.GetHotKey(HotkeyAction.SetFunction1);
                        hotKey3 = settings.GetHotKey(HotkeyAction.SetFunction2);
                        if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
                        miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                        if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

                        if (arguments[1] == 0)
                        {
                            if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
                            if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
                            if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
                            miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
                            if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
                            if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
                            if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }
                        }
                        else
                        {
                            if (hotKey3.Alt) { miniDirective.Add("6 " + "18"); }
                            if (hotKey3.Control) { miniDirective.Add("6 " + "17"); }
                            if (hotKey3.Shift) { miniDirective.Add("6 " + "16"); }
                            miniDirective.Add("8 " + ((int)hotKey3.MainKey).ToString());
                            if (hotKey3.Alt) { miniDirective.Add("7 " + "18"); }
                            if (hotKey3.Control) { miniDirective.Add("7 " + "17"); }
                            if (hotKey3.Shift) { miniDirective.Add("7 " + "16"); }
                        }

                        if (arguments.Count > 2)
                        {
                            // move
                            miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }
                        // move
                        miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast ));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }
                        break;
                    case ActionTypes.SellHero: // 出售英雄
                        hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
                        hotKey2 = settings.GetHotKey(HotkeyAction.Sell);
                        if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
                        miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
                        if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
                        if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
                        if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

                        if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
                        if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
                        if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
                        miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
                        miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
                        if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
                        if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
                        if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }
                        break;
                    case ActionTypes.MouseClick: // 鼠标点击指令
                        // move
                        miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], false));
                        for (int j = 0; j < arguments[1]; j++)
                        {
                            miniDirective.Add("2");
                        }
                        // click
                        break;
                    case ActionTypes.AdjustMonkeyCoordinates:
                        miniDirective.Add("2");
                        objectList[arguments[1]].SetCoordinates((arguments[2], arguments[3]));
                        break;
                    case ActionTypes.WaitMilliseconds:
                        int times = (arguments[1] / settings.OperationInterval > 1 ? arguments[1] / settings.OperationInterval : 1);
                        for (int k = 0; k < times; k++)
                        {
                            miniDirective.Add("10");
                        }
                        break;
                    case ActionTypes.Jump:
                        miniDirective.Add("16 " + arguments[1].ToString());
                        break;
                    case ActionTypes.StartFreeplay:
                        miniDirective.Add("21");
                        times = (3000 / settings.OperationInterval > 1 ? 3000 / settings.OperationInterval : 1);
                        for (int m = 0; m < times; m++)
                        {
                            miniDirective.Add("10");
                        }
                        break;
                    case ActionTypes.EndFreeplay:
                        times = (4500 / settings.OperationInterval > 1 ? 4500 / settings.OperationInterval : 1);
                        for (int m = 0; m < times; m++)
                        {
                            miniDirective.Add("10");
                        }

                        miniDirective.Add("1 " + "1600 " + "45");
                        miniDirective.Add("2");

                        for (int m = 0; m < times; m++)
                        {
                            miniDirective.Add("10");
                        }
                        //miniDirective.Add("20");
                        break;
                }
                compilerDirective.Add(miniDirective);
            }
        }

        public bool IfNext(int currentIndex)
        {
            List<int> arguments1 = Digitalinstructions[currentIndex].Split(' ').Select(Int32.Parse).ToList();
            if (currentIndex + 1 == Digitalinstructions.Count)
            {
                return false;
            }
            List<int> arguments2 = Digitalinstructions[currentIndex + 1].Split(' ').Select(Int32.Parse).ToList();
            ActionTypes type1 = (ActionTypes)arguments1[0];
            ActionTypes type2 = (ActionTypes)arguments2[0];
            if (type1 == type2)
            {
                if ((type1 == ActionTypes.UpgradeMonkey || 
                    type2 == ActionTypes.SwitchSpeed) && 
                    arguments1[1] == arguments2[1])
                {
                    return true;
                }
                if (type1 == ActionTypes.UpgradeHero)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IfLast(int currentIndex)
        {
            List<int> arguments1 = Digitalinstructions[currentIndex].Split(' ').Select(Int32.Parse).ToList();
            if (currentIndex == 0)
            {
                return false;
            }
            List<int> arguments2 = Digitalinstructions[currentIndex - 1].Split(' ').Select(Int32.Parse).ToList();
            ActionTypes type1 = (ActionTypes)arguments1[0];
            ActionTypes type2 = (ActionTypes)arguments2[0];
            if (type1 == type2)
            {
                if ((type1 == ActionTypes.UpgradeMonkey ||
                    type2 == ActionTypes.SwitchSpeed) &&
                    arguments1[1] == arguments2[1])
                {
                    return true;
                }
                if (type1 == ActionTypes.UpgradeHero)
                {
                    return true;
                }
            }
            return false;
        }

        public static string ExistScript(string selectDir1, string selectDir2, string scriptName)
        {
            string filePath1 = Path.GetFullPath(Path.Combine("data", "我的脚本", selectDir1, selectDir2, scriptName + ".btd6"));
            string filePath2 = Path.GetFullPath(Path.Combine("data", "我的脚本", selectDir1, selectDir2, scriptName + ".json"));

            if (File.Exists(filePath1))
            {
                return filePath1;
            }
            else if (File.Exists(filePath2))
            {
                return filePath2;
            }
            else
            {
                return null;
            }
        }

        public void SaveDirectiveToJson()
        {
            string filePath = $@"微指令.json";
            //string filePath = "你好.json";
            var partialInfo = new
            {
                compilerDirective
            };
            string jsonString = JsonConvert.SerializeObject(partialInfo, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
        }

        public void Clear()
        {
            hero.exsitence = false;
            ObjectId.Clear();
            ObjectCount.Clear();
            Displayinstructions.Clear();
            Digitalinstructions.Clear();
            objectList.Clear();
        }


    }
}
