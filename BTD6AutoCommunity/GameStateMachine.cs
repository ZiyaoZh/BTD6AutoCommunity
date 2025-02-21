//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace BTD6AutoCommunity
//{
//    public enum GameState
//    {
//        GameMainScreen,
//        DailyRewardsScreen,
//        DailyActivitiesScreen,
//        TasksScreen,
//        RaceResultsScreen,
//        BossResultsScreen,
//        HeroSelectionScreen,
//        LevelSelectionScreen,
//        LevelSearchScreen,
//        LevelDifficultySelectionScreen,
//        LevelEasyModeSelectionScreen,
//        LevelMediumModeSelectionScreen,
//        LevelHardModeSelectionScreen,
//        LevelLoadingScreen,
//        LevelTipScreen,
//        LevelChallengingScreen,
//        LevelChallengingWithTipScreen,
//        LevelFailedScreen,
//        LevelPassedScreen,
//        LevelSettlementScreen,
//        RacePassedScreen,
//        RaceSettlementScreen,
//        CollectionActivitiesScreen,
//        ChestAnimationScreen,
//        ThreeChestsScreen,
//        TwoChestsScreen,
//        ChestsOpenedScreen,
//        UnKnown,
//    }



//    public class GameStateMachine
//    {
//        private GameState currentState;

//        public GameStateMachine()
//        {
//            currentState = GameState.UnKnown;
//            // 其他初始化操作...
//        }


//        public GameState GetCurrentState()
//        {

//            // 获取游戏主界面状态
//            if (IsColorAt(gameWindowHandle, 100, 100, Color.Red)) // 假设游戏主界面的某个像素点为红色
//            {
//                return GameState.GameMainScreen;
//            }

//            // 获取每日奖励界面状态
//            if (IsColorAt(gameWindowHandle, 200, 200, Color.Blue)) // 假设每日奖励界面的某个像素点为蓝色
//            {
//                return GameState.DailyRewardsScreen;
//            }

//            // 获取每日活动界面状态
//            if (IsColorAt(gameWindowHandle, 300, 300, Color.Green)) // 假设每日活动界面的某个像素点为绿色
//            {
//                return GameState.DailyActivitiesScreen;
//            }

//            // 获取任务界面状态
//            if (IsColorAt(gameWindowHandle, 400, 400, Color.Yellow)) // 假设任务界面的某个像素点为黄色
//            {
//                return GameState.TasksScreen;
//            }

//            // 获取竞速成绩界面状态
//            if (IsColorAt(gameWindowHandle, 500, 500, Color.Purple)) // 假设竞速成绩界面的某个像素点为紫色
//            {
//                return GameState.RaceResultsScreen;
//            }

//            // 获取Boss成绩界面状态
//            if (IsColorAt(gameWindowHandle, 600, 600, Color.Orange)) // 假设Boss成绩界面的某个像素点为橙色
//            {
//                return GameState.BossResultsScreen;
//            }

//            // 获取英雄选择界面状态
//            if (IsColorAt(gameWindowHandle, 700, 700, Color.Brown)) // 假设英雄选择界面的某个像素点为棕色
//            {
//                return GameState.HeroSelectionScreen;
//            }

//            // 获取关卡选择界面状态
//            if (IsColorAt(gameWindowHandle, 800, 800, Color.Gray)) // 假设关卡选择界面的某个像素点为灰色
//            {
//                return GameState.LevelSelectionScreen;
//            }

//            // 获取关卡搜索界面状态
//            if (IsColorAt(gameWindowHandle, 900, 900, Color.Black)) // 假设关卡搜索界面的某个像素点为黑色
//            {
//                return GameState.LevelSearchScreen;
//            }

//            // 获取关卡难度选择界面状态
//            if (IsColorAt(gameWindowHandle, 1000, 1000, Color.White)) // 假设关卡难度选择界面的某个像素点为白色
//            {
//                return GameState.LevelDifficultySelectionScreen;
//            }

//            // 获取关卡简单模式选择界面状态
//            if (IsColorAt(gameWindowHandle, 1100, 1100, Color.Pink)) // 假设关卡简单模式选择界面的某个像素点为粉色
//            {
//                return GameState.LevelEasyModeSelectionScreen;
//            }

//            // 获取关卡中级模式选择界面状态
//            if (IsColorAt(gameWindowHandle, 1200, 1200, Color.LightBlue)) // 假设关卡中级模式选择界面的某个像素点为浅蓝色
//            {
//                return GameState.LevelMediumModeSelectionScreen;
//            }

