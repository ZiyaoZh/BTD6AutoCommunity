using System;
using static BTD6AutoCommunity.Core.WindowApiWrapper;
using static BTD6AutoCommunity.Core.GameVisionRecognizer;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Buffers.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Diagnostics;

namespace BTD6AutoCommunity.Core
{
    public enum GameState
    {
        GameMainScreen,
        DailyRewardsScreen,
        EventsScreen,
        EventInfoScreen,
        TasksScreen,
        RaceResultsScreen,
        BossResultsScreen,
        HeroSelectionScreen,
        LevelSelectionScreen,
        LevelSearchScreen,
        LevelSearchedScreen,
        LevelDifficultySelectionScreen,
        LevelEasyModeSelectionScreen,
        LevelMediumModeSelectionScreen,
        LevelHardModeSelectionScreen,
        LevelLoadingScreen,
        LevelTipScreen,
        LevelChallengingScreen,
        LevelChallengingWithTipScreen,
        LevelSettingScreen,
        LevelUpgradingScreen,
        LevelFailedScreen,
        LevelPassedScreen,
        LevelSettlementScreen,
        RacePassedScreen,
        RaceSettlementScreen,
        CollectionActivitiesScreen,
        CollectionActivitiesAvailableScreen,
        ChestAnimationScreen,
        ThreeChestsScreen,
        TwoChestsScreen,
        InstaScreen,
        ChestsOpenedScreen,
        ReturnableScreen,
        UnKnown,
    }

    public static class GameStateDescription
    {
        public static readonly Dictionary<GameState, string> stateMap = new Dictionary<GameState, string>
        {
            { GameState.GameMainScreen, "游戏主界面" },
            { GameState.DailyRewardsScreen, "每日奖励界面" },
            { GameState.EventsScreen, "每日活动界面" },
            { GameState.EventInfoScreen, "活动信息界面" },
            { GameState.TasksScreen, "任务界面" },
            { GameState.RaceResultsScreen, "竞速成绩界面" },
            { GameState.BossResultsScreen, "Boss成绩界面" },
            { GameState.HeroSelectionScreen, "英雄选择界面" },
            { GameState.LevelSelectionScreen, "关卡选择界面" },
            { GameState.LevelSearchScreen, "关卡搜索界面" },
            { GameState.LevelSearchedScreen, "关卡搜索后界面"},
            { GameState.LevelDifficultySelectionScreen, "关卡难度选择界面" },
            { GameState.LevelEasyModeSelectionScreen, "关卡简单模式选择界面" },
            { GameState.LevelMediumModeSelectionScreen, "关卡中级模式选择界面" },
            { GameState.LevelHardModeSelectionScreen, "关卡困难模式选择界面" },
            { GameState.LevelLoadingScreen, "关卡加载中界面" },
            { GameState.LevelTipScreen, "关卡提示界面" },
            { GameState.LevelChallengingScreen, "关卡挑战中界面" },
            { GameState.LevelChallengingWithTipScreen, "关卡挑战中遇到提示界面" },
            { GameState.LevelSettingScreen, "关卡设置界面" },
            { GameState.LevelUpgradingScreen, "关卡升级界面" },
            { GameState.LevelFailedScreen, "关卡挑战失败界面" },
            { GameState.LevelPassedScreen, "关卡挑战成功界面" },
            { GameState.LevelSettlementScreen, "关卡结算界面" },
            { GameState.RacePassedScreen, "竞速挑战成功界面" },
            { GameState.RaceSettlementScreen, "竞速结算界面" },
            { GameState.CollectionActivitiesScreen, "收集活动界面" },
            { GameState.CollectionActivitiesAvailableScreen, "收集活动可开箱界面" },
            { GameState.ChestAnimationScreen, "开箱动画界面" },
            { GameState.ThreeChestsScreen, "三个箱子界面" },
            { GameState.TwoChestsScreen, "两个箱子界面" },
            { GameState.InstaScreen, "Insta获取界面"},
            { GameState.ChestsOpenedScreen, "开完箱子界面" },
            { GameState.ReturnableScreen, "可返回界面"},
            { GameState.UnKnown, "未知界面" }
        };

        public static string GetChineseDescription(GameState state)
        {
            return stateMap.TryGetValue(state, out var description)
                ? description
                : "未定义界面";
        }
    }

    public class GameStateMachine
    {
        private GameState currentState;
        private readonly GameContext _context;
        private readonly List<StateRule> _rules;
        private readonly string _rulesPath = Path.Combine("config", "State_rules.json");

