using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.GameObjects
{
    public enum Maps
    {
        // 猴子草甸
        MonkeyMeadow = 0,
        // 循环
        InTheLoop = 1,
        //三矿回合
        ThreeMilesRound = 24,
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
        // 失落冰隙
        LostCrevasse = 53,

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
        Ouch = 101,

        Unknown = 255
    }

    public enum LevelModes
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
        CHIMPS = 8,

        Unkown = 255
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
        CHIMPS = 13,

        Unkown = 255
    }

    public enum MapTypes
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert
    }

    public enum LevelDifficulties
    {
        Easy = 0,
        Medium = 1,
        Hard = 2,
        Any = 3,
        Unknown = 255
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
        Silas = 16,
        Unkown = 255
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
        BeastHandler = 34,        // 驯兽大师

        Unkown = 255
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
}
