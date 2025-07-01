using BTD6AutoCommunity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BTD6AutoCommunity.Strategies;

namespace BTD6AutoCommunity.Core
{
    public enum MapTypes
    { 
        Beginner,
        Intermediate,
        Advanced,
        Expert
    }

    public enum Maps
    {
        // 猴子草甸
        MonkeyMeadow = 0,
        // 循环
        InTheLoop = 1,
        // 道路中间
        MiddleOfTheRoad = 2,
        // 水疗温泉
        SpaPits = 23,
        // 汀克顿
        TinkerTon = 3,
        // 树桩
        TreeStump = 4,
        // 市中心
        TownCenter = 5,
        // 一二杉
        OneTwoTree = 6,
        // 废料厂
        ScrapYard = 7,
        // 小木屋
        TheCabin = 8,
        // 度假胜地
        Resort = 9,
        // 溜冰鞋
        Skates = 10,
        // 莲花岛
        LotusIsland = 11,
        // 糖果瀑布
        CandyFalls = 12,
        // 冬季公园
        WinterPark = 13,
        // 鬼脸南瓜
        Carved = 14,
        // 公园路径
        ParkPath = 15,
        // 高山竞速
        AlpineRun = 16,
        // 冰冻三尺
        FrozenOver = 17,
        // 立体主义
        Cubism = 18,
        // 四个圈子
        FourCircles = 19,
        // 树篱
        Hedge = 20,
        // 路的尽头
        EndOfTheRoad = 21,
        // 原木
        Logs = 22,

        // 中级图
        // 硫磺泉
        SulfurSprings = 30,
        // 水上乐园
        WaterPark = 31,
        // 独眼巨人
        Polyphemus = 32,
        // 隐蔽的花园
        CoveredGarden = 33,
        // 采石场
        Quarry = 34,
        // 静谧街道
        QuietStreet = 35,
        // 布隆纳留斯精英
        BloonariusPrime = 36,
        // 平衡
        Balance = 37,
        // 已加密
        Encrypted = 38,
        // 集市
        Bazaar = 39,
        // 阿多拉神庙
        AdorasTemple = 40,
        // 复活节春天
        SpringSpring = 41,
        // 飞镖卡丁车
        KartMonkey = 42,
        // 登月
        MoonLanding = 43,
        // 鬼屋
        Haunted = 44,
        // 顺流而下
        Downstream = 45,
        // 靶场
        FiringRange = 46,
        // 龟裂之地
        Cracked = 47,
        // 河床
        Streambed = 48,
        // 滑槽
        Chutes = 49,
        // 耙
        Rake = 50,
        // 香料群岛
        SpiceIslands = 51,
        // 夜光海湾
        LuminousCove = 52,

        // 高级图
        // 城堡复仇
        CastleRevenge = 60,
        // 黑暗之径
        DarkPath = 61,
        // 侵蚀
        Erosion = 62,
        // 午夜豪宅
        MidnightMansion = 63,
        // 凹陷的柱子
        SunkenColumns = 64,
        // X因子
        XFactor = 65,
        // 梅萨
        Mesa = 66,
        // 齿轮转动
        Geared = 67,
        // 泄洪道
        Spillway = 68,
        // 货运
        Cargo = 69,
        // 帕特的池塘
        PatsPond = 70,
        // 半岛
        Peninsula = 71,
        // 高级金融
        HighFinance = 72,
        // 另一块砖
        AnotherBrick = 73,
        // 海岸
        OffTheCoast = 74,
        // 玉米地
        Cornfield = 75,
        // 地下
        Underground = 76,
        // 古代传送门
        AncientPortal = 77,
        // 破釜沉舟
        LastResort = 78,
        // 魔法林地
        EnchantedGlade = 79,
        //日落峡谷
        SunsetGulch = 80,

        // 专家图
        // 冰河之径
        GlacialTrail = 90,
        // 黑暗地下城
        DarkDungeon = 91,
        // 避难所
        Sanctuary = 92,
        // 峡谷
        Ravine = 93,
        // 水淹山谷
        FloodedValley = 94,
        // 炼狱
        Infernal = 95,
        // 血腥水坑
        BloodyPuddles = 96,
        // 工坊
        Workshop = 97,
        // 方院
        Quad = 98,
        // 黑暗城堡
        DarkCastle = 99,
        // 泥泞的水坑
        MuddyPuddles = 100,
        // #哎哟
        Ouch = 101
    }

    public enum LevelMode
    {
        /// <summary>标准模式（默认游戏模式）</summary>
        Standard = 0,

        /// <summary>仅限基础塔</summary>
        PrimaryOnly = 9,

        /// <summary>通货紧缩（初始资金固定）</summary>
        Deflation = 1,

        /// <summary>仅军事类塔</summary>
        MilitaryOnly = 10,

        /// <summary>天启模式（持续不断的气球流）</summary>
        Apopalypse = 2,

        /// <summary>反向路径（气球反向行进）</summary>
        Reverse = 3,

        /// <summary>仅魔法塔</summary>
        MagicMonkeysOnly = 11,

        /// <summary>双倍MOAB血量</summary>
        DoubleHpMoabs = 5,

        /// <summary>半额现金（初始资金减半）</summary>
        HalfCash = 4,

