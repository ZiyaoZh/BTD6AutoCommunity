using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BTD6AutoCommunity.Strategies;
using BTD6AutoCommunity.ScriptEngine;
using BTD6AutoCommunity.GameObjects;

namespace BTD6AutoCommunity.Core
{
    public static class Constants
    {
        //public static Dictionary<int, string> InstructionPackages => new Dictionary<int, string>
        //{
        //    { 0, "022飞镖猴(模范)" },
        //    { 1, "022回旋镖猴(模范)" },
        //    { 5, "022海盗(模范)" },
        //    { 6, "220潜艇(模范)" },
        //    { 7, "022王牌飞行员(模范)" },
        //    { 10, "022法师猴(模范)" },
        //    { 11, "022忍者猴(模范)" },
        //    { 15, "022工程师(模范)" },
        //    { 20, "032飞镖猴(强力)" },
        //    { 21, "204回旋镖(强力)" },
        //    { 22, "024回旋镖(强力)" },
        //    { 23, "031大炮(强力)" },
        //    { 24, "050大炮(强力+出售)" },
        //    { 25, "420冰猴" },
        //    { 26, "204图钉塔" },
        //    { 27, "420火锅" },
        //    { 30, "302狙击猴(强力)" },
        //    { 31, "204狙击猴(强力)" },
        //    { 32, "042空投狙" },
        //    { 33, "004商船" },
        //    { 34, "042炮船" },
        //    { 35, "420驱逐舰(强力)" },
        //    { 36, "050沙皇炸弹" },
        //    { 37, "204科曼奇" },
        //    { 38, "420长空(翼猴)" },
        //    { 39, "204潜艇" },
        //    { 40, "130德鲁伊" },
        //    { 41, "302超猴(强力)" },
        //    { 42, "203超猴" },
        //    { 43, "300炼金(出售)" },
        //    { 44, "320炼金术士(出售)" },
        //    { 45, "401人鱼猴" },
        //    { 50, "023香蕉农场" },
        //    { 51, "420香蕉农场" },
        //    { 52, "240刺钉工厂" },
        //    { 53, "032刺钉工厂(靠近)" }
        //};

        public static Dictionary<LevelModes, LevelDifficulties> LevelModeToDifficulty => new Dictionary<LevelModes, LevelDifficulties>()
        {
            { LevelModes.Standard, LevelDifficulties.Any },
            { LevelModes.PrimaryOnly, LevelDifficulties.Easy },
            { LevelModes.Deflation, LevelDifficulties.Easy },
            { LevelModes.MilitaryOnly, LevelDifficulties.Medium },
            { LevelModes.Apopalypse, LevelDifficulties.Medium },
            { LevelModes.Reverse, LevelDifficulties.Medium },
            { LevelModes.MagicMonkeysOnly, LevelDifficulties.Hard },
            { LevelModes.DoubleHpMoabs, LevelDifficulties.Hard },
            { LevelModes.HalfCash, LevelDifficulties.Hard },
            { LevelModes.AlternateBloonsRounds, LevelDifficulties.Hard },
            { LevelModes.Impoppable, LevelDifficulties.Hard },
            { LevelModes.CHIMPS, LevelDifficulties.Hard }
        };

        public static Dictionary<CollectionMode, string> CollectionScripts => new Dictionary<CollectionMode, string>()
        {
            { CollectionMode.SimpleCollection, "简单收集" },
            { CollectionMode.DoubleCashCollection, "双金收集" },
            { CollectionMode.FastPathCollection, "快速路径收集" }
        };


        private static readonly Dictionary<LevelModes, Point> LevelModePoint = new Dictionary<LevelModes, Point>()
        {
            { LevelModes.Standard, new Point(630, 590) },
            { LevelModes.PrimaryOnly, new Point(960, 450) },
            { LevelModes.Deflation, new Point(1300, 450) },
            { LevelModes.MilitaryOnly, new Point(960, 450) },
            { LevelModes.Apopalypse, new Point(1300, 450) },
            { LevelModes.Reverse, new Point(960, 750) },
            { LevelModes.MagicMonkeysOnly, new Point(960, 450) },
            { LevelModes.DoubleHpMoabs, new Point(1300, 450) },
            { LevelModes.HalfCash, new Point(1600, 450) },
            { LevelModes.AlternateBloonsRounds, new Point(960, 750) },
            { LevelModes.Impoppable, new Point(1300, 750) },
            { LevelModes.CHIMPS, new Point(1600, 750) }
        };

