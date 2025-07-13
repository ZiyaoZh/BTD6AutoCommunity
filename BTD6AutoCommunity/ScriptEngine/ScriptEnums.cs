using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BTD6AutoCommunity.Core;

namespace BTD6AutoCommunity.ScriptEngine
{
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
        InstructionsBundle = 25         // 快捷指令包
    }

    public enum TargetTypes
    { 
        /// <summary>
        /// 目标右改一次
        /// </summary>
        Right = 0,

        /// <summary>
        /// 目标右改两次
        /// </summary>
        RightDouble = 1,

        /// <summary>
        /// 目标右改三次
        /// </summary>
        RightTriple = 2,
        
        /// <summary>
        /// 目标左改一次
        /// </summary>
        Left = 3,

        /// <summary>
        /// 目标左改两次
        /// </summary>
        LeftDouble = 4,

        /// <summary>
        /// 目标左改三次
        /// </summary>
        LeftTriple = 5,

    }

    public enum UpgradeTypes
    {
        /// <summary>
        /// 上路升级
        /// </summary>
        Top = 0,

        /// <summary>
        /// 中路升级
        /// </summary>
        Middle = 1,

        /// <summary>
        /// 下路升级
        /// </summary>
        Bottom = 2,

        /// <summary>
        /// 上路升级(仅一次)
        /// </summary>
        TopOnce = 3,

        /// <summary>
        /// 中路升级(仅一次)
        /// </summary>
        MiddleOnce = 4,

        /// <summary>
        /// 下路升级(仅一次)
        /// </summary>
        BottomOnce = 5,
        
    }

    public enum CoordinateTypes
    {
        /// <summary>
        /// 无坐标选择
        /// </summary>
        None = 0,

        /// <summary>
        /// 有坐标选择
        /// </summary>
        Coordinate = 1,
    }

    public enum MonkeyFunctionTypes
    {

        /// <summary>
        /// 功能1
        /// </summary>
        Function1 = 0,

        /// <summary>
        /// 功能1有坐标选择
        /// </summary>
        Function1Coordinate = 1,

        /// <summary>
        /// 功能2
        /// </summary>
        Function2 = 2,

        /// <summary>
        /// 功能2有坐标选择
        /// </summary>
        Function2Coordinate = 3,
    }

    // 倍速选择
    public enum SpeedTypes
    {
        /// <summary>
        /// 切换倍速
        /// </summary>
        Switch = 0,

        /// <summary>
        /// 竞速下一回合
        /// </summary>
        NextRound = 1,
    }

    // 是否有放置检测
    public enum PlaceCheckTypes
    {
        /// <summary>
        /// 无放置检测
        /// </summary>
        None = 1,

        /// <summary>
        /// 有放置检测
        /// </summary>
        Check = 0,
    }
}