        /// <summary>交替气球回合（特殊回合序列）</summary>
        AlternateBloonsRounds = 6,

        /// <summary>不可击败模式（限制强化道具）</summary>
        Impoppable = 7,

        /// <summary>CHIMPS模式（无收入/能量/出售）</summary>
        /// <remarks>
        /// 规则限制：
        /// C - No Continues       (无继续)
        /// H - Heartslost         (心数限制)
        /// I - No Income           (无额外收入)
        /// M - No Monkeys Knowledge(无猴子知识)
        /// P - No Powerups         (无强化道具)
        /// S - No Selling          (无法出售塔)
        /// </remarks>
        CHIMPS = 8
    }

    public enum LevelModeBudgets
    { 
        EasyStandard = 0,
        PrimaryOnly = 1,
        Deflation = 2,
        MediumStandard = 3,
        MilitaryOnly = 4,
        Apopalypse = 5,
        Reverse = 6,
        HardStandard = 7,
        MagicMonkeysOnly = 8,
        DoubleHpMoabs = 9,
        HalfCash = 10,
        AlternateBloonsRounds = 11,
        Impoppable = 12,
        CHIMPS = 13
    }


    public enum LevelDifficulties
    {
        Easy = 0,
        Medium = 1,
        Hard = 2,
        Any // standard mode
    }

    public enum Heroes
    {
        Quincy = 0,
        Gwendolin = 1,
        StrikerJones = 2,
        ObynGreenfoot = 3,
        Rosalia = 4,
        CaptainChurchill = 5,
        Benjamin = 6,
        PatFusty = 7,
        Ezili = 8,
        Adora = 9,
        Etienne = 10,
        Sauda = 11,
        AdmiralBrickell = 12,
        Psi = 13,
        Geraldo = 14,
        Corvus = 15,
    }

    public enum Monkeys
    {
        DartMonkey = 0,          // 飞镖猴
        BoomerangMonkey = 1,     // 回旋镖猴
        BombShooter = 2,         // 大炮
        TackShooter = 3,         // 图钉塔
        IceMonkey = 4,           // 冰猴
        GlueGunner = 5,          // 胶水猴
        Desperado = 6,           // 亡命猴

        SniperMonkey = 10,       // 狙击猴
        MonkeySub = 11,    // 潜水艇猴
        MonkeyBuccaneer = 12,    // 海盗猴
        MonkeyAce = 13,           // 王牌飞行员
        HeliPilot = 14,          // 直升机
        MortarMonkey = 15,       // 迫击炮猴
        DartlingGunner = 16,     // 机枪猴

        WizardMonkey = 20,       // 法师猴
        SuperMonkey = 21,         // 超猴
        NinjaMonkey = 22,        // 忍者猴
        Alchemist = 23,          // 炼金术士
        Druid = 24,              // 德鲁伊
        MerMonkey = 25,      // 人鱼猴

        BananaFarm = 30,         // 香蕉农场
        SpikeFactory = 31,       // 刺钉工厂
        MonkeyVillage = 32,      // 猴村
        EngineerMonkey = 33,     // 工程师猴
        BeastHandler = 34        // 驯兽大师
    }

    public enum HotkeyAction
    {
        // 英雄
        Hero,

        Sell, // 出售猴子

        // 升级路径
        UpgradeTopPath,      // 上路升级
        UpgradeMiddlePath,   // 中路升级
        UpgradeBottomPath,   // 下路升级

        // 猴子操作
        SwitchTarget,        // 切换猴子目标
        ReverseSwitchTarget, // 反向切换猴子目标
        SetFunction1,        // 设置猴子功能1
        SetFunction2,        // 设置猴子功能2

        // 游戏控制
        ChangeSpeed, // 切换倍速
        NextRound,            // 竞速下一波
    }

    public enum SkillTypes
    {
        // 技能快捷键
        Skill1 = 0,              // 技能1
        Skill2 = 1,              // 技能2
        Skill3 = 2,              // 技能3
        Skill4 = 3,              // 技能4
        Skill5 = 4,              // 技能5
        Skill6 = 5,              // 技能6
        Skill7 = 6,              // 技能7
        Skill8 = 7,              // 技能8
        Skill9 = 8,              // 技能9
        Skill10 = 9,             // 技能10
        Skill11 = 10,            // 技能11
        Skill12 = 11             // 技能12
    }
    
    public enum HeroObjectTypes
    {
        HeroObject1 = 1,
        HeroObject2 = 2,
        HeroObject3 = 3,
        HeroObject4 = 4,
        HeroObject5 = 5,
        HeroObject6 = 6,
        HeroObject7 = 7,
        HeroObject8 = 8,
        HeroObject9 = 9,
        HeroObject10 = 10,
        HeroObject11 = 11,
        HeroObject12 = 12,
        HeroObject13 = 13,
        HeroObject14 = 14,
        HeroObject15 = 15,
        HeroObject16 = 16
    }

