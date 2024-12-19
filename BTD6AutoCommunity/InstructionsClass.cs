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

namespace BTD6AutoCommunity
{
    internal class InstructionsClass
    {
        public int selectedMap { get; set; }
        public int selectedDifficulty { get; set; }
        public int selectedMode { get; set; }
        public int selectedHero { get; set; }
        public (int, int) anchorCoords { get; set; }
        public List<int> objectCount { get; set; } // 猴子的数量

        public Dictionary<int, string> upgradeCount;
        public List<(int, int)> objectId { get; set; } // 猴子的ID
        public List<MonkeyTowerClass> objectList; // 每一个猴子对象
        public HeroClass hero;
        public List<(int, int)> triggering;
        public List<List<string>> compilerDirective;

        public Dictionary<int, string> monkeysToDisplay;
        public Dictionary<int, string> objectToDisplay;
        public Dictionary<int, string> typeToDisplay;
        public Dictionary<int, string> abilityToDisplay;
        public Dictionary<int, string> targetToChange;
        public Dictionary<int, string> instructionPackages;

        public Dictionary<int, string> maps;
        public Dictionary<int, string> heros;
        public Dictionary<int, string> difficulty;
        public Dictionary<int, string> mode;
        public List<string> displayinstructions { get; set; }
        public List<string> digitalinstructions { get; set; }
        public InstructionsClass() 
        {
            displayinstructions = new List<string>();
            digitalinstructions = new List<string>();

            typeToDisplay = new Dictionary<int, string>
            {
                { 0, "放置猴子" },
                { 1, "升级猴子" },
                { 2, "切换猴子目标" },
                { 3, "使用技能" },
                { 4, "切换倍速" },
                { 5, "出售猴子" },
                { 6, "设置猴子功能"},
                { 7, "放置英雄"},
                { 8, "升级英雄"},
                { 9, "英雄物品放置"},
                { 10, "切换英雄目标" },
                { 11, "设置英雄功能"},
                { 12, "出售英雄" },
                { 13, "鼠标点击" },
                { 14, "修改猴子坐标" },
                { 15, "等待(ms)"},
                { 16, "开启自由游戏" },
                { 17, "结束自由游戏" },
                { 25, "快捷指令包"}
            };
            monkeysToDisplay = new Dictionary<int, string>
            {
                { 0, "飞镖猴" },
                { 1, "回旋镖猴" },
                { 2, "大炮" },
                { 3, "图钉塔" },
                { 4, "冰猴" },
                { 5, "胶水猴" },

                { 10, "狙击猴" },
                { 11, "潜水艇猴" },
                { 12, "海盗猴" },
                { 13, "王牌飞行员" },
                { 14, "直升机" },
                { 15, "迫击炮猴" },
                { 16, "机枪猴" },

                { 20, "法师猴" },
                { 21, "超猴" },
                { 22, "忍者猴" },
                { 23, "炼金术士" },
                { 24, "德鲁伊" },
                { 25, "人鱼猴"},

                { 30, "香蕉农场" },
                { 31, "刺钉工厂" },
                { 32, "猴村" },
                { 33, "工程师猴" },
                { 34, "驯兽大师" }
            };
            abilityToDisplay = new Dictionary<int, string>
            {
                { 0,"\"1\""},
                { 1,"\"2\""},
                { 2,"\"3\""},
                { 3,"\"4\""},
                { 4,"\"5\""},
                { 5,"\"6\""},
                { 6,"\"7\""},
                { 7,"\"8\""},
                { 8,"\"9\""},
                { 9,"\"0\""},
                { 10,"\"-\""},
                { 11,"\"+\""}
            };
            targetToChange = new Dictionary<int, string>
            {
                { 0,"右改1次"},
                { 1,"右改2次"},
                { 2,"右改3次"},
                { 3,"左改1次"},
                { 4,"左改2次"},
                { 5,"左改3次"}
            };
            instructionPackages = new Dictionary<int, string>
            {
                { 0, "022飞镖猴(模范)" },
                { 1, "022回旋镖猴(模范)" },
                { 5, "022海盗(模范)" },
                { 6, "220潜艇(模范)" },
                { 7, "022王牌飞行员(模范)" },
                { 10, "022法师猴(模范)" },
                { 11, "022忍者猴(模范)" },
                { 15, "022工程师(模范)" },

                { 20, "032飞镖猴(强力)" },
                { 21, "204回旋镖(强力)" },
                { 22, "024回旋镖(强力)" },
                { 23, "031大炮(强力)" },
                { 24, "050大炮(强力+出售)" },
                { 25, "420冰猴" },
                { 26, "204图钉塔" },
                { 27, "420火锅" },

                { 30, "302狙击猴(强力)" },
                { 31, "204狙击猴(强力)" },
                { 32, "042空投狙" },
                { 33, "004商船" },
                { 34, "042炮船" },
                { 35, "420驱逐舰(强力)" },
                { 36, "050沙皇炸弹" },
                { 37, "204科曼奇" },
                { 38, "420长空(翼猴)"},
                { 39, "204潜艇" },

                { 40, "130德鲁伊" },
                { 41, "302超猴(强力)" },
                { 42, "203超猴" },
                { 43, "300炼金(出售)" },
                { 44, "320炼金术士(出售)" },
                { 45, "401人鱼猴" },

                { 50, "023香蕉农场" },
                { 51, "420香蕉农场" },
                { 52, "240刺钉工厂" },
                { 53, "032刺钉工厂(靠近)" },
            };
            maps = new Dictionary<int, string>
            {
                { 0, "猴子草甸" },
                { 1, "循环" },
                { 2, "道路中间" },
                { 3, "汀克顿" },
                { 4, "树桩" },
                { 5, "市中心" },
                { 6, "一二杉" },
                { 7, "废料厂" },
                { 8, "小木屋" },
                { 9, "度假胜地" },
                { 10, "溜冰鞋" },
                { 11, "莲花岛" },
                { 12, "糖果瀑布" },
                { 13, "冬季公园" },
                { 14, "鬼脸南瓜" },
                { 15, "公园路径" },
                { 16, "高山竞速" },
                { 17, "冰冻三尺" },
                { 18, "立体主义" },
                { 19, "四个圈子" },
                { 20, "树篱" },
                { 21, "路的尽头" },
                { 22, "原木" },

                { 30, "硫磺泉" },
                { 31, "水上乐园" },
                { 32, "独眼巨人" },
                { 33, "隐蔽的花园" },
                { 34, "采石场" },
                { 35, "静谧街道" },
                { 36, "布隆纳留斯精英" },
                { 37, "平衡" },
                { 38, "已加密" },
                { 39, "集市" },
                { 40, "阿多拉神庙" },
                { 41, "复活节春天" },
                { 42, "飞镖卡丁车" },
                { 43, "登月" },
                { 44, "鬼屋" },
                { 45, "顺流而下" },
                { 46, "靶场" },
                { 47, "龟裂之地" },
                { 48, "河床" },
                { 49, "滑槽" },
                { 50, "耙" },
                { 51, "香料群岛" },
                { 52, "夜光海湾" },

                { 60, "城堡复仇" },
                { 61, "黑暗之径" },
                { 62, "侵蚀" },
                { 63, "午夜豪宅" },
                { 64, "凹陷的柱子" },
                { 65, "X因子" },
                { 66, "梅萨" },
                { 67, "齿轮转动" },
                { 68, "泄洪道" },
                { 69, "货运" },
                { 70, "帕特的池塘" },
                { 71, "半岛" },
                { 72, "高级金融" },
                { 73, "另一块砖" },
                { 74, "海岸" },
                { 75, "玉米地" },
                { 76, "地下" },
                { 77, "古代传送门" },
                { 78, "破釜沉舟" },

                { 90, "冰河之径" },
                { 91, "黑暗地下城" },
                { 92, "避难所" },
                { 93, "峡谷" },
                { 94, "水淹山谷" },
                { 95, "炼狱" },
                { 96, "血腥水坑" },
                { 97, "工坊" },
                { 98, "方院" },
                { 99, "黑暗城堡" },
                { 100, "泥泞的水坑" },
                { 101, "#哎哟" },
            };
            heros = new Dictionary<int, string>
            {
                { 0, "昆西" },
                { 1, "格温多琳" },
                { 2, "先锋琼斯" },
                { 3, "奥本" },
                { 4, "罗莎莉亚" },
                { 5, "上尉丘吉尔" },
                { 6, "本杰明" },
                { 7, "帕特" },
                { 8, "艾泽里" },
                { 9, "阿多拉" },
                { 10, "艾蒂安" },
                { 11, "萨乌达" },
                { 12, "海军上将布里克尔" },
                { 13, "灵机" },
                { 14, "杰拉尔多" },
                { 15, "科沃斯" }
            };
            difficulty = new Dictionary<int, string>
            {
                { 0, "简单" },
                { 1, "中级" },
                { 2, "困难" }
            };
            mode = new Dictionary<int, string>
            {
                { 0, "标准" },
                { 1, "放气" },
                { 2, "天启" },
                { 3, "相反" },
                { 4, "现金减半" },
                { 5, "双倍生命值MOAB" },
                { 6, "代替气球回合" },
                { 7, "极难模式" },
                { 8, "点击" },
                { 9, "仅初级" },
                { 10, "仅军事" },
                { 11, "仅魔法" },

            };

            // 初始化计数列表
            objectCount = new List<int>();

            objectId = new List<(int, int)>();
            objectList = new List<MonkeyTowerClass>();
            hero = new HeroClass();
            triggering = new List<(int, int)>();
        }

