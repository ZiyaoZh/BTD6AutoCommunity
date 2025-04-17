using System;
using static BTD6AutoCommunity.WindowApiWrapper;
using static BTD6AutoCommunity.GameVisionRecognizer;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Buffers.Text;

namespace BTD6AutoCommunity
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

    public static class GameStateLocalization
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
        public GameContext Context => _context;

        public GameStateMachine(GameContext context)
        {
            _context = context;
            currentState = GameState.UnKnown;
            // 其他初始化操作...
        }

        public GameState GetCurrentState()
        {
            if (!_context.IsValid) return GameState.UnKnown;

            if ((CheckColor(_context, 780, 380, 0xF34A12) && CheckColor(_context, 780, 760, 0x5388D2) && CheckColor(_context, 900, 760, 0x62E200)) ||
                (CheckColor(_context, 820, 330, 0xF24710) && CheckColor(_context, 820, 760, 0xD2D2D2) && CheckColor(_context, 960, 760, 0xFFFFFF)))
            {
                return GameState.LevelChallengingWithTipScreen;
            }
            // 关卡挑战中界面
            if (CheckColor(_context, 1910, 40, 0xB1814A) && CheckColor(_context, 13, 40, 0xB1814A))
            {

                return GameState.LevelChallengingScreen;
            }
            
            // 关卡挑战中遇到提示界面
            if (CheckColor(_context, 1658, 40, 0x6A573C) && CheckColor(_context, 13, 40, 0x62482E))
            {
                // 关卡设置界面
                if (CheckColor(_context, 580, 240, 0xBE945B) && CheckColor(_context, 550, 380, 0x9F7843))
                {
                    return GameState.LevelSettingScreen;
                }
            }

            if (CheckColor(_context, 722, 383, 0x6397D8) && CheckColor(_context, 1213, 383, 0x6397D8) && CheckColor(_context, 988, 118, 0xFFFFFF))
                return GameState.LevelFailedScreen;

            if (CheckColor(_context, 1910, 40, 0x543E2A) && CheckColor(_context, 13, 40, 0x543E2A))
            {
                return GameState.LevelUpgradingScreen;
            }

            // 收集活动相关界面需要优先判断
            if (CheckColor(_context, 960, 180, 0x121417) && CheckColor(_context, 750, 60, 0x121417) && CheckColor(_context, 1130, 200, 0x121417) && CheckColor(_context, 1725, 500, 0x121417))
            {
                if (CheckColor(_context, 884, 1000, 0x67E400) && CheckColor(_context, 1037, 1000, 0x67E400))
                    return GameState.ChestsOpenedScreen;

                if (!CheckColor(_context, 828, 537, 0x121417) && !CheckColor(_context, 1130, 537, 0x121417))
                    return GameState.TwoChestsScreen;

                if (!CheckColor(_context, 677, 537, 0x121417) && CheckColor(_context, 1277, 537, 0x121417))
                    return GameState.ThreeChestsScreen;
            }

            // 主界面判断
            if (CheckColor(_context, 966, 945, 0xFFFFFF) && CheckColor(_context, 1382, 942, 0xFFFFFF) && CheckColor(_context, 80, 186, 0xC4E8EB))
                return GameState.GameMainScreen;

            // 地图搜索相关界面
            if (CheckColor(_context, 983, 48, 0x385373) && CheckColor(_context, 778, 43, 0x578CD4))
            {
                if (CheckColor(_context, 962, 837, 0x409FFF))
                    return GameState.LevelSearchedScreen;
                return GameState.LevelSearchScreen;
            }

            if (CheckColor(_context, 68, 54, 0xFFFFFF))
            {
                // 地图选择界面
                if (CheckColor(_context, 273, 432, 0xFFD500) && CheckColor(_context, 1649, 432, 0xFFD500) && CheckColor(_context, 1313, 960, 0xFF3400))
                    return GameState.LevelSelectionScreen;

                // 难度选择界面
                if (CheckColor(_context, 716, 627, 0x9F7842) && CheckColor(_context, 1049, 627, 0x9F7842)
                    && CheckColor(_context, 1388, 627, 0x9F7842))
                    return GameState.LevelDifficultySelectionScreen;

                if (CheckColor(_context, 820, 40, 0x804A24) && CheckColor(_context, 1200, 40, 0x804A24))
                {
                    // 简单模式选择界面
                    if (CheckColor(_context, 720, 50, 0xF2C776))
                        return GameState.LevelEasyModeSelectionScreen;

                    // 中级模式选择界面
                    if (CheckColor(_context, 720, 50, 0xC6DFEB))
                        return GameState.LevelMediumModeSelectionScreen;

                    // 困难模式选择界面
                    if (CheckColor(_context, 720, 50, 0xFFED00))
                        return GameState.LevelHardModeSelectionScreen;
                }

                // 英雄选择界面
                if (CheckColor(_context, 1601, 1005, 0xFFFFFF) && CheckColor(_context, 636, 51, 0xFFFFFF))
                    return GameState.HeroSelectionScreen;

                if (CheckColor(_context, 750, 60, 0x996633) && CheckColor(_context, 1190, 60, 0x996633))
                {
                    if (CheckColor(_context, 600, 320, 0xE2E2E2))
                        return GameState.EventsScreen;
                    else
                        return GameState.EventInfoScreen;
                }

                // 收集活动主界面
                if ((CheckColor(_context, 750, 60, 0xBF330B) || CheckColor(_context, 750, 60, 0xCD78FF))
                    && (CheckColor(_context, 1100, 60, 0xBF330B) || CheckColor(_context, 1100, 60, 0xCD78FF)))
                    return GameState.CollectionActivitiesScreen;

                // 可返回界面
                return GameState.ReturnableScreen;
            }

            // 关卡完成界面
            if (CheckColor(_context, 555, 197, 0xFFFFFF))
            {
                if (CheckColor(_context, 1365, 324, 0xFFFFFF) && CheckColor(_context, 791, 193, 0xF34A12))
                {
                    return GameState.LevelSettlementScreen;
                }
                if (CheckColor(_context, 1221, 152, 0xFFFFFF) && CheckColor(_context, 585, 579, 0x5F93D7))
                {
                    return GameState.LevelPassedScreen;
                }
            }




            if (CheckColor(_context, 1080, 400, 0x71E800) && CheckColor(_context, 1080, 500, 0x6095D7) && CheckColor(_context, 1080, 725, 0x69E500))
                return GameState.LevelTipScreen;

            // 收集活动可开箱界面
            if ((CheckColor(_context, 750, 60, 0xBF330B) || CheckColor(_context, 750, 60, 0xCD78FF)) 
                && (CheckColor(_context, 1100, 60, 0xBF330B) || CheckColor(_context, 1100, 60, 0xCD78FF))
                && CheckColor(_context, 885, 681, 0x67E200))
                return GameState.CollectionActivitiesAvailableScreen;

            if (CheckColor(_context, 760, 660, 0xF34E13) && CheckColor(_context, 1130, 660, 0xF45417))
                return GameState.InstaScreen;

            if (CheckColor(_context, 880, 830, 0x61E200) && CheckColor(_context, 700, 350, 0xF34A12) && 
                CheckColor(_context, 500, 390, 0x6599D9) && CheckColor(_context, 830, 570, 0xFFD200))
                return GameState.RaceResultsScreen;

            if (CheckColor(_context, 500, 140, 0x6B1000) && CheckColor(_context, 800, 205, 0x118FB3) &&
                CheckColor(_context, 1400, 140, 0x6B1000) && CheckColor(_context, 1150, 205, 0x118FB3))
                return GameState.BossResultsScreen;
            return GameState.UnKnown;
        }

    }
}