    public enum ActionTypes
    {
        PlaceMonkey = 0,                // 放置猴子
        UpgradeMonkey = 1,              // 升级猴子
        SwitchMonkeyTarget = 2,         // 切换猴子目标
        UseAbility = 3,                 // 使用技能
        SwitchSpeed = 4,                // 切换倍速
        SellMonkey = 5,                 // 出售猴子
        SetMonkeyFunction = 6,          // 设置猴子功能
        PlaceHero = 7,                  // 放置英雄
        UpgradeHero = 8,                // 升级英雄
        PlaceHeroItem = 9,              // 英雄物品放置
        SwitchHeroTarget = 10,          // 切换英雄目标
        SetHeroFunction = 11,           // 设置英雄功能
        SellHero = 12,                  // 出售英雄
        MouseClick = 13,                // 鼠标点击
        AdjustMonkeyCoordinates = 14,   // 修改猴子坐标
        WaitMilliseconds = 15,          // 等待(ms)
        StartFreeplay = 16,             // 开启自由游戏
        EndFreeplay = 17,               // 结束自由游戏
        Jump = 18,                      // 指令跳转 
        QuickCommandBundle = 25         // 快捷指令包
    }
    // 添加指令步骤：
    // step1 增加Constants类中的ActionTypes枚举
    // step2 增加Constants类中指令对应的描指令对应的描述字符串
    // step3 修改ScriptsEditorPage中的指令列表
    // step4 修改ScriptsEditorPage中的SetInstructionVision函数
    // step5 修改ScriptEditorSuite中的MakeInstruction函数
    // step6 修改ScriptEditorSuite中的ArgumentsToInstruction函数
    // step7 修改ScriptEditorSuite中的ModifiyInstruction函数
    // step8 修改ScriptsEditorPage中的InstructionsViewTL_SelectedIndexChanged函数
    // step9 修改ScriptEditorSuite中的Compile函数
    // step10 添加StrategyExecutor中的Handler函数

    public static class Constants
    {
        public static Dictionary<int, string> AbilityToDisplay => new Dictionary<int, string>
        {
            { 0, "\"1\"" },
            { 1, "\"2\"" },
            { 2, "\"3\"" },
            { 3, "\"4\"" },
            { 4, "\"5\"" },
            { 5, "\"6\"" },
            { 6, "\"7\"" },
            { 7, "\"8\"" },
            { 8, "\"9\"" },
            { 9, "\"10\"" },
            { 10, "\"11\"" },
            { 11, "\"12\"" }
        };

        public static Dictionary<int, string> TargetToChange => new Dictionary<int, string>
        {
            { 0, "右改1次" },
            { 1, "右改2次" },
            { 2, "右改3次" },
            { 3, "左改1次" },
            { 4, "左改2次" },
            { 5, "左改3次" }
        };

        public static Dictionary<int, string> InstructionPackages => new Dictionary<int, string>
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
            { 38, "420长空(翼猴)" },
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
            { 53, "032刺钉工厂(靠近)" }
        };

        public static Dictionary<LevelMode, LevelDifficulties> LevelModeToDifficulty => new Dictionary<LevelMode, LevelDifficulties>()
        {
            { LevelMode.Standard, LevelDifficulties.Any },
            { LevelMode.PrimaryOnly, LevelDifficulties.Easy },
            { LevelMode.Deflation, LevelDifficulties.Easy },
            { LevelMode.MilitaryOnly, LevelDifficulties.Medium },
            { LevelMode.Apopalypse, LevelDifficulties.Medium },
            { LevelMode.Reverse, LevelDifficulties.Medium },
            { LevelMode.MagicMonkeysOnly, LevelDifficulties.Hard },
            { LevelMode.DoubleHpMoabs, LevelDifficulties.Hard },
            { LevelMode.HalfCash, LevelDifficulties.Hard },
            { LevelMode.AlternateBloonsRounds, LevelDifficulties.Hard },
            { LevelMode.Impoppable, LevelDifficulties.Hard },
            { LevelMode.CHIMPS, LevelDifficulties.Hard }
        };

        public static Dictionary<CollectionMode, string> CollectionScripts => new Dictionary<CollectionMode, string>()
        {
            { CollectionMode.SimpleCollection, "简单收集" },
            { CollectionMode.DoubleCashCollection, "双金收集" },
            { CollectionMode.FastPathCollection, "快速路径收集" }
        };

        
        private static readonly Dictionary<LevelMode, Point> LevelModePoint = new Dictionary<LevelMode, Point>()
        {
            { LevelMode.Standard, new Point(630, 590) },
            { LevelMode.PrimaryOnly, new Point(960, 450) },
            { LevelMode.Deflation, new Point(1300, 450) },
            { LevelMode.MilitaryOnly, new Point(960, 450) },
            { LevelMode.Apopalypse, new Point(1300, 450) },
            { LevelMode.Reverse, new Point(960, 750) },
            { LevelMode.MagicMonkeysOnly, new Point(960, 450) },
            { LevelMode.DoubleHpMoabs, new Point(1300, 450) },
            { LevelMode.HalfCash, new Point(1600, 450) },
            { LevelMode.AlternateBloonsRounds, new Point(960, 750) },
            { LevelMode.Impoppable, new Point(1300, 750) },
            { LevelMode.CHIMPS, new Point(1600, 750) }
        };