//            // 获取关卡困难模式选择界面状态
//            if (IsColorAt(gameWindowHandle, 1300, 1300, Color.LightGreen)) // 假设关卡困难模式选择界面的某个像素点为浅绿色
//            {
//                return GameState.LevelHardModeSelectionScreen;
//            }

//            // 获取关卡加载中界面状态
//            if (IsColorAt(gameWindowHandle, 1400, 1400, Color.LightGray)) // 假设关卡加载中界面的某个像素点为浅灰色
//            {
//                return GameState.LevelLoadingScreen;
//            }

//            // 获取关卡提示界面状态
//            if (IsColorAt(gameWindowHandle, 1500, 1500, Color.LightYellow)) // 假设关卡提示界面的某个像素点为浅黄色
//            {
//                return GameState.LevelTipScreen;
//            }

//            // 获取关卡挑战中界面状态
//            if (IsColorAt(gameWindowHandle, 1600, 1600, Color.LightPink)) // 假设关卡挑战中界面的某个像素点为浅粉色
//            {
//                return GameState.LevelChallengingScreen;
//            }

//            // 获取关卡挑战中遇到提示界面状态
//            if (IsColorAt(gameWindowHandle, 1700, 1700, Color.DarkRed)) // 假设关卡挑战中遇到提示界面的某个像素点为暗红色
//            {
//                return GameState.LevelChallengingWithTipScreen;
//            }

//            // 获取关卡挑战失败界面状态
//            if (IsColorAt(gameWindowHandle, 1800, 1800, Color.DarkBlue)) // 假设关卡挑战失败界面的某个像素点为暗蓝色
//            {
//                return GameState.LevelFailedScreen;
//            }

//            // 获取关卡挑战成功界面状态
//            if (IsColorAt(gameWindowHandle, 1900, 1900, Color.DarkGreen)) // 假设关卡挑战成功界面的某个像素点为暗绿色
//            {
//                return GameState.LevelPassedScreen;
//            }

//            // 获取关卡结算界面状态
//            if (IsColorAt(gameWindowHandle, 2000, 2000, Color.DarkYellow)) // 假设关卡结算界面的某个像素点为暗黄色
//            {
//                return GameState.LevelSettlementScreen;
//            }

//            // 获取竞速挑战成功界面状态
//            if (IsColorAt(gameWindowHandle, 2100, 2100, Color.DarkPurple)) // 假设竞速挑战成功界面的某个像素点为暗紫色
//            {
//                return GameState.RacePassedScreen;
//            }

//            // 获取竞速结算界面状态
//            if (IsColorAt(gameWindowHandle, 2200, 2200, Color.DarkOrange)) // 假设竞速结算界面的某个像素点为暗橙色
//            {
//                return GameState.RaceSettlementScreen;
//            }

//            // 获取收集活动界面状态
//            if (IsColorAt(gameWindowHandle, 2300, 2300, Color.DarkBrown)) // 假设收集活动界面的某个像素点为暗棕色
//            {
//                return GameState.CollectionActivitiesScreen;
//            }

//            // 获取开箱动画界面状态
//            if (IsColorAt(gameWindowHandle, 2400, 2400, Color.DarkGray)) // 假设开箱动画界面的某个像素点为暗灰色
//            {
//                return GameState.ChestAnimationScreen;
//            }

//            // 获取三个箱子界面状态
//            if (IsColorAt(gameWindowHandle, 2500, 2500, Color.DarkBlack)) // 假设三个箱子界面的某个像素点为暗黑色
//            {
//                return GameState.ThreeChestsScreen;
//            }

//            // 获取两个箱子界面状态
//            if (IsColorAt(gameWindowHandle, 2600, 2600, Color.DarkWhite)) // 假设两个箱子界面的某个像素点为暗白色
//            {
//                return GameState.TwoChestsScreen;
//            }

//            // 获取开完箱子界面状态
//            if (IsColorAt(gameWindowHandle, 2700, 2700, Color.DarkPink)) // 假设开完箱子界面的某个像素点为暗粉色
//            {
//                return GameState.ChestsOpenedScreen;
//            }

//            // 如果没有匹配的状态，默认返回 Uninitialized
//            return GameState.Uninitialized;
//        }


//    }
//}