        public GameContext Context => _context;

        public GameStateMachine(GameContext context)
        {
            _context = context;
            currentState = GameState.UnKnown;
            _rules = LoadRules(_rulesPath) ?? new List<StateRule>();
        }

        /// <summary>
        /// 优先使用 data/state_rules.json 中的规则判断（规则之间是先后优先）。
        /// OR 行为由添加多条同名 state 规则实现。
        /// 规则中的单项支持：
        ///   - "color": 十六进制字符串 "RRGGBB" 或 "#RRGGBB" 或 "0xRRGGBB"
        ///   - "tolerance": 可选，整数（RGB 差的允许累计值），默认 50
        ///   - "not": 可选，布尔，若为 true 则表示“非”条件（即颜色不匹配）
        /// </summary>
        public GameState GetCurrentState(Bitmap bmp)
        {
            if (!_context.IsValid) return GameState.UnKnown;

            if (_rules != null && _rules.Count > 0)
            {
                foreach (var rule in _rules)
                {
                    //Debug.WriteLine($"Evaluating rule for state: {rule.State}");
                    if (EvaluateRule(rule, bmp))
                    {
                        if (Enum.TryParse(rule.State, true, out GameState matched))
                        {
                            return matched;
                        }
                    }
                }

                // 若未匹配任何规则，回退到 legacy 检测保证向后兼容
            }

            return GetCurrentStateLegacy(bmp);
        }

        #region JSON 规则模型与加载

        [DataContract]
        private class StateRule
        {
            [DataMember(Name = "state")]
            public string State { get; set; }

            [DataMember(Name = "checks")]
            public List<ColorCheck> Checks { get; set; }
        }

        [DataContract]
        private class ColorCheck
        {
            [DataMember(Name = "x")]
            public int X { get; set; }

            [DataMember(Name = "y")]
            public int Y { get; set; }

            // 支持 "F34A12" / "#F34A12" / "0xF34A12"
            [DataMember(Name = "color")]
            public string Color { get; set; }

            // 容差，默认为 50（累加 RGB 分量差）
            [DataMember(Name = "tolerance")]
            public int Tolerance { get; set; } = 50;

            // 非（not）标记：若为 true 则表示颜色不匹配时为通过
            [DataMember(Name = "not")]
            public bool Not { get; set; } = false;
        }