        public static Point GetLevelModePos(LevelMode levelMode)
        {
            return LevelModePoint.TryGetValue(levelMode, out var pos)
                ? pos
                 : new Point(0, 0);
        }

        
        private static readonly Dictionary<MapTypes, Point> MapTypePoint = new Dictionary<MapTypes, Point>()
        {
            { MapTypes.Beginner, new Point(590, 980) },
            { MapTypes.Intermediate, new Point(840, 980) },
            { MapTypes.Advanced, new Point(1090, 980) },
            { MapTypes.Expert, new Point(1340, 980) }
        };

        public static Point GetMapTypePos(MapTypes mapType)
        {
            return MapTypePoint.TryGetValue(mapType, out var pos)
                ? pos
                 : new Point(0, 0);
        }
        
        private static readonly Dictionary<LevelModeBudgets, Point> LevelModeBudgetPoint = new Dictionary<LevelModeBudgets, Point>()
        {
            { LevelModeBudgets.EasyStandard, new Point(406, 365) },
            { LevelModeBudgets.PrimaryOnly, new Point(382, 396) },
            { LevelModeBudgets.Deflation, new Point(435, 396) },
            { LevelModeBudgets.MediumStandard, new Point(492, 365) },
            { LevelModeBudgets.MilitaryOnly, new Point(467, 396) },
            { LevelModeBudgets.Apopalypse, new Point(494, 396) },
            { LevelModeBudgets.Reverse, new Point(520, 396) },
            { LevelModeBudgets.HardStandard, new Point(580, 365) },
            { LevelModeBudgets.MagicMonkeysOnly, new Point(543, 373) },
            { LevelModeBudgets.DoubleHpMoabs, new Point(552, 396) },
            { LevelModeBudgets.HalfCash, new Point(607, 396) },
            { LevelModeBudgets.AlternateBloonsRounds, new Point(616, 373) },
            { LevelModeBudgets.Impoppable, new Point(666, 365) },
            { LevelModeBudgets.CHIMPS, new Point(693, 396) }
        };

        public static Point GetLevelModeBudgetPos(LevelModeBudgets levelModeBudget, int index)
        {
            Point basePos = LevelModeBudgetPoint.TryGetValue(levelModeBudget, out var pos)
                ? pos
                 : new Point(0, 0);
            return new Point(basePos.X + 424 * (index % 3), basePos.Y + 314 * (index / 3));
        }

        public static List<Point> UpgradeYellowPosition => new List<Point>()
        {
            new Point(56, 440),
            new Point(56, 465),
            new Point(56, 490),
            new Point(56, 515),
            new Point(56, 540),
            new Point(56, 590),
            new Point(56, 615),
            new Point(56, 640),
            new Point(56, 665),
            new Point(56, 690),
            new Point(56, 740),
            new Point(56, 765),
            new Point(56, 790),
            new Point(56, 815),
            new Point(56, 840),
            new Point(1278, 440),
            new Point(1278, 465),
            new Point(1278, 490),
            new Point(1278, 515),
            new Point(1278, 540),
            new Point(1278, 590),
            new Point(1278, 615),
            new Point(1278, 640),
            new Point(1278, 665),
            new Point(1278, 690),
            new Point(1278, 740),
            new Point(1278, 765),
            new Point(1278, 790),
            new Point(1278, 815),
            new Point(1278, 840)
        };

        
        public static readonly Dictionary<LevelDifficulties, string> _levelDifficultyNames = new Dictionary<LevelDifficulties, string>
        {
            { LevelDifficulties.Easy, "简单" },
            { LevelDifficulties.Medium, "中级" },
            { LevelDifficulties.Hard, "困难" },
            { LevelDifficulties.Any, "任意" }
        };

        public static string GetTypeName(LevelDifficulties difficulty)
        {
            return _levelDifficultyNames.TryGetValue(difficulty, out var name)
                ? name
                 : "未知难度";
        }

        
        private static readonly Dictionary<FunctionTypes, string> _functionNames = new Dictionary<FunctionTypes, string>
        {
            { FunctionTypes.Custom, "自定义" },
            { FunctionTypes.Collection, "刷收集" },
            { FunctionTypes.Circulation, "循环刷关" },
            { FunctionTypes.Race, "自动凹竞速" },
            { FunctionTypes.BlackBorder, "刷黑框" },
            { FunctionTypes.Events, "刷每日挑战" }
        };

        public static string GetTypeName(FunctionTypes FunctionTypes)
        {
            return _functionNames.TryGetValue(FunctionTypes, out var name)
                ? name
                 : "未知功能";
        }

        
        private static readonly Dictionary<Heroes, string> _heroNames = new Dictionary<Heroes, string>
        {
            { Heroes.Quincy, "昆西" },
            { Heroes.Gwendolin, "格温多林" },
            { Heroes.StrikerJones, "先锋琼斯"},
            { Heroes.ObynGreenfoot, "奥本"},
            { Heroes.Rosalia, "罗莎莉亚" },
            { Heroes.CaptainChurchill, "上尉丘吉尔"},
            { Heroes.Benjamin, "本杰明"},
            { Heroes.PatFusty, "帕特"},
            { Heroes.Ezili, "艾泽里"},
            { Heroes.Adora, "阿多拉"},
            { Heroes.Etienne, "艾蒂安"},
            { Heroes.Sauda, "萨乌达"},
            { Heroes.AdmiralBrickell, "海军上将布里克尔"},
            { Heroes.Psi, "灵机"},
            { Heroes.Geraldo, "杰拉尔多"},
            { Heroes.Corvus, "科沃斯"}
        };