        private (string, string) MakeInstruction(int type, List<int> args, int roundTriggering, int coinTriggering, (int, int) coords)
        {
            if (objectCount.Count == 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    objectCount.Add(0);
                }
            }
            bool succeeded = true;
            List<int> arguments = new List<int>();
            arguments.Add(type);
            switch (type)
            {
                case 0: // 放置指令
                    objectCount[args[0]]++;
                    objectId.Add((args[0], objectCount[args[0]]));

                    string jsonString = File.ReadAllText("Monkey.json");
                    MonkeyTowerClass newObject = JsonConvert.DeserializeObject<MonkeyTowerClass>(jsonString);
                    newObject.ifDelete = false;
                    newObject.SetCoordinates(coords);
                    objectList.Add(newObject);

                    arguments.Add(args[0]);
                    arguments.Add(objectList.Count() - 1);
                    arguments.Add(coords.Item1);
                    arguments.Add(coords.Item2);
                    break;
                case 1: // 升级指令
                    //if (objectList[args[0]].exsitence == false)
                    //{
                    //    succeeded = false;
                    //    break;
                    //}
                    succeeded = objectList[args[0]].Upgrade(args[1]);

                    arguments.Add(args[0]);
                    arguments.Add(args[1]);
                    break;
                case 2: // 切换目标指令
                    //if (objectList[args[0]].exsitence == false)
                    //{
                    //    succeeded = false;
                    //    break;
                    //}

                    arguments.Add(args[0]);
                    arguments.Add(args[1]);
                    break;
                case 3: // 释放技能指令
                    arguments.Add(args[0]);
                    if (args[1] == 1)
                    {
                        arguments.Add(coords.Item1);
                        arguments.Add(coords.Item2);
                    }
                    break;
                case 4: // 倍速指令
                    arguments.Add(args[0]);

                    break;
                case 5: // 出售指令
                    succeeded = objectList[args[0]].Sale();
                    arguments.Add(args[0]);

                    break;
                case 6: // 设置猴子功能
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
                case 7:
                    arguments.Add(coords.Item1);
                    arguments.Add(coords.Item2);

                    break;
                case 8:

                    break;
                case 9:

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
                case 10:

                    arguments.Add(args[1]);

                    break;
                case 11:
                    arguments.Add(args[0]);

                    if (args[1] == 1)
                    {
                        arguments.Add(coords.Item1);
                        arguments.Add(coords.Item2);
                    }

                    break;
                case 12:

                    break;
                case 13:
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
                case 14:
                    arguments.Add(args[0]);
                    arguments.Add(coords.Item1);
                    arguments.Add(coords.Item2);

                    break;
                case 15:
                    if (args[0] < 1)
                    {
                        arguments.Add(100);
                    }
                    else
                    {
                        arguments.Add(args[0]);
                    }
                    break;
                case 16:
                    break;
                case 17:
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
        
        public bool AddInstruction(int type, List<int> args, int roundTriggering, int coinTriggering, (int, int) coords)
        {
            try
            {
                (string, string) instructions = MakeInstruction(type, args, roundTriggering, coinTriggering, coords);
                if (instructions.Item1 != "")
                {
                    displayinstructions.Add(instructions.Item1);
                    digitalinstructions.Add(instructions.Item2);
                    if (type == 1 || type == 2 || type == 5 || type == 6)
                    {
                        RefreshDisplayInstructions(args[0]);
                    }
                    if (type >= 7 && type <= 12)
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
                            InsertInstruction(pos++, 0, new List<int> { 0, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 1: // 022回旋镖猴(模范)
                            InsertInstruction(pos++, 0, new List<int> { 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 5: // 022海盗(模范)
                            InsertInstruction(pos++, 0, new List<int> { 12, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 6: // 220潜艇(模范)
                            InsertInstruction(pos++, 0, new List<int> { 11, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 7: // 022王牌飞行员(模范)
                            InsertInstruction(pos++, 0, new List<int> { 13, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;

                        case 10: // 022法师猴(模范)
                            InsertInstruction(pos++, 0, new List<int> { 20, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 11: // 022忍者猴(模范)
                            InsertInstruction(pos++, 0, new List<int> { 22, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 15: // 022工程师(模范)
                            InsertInstruction(pos++, 0, new List<int> { 33, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;

                        case 20: // 032飞镖猴(强力)
                            InsertInstruction(pos++, 0, new List<int> { 0, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 2, new List<int> { objectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 21: // 204回旋镖(强力)
                            InsertInstruction(pos++, 0, new List<int> { 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 2, new List<int> { objectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 22: // 024回旋镖(强力)
                            InsertInstruction(pos++, 0, new List<int> { 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 2, new List<int> { objectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 23: // 031大炮(强力)
                            InsertInstruction(pos++, 0, new List<int> { 2, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 2, new List<int> { objectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 24: // 050大炮(强力+出售)
                            InsertInstruction(pos++, 0, new List<int> { 2, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 2, new List<int> { objectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 3, new List<int> { 0, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 5, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 25: // 420冰猴
                            InsertInstruction(pos++, 0, new List<int> { 4, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 26: // 204图钉塔
                            InsertInstruction(pos++, 0, new List<int> { 3, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 27: // 420火锅
                            InsertInstruction(pos++, 0, new List<int> { 3, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;

                        case 30: // 302狙击猴(强力)
                            InsertInstruction(pos++, 0, new List<int> { 10, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 2, new List<int> { objectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));

                            break;
                        case 31: // 204狙击猴(强力)
                            InsertInstruction(pos++, 0, new List<int> { 10, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 2, new List<int> { objectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 32: // 042空投狙
                            InsertInstruction(pos++, 0, new List<int> { 10, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 33: // 004商船
                            InsertInstruction(pos++, 0, new List<int> { 12, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 34: // 042炮船
                            InsertInstruction(pos++, 0, new List<int> { 12, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 35: // 420驱逐舰(强力)
                            InsertInstruction(pos++, 0, new List<int> { 12, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 2, new List<int> { objectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 36: // 050沙皇炸弹
                            InsertInstruction(pos++, 0, new List<int> { 13, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 3, new List<int> { 0, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 5, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 37: // 204科曼奇
                            InsertInstruction(pos++, 0, new List<int> { 14, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            break;
                        case 38: // 420长空
                            InsertInstruction(pos++, 0, new List<int> { 13, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 2, new List<int> { objectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 39: // 204潜艇
                            InsertInstruction(pos++, 0, new List<int> { 11, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 40: // 130德鲁伊
                            InsertInstruction(pos++, 0, new List<int> { 24, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 41: // 302超猴
                            InsertInstruction(pos++, 0, new List<int> { 21, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 2, new List<int> { objectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 42: // 203超猴
                            InsertInstruction(pos++, 0, new List<int> { 21, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 43: // 300炼金(出售)
                            InsertInstruction(pos++, 0, new List<int> { 23, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 5, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 44: // 320炼金术士(出售)
                            InsertInstruction(pos++, 0, new List<int> { 23, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 5, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 45: // 401人鱼猴
                            InsertInstruction(pos++, 0, new List<int> { 25, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 50: // 023香蕉农场
                            InsertInstruction(pos++, 0, new List<int> { 30, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 51: // 420香蕉农场
                            InsertInstruction(pos++, 0, new List<int> { 30, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            break;
                        case 52: // 240刺钉工厂
                            InsertInstruction(pos++, 0, new List<int> { 31, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 0 }, 0, 0, (0, 0));
                            break;
                        case 53: // 032刺钉工厂(靠近)
                            InsertInstruction(pos++, 0, new List<int> { 31, 0 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 2 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 2, new List<int> { objectId.Count - 1, 3 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
                            InsertInstruction(pos++, 1, new List<int> { objectId.Count - 1, 1 }, 0, 0, (0, 0));
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

        public bool InsertInstruction(int index, int type, List<int> args, int roundTriggering, int coinTriggering, (int, int) coords)
        {
            try
            {
                (string, string) instructions = MakeInstruction(type, args, roundTriggering, coinTriggering, coords);
                if (instructions.Item1 != "")
                {
                    displayinstructions.Insert(index + 1, instructions.Item1);
                    digitalinstructions.Insert(index + 1, instructions.Item2);
                    if (type == 1 || type == 2 || type == 5 || type == 6)
                    {
                        RefreshDisplayInstructions(args[0]);
                    }
                    if (type >= 7 && type <= 12)
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

        public bool IfSame(int index, int type, List<int> args)
        {
            bool same = false;
            if (index < 0) return false;
            if (index >= digitalinstructions.Count) return false;
            List<int> arguments = GetArguments(index);
            if (type != arguments[0]) return false;
            switch (arguments[0])
            {
                case 0:
                case 1:
                case 2:
                case 5:
                case 6:
                case 14:
                    if (arguments[1] == args[0]) same = true;
                    break;
                case 3:
                case 4:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 15:
                case 16:
                case 17:
                    same = true;
                    break;
            }
            return same;
        }

        private (string, string) ArgumentsToInstruction(List<int> arguments)
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
            switch (arguments[0])
            {
                case 0: // 放置指令
                    displayInstruction += monkeysToDisplay[objectId[arguments[2]].Item1] + objectId[arguments[2]].Item2.ToString();
                    displayInstruction += "放置";
                    displayInstruction += "于(" + arguments[3].ToString() + ", " + arguments[4].ToString() + ")";

                    break;
                case 1: // 升级指令
                    displayInstruction += monkeysToDisplay[objectId[arguments[1]].Item1] + objectId[arguments[1]].Item2.ToString();
                    displayInstruction += "升级至";
                    displayInstruction += objectList[arguments[1]].GetAllUpgradeLevel();

                    break;
                case 2: // 切换目标指令
                    displayInstruction += monkeysToDisplay[objectId[arguments[1]].Item1] + objectId[arguments[1]].Item2.ToString();
                    displayInstruction += "目标";
                    displayInstruction += targetToChange[arguments[2]];
                    break;
                case 3: // 释放技能指令
                    displayInstruction += "释放技能";
                    displayInstruction += abilityToDisplay[arguments[1]];
                    if (arguments.Count > 4)
                    {
                        displayInstruction += "于(" + arguments[2].ToString() + ", " + arguments[3].ToString() + ")";
                    }
                    break;
                case 4: // 倍速指令
                    if (arguments[1] == 0)
                    {
                        displayInstruction += "切换倍速";
                    }
                    else if (arguments[1] == 1)
                    {
                        displayInstruction += "下一回合";
                    }

                    break;
                case 5: // 出售指令
                    displayInstruction += "出售";
                    displayInstruction += monkeysToDisplay[objectId[arguments[1]].Item1] + objectId[arguments[1]].Item2.ToString();

                    break;
                case 6: // 设置猴子功能
                    displayInstruction += monkeysToDisplay[objectId[arguments[1]].Item1] + objectId[arguments[1]].Item2.ToString();
                    displayInstruction += "更改功能";

                    if (arguments.Count > 4)
                    {
                        displayInstruction += "于(" + arguments[2].ToString() + ", " + arguments[3].ToString() + ")";
                    }

                    break;
                case 7:
                    displayInstruction += "放置英雄";
                    displayInstruction += "于(" + arguments[1].ToString() + ", " + arguments[2].ToString() + ")";


                    break;
                case 8:
                    displayInstruction += "升级英雄";

                    break;
                case 9:
                    displayInstruction += "放置英雄技能面板物品";
                    displayInstruction += arguments[1].ToString();
                    if (arguments.Count > 4)
                    {
                        displayInstruction += "于(" + arguments[2].ToString() + ", " + arguments[3].ToString() + ")";
                    }

                    break;
                case 10:
                    displayInstruction += "英雄目标";
                    displayInstruction += targetToChange[arguments[1]];

                    break;
                case 11:
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
                case 12:
                    displayInstruction += "出售英雄";

                    break;
                case 13:
                    displayInstruction += "鼠标点击";
                    displayInstruction += "(" + arguments[2].ToString() + ", " + arguments[3].ToString() + ")";
                    displayInstruction += arguments[1].ToString() + "次";

                    break;
                case 14:
                    displayInstruction += "修改" + monkeysToDisplay[objectId[arguments[1]].Item1] + objectId[arguments[1]].Item2.ToString() + "坐标";
                    displayInstruction += "于(" + arguments[2].ToString() + ", " + arguments[3].ToString() + ")";

                    break;
                case 15:
                    displayInstruction += "等待" + arguments[1].ToString() + "ms";

                    break;
                case 16:
                    displayInstruction += "开始自由游戏";

                    break;
                case 17:
                    displayInstruction += "结束自由游戏";
                    
                    break;
            }
            return (displayInstruction, digitalInstruction);
        }

        public bool ModifyInstruction(int index, int type, List<int> args, int roundTriggering, int coinTriggering, (int, int) coords)
        {
            if (IfSame(index, type, args))
            {
                List<int> arguments = GetArguments(index);
                switch (type)
                {
                    case 0:
                        arguments[3] = coords.Item1;
                        arguments[4] = coords.Item2;
                        arguments[5] = roundTriggering;
                        arguments[6] = coinTriggering;
                        break;
                    case 1 :
                    case 2 :
                        arguments[2] = args[1];
                        arguments[3] = roundTriggering;
                        arguments[4] = coinTriggering;
                        break;
                    case 3 :
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
                    case 4 : // 倍速指令
                        arguments[1] = args[0];
                        arguments[2] = roundTriggering;
                        arguments[3] = coinTriggering;
                        break;
                    case 5 : // 出售指令
                        arguments[2] = roundTriggering;
                        arguments[3] = coinTriggering;
                        break;
                    case 6 : // 设置猴子功能
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
                    case 7 :
                    case 8 : 
                        arguments[1] = coords.Item1;
                        arguments[2] = coords.Item2;
                        arguments[3] = roundTriggering;
                        arguments[4] = coinTriggering;
                        break;
                    case 9 :
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
                    case 10 :
                        arguments[1] = args[1];
                        arguments[2] = roundTriggering;
                        arguments[3] = coinTriggering;
                        break;
                    case 11 :
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
                    case 12 :
                        arguments[1] = roundTriggering;
                        arguments[2] = coinTriggering;
                        break;
                    case 13 :
                        arguments[1] = args[0];
                        arguments[2] = coords.Item1;
                        arguments[3] = coords.Item2;
                        arguments[4] = roundTriggering;
                        arguments[5] = coinTriggering;

                        break;
                    case 14 :
                        arguments[2] = coords.Item1;
                        arguments[3] = coords.Item2;
                        arguments[4] = roundTriggering;
                        arguments[5] = coinTriggering;
                        break;
                    case 15:
                        arguments[1] = args[0];
                        arguments[2] = roundTriggering;
                        arguments[3] = coinTriggering;
                        break;
                    case 16:
                        arguments[1] = roundTriggering;
                        arguments[2] = coinTriggering;
                        break;
                    case 17:
                        arguments[1] = roundTriggering;
                        arguments[2] = coinTriggering;
                        break;
                }
                (string, string) Instruction = ArgumentsToInstruction(arguments);
                if (Instruction.Item1 != "")
                {
                    displayinstructions[index] = (Instruction.Item1);
                    digitalinstructions[index] = (Instruction.Item2);
                    if (type == 1 || type == 2 || type == 5 || type == 6)
                    {
                        RefreshDisplayInstructions(args[0]);
                    }
                    if (type >= 7 && type <= 12)
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
            if (args[0] == 0)
            {
                int monkeyIndex = args[2];
                objectList[monkeyIndex].ifDelete = true;
                for (int i = digitalinstructions.Count() - 1; i >= index; i--)
                {
                    List<int> currentArguments = GetArguments(i);
                    if ((currentArguments[0] == 1 || currentArguments[0] == 2 || currentArguments[0] == 5) && currentArguments[1] == monkeyIndex)
                    {
                        digitalinstructions.RemoveAt(i);
                        displayinstructions.RemoveAt(i);
                    }
                }
                digitalinstructions.RemoveAt(index);
                displayinstructions.RemoveAt(index);
            }
            else if (args[0] == 1) // 升级指令
            {
                digitalinstructions.RemoveAt(index);
                displayinstructions.RemoveAt(index);
                RefreshDisplayInstructions(args[1]);
            }
            else if (args[0] == 5) // 出售指令
            {
                objectList[args[1]].exsitence = true;
                digitalinstructions.RemoveAt(index);
                displayinstructions.RemoveAt(index);
            }
            else if (args[0] == 7) // 英雄指令
            {
                hero.exsitence = false;
                for (int i = digitalinstructions.Count() - 1; i >= index; i--)
                {
                    List<int> currentArguments = GetArguments(i);
                    if (currentArguments[0] >= 7 && currentArguments[0] <= 12) // 英雄指令
                    {
                        digitalinstructions.RemoveAt(i);
                        displayinstructions.RemoveAt(i);
                    }
                }
            }
            else if (args[0] == 10) // 英雄目标指令
            {
                hero.exsitence = true;
                digitalinstructions.RemoveAt(index);
                displayinstructions.RemoveAt(index);
            }
            else // 其他指令
            {
                digitalinstructions.RemoveAt(index);
                displayinstructions.RemoveAt(index);
            }
        }

        public bool ChangeInstructionPosition(int index)
        {
            bool succeeded = true;
            if (index == digitalinstructions.Count() - 1)
            {
                return false;
            }

            List<int> args1 = GetArguments(index);
            List<int> args2 = GetArguments(index + 1);
            if (args1[0] == 0 && args2[0] == 1 && args1[2] == args2[1])
            {
                succeeded = false;
            }
            else if (args1[0] == 1 && args2[0] == 1 && args1[1] == args2[1])
            {
                (digitalinstructions[index + 1], digitalinstructions[index]) = (digitalinstructions[index], digitalinstructions[index + 1]);
                RefreshDisplayInstructions(args1[1]);
            }
            else if (args1[0] == 0 && args2[0] == 5 && args1[2] == args2[1])
            {
                succeeded = false;
            }
            else if (args1[0] == 1 && args2[0] == 5 && args1[1] == args2[1])
            {
                succeeded = false;
            }
            else if (args1[0] == 7 && args2[0] > 7 && args2[0] < 13)
            {
                succeeded = false;
            }
            else if (args1[0] >= 7 && args1[0] < 12 && args2[0] == 12)
            {
                succeeded = false;
            }
            else if (args1[0] == 15 && args2[0] == 16)
            {
                succeeded = false;
            }
            else
            {
                (digitalinstructions[index + 1], digitalinstructions[index]) = (digitalinstructions[index], digitalinstructions[index + 1]);
                (displayinstructions[index + 1], displayinstructions[index]) = (displayinstructions[index], displayinstructions[index + 1]);
            }
            return succeeded;
        }

        public void RefreshDisplayInstructions(int monkeyId)
        {
            List<int> deleteQue = new List<int>();
            if (monkeyId == -1)
            {
                hero.exsitence = false;
                for (int i = 0; i < digitalinstructions.Count(); i++)
                {
                    List<int> arguments = GetArguments(i);
                    if (arguments[0] == 7)
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
                    if (arguments[0] > 7 && arguments[0] < 12 && hero.exsitence == false)
                    {
                        deleteQue.Add(i);
                    }
                    if (arguments[0] == 12)
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
                for (int i = 0; i < digitalinstructions.Count(); i++)
                {
                    List<int> arguments = GetArguments(i);
                    if (arguments[0] == 1 && monkeyId == arguments[1])
                    {
                        if (objectList[monkeyId].exsitence == false)
                        {
                            deleteQue.Add(i);
                        }
                        else
                        {
                            objectList[monkeyId].Upgrade(arguments[2]);
                            displayinstructions[i] = ArgumentsToInstruction(arguments).Item1;
                        }
                    }
                    else if (arguments[0] == 2 && monkeyId == arguments[1])
                    {
                        if (objectList[monkeyId].exsitence == false)
                        {
                            deleteQue.Add(i);
                        }
                    }
                    else if (arguments[0] == 6 && monkeyId == arguments[1])
                    {
                        if (objectList[monkeyId].exsitence == false)
                        {
                            deleteQue.Add(i);
                        }
                    }
                    else if (arguments[0] == 5 && monkeyId == arguments[1])
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
                digitalinstructions.RemoveAt(deleteQue[i]);
                displayinstructions.RemoveAt(deleteQue[i]);
            }
        }

        public List<int> GetArguments(int index) 
        {
            if (index < 0 || index >= digitalinstructions.Count)
            {
                return null;
            }
            return digitalinstructions[index].Split(' ').Select(Int32.Parse).ToList();
        }

        public string SaveToJson()
        {
            int intTime = DateTime.Now.GetHashCode() % 1000;
            string Time = (intTime > 0 ? intTime : -1 * intTime).ToString();
            string filePath = $@"data\我的脚本\{maps[selectedMap]}\{difficulty[selectedDifficulty]}\{mode[selectedMode]}-{heros[selectedHero]}-" + Time + ".btd6";
            string directoryPath = Path.GetDirectoryName(filePath);
            //string filePath = "你好.json";
            var partialInfo = new
            {
                selectedMap,
                selectedDifficulty,
                selectedMode,
                selectedHero,
                anchorCoords,
                objectCount,
                objectId,
                displayinstructions,
                digitalinstructions
            };
            string jsonString = JsonConvert.SerializeObject(partialInfo, Formatting.Indented);
            if (!Directory.Exists(directoryPath))
            {

                Directory.CreateDirectory(directoryPath);
            }
            File.WriteAllText(filePath, jsonString);

            return directoryPath;
        }

        public void BluidObjectList()
        {
            upgradeCount = new Dictionary<int, string>();
            for (int i = 0; i < objectId.Count; i++)
            {
                string jsonString = File.ReadAllText("Monkey.json");
                MonkeyTowerClass newObject = JsonConvert.DeserializeObject<MonkeyTowerClass>(jsonString);
                objectList.Add(newObject);
            }
            for (int i = 0; i < digitalinstructions.Count; i++) 
            {
                List<int> args = GetArguments(i);
                switch (args[0])
                {
                    case 0:
                        objectList[args[2]].ifDelete = false;
                        objectList[args[2]].SetCoordinates((args[3], args[4]));
                        break;
                    case 1:
                        objectList[args[1]].Upgrade(args[2]);
                        upgradeCount.Add(i, objectList[args[1]].GetAllUpgradeLevel());
                        //File.AppendAllText(@"levels.txt", objectList[args[1]].upgradeLevels[0].ToString() + " " + objectList[args[1]].upgradeLevels[1].ToString() + " " + objectList[args[1]].upgradeLevels[2].ToString() + " \n");
                        break;
                    case 5:
                        objectList[args[1]].Sale();
                        break;
                    case 7:
                        hero.exsitence = true;
                        break;
                    case 12:
                        hero.exsitence = false;
                        break;
                }
            }

        }

        private string GetMoveInstruction(int argument1, int argument2, double scale, bool ifClick)
        {
            if (!ifClick)
            {
                return "1 " + ((int)(argument1 * scale)).ToString() + " " + ((int)(argument2 * scale)).ToString();
            }
            else
            {
                return "11 " + ((int)(argument1 * scale)).ToString() + " " + ((int)(argument2 * scale)).ToString();
            }
        }

        public void Compile(int dpi, int executeInterval, bool ifFast)
        {
            // 0 trigger
            // 1 move
            // 2 click
            // 3 mousedown
            // 4 mouseup
            // 5 mousewheel
            // 6 keydown
            // 7 keyup

            compilerDirective = new List<List<string>>();
            Dictionary<int, int> objectKeyPairs = new Dictionary<int, int>
            {
                { 0, 81 },
                { 1, 87 },
                { 2, 69 },
                { 3, 82 },
                { 4, 84 },
                { 5, 89 },

                { 10, 90 },
                { 11, 88 },
                { 12, 67 },
                { 13, 86 },
                { 14, 66 },
                { 15, 78 },
                { 16, 77 },

                { 20, 65 },
                { 21, 83 },
                { 22, 68 },
                { 23, 70 },
                { 24, 71 },
                { 25, 79 },

                { 30, 72 },
                { 31, 74 },
                { 32, 75 },
                { 33, 76 },
                { 34, 73 }
            };
            Dictionary<int, int> heroAbilityKeyPairs = new Dictionary<int, int>
            {
                { 0, 81 },
                { 1, 87 },
                { 2, 69 },
                { 3, 82 },
                { 4, 84 },

                { 5, 65 },
                { 6, 83 },
                { 7, 68 },
                { 8, 70 },
                { 9, 71 },
                { 10, 72 },

                { 11, 90 },
                { 12, 88 },
                { 13, 67 },
                { 14, 86 },
                { 15, 66 },
            };
            for (int i = 0; i < digitalinstructions.Count; i++)
            {
                string digitalInstruction = digitalinstructions[i];
                List<string> miniDirective = new List<string>();
                List<int> arguments = digitalInstruction.Split(' ').Select(Int32.Parse).ToList();
                
                int coinTrigger = arguments.Last();
                arguments.RemoveAt(arguments.Count() - 1);
                int roundTrigger = arguments.Last();
                arguments.RemoveAt(arguments.Count() - 1);
                miniDirective.Add("0 " + roundTrigger.ToString() + " " + coinTrigger.ToString());
                double scale = 1.0;
                switch (arguments[0])
                {
                    case 0: // 放置猴子
                        // move
                        miniDirective.Add(GetMoveInstruction(arguments[3], arguments[4], scale, false));

                        miniDirective.Add("8 " + objectKeyPairs[arguments[1]].ToString());
                        // click
                        miniDirective.Add("2");
                        break;
                    case 1: // 升级猴子
                        if (!IfLast(i))
                        {
                            // move
                            miniDirective.Add(GetMoveInstruction(objectList[arguments[1]].coordinates.Item1, objectList[arguments[1]].coordinates.Item2, scale, ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }

                        if (arguments[2] == 0)
                        {
                            miniDirective.Add("8 " + "188");
                        }
                        if (arguments[2] == 1)
                        {
                            miniDirective.Add("8 " + "190");
                        }
                        if (arguments[2] == 2)
                        {
                            miniDirective.Add("8 " + "191");
                        }
                        if (!IfNext(i))
                        {
                            // move 
                            miniDirective.Add(GetMoveInstruction(anchorCoords.Item1, anchorCoords.Item2, scale, ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }

                        break;
                    case 2: // 切换目标
                        // move
                        miniDirective.Add(GetMoveInstruction(objectList[arguments[1]].coordinates.Item1, objectList[arguments[1]].coordinates.Item2, scale, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }
                        // keydown & up
                        if (arguments[2] == 0)
                        {
                            miniDirective.Add("8 " + "9");
                        }
                        else if (arguments[2] == 1)
                        {
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                        }
                        else if (arguments[2] == 2)
                        {
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                        }
                        else if (arguments[2] == 3)
                        {
                            miniDirective.Add("6 " + "17");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("7 " + "17");
                        }
                        else if (arguments[2] == 4)
                        {
                            miniDirective.Add("6 " + "17");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("7 " + "17");
                        }
                        else if (arguments[2] == 5)
                        {
                            miniDirective.Add("6 " + "17");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("7 " + "17");
                        }
                        // move
                        miniDirective.Add(GetMoveInstruction(anchorCoords.Item1, anchorCoords.Item2, scale, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }
                        break;
                    case 3: // 使用技能
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
                            miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], scale, ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }
                        break;
                    case 4: // 切换倍速
                        if (arguments[1] == 0)
                        {
                            miniDirective.Add("8 " + "32");
                        }
                        if (arguments[1] == 1)
                        {
                            if (!IfLast(i))
                            {
                                miniDirective.Add("6 " + "16");
                            }
                            miniDirective.Add("8 " + "32");
                            if (!IfNext(i))
                            {
                                miniDirective.Add("7 " + "16");
                            }
                        }

                        break;
                    case 5: // 出售猴子
                        // move
                        miniDirective.Add(GetMoveInstruction(objectList[arguments[1]].coordinates.Item1, objectList[arguments[1]].coordinates.Item2, scale, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }

                        miniDirective.Add("8 " + "8");
                        break;
                    case 6:
                        // move
                        miniDirective.Add(GetMoveInstruction(objectList[arguments[1]].coordinates.Item1, objectList[arguments[1]].coordinates.Item2, scale, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }

                        miniDirective.Add("8 " + "34");

                        if (arguments.Count > 2)
                        {
                            // move
                            miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], scale, ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }
                        // move
                        miniDirective.Add(GetMoveInstruction(anchorCoords.Item1, anchorCoords.Item2, scale, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }

                        break;
                    case 7: // 放置英雄
                        miniDirective.Add("23");

                        miniDirective.Add(GetMoveInstruction(1715, 230, scale, false));

                        miniDirective.Add("2");

                        // move
                        miniDirective.Add(GetMoveInstruction(arguments[1], arguments[2], scale, false));

                        miniDirective.Add("7 " + "85");
                        // click
                        miniDirective.Add("2");
                        break;
                    case 8:
                        if (!IfLast(i))
                        {
                            miniDirective.Add("8 " + "85");
                        }
                        miniDirective.Add("8 " + "188");
                        if (!IfNext(i))
                        {
                            miniDirective.Add("8 " + "85");
                        }

                        break;
                    case 9:
                        miniDirective.Add("8 " + "85");
                        // keydown
                        miniDirective.Add("6 " + "16");
                        miniDirective.Add("8 " + heroAbilityKeyPairs[arguments[1] - 1].ToString());
                        // keyup
                        miniDirective.Add("7 " + "16");

                        if (arguments.Count >  2)
                        {
                            // move
                            miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], scale, ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }
                        // move
                        miniDirective.Add(GetMoveInstruction(anchorCoords.Item1, anchorCoords.Item2, scale, ifFast));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }
                        break;
                    case 10:
                        // keydown
                        miniDirective.Add("6 " + "85");
                        // keyup
                        miniDirective.Add("7 " + "85");

                        if (arguments[1] == 0)
                        {
                            miniDirective.Add("8 " + "9");
                        }
                        else if (arguments[1] == 1)
                        {
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                        }
                        else if (arguments[1] == 2)
                        {
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                        }
                        else if (arguments[1] == 3)
                        {
                            miniDirective.Add("6 " + "17");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("7 " + "17");
                        }
                        else if (arguments[1] == 4)
                        {
                            miniDirective.Add("6 " + "17");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("7 " + "17");
                        }
                        else if (arguments[1] == 5)
                        {
                            miniDirective.Add("6 " + "17");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("8 " + "9");
                            miniDirective.Add("7 " + "17");
                        }
                        miniDirective.Add("8 " + "85");
                        break;
                    case 11: // 
                        miniDirective.Add("8 " + "85");

                        if (arguments[1] == 0)
                        {
                            miniDirective.Add("8 " + "34");
                        }
                        else
                        {
                            miniDirective.Add("8 " + "33");
                        }

                        if (arguments.Count > 2)
                        {
                            // move
                            miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], scale, ifFast));
                            // click
                            if (!ifFast)
                            {
                                miniDirective.Add("2");
                            }
                        }
                        // move
                        miniDirective.Add(GetMoveInstruction(anchorCoords.Item1, anchorCoords.Item2, scale, ifFast ));
                        // click
                        if (!ifFast)
                        {
                            miniDirective.Add("2");
                        }
                        break;
                    case 12:
                        miniDirective.Add("8 " + "85");
                        miniDirective.Add("8 " + "8");
                        miniDirective.Add("8 " + "8");
                        break;
                    case 13: // 鼠标点击指令
                        // move
                        miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], scale, false));
                        for (int j = 0; j < arguments[1]; j++)
                        {
                            miniDirective.Add("2");
                        }
                        // click
                        break;
                    case 14:
                        miniDirective.Add("2");
                        objectList[arguments[1]].SetCoordinates((arguments[2], arguments[3]));
                        break;
                    case 15:
                        int times = (arguments[1] / executeInterval > 1 ? arguments[1] / executeInterval : 1);
                        for (int k = 0; k < times; k++)
                        {
                            miniDirective.Add("10");
                        }
                        break;
                    case 16:
                        miniDirective.Add("17 " + "557 189 1370 320");
                        miniDirective.Add("21");
                        miniDirective.Add(GetMoveInstruction(960, 900, scale, false));
                        // click
                        miniDirective.Add("2");
                        times = (1000 / executeInterval > 1 ? 1000 / executeInterval : 1);
                        for (int m = 0; m < times; m++)
                        {
                            miniDirective.Add("10");
                        }
                        miniDirective.Add(GetMoveInstruction(1200, 850, scale, false));
                        // click
                        miniDirective.Add("2");
                        for (int m = 0; m < times; m++)
                        {
                            miniDirective.Add("10");
                        }
                        miniDirective.Add(GetMoveInstruction(960, 750, scale, false));
                        // click
                        miniDirective.Add("2");

                        break;
                    case 17:
                        times = (3000 / executeInterval > 1 ? 1000 / executeInterval : 1);
                        for (int m = 0; m < times; m++)
                        {
                            miniDirective.Add("2");
                        }

                        miniDirective.Add("8 " + "27");

                        for (int m = 0; m < times / 3; m++)
                        {
                            miniDirective.Add("10");
                        }
                        miniDirective.Add(GetMoveInstruction(850, 840, scale, false));
                        // click
                        miniDirective.Add("2");

                        miniDirective.Add("20");
                        break;
                }
                compilerDirective.Add(miniDirective);
            }
        }

        public bool IfNext(int currentIndex)
        {
            List<int> arguments1 = digitalinstructions[currentIndex].Split(' ').Select(Int32.Parse).ToList();
            if (currentIndex + 1 == digitalinstructions.Count)
            {
                return false;
            }
            List<int> arguments2 = digitalinstructions[currentIndex + 1].Split(' ').Select(Int32.Parse).ToList();
            if (arguments1[0] == arguments2[0]) 
            {
                if ((arguments1[0] == 1 || arguments1[0] == 4) && arguments1[1] == arguments2[1])
                {
                    return true;
                }
                if (arguments1[0] == 8)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IfLast(int currentIndex)
        {
            List<int> arguments1 = digitalinstructions[currentIndex].Split(' ').Select(Int32.Parse).ToList();
            if (currentIndex == 0)
            {
                return false;
            }
            List<int> arguments2 = digitalinstructions[currentIndex - 1].Split(' ').Select(Int32.Parse).ToList();
            
            if (arguments1[0] == arguments2[0])
            {
                if ((arguments1[0] == 1 || arguments1[0] == 4) && arguments1[1] == arguments2[1])
                {
                    return true;
                }
                if (arguments1[0] == 8)
                {
                    return true;
                }
            }
            return false;
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
            objectId.Clear();
            objectCount.Clear();
            displayinstructions.Clear();
            digitalinstructions.Clear();
            objectList.Clear();
        }


    }
}