        private List<StateRule> LoadRules(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                using (var fs = File.OpenRead(path))
                {
                    var ser = new DataContractJsonSerializer(typeof(List<StateRule>));
                    var obj = ser.ReadObject(fs) as List<StateRule>;
                    return obj ?? new List<StateRule>();
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 规则评估与工具函数

        private bool EvaluateRule(StateRule rule, Bitmap bmp)
        {
            if (rule?.Checks == null || bmp == null) return false;
            foreach (var c in rule.Checks)
            {
                int expectedInt;
                if (!TryParseHexColor(c.Color, out expectedInt))
                {
                    return false; // 规则配置错误，放弃该规则
                }

                bool match = CheckColorFromBitmap(_context, bmp, c.X, c.Y, expectedInt, c.Tolerance);
                // 如果设置了 not，则要求“不匹配”才能通过该项
                if (c.Not)
                {
                    if (match) return false;
                }
                else
                {
                    if (!match) return false;
                }
            }
            return true;
        }

        private bool TryParseHexColor(string s, out int colorInt)
        {
            colorInt = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var str = s.Trim();
            if (str.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                str = str.Substring(2);
            if (str.StartsWith("#")) str = str.Substring(1);
            if (str.Length != 6) return false;
            try
            {
                colorInt = Convert.ToInt32(str, 16);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 原始硬编码检测（legacy，保留以兼容现有逻辑）

        private GameState GetCurrentStateLegacy(Bitmap bmp)
        {
            if (!_context.IsValid) return GameState.UnKnown;

            if ((CheckColorFromBitmap(_context, bmp, 780, 380, 0xF34A12) && CheckColorFromBitmap(_context, bmp, 780, 760, 0x5388D2) && CheckColorFromBitmap(_context, bmp, 900, 760, 0x62E200)) ||
                (CheckColorFromBitmap(_context, bmp, 820, 330, 0xF24710) && CheckColorFromBitmap(_context, bmp, 820, 760, 0xD2D2D2) && CheckColorFromBitmap(_context, bmp, 960, 760, 0xFFFFFF)))
            {
                return GameState.LevelChallengingWithTipScreen;
            }
            // 关卡挑战中界面
            if (CheckColorFromBitmap(_context, bmp, 1910, 40, 0xB1814A) && CheckColorFromBitmap(_context, bmp, 13, 40, 0xB1814A))
            {

                return GameState.LevelChallengingScreen;
            }
            
            // 关卡挑战中遇到提示界面
            if (CheckColorFromBitmap(_context, bmp, 1658, 40, 0x6A573C) && CheckColorFromBitmap(_context, bmp, 13, 40, 0x62482E))
            {
                // 关卡设置界面
                if (CheckColorFromBitmap(_context, bmp, 580, 240, 0xBE945B) && CheckColorFromBitmap(_context, bmp, 550, 380, 0x9F7843))
                {
                    return GameState.LevelSettingScreen;
                }
            }

            if (CheckColorFromBitmap(_context, bmp, 722, 383, 0x6397D8) && CheckColorFromBitmap(_context, bmp, 1213, 383, 0x6397D8) && CheckColorFromBitmap(_context, bmp, 988, 118, 0xFFFFFF))
                return GameState.LevelFailedScreen;



            // 收集活动相关界面需要优先判断
            if (CheckColorFromBitmap(_context, bmp, 960, 180, 0x121417) && CheckColorFromBitmap(_context, bmp, 750, 60, 0x121417) && CheckColorFromBitmap(_context, bmp, 1130, 200, 0x121417) && CheckColorFromBitmap(_context, bmp, 1725, 500, 0x121417))
            {
                if (CheckColorFromBitmap(_context, bmp, 884, 1000, 0x67E400) && CheckColorFromBitmap(_context, bmp, 1037, 1000, 0x67E400))
                    return GameState.ChestsOpenedScreen;

                if (!CheckColorFromBitmap(_context, bmp, 828, 600, 0x121417) && !CheckColorFromBitmap(_context, bmp, 1130, 600, 0x121417))
                    return GameState.TwoChestsScreen;

                if (!CheckColorFromBitmap(_context, bmp, 660, 600, 0x121417) && !CheckColorFromBitmap(_context, bmp, 1260, 600, 0x121417))
                    return GameState.ThreeChestsScreen;
            }

            // 主界面判断
            if (CheckColorFromBitmap(_context, bmp, 966, 945, 0xFFFFFF) && CheckColorFromBitmap(_context, bmp, 1382, 942, 0xFFFFFF) && CheckColorFromBitmap(_context, bmp, 80, 186, 0xC4E8EB))
                return GameState.GameMainScreen;

            // 地图搜索相关界面
            if (CheckColorFromBitmap(_context, bmp, 983, 48, 0x385373) && CheckColorFromBitmap(_context, bmp, 778, 43, 0x578CD4))
            {
                if (CheckColorFromBitmap(_context, bmp, 962, 837, 0x409FFF))
                    return GameState.LevelSearchedScreen;
                return GameState.LevelSearchScreen;
            }

            if (CheckColorFromBitmap(_context, bmp, 68, 54, 0xFFFFFF))
            {
                // 地图选择界面
                if (CheckColorFromBitmap(_context, bmp, 273, 432, 0xFFD500) && CheckColorFromBitmap(_context, bmp, 1649, 432, 0xFFD500) && CheckColorFromBitmap(_context, bmp, 1313, 960, 0xFF3400))
                    return GameState.LevelSelectionScreen;

                // 难度选择界面
                if (CheckColorFromBitmap(_context, bmp, 716, 627, 0x9F7842) && CheckColorFromBitmap(_context, bmp, 1049, 627, 0x9F7842)
                    && CheckColorFromBitmap(_context, bmp, 1388, 627, 0x9F7842))
                    return GameState.LevelDifficultySelectionScreen;

                if (CheckColorFromBitmap(_context, bmp, 820, 40, 0x804A24) && CheckColorFromBitmap(_context, bmp, 1200, 40, 0x804A24))
                {
                    // 简单模式选择界面
                    if (CheckColorFromBitmap(_context, bmp, 720, 50, 0xF2C776))
                        return GameState.LevelEasyModeSelectionScreen;

                    // 中级模式选择界面
                    if (CheckColorFromBitmap(_context, bmp, 720, 50, 0xC6DFEB))
                        return GameState.LevelMediumModeSelectionScreen;

                    // 困难模式选择界面
                    if (CheckColorFromBitmap(_context, bmp, 720, 50, 0xFFED00))
                        return GameState.LevelHardModeSelectionScreen;
                }

                // 英雄选择界面
                if (CheckColorFromBitmap(_context, bmp, 1601, 1005, 0xFFFFFF) && CheckColorFromBitmap(_context, bmp, 636, 51, 0xFFFFFF) && CheckColorFromBitmap(_context, bmp, 1900, 1060, 0x050505))
                    return GameState.HeroSelectionScreen;

                if (CheckColorFromBitmap(_context, bmp, 750, 60, 0x996633) && CheckColorFromBitmap(_context, bmp, 1190, 60, 0x996633))
                {
                    if (CheckColorFromBitmap(_context, bmp, 600, 320, 0xE2E2E2))
                        return GameState.EventsScreen;
                    else
                        return GameState.EventInfoScreen;
                }

                // 收集活动主界面
                if ((CheckColorFromBitmap(_context, bmp, 750, 60, 0xBF330B) || CheckColorFromBitmap(_context, bmp, 750, 60, 0xCD78FF) || CheckColorFromBitmap(_context, bmp, 750, 60, 0x912DC9))
                    && (CheckColorFromBitmap(_context, bmp, 1100, 60, 0xBF330B) || CheckColorFromBitmap(_context, bmp, 1100, 60, 0xCD78FF) || CheckColorFromBitmap(_context, bmp, 1100, 60, 0x912DC9)))
                    return GameState.CollectionActivitiesScreen;

                // 可返回界面
                return GameState.ReturnableScreen;
            }

            // 关卡完成界面
            if (CheckColorFromBitmap(_context, bmp, 555, 197, 0xFFFFFF))
            {
                if (CheckColorFromBitmap(_context, bmp, 1365, 324, 0xFFFFFF) && CheckColorFromBitmap(_context, bmp, 791, 193, 0xF34A12))
                {
                    return GameState.LevelSettlementScreen;
                }
                if (CheckColorFromBitmap(_context, bmp, 1221, 152, 0xFFFFFF) && CheckColorFromBitmap(_context, bmp, 585, 579, 0x5F93D7))
                {
                    return GameState.LevelPassedScreen;
                }
            }

            if (CheckColorFromBitmap(_context, bmp, 1910, 40, 0x543E2A) && CheckColorFromBitmap(_context, bmp, 13, 40, 0x543E2A))
            {
                return GameState.LevelUpgradingScreen;
            }

            if (CheckColorFromBitmap(_context, bmp, 1080, 400, 0x71E800) && CheckColorFromBitmap(_context, bmp, 1080, 500, 0x6095D7) && CheckColorFromBitmap(_context, bmp, 1080, 725, 0x69E500))
                return GameState.LevelTipScreen;

            // 收集活动可开箱界面
            if ((CheckColorFromBitmap(_context, bmp, 750, 60, 0xBF330B) || CheckColorFromBitmap(_context, bmp, 750, 60, 0xCD78FF) || CheckColorFromBitmap(_context, bmp, 750, 60, 0x912DC9))
                && (CheckColorFromBitmap(_context, bmp, 1100, 60, 0xBF330B) || CheckColorFromBitmap(_context, bmp, 1100, 60, 0xCD78FF) || CheckColorFromBitmap(_context, bmp, 1100, 60, 0x912DC9))
                && CheckColorFromBitmap(_context, bmp, 885, 681, 0x67E200))
                return GameState.CollectionActivitiesAvailableScreen;

            if (CheckColorFromBitmap(_context, bmp, 50, 50, 0x010001) && CheckColorFromBitmap(_context, bmp, 1870, 50, 0x010001) 
                && CheckColorFromBitmap(_context, bmp, 750, 200, 0x991112) && CheckColorFromBitmap(_context, bmp, 1150, 200, 0x991112))
                return GameState.InstaScreen;

            if (CheckColorFromBitmap(_context, bmp, 880, 830, 0x61E200) && CheckColorFromBitmap(_context, bmp, 700, 350, 0xF34A12) && 
                CheckColorFromBitmap(_context, bmp, 500, 390, 0x6599D9) && CheckColorFromBitmap(_context, bmp, 830, 570, 0xFFD200))
                return GameState.RaceResultsScreen;

            if (CheckColorFromBitmap(_context, bmp, 500, 140, 0x6B1000) && CheckColorFromBitmap(_context, bmp, 800, 205, 0x118FB3) &&
                CheckColorFromBitmap(_context, bmp, 1400, 140, 0x6B1000) && CheckColorFromBitmap(_context, bmp, 1150, 205, 0x118FB3))
                return GameState.BossResultsScreen;
            return GameState.UnKnown;
        }

        #endregion
    }
}