        // 获取中文名称
        public static string GetTypeName(Heroes hero)
        {
            return _heroNames.TryGetValue(hero, out var name)
                ? name
                 : "未知英雄";
        }

        // 通过数字获取枚举值
        public static Heroes GetHeroType(int id)
        {
            return Enum.GetValues(typeof(Heroes))
                .Cast<Heroes>()
                .FirstOrDefault(e => (int)e == id);
        }

        
        private static readonly Dictionary<Monkeys, string> _monkeyNames = new Dictionary<Monkeys, string>
        {
            { Monkeys.DartMonkey, "飞镖猴" },
            { Monkeys.BoomerangMonkey, "回旋镖猴" },
            { Monkeys.BombShooter, "大炮" },
            { Monkeys.TackShooter, "图钉塔" },
            { Monkeys.IceMonkey, "冰猴" },
            { Monkeys.GlueGunner, "胶水猴" },
            { Monkeys.Desperado, "亡命猴" },
            { Monkeys.SniperMonkey, "狙击猴" },
            { Monkeys.MonkeySub, "潜水艇猴" },
            { Monkeys.MonkeyBuccaneer, "海盗猴" },
            { Monkeys.MonkeyAce, "王牌飞行员" },
            { Monkeys.HeliPilot, "直升机" },
            { Monkeys.MortarMonkey, "迫击炮猴" },
            { Monkeys.DartlingGunner, "机枪猴" },
            { Monkeys.WizardMonkey, "法师猴" },
            { Monkeys.SuperMonkey, "超猴" },
            { Monkeys.NinjaMonkey, "忍者猴" },
            { Monkeys.Alchemist, "炼金术士" },
            { Monkeys.Druid, "德鲁伊" },
            { Monkeys.MerMonkey, "人鱼猴" },
            { Monkeys.BananaFarm, "香蕉农场" },
            { Monkeys.SpikeFactory, "刺钉工厂" },
            { Monkeys.MonkeyVillage, "猴村" },
            { Monkeys.EngineerMonkey, "工程师猴" },
            { Monkeys.BeastHandler, "驯兽大师" }
        };

        // 获取中文名称
        public static string GetTypeName(Monkeys monkey)
        {
            return _monkeyNames.TryGetValue(monkey, out var name)
                ? name
                : "未知猴塔";
        }

        // 通过数字获取枚举值
        public static Monkeys? GetMonkeyType(int id)
        {
            return Enum.GetValues(typeof(Monkeys))
                .Cast<Monkeys>()
                .FirstOrDefault(e => (int)e == id);
        }

        public static List<Maps> MapsToDisplay = new List<Maps>
        {
            Maps.MonkeyMeadow,
            Maps.InTheLoop,
            Maps.MiddleOfTheRoad,
            Maps.SpaPits,
            Maps.TinkerTon,
            Maps.TreeStump,
            Maps.TownCenter,
            Maps.OneTwoTree,
            Maps.ScrapYard,
            Maps.TheCabin,
            Maps.Resort,
            Maps.Skates,
            Maps.LotusIsland,
            Maps.CandyFalls,
            Maps.WinterPark,
            Maps.Carved,
            Maps.ParkPath,
            Maps.AlpineRun,
            Maps.FrozenOver,
            Maps.Cubism,
            Maps.FourCircles,
            Maps.Hedge,
            Maps.EndOfTheRoad,
            Maps.Logs,
            Maps.LuminousCove,
            Maps.SulfurSprings,
            Maps.WaterPark,
            Maps.Polyphemus,
            Maps.CoveredGarden,
            Maps.Quarry,
            Maps.QuietStreet,
            Maps.BloonariusPrime,
            Maps.Balance,
            Maps.Encrypted,
            Maps.Bazaar,
            Maps.AdorasTemple,
            Maps.SpringSpring,
            Maps.KartMonkey,
            Maps.MoonLanding,
            Maps.Haunted,
            Maps.Downstream,
            Maps.FiringRange,
            Maps.Cracked,
            Maps.Streambed,
            Maps.Chutes,
            Maps.Rake,
            Maps.SpiceIslands,
            Maps.SunsetGulch,
            Maps.EnchantedGlade,
            Maps.LastResort,
            Maps.AncientPortal,
            Maps.CastleRevenge,
            Maps.DarkPath,
            Maps.Erosion,
            Maps.MidnightMansion,
            Maps.SunkenColumns,
            Maps.XFactor,
            Maps.Mesa,
            Maps.Geared,
            Maps.Spillway,
            Maps.Cargo,
            Maps.PatsPond,
            Maps.Peninsula,
            Maps.HighFinance,
            Maps.AnotherBrick,
            Maps.OffTheCoast,
            Maps.Cornfield,
            Maps.Underground,
            Maps.GlacialTrail,
            Maps.DarkDungeon,
            Maps.Sanctuary,
            Maps.Ravine,
            Maps.FloodedValley,
            Maps.Infernal,
            Maps.BloodyPuddles,
            Maps.Workshop,
            Maps.Quad,
            Maps.DarkCastle,
            Maps.MuddyPuddles,
            Maps.Ouch
        };