        public static Point GetLevelModePos(LevelModes levelMode)
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
            { LevelDifficulties.Unknown, "未知难度" }
        };

        public static string GetTypeName(LevelDifficulties difficulty)
        {
            return _levelDifficultyNames.TryGetValue(difficulty, out var name)
                ? name
                 : "未知难度";
        }

        public static List<LevelDifficulties> DifficultiesList => new List<LevelDifficulties>
        { LevelDifficulties.Easy, LevelDifficulties.Medium, LevelDifficulties.Hard};


        private static readonly Dictionary<FunctionTypes, string> _functionNames = new Dictionary<FunctionTypes, string>
        {
            { FunctionTypes.Custom, "自定义" },
            { FunctionTypes.Collection, "刷收集" },
            { FunctionTypes.Circulation, "循环刷关" },
            { FunctionTypes.Race, "自动凹竞速" },
            { FunctionTypes.BlackBorder, "刷黑框" }
        };

        public static string GetTypeName(FunctionTypes FunctionTypes)
        {
            return _functionNames.TryGetValue(FunctionTypes, out var name)
                ? name
                 : "未知功能";
        }

        public static List<FunctionTypes> FunctionsList => new List<FunctionTypes>
        { FunctionTypes.Custom, FunctionTypes.Collection, FunctionTypes.Circulation, FunctionTypes.Race, FunctionTypes.BlackBorder};