        private static readonly Dictionary<Maps, string> _mapNames = new Dictionary<Maps, string>
        {
            { Maps.MonkeyMeadow, "猴子草甸" },
            { Maps.InTheLoop, "循环" },
            { Maps.MiddleOfTheRoad, "道路中间" },
            { Maps.TinkerTon, "汀克顿" },
            { Maps.SpaPits, "水疗温泉" },
            { Maps.TreeStump, "树桩" },
            { Maps.TownCenter, "市中心" },
            { Maps.OneTwoTree, "一二杉" },
            { Maps.ScrapYard, "废料厂" },
            { Maps.TheCabin, "小木屋" },
            { Maps.Resort, "度假胜地" },
            { Maps.Skates, "溜冰鞋" },
            { Maps.LotusIsland, "莲花岛" },
            { Maps.CandyFalls, "糖果瀑布" },
            { Maps.WinterPark, "冬季公园" },
            { Maps.Carved, "鬼脸南瓜" },
            { Maps.ParkPath, "公园路径" },
            { Maps.AlpineRun, "高山竞速" },
            { Maps.FrozenOver, "冰冻三尺" },
            { Maps.Cubism, "立体主义" },
            { Maps.FourCircles, "四个圈子" },
            { Maps.Hedge, "树篱" },
            { Maps.EndOfTheRoad, "路的尽头" },
            { Maps.Logs, "原木" },
            { Maps.SulfurSprings, "硫磺泉" },
            { Maps.WaterPark, "水上乐园" },
            { Maps.Polyphemus, "独眼巨人" },
            { Maps.CoveredGarden, "隐蔽的花园" },
            { Maps.Quarry, "采石场" },
            { Maps.QuietStreet, "静谧街道" },
            { Maps.BloonariusPrime, "布隆纳留斯精英" },
            { Maps.Balance, "平衡" },
            { Maps.Encrypted, "已加密" },
            { Maps.Bazaar, "集市" },
            { Maps.AdorasTemple, "阿多拉神庙" },
            { Maps.SpringSpring, "复活节春天" },
            { Maps.KartMonkey, "飞镖卡丁车" },
            { Maps.MoonLanding, "登月" },
            { Maps.Haunted, "鬼屋" },
            { Maps.Downstream, "顺流而下" },
            { Maps.FiringRange, "靶场" },
            { Maps.Cracked, "龟裂之地" },
            { Maps.Streambed, "河床" },
            { Maps.Chutes, "滑槽" },
            { Maps.Rake, "耙" },
            { Maps.SpiceIslands, "香料群岛" },
            { Maps.LuminousCove, "夜光海湾" },
            { Maps.CastleRevenge, "城堡复仇" },
            { Maps.DarkPath, "黑暗之径" },
            { Maps.Erosion, "侵蚀" },
            { Maps.MidnightMansion, "午夜豪宅" },
            { Maps.SunkenColumns, "凹陷的柱子" },
            { Maps.XFactor, "X因子" },
            { Maps.Mesa, "梅萨" },
            { Maps.Geared, "齿轮转动" },
            { Maps.Spillway, "泄洪道" },
            { Maps.Cargo, "货运" },
            { Maps.PatsPond, "帕特的池塘" },
            { Maps.Peninsula, "半岛" },
            { Maps.HighFinance, "高级金融" },
            { Maps.AnotherBrick, "另一块砖" },
            { Maps.OffTheCoast, "海岸" },
            { Maps.Cornfield, "玉米地" },
            { Maps.Underground, "地下" },
            { Maps.AncientPortal, "古代传送门" },
            { Maps.LastResort, "破釜沉舟" },
            { Maps.EnchantedGlade, "魔法林地" },
            { Maps.SunsetGulch, "日落峡谷" },
            { Maps.GlacialTrail, "冰河之径" },
            { Maps.DarkDungeon, "黑暗地下城" },
            { Maps.Sanctuary, "避难所" },
            { Maps.Ravine, "峡谷" },
            { Maps.FloodedValley, "水淹山谷" },
            { Maps.Infernal, "炼狱" },
            { Maps.BloodyPuddles, "血腥水坑" },
            { Maps.Workshop, "工坊" },
            { Maps.Quad, "方院" },
            { Maps.DarkCastle, "黑暗城堡" },
            { Maps.MuddyPuddles, "泥泞的水坑" },
            { Maps.Ouch, "#哎哟" }
        };

        // 获取中文名称
        public static string GetTypeName(Maps map)
        {
            //Debug.WriteLine($"maps: {map}");

            return _mapNames.TryGetValue(map, out var name)
                ? name
                 : "未知地图";
        }

        public static Maps GetMapType(int id)
        {
            return Enum.GetValues(typeof(Maps))
                .Cast<Maps>()
                .FirstOrDefault(e => (int)e == id);
        }

        
        private static readonly Dictionary<Maps, MapTypes> _mapTypes = new Dictionary<Maps, MapTypes>
        {
            { Maps.MonkeyMeadow, MapTypes.Beginner },
            { Maps.InTheLoop, MapTypes.Beginner },
            { Maps.MiddleOfTheRoad, MapTypes.Beginner },
            { Maps.TinkerTon, MapTypes.Beginner },
            { Maps.SpaPits, MapTypes.Beginner },
            { Maps.TreeStump, MapTypes.Beginner },
            { Maps.TownCenter, MapTypes.Beginner },
            { Maps.OneTwoTree, MapTypes.Beginner },
            { Maps.ScrapYard, MapTypes.Beginner },
            { Maps.TheCabin, MapTypes.Beginner },
            { Maps.Resort, MapTypes.Beginner },
            { Maps.Skates, MapTypes.Beginner },
            { Maps.LotusIsland, MapTypes.Beginner },
            { Maps.CandyFalls, MapTypes.Beginner },
            { Maps.WinterPark, MapTypes.Beginner },
            { Maps.Carved, MapTypes.Beginner },
            { Maps.ParkPath, MapTypes.Beginner },
            { Maps.AlpineRun, MapTypes.Beginner },
            { Maps.FrozenOver, MapTypes.Beginner },
            { Maps.Cubism, MapTypes.Beginner },
            { Maps.FourCircles, MapTypes.Beginner },
            { Maps.Hedge, MapTypes.Beginner },
            { Maps.EndOfTheRoad, MapTypes.Beginner },
            { Maps.Logs, MapTypes.Beginner },
            { Maps.SulfurSprings, MapTypes.Intermediate},
            { Maps.WaterPark, MapTypes.Intermediate },
            { Maps.Polyphemus, MapTypes.Intermediate },
            { Maps.CoveredGarden, MapTypes.Intermediate },
            { Maps.Quarry, MapTypes.Intermediate },
            { Maps.QuietStreet, MapTypes.Intermediate },
            { Maps.BloonariusPrime, MapTypes.Intermediate },
            { Maps.Balance, MapTypes.Intermediate },
            { Maps.Encrypted, MapTypes.Intermediate },
            { Maps.Bazaar, MapTypes.Intermediate },
            { Maps.AdorasTemple, MapTypes.Intermediate },
            { Maps.SpringSpring, MapTypes.Intermediate },
            { Maps.KartMonkey, MapTypes.Intermediate },
            { Maps.MoonLanding, MapTypes.Intermediate },
            { Maps.Haunted, MapTypes.Intermediate },
            { Maps.Downstream, MapTypes.Intermediate },
            { Maps.FiringRange, MapTypes.Intermediate },
            { Maps.Cracked, MapTypes.Intermediate },
            { Maps.Streambed, MapTypes.Intermediate },
            { Maps.Chutes, MapTypes.Intermediate },
            { Maps.Rake, MapTypes.Intermediate },
            { Maps.SpiceIslands, MapTypes.Intermediate },
            { Maps.LuminousCove, MapTypes.Intermediate },
            { Maps.CastleRevenge, MapTypes.Advanced },
            { Maps.DarkPath, MapTypes.Advanced },
            { Maps.Erosion, MapTypes.Advanced },
            { Maps.MidnightMansion, MapTypes.Advanced },
            { Maps.SunkenColumns, MapTypes.Advanced },
            { Maps.XFactor, MapTypes.Advanced },
            { Maps.Mesa, MapTypes.Advanced },
            { Maps.Geared, MapTypes.Advanced },
            { Maps.Spillway, MapTypes.Advanced },
            { Maps.Cargo, MapTypes.Advanced },
            { Maps.PatsPond, MapTypes.Advanced },
            { Maps.Peninsula, MapTypes.Advanced },
            { Maps.HighFinance, MapTypes.Advanced },
            { Maps.AnotherBrick, MapTypes.Advanced },
            { Maps.OffTheCoast, MapTypes.Advanced },
            { Maps.Cornfield, MapTypes.Advanced },
            { Maps.Underground, MapTypes.Advanced },
            { Maps.AncientPortal, MapTypes.Advanced },
            { Maps.LastResort, MapTypes.Advanced },
            { Maps.EnchantedGlade, MapTypes.Advanced },
            { Maps.SunsetGulch, MapTypes.Advanced },
            { Maps.GlacialTrail, MapTypes.Expert },
            { Maps.DarkDungeon, MapTypes.Expert },
            { Maps.Sanctuary, MapTypes.Expert },
            { Maps.Ravine, MapTypes.Expert },
            { Maps.FloodedValley, MapTypes.Expert },
            { Maps.Infernal, MapTypes.Expert },
            { Maps.BloodyPuddles, MapTypes.Expert },
            { Maps.Workshop, MapTypes.Expert },
            { Maps.Quad, MapTypes.Expert },
            { Maps.DarkCastle, MapTypes.Expert },
            { Maps.MuddyPuddles, MapTypes.Expert },
            { Maps.Ouch, MapTypes.Expert }
        };