        private static readonly Dictionary<Heroes, string> _heroNames = new Dictionary<Heroes, string>
        {
            { Heroes.Quincy, "昆西" },
            { Heroes.Gwendolin, "格温多琳" },
            { Heroes.StrikerJones, "先锋琼斯"},
            { Heroes.ObynGreenfoot, "奥本"},
            { Heroes.Rosalia, "罗莎莉娅" },
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
            { Heroes.Corvus, "科沃斯"},
            { Heroes.Silas, "西拉斯" },
            { Heroes.Unkown, "未知英雄" }
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

        public static List<Heroes> HeroesList => new List<Heroes>
        {
            Heroes.Quincy,
            Heroes.Gwendolin,
            Heroes.StrikerJones,
            Heroes.ObynGreenfoot,
            Heroes.Silas,
            Heroes.Benjamin,
            Heroes.PatFusty,
            Heroes.CaptainChurchill,
            Heroes.Ezili,
            Heroes.Rosalia,
            Heroes.Etienne,
            Heroes.Sauda,
            Heroes.Adora,
            Heroes.AdmiralBrickell,
            Heroes.Psi,
            Heroes.Geraldo,
            Heroes.Corvus
        };


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
            { Monkeys.BeastHandler, "驯兽大师" },
            { Monkeys.Unkown, "未知猴塔" }
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

        public static List<Monkeys> MonkeysList => new List<Monkeys>
        {
            Monkeys.DartMonkey,
            Monkeys.BoomerangMonkey,
            Monkeys.BombShooter,
            Monkeys.TackShooter,
            Monkeys.IceMonkey,
            Monkeys.GlueGunner,
            Monkeys.Desperado,
            Monkeys.SniperMonkey,
            Monkeys.MonkeySub,
            Monkeys.MonkeyBuccaneer,
            Monkeys.MonkeyAce,
            Monkeys.HeliPilot,
            Monkeys.MortarMonkey,
            Monkeys.DartlingGunner,
            Monkeys.WizardMonkey,
            Monkeys.SuperMonkey,
            Monkeys.NinjaMonkey,
            Monkeys.Alchemist,
            Monkeys.Druid,
            Monkeys.MerMonkey,
            Monkeys.BananaFarm,
            Monkeys.SpikeFactory,
            Monkeys.MonkeyVillage,
            Monkeys.EngineerMonkey,
            Monkeys.BeastHandler
        };

        private static readonly Dictionary<Maps, string> _mapNames = new Dictionary<Maps, string>
        {
            { Maps.MonkeyMeadow, "猴子草甸" },
            { Maps.InTheLoop, "循环" },
            { Maps.ThreeMilesRound, "三矿回合" },
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
            { Maps.LostCrevasse, "失落冰隙" },
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
            { Maps.Ouch, "#哎哟" },
            { Maps.TrickyTracks, "棘手的轨道" },
            { Maps.Unknown, "未知地图" }
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

        public static List<Maps> MapsList => new List<Maps>
        {
            Maps.MonkeyMeadow,
            Maps.InTheLoop,
            Maps.ThreeMilesRound,
            Maps.SpaPits,
            Maps.TinkerTon,
            Maps.TreeStump,
            Maps.TownCenter,
            Maps.MiddleOfTheRoad,
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
            Maps.LostCrevasse,
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
            Maps.TrickyTracks,
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


        private static readonly Dictionary<Maps, MapTypes> _mapTypes = new Dictionary<Maps, MapTypes>
        {
            { Maps.MonkeyMeadow, MapTypes.Beginner },
            { Maps.InTheLoop, MapTypes.Beginner },
            { Maps.ThreeMilesRound, MapTypes.Beginner },
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
            { Maps.LostCrevasse, MapTypes.Intermediate },
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
            { Maps.TrickyTracks, MapTypes.Expert },
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


        private static readonly Dictionary<LevelModes, string> _levelModeNames = new Dictionary<LevelModes, string>
        {
            { LevelModes.Standard, "标准" },
            { LevelModes.PrimaryOnly, "仅初级" },
            { LevelModes.Deflation, "放气" },
            { LevelModes.MilitaryOnly, "仅军事" },
            { LevelModes.Apopalypse, "天启" },
            { LevelModes.Reverse, "相反" },
            { LevelModes.MagicMonkeysOnly, "仅魔法" },
            { LevelModes.DoubleHpMoabs, "双倍生命" },
            { LevelModes.HalfCash, "现金减半" },
            { LevelModes.AlternateBloonsRounds, "替代气球" },
            { LevelModes.Impoppable, "极难" },
            { LevelModes.CHIMPS, "点击" }
        };

        // 获取中文名称
        public static string GetTypeName(LevelModes mode)
        {
            return _levelModeNames.TryGetValue(mode, out var name)
                ? name
                 : "未知模式";
        }

        // 通过数字获取枚举值
        public static LevelModes GetLevelModeType(int id)
        {
            return Enum.GetValues(typeof(LevelModes))
                .Cast<LevelModes>()
                .FirstOrDefault(e => (int)e == id);
        }

        public static List<LevelModes> ModesList => new List<LevelModes>
        {
            LevelModes.Standard,
            LevelModes.PrimaryOnly,
            LevelModes.Deflation,
            LevelModes.MilitaryOnly,
            LevelModes.Apopalypse,
            LevelModes.Reverse,
            LevelModes.MagicMonkeysOnly,
            LevelModes.DoubleHpMoabs,
            LevelModes.HalfCash,
            LevelModes.AlternateBloonsRounds,
            LevelModes.Impoppable,
            LevelModes.CHIMPS
        };


        private static readonly Dictionary<ActionTypes, string> _actionNames = new Dictionary<ActionTypes, string>
        {
            { ActionTypes.PlaceMonkey, "放置猴子" },
            { ActionTypes.UpgradeMonkey, "升级猴子" },
            { ActionTypes.SwitchMonkeyTarget, "切换猴子目标" },
            { ActionTypes.SetMonkeyFunction, "设置猴子功能" },
            { ActionTypes.AdjustMonkeyCoordinates, "修改猴子坐标" },
            { ActionTypes.SellMonkey, "出售猴子" },
            { ActionTypes.PlaceHero, "放置英雄" },
            { ActionTypes.UpgradeHero, "升级英雄" },
            { ActionTypes.PlaceHeroItem, "英雄物品放置" },
            { ActionTypes.SwitchHeroTarget, "切换英雄目标" },
            { ActionTypes.SetHeroFunction, "设置英雄功能" },
            { ActionTypes.SellHero, "出售英雄" },
            { ActionTypes.UseAbility, "使用技能" },
            { ActionTypes.SwitchSpeed, "切换倍速" },
            { ActionTypes.MouseClick, "鼠标点击" },
            { ActionTypes.WaitMilliseconds, "等待(ms)" },
            { ActionTypes.StartFreeplay, "开启自由游戏" },
            { ActionTypes.EndFreeplay, "结束自由游戏" },
            { ActionTypes.Jump, "指令跳转" },
            { ActionTypes.InstructionsBundle, "快捷指令包" }
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

        public static List<ActionTypes> ActionsList => new List<ActionTypes>
        {
            ActionTypes.PlaceMonkey,
            ActionTypes.UpgradeMonkey,
            ActionTypes.SwitchMonkeyTarget,
            ActionTypes.SetMonkeyFunction,
            ActionTypes.AdjustMonkeyCoordinates,
            ActionTypes.SellMonkey,
            ActionTypes.PlaceHero,
            ActionTypes.UpgradeHero,
            ActionTypes.PlaceHeroItem,
            ActionTypes.SwitchHeroTarget,
            ActionTypes.SetHeroFunction,
            ActionTypes.SellHero,
            ActionTypes.UseAbility,
            ActionTypes.SwitchSpeed,
            ActionTypes.MouseClick,
            ActionTypes.WaitMilliseconds,
            ActionTypes.StartFreeplay,
            ActionTypes.EndFreeplay,
            ActionTypes.Jump,
            ActionTypes.InstructionsBundle
        };


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

        public static List<SkillTypes> SkillsList => new List<SkillTypes>
        {
            SkillTypes.Skill1,
            SkillTypes.Skill2,
            SkillTypes.Skill3,
            SkillTypes.Skill4,
            SkillTypes.Skill5,
            SkillTypes.Skill6,
            SkillTypes.Skill7,
            SkillTypes.Skill8,
            SkillTypes.Skill9,
            SkillTypes.Skill10,
            SkillTypes.Skill11,
            SkillTypes.Skill12
        };


        private static readonly Dictionary<TargetTypes, string> _targetNames = new Dictionary<TargetTypes, string>
        {
            { TargetTypes.Right, "右改1次" },
            { TargetTypes.RightDouble, "右改2次" },
            { TargetTypes.RightTriple, "右改3次" },
            { TargetTypes.Left, "左改1次" },
            { TargetTypes.LeftDouble, "左改2次" },
            { TargetTypes.LeftTriple, "左改3次" }
        };

        // 获取中文名称
        public static string GetTypeName(TargetTypes target)
        {
            return _targetNames.TryGetValue(target, out var name)
                ? name
                 : "未知目标";
        }

        // 通过数字获取枚举值
        public static TargetTypes GetTargetType(int id)
        {
            return Enum.GetValues(typeof(TargetTypes))
                .Cast<TargetTypes>()
                .FirstOrDefault(e => (int)e == id);
        }

        public static List<TargetTypes> TargetsList => new List<TargetTypes>
        {
            TargetTypes.Right,
            TargetTypes.RightDouble,
            TargetTypes.RightTriple,
            TargetTypes.Left,
            TargetTypes.LeftDouble,
            TargetTypes.LeftTriple
        };

        private static readonly Dictionary<HeroObjectTypes, string> _heroObjectNames = new Dictionary<HeroObjectTypes, string>
        {
            { HeroObjectTypes.HeroObject1, "英雄物品1"},
            { HeroObjectTypes.HeroObject2, "英雄物品2"},
            { HeroObjectTypes.HeroObject3, "英雄物品3"},
            { HeroObjectTypes.HeroObject4, "英雄物品4"},
            { HeroObjectTypes.HeroObject5, "英雄物品5"},
            { HeroObjectTypes.HeroObject6, "英雄物品6"},
            { HeroObjectTypes.HeroObject7, "英雄物品7"},
            { HeroObjectTypes.HeroObject8, "英雄物品8"},
            { HeroObjectTypes.HeroObject9, "英雄物品9"},
            { HeroObjectTypes.HeroObject10, "英雄物品10"},
            { HeroObjectTypes.HeroObject11, "英雄物品11"},
            { HeroObjectTypes.HeroObject12, "英雄物品12"},
            { HeroObjectTypes.HeroObject13, "英雄物品13"},
            { HeroObjectTypes.HeroObject14, "英雄物品14"},
            { HeroObjectTypes.HeroObject15, "英雄物品15"},
            { HeroObjectTypes.HeroObject16, "英雄物品16"}
        };

        // 获取中文名称
        public static string GetTypeName(HeroObjectTypes heroObject)
        {
            return _heroObjectNames.TryGetValue(heroObject, out var name)
                ? name
                 : "未知英雄物品";
        }

        // 通过数字获取枚举值
        public static HeroObjectTypes GetHeroObjectType(int id)
        {
            return Enum.GetValues(typeof(HeroObjectTypes))
                .Cast<HeroObjectTypes>()
                .FirstOrDefault(e => (int)e == id);
        }

        public static List<HeroObjectTypes> HeroObjectsList => new List<HeroObjectTypes>
        {
            HeroObjectTypes.HeroObject1,
            HeroObjectTypes.HeroObject2,
            HeroObjectTypes.HeroObject3,
            HeroObjectTypes.HeroObject4,
            HeroObjectTypes.HeroObject5,
            HeroObjectTypes.HeroObject6,
            HeroObjectTypes.HeroObject7,
            HeroObjectTypes.HeroObject8,
            HeroObjectTypes.HeroObject9,
            HeroObjectTypes.HeroObject10,
            HeroObjectTypes.HeroObject11,
            HeroObjectTypes.HeroObject12,
            HeroObjectTypes.HeroObject13,
            HeroObjectTypes.HeroObject14,
            HeroObjectTypes.HeroObject15,
            HeroObjectTypes.HeroObject16
        };

        private static readonly Dictionary<UpgradeTypes, string> _upgradePathNames = new Dictionary<UpgradeTypes, string>
        {
            { UpgradeTypes.Top, "上路"},
            { UpgradeTypes.Middle, "中路"},
            { UpgradeTypes.Bottom, "下路"},
            { UpgradeTypes.TopOnce, "上路(仅一次)"},
            { UpgradeTypes.MiddleOnce, "中路(仅一次)"},
            { UpgradeTypes.BottomOnce, "下路(仅一次)"}
        };

        // 获取中文名称
        public static string GetTypeName(UpgradeTypes upgradePath)
        {
            return _upgradePathNames.TryGetValue(upgradePath, out var name)
                ? name
                 : "未知路线";
        }

        // 通过数字获取枚举值
        public static UpgradeTypes GetUpgradePathType(int id)
        {
            return Enum.GetValues(typeof(UpgradeTypes))
                .Cast<UpgradeTypes>()
                .FirstOrDefault(e => (int)e == id);
        }

        public static List<UpgradeTypes> UpgradePathsList => new List<UpgradeTypes>
        {
            UpgradeTypes.Top,
            UpgradeTypes.Middle,
            UpgradeTypes.Bottom,
            UpgradeTypes.TopOnce,
            UpgradeTypes.MiddleOnce,
            UpgradeTypes.BottomOnce
        };

        private static readonly Dictionary<MonkeyFunctionTypes, string> _monkeyFunctionNames = new Dictionary<MonkeyFunctionTypes, string>
        {
            { MonkeyFunctionTypes.Function1, "功能1无坐标选择"},
            { MonkeyFunctionTypes.Function2, "功能2无坐标选择"},
            { MonkeyFunctionTypes.Function1Coordinate, "功能1有坐标选择"},
            { MonkeyFunctionTypes.Function2Coordinate, "功能2有坐标选择"}
        };

        // 获取中文名称
        public static string GetTypeName(MonkeyFunctionTypes monkeyFunction)
        {
            return _monkeyFunctionNames.TryGetValue(monkeyFunction, out var name)
                ? name
                 : "未知功能";
        }

        // 通过数字获取枚举值
        public static MonkeyFunctionTypes GetMonkeyFunctionType(int id)
        {
            return Enum.GetValues(typeof(MonkeyFunctionTypes))
                .Cast<MonkeyFunctionTypes>()
                .FirstOrDefault(e => (int)e == id);
        }

        public static List<MonkeyFunctionTypes> MonkeyFunctionsList => new List<MonkeyFunctionTypes>
        {
            MonkeyFunctionTypes.Function1,
            MonkeyFunctionTypes.Function2,
            MonkeyFunctionTypes.Function1Coordinate,
            MonkeyFunctionTypes.Function2Coordinate
        };

        private static readonly Dictionary<CoordinateTypes, string> _coordinateNames = new Dictionary<CoordinateTypes, string>
        {
            { CoordinateTypes.None, "无坐标选择" },
            { CoordinateTypes.Coordinate, "有坐标选择" }
        };

        // 获取中文名称
        public static string GetTypeName(CoordinateTypes coordinate)
        {
            return _coordinateNames.TryGetValue(coordinate, out var name)
                ? name
                 : "未知坐标选择";
        }

        // 通过数字获取枚举值
        public static CoordinateTypes GetCoordinateType(int id)
        {
            return Enum.GetValues(typeof(CoordinateTypes))
                .Cast<CoordinateTypes>()
                .FirstOrDefault(e => (int)e == id);
        }

        public static List<CoordinateTypes> CoordinatesList => new List<CoordinateTypes>
        {
            CoordinateTypes.None,
            CoordinateTypes.Coordinate
        };

        private static readonly Dictionary<SpeedTypes, string> _speedNames = new Dictionary<SpeedTypes, string>
        {
            { SpeedTypes.Switch, "切换倍速" },
            { SpeedTypes.NextRound, "竞速下一回合" }
        };

        // 获取中文名称
        public static string GetTypeName(SpeedTypes speed)
        {
            return _speedNames.TryGetValue(speed, out var name)
                ? name
                 : "未知倍速";
        }

        // 通过数字获取枚举值
        public static SpeedTypes GetSpeedType(int id)
        {
            return Enum.GetValues(typeof(SpeedTypes))
                .Cast<SpeedTypes>()
                .FirstOrDefault(e => (int)e == id);
        }

        public static List<SpeedTypes> SpeedsList => new List<SpeedTypes>
        {
            SpeedTypes.Switch,
            SpeedTypes.NextRound
        };

        private static readonly Dictionary<PlaceCheckTypes, string> _placeCheckNames = new Dictionary<PlaceCheckTypes, string>
        {
            { PlaceCheckTypes.None, "无放置检测" },
            { PlaceCheckTypes.Check, "有放置检测" }
        };

        // 获取中文名称
        public static string GetTypeName(PlaceCheckTypes placeCheck)
        {
            return _placeCheckNames.TryGetValue(placeCheck, out var name)
                ? name
                 : "未知放置检测";
        }

        // 通过数字获取枚举值
        public static PlaceCheckTypes GetPlaceCheckType(int id)
        {
            return Enum.GetValues(typeof(PlaceCheckTypes))
                .Cast<PlaceCheckTypes>()
                .FirstOrDefault(e => (int)e == id);
        }

        public static List<PlaceCheckTypes> PlaceChecksList => new List<PlaceCheckTypes>
        {
            PlaceCheckTypes.None,
            PlaceCheckTypes.Check
        };

        public static string GetTypeName(object obj, Type type)
        {
            if (type == typeof(Maps))
            {
                return GetTypeName((Maps)obj);
            }
            else if (type == typeof(LevelModes))
            {
                return GetTypeName((LevelModes)obj);
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
            else if (type == typeof(TargetTypes))
            {
                return GetTypeName((TargetTypes)obj);
            }
            else if (type == typeof(HeroObjectTypes))
            {
                return GetTypeName((HeroObjectTypes)obj);
            }
            else if (type == typeof(UpgradeTypes))
            {
                return GetTypeName((UpgradeTypes)obj);
            }
            else if (type == typeof(MonkeyFunctionTypes))
            {
                return GetTypeName((MonkeyFunctionTypes)obj);
            }
            else if (type == typeof(CoordinateTypes))
            {
                return GetTypeName((CoordinateTypes)obj);
            }
            else if (type == typeof(SpeedTypes))
            {
                return GetTypeName((SpeedTypes)obj);
            }
            else if (type == typeof(PlaceCheckTypes))
            {
                return GetTypeName((PlaceCheckTypes)obj);
            }
            else if (type == typeof(MonkeyId))
            {
                return ((MonkeyId)obj).ToString();
            }
            else
            {
                return obj.ToString();
            }
        }
    }
}