        public static MapTypes GetMapType(Maps map)
        {
            return _mapTypes.TryGetValue(map, out var type)
                ? type
                 : MapTypes.Beginner;
        }

        
        private static readonly Dictionary<LevelMode, string> _levelModeNames = new Dictionary<LevelMode, string>
        {
            { LevelMode.Standard, "标准" },
            { LevelMode.PrimaryOnly, "仅初级" },
            { LevelMode.Deflation, "放气" },
            { LevelMode.MilitaryOnly, "仅军事" },
            { LevelMode.Apopalypse, "天启" },
            { LevelMode.Reverse, "相反" },
            { LevelMode.MagicMonkeysOnly, "仅魔法" },
            { LevelMode.DoubleHpMoabs, "双倍生命值MOAB" },
            { LevelMode.HalfCash, "现金减半" },
            { LevelMode.AlternateBloonsRounds, "代替气球回合" },
            { LevelMode.Impoppable, "极难模式" },
            { LevelMode.CHIMPS, "点击" }
        };

        // 获取中文名称
        public static string GetTypeName(LevelMode mode)
        {
            return _levelModeNames.TryGetValue(mode, out var name)
                ? name
                 : "未知模式";
        }

        // 通过数字获取枚举值
        public static LevelMode GetLevelModeType(int id)
        {
            return Enum.GetValues(typeof(LevelMode))
                .Cast<LevelMode>()
                .FirstOrDefault(e => (int)e == id);
        }

        
        private static readonly Dictionary<ActionTypes, string> _actionNames = new Dictionary<ActionTypes, string>
        {
            { ActionTypes.PlaceMonkey, "放置猴子" },
            { ActionTypes.UpgradeMonkey, "升级猴子" },
            { ActionTypes.SwitchMonkeyTarget, "切换猴子目标" },
            { ActionTypes.UseAbility, "使用技能" },
            { ActionTypes.SwitchSpeed, "切换倍速" },
            { ActionTypes.SellMonkey, "出售猴子" },
            { ActionTypes.SetMonkeyFunction, "设置猴子功能" },
            { ActionTypes.PlaceHero, "放置英雄" },
            { ActionTypes.UpgradeHero, "升级英雄" },
            { ActionTypes.PlaceHeroItem, "英雄物品放置" },
            { ActionTypes.SwitchHeroTarget, "切换英雄目标" },
            { ActionTypes.SetHeroFunction, "设置英雄功能" },
            { ActionTypes.SellHero, "出售英雄" },
            { ActionTypes.MouseClick, "鼠标点击" },
            { ActionTypes.AdjustMonkeyCoordinates, "修改猴子坐标" },
            { ActionTypes.WaitMilliseconds, "等待(ms)" },
            { ActionTypes.StartFreeplay, "开启自由游戏" },
            { ActionTypes.EndFreeplay, "结束自由游戏" },
            { ActionTypes.Jump, "指令跳转" },
            { ActionTypes.QuickCommandBundle, "快捷指令包" }
        };

        // 获取中文名称
        public static string GetTypeName(ActionTypes action)
        {
            return _actionNames.TryGetValue(action, out var name)
                ? name
                 : "未知动作";
        }

        // 通过数字获取枚举值
        public static ActionTypes GetActionType(int id)
        {
            return Enum.GetValues(typeof(ActionTypes))
                .Cast<ActionTypes>()
                .FirstOrDefault(e => (int)e == id);
        }


        private static readonly Dictionary<SkillTypes, string> _skillNames = new Dictionary<SkillTypes, string>
        {
            { SkillTypes.Skill1, "技能1" },
            { SkillTypes.Skill2, "技能2" },
            { SkillTypes.Skill3, "技能3" },
            { SkillTypes.Skill4, "技能4" },
            { SkillTypes.Skill5, "技能5" },
            { SkillTypes.Skill6, "技能6" },
            { SkillTypes.Skill7, "技能7" },
            { SkillTypes.Skill8, "技能8" },
            { SkillTypes.Skill9, "技能9" },
            { SkillTypes.Skill10, "技能10" },
            { SkillTypes.Skill11, "技能11" },
            { SkillTypes.Skill12, "技能12" }
        };

        // 获取中文名称
        public static string GetTypeName(SkillTypes skill)
        {
            return _skillNames.TryGetValue(skill, out var name)
                ? name
                 : "未知技能";
        }

        // 通过数字获取枚举值
        public static SkillTypes GetSkillType(int id)
        {
            return Enum.GetValues(typeof(SkillTypes))
                .Cast<SkillTypes>()
                .FirstOrDefault(e => (int)e == id);
        }

        public static string GetTypeName(object obj, Type type)
        {
            if (type == typeof(Maps))
            {
                return GetTypeName((Maps)obj);
            }
            else if (type == typeof(LevelMode))
            {
                return GetTypeName((LevelMode)obj);
            }
            else if (type == typeof(ActionTypes))
            {
                return GetTypeName((ActionTypes)obj);
            }
            else if (type == typeof(Monkeys))
            {
                return GetTypeName((Monkeys)obj);
            }
            else if (type == typeof(Heroes))
            {
                return GetTypeName((Heroes)obj);
            }
            else if (type == typeof(FunctionTypes))
            {
                return GetTypeName((FunctionTypes)obj);
            }
            else if (type == typeof(LevelDifficulties))
            {
                return GetTypeName((LevelDifficulties)obj);
            }
            else if (type == typeof(SkillTypes))
            {
                return GetTypeName((SkillTypes)obj);
            }
            else 
            {
                return obj.ToString();
            }
        }
    }
}
