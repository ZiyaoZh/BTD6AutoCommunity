using BTD6AutoCommunity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BTD6AutoCommunity.ScriptEngine
{
    public class InstructionCompiler
    {
        private readonly ScriptSettings settings;
        private readonly List<Instruction> instructions;

        // Func带有一个int index参数，有返回值：返回一个ExecutableInstruction：
        private Dictionary<ActionTypes, Func<int, ExecutableInstruction>> compileHandlers;



        public InstructionCompiler(ScriptSettings scriptSettings, List<Instruction> insts)
        {
            settings = scriptSettings;
            instructions = insts;
            InitCompileHandler();
        }

        private void InitCompileHandler()
        {
        //        public enum ActionTypes
        //{
        //    PlaceMonkey = 0,                // 放置猴子
        //    UpgradeMonkey = 1,              // 升级猴子
        //    SwitchMonkeyTarget = 2,         // 切换猴子目标
        //    UseAbility = 3,                 // 使用技能
        //    SwitchSpeed = 4,                // 切换倍速
        //    SellMonkey = 5,                 // 出售猴子
        //    SetMonkeyFunction = 6,          // 设置猴子功能
        //    PlaceHero = 7,                  // 放置英雄
        //    UpgradeHero = 8,                // 升级英雄
        //    PlaceHeroItem = 9,              // 英雄物品放置
        //    SwitchHeroTarget = 10,          // 切换英雄目标
        //    SetHeroFunction = 11,           // 设置英雄功能
        //    SellHero = 12,                  // 出售英雄
        //    MouseClick = 13,                // 鼠标点击
        //    AdjustMonkeyCoordinates = 14,   // 修改猴子坐标
        //    WaitMilliseconds = 15,          // 等待(ms)
        //    StartFreeplay = 16,             // 开启自由游戏
        //    EndFreeplay = 17,               // 结束自由游戏
        //    Jump = 18,                      // 指令跳转 
        //    QuickCommandBundle = 25         // 快捷指令包
        //}
        compileHandlers = new Dictionary<ActionTypes, Func<int, ExecutableInstruction>> {
                { ActionTypes.PlaceMonkey, CompilePlaceMonkey },
                { ActionTypes.UpgradeMonkey, CompileUpgradeMonkey },
                { ActionTypes.SwitchMonkeyTarget, CompileSwitchMonkeyTarget },
                { ActionTypes.UseAbility, CompileUseAbility },
                { ActionTypes.SwitchSpeed, CompileSwitchSpeed },
                { ActionTypes.SellMonkey, CompileSellMonkey },
                { ActionTypes.SetMonkeyFunction, CompileSetMonkeyFunction },
                { ActionTypes.PlaceHero, CompilePlaceHero },
                { ActionTypes.UpgradeHero, CompileUpgradeHero },
                { ActionTypes.PlaceHeroItem, CompilePlaceHeroItem },
                { ActionTypes.SwitchHeroTarget, CompileSwitchHeroTarget },
                { ActionTypes.SetHeroFunction, CompileSetHeroFunction },
                { ActionTypes.SellHero, CompileSellHero },
                { ActionTypes.MouseClick, CompileMouseClick },
                { ActionTypes.AdjustMonkeyCoordinates, CompileAdjustMonkeyCoordinates },
                { ActionTypes.WaitMilliseconds, CompileWaitMilliseconds },
                { ActionTypes.StartFreeplay, CompileStartFreeplay },
                { ActionTypes.EndFreeplay, CompileEndFreeplay },
                { ActionTypes.Jump, CompileJump },
                { ActionTypes.QuickCommandBundle, CompileQuickCommandBundle }
            };
        }

        private ExecutableInstruction CompilePlaceMonkey(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            HotKey hotKey = settings.GetHotKey((Monkeys)inst.Arguments[0]); ;

            compiled.Add(new MicroInstruction(MicroInstructionType.MoveMouse, inst.Coordinates.X, inst.Coordinates.Y));
            compiled.Add(hotKey);

            compiled.Add(new MicroInstruction(MicroInstructionType.LeftClick));
            compiled.Add(new MicroInstruction(MicroInstructionType.CheckPlaceSuccess));

            return compiled;
        }

        private ExecutableInstruction CompileUpgradeMonkey(int index)
        {

        }
        private ExecutableInstruction CompileSwitchMonkeyTarget(int index)
        {
        }
        private ExecutableInstruction CompileUseAbility(int index)
        {   
        }
        private ExecutableInstruction CompileSwitchSpeed(int index)
        {

        }
        private ExecutableInstruction CompileSellMonkey(int index)
        {

        }
        private ExecutableInstruction CompileSetMonkeyFunction(int index)
        {

        }
        private ExecutableInstruction CompilePlaceHero(int index)
        {

        }
        private ExecutableInstruction CompileUpgradeHero(int index)
        {

        }
        private ExecutableInstruction CompilePlaceHeroItem(int index)
        {

        }
        private ExecutableInstruction CompileSwitchHeroTarget(int index)
        {

        }
        private ExecutableInstruction CompileSetHeroFunction(int index)
        {

        }
        private ExecutableInstruction CompileSellHero(int index)
        {

        }
        private ExecutableInstruction CompileMouseClick(int index)
        {

        }
        private ExecutableInstruction CompileAdjustMonkeyCoordinates(int index)
        {

        }
        private ExecutableInstruction CompileWaitMilliseconds(int index)
        {

        }
        private ExecutableInstruction CompileStartFreeplay(int index)
        {

        }
        private ExecutableInstruction CompileEndFreeplay(int index)
        {

        }
        private ExecutableInstruction CompileJump(int index)
        {

        }
        private ExecutableInstruction CompileQuickCommandBundle(int index)
        {

        }

        public List<ExecutableInstruction> Compile()
        {
            var compiled = new List<ExecutableInstruction>();

            for(int i = 0; i < instructions.Count; i++)
            {
                var compiledLine = CompileInstruction(i);
                if (compiledLine != null)
                {
                    compiled.Add(compiledLine);
                }
            }

            return compiled;
        }

        private ExecutableInstruction CompileInstruction(int index)
        {
            // 用Handler来处理指令
            var inst = instructions[index];
            var compiled = new ExecutableInstruction(inst);
            if (compileHandlers.TryGetValue(inst.Type, out var handler))
            {
                compiled = handler(index);
            }
            return compiled;
        }
            //var inst = instructions[index];
            //ExecutableInstruction compiled = new ExecutableInstruction(inst);
            //bool ifFast = false;
            //HotKey hotKey1, hotKey2, hotKey3;
            //int count = 0;
            //public enum MicroInstructionType
            //{
            //    MoveMouse = 1,
            //    LeftClick = 2,
            //    LeftDown = 3,
            //    LeftUp = 4,
            //    MouseWheel = 5,
            //    KeyboardPress = 6,
            //    KeyboardRelease = 7,
            //    KeyboardPressAndRelease = 8,
            //    CheckColorAndHitKey = 9,
            //    Empty = 10,
            //    MoveMouseAndLeftClick = 11,
            //    JumpTo = 16,
            //    FindCollectMap = 18,
            //    FindHero = 19,
            //    EndFreeGame = 20,
            //    StartAutoRound = 21,
            //    FindReplayPosition = 22,
            //    IsHeroCanPlace = 23,
            //    SkillReleaseBeforeRgb = 24,
            //    SkillReleaseAfterRgb = 25,
            //    CheckPlaceSuccess = 26
            //}
        //    switch (inst.Type)
        //    {
        //        case ActionTypes.PlaceMonkey: // 放置猴子
        //            hotKey1 = settings.GetHotKey((Monkeys)inst.Arguments[0]);

        //            compiled.Add(new MicroInstruction(MicroInstructionType.MoveMouse, inst.Coordinates.X, inst.Coordinates.Y));
        //            // 用虚拟键码替换18
        //            if (hotKey1.Alt) { compiled.Add(new MicroInstruction(MicroInstructionType.KeyboardPress, 18)); }
        //            if (hotKey1.Control) { compiled.Add(new MicroInstruction(MicroInstructionType.KeyboardPress, 17)); }
        //            if (hotKey1.Shift) { compiled.Add(new MicroInstruction(MicroInstructionType.KeyboardPress, 16)); }
        //            compiled.Add(new MicroInstruction(MicroInstructionType.KeyboardPressAndRelease, (int)hotKey1.MainKey));
        //            if (hotKey1.Alt) { compiled.Add(new MicroInstruction(MicroInstructionType.KeyboardRelease, 18)); }
        //            if (hotKey1.Control) { compiled.Add(new MicroInstruction(MicroInstructionType.KeyboardRelease, 17)); }
        //            if (hotKey1.Shift) { compiled.Add(new MicroInstruction(MicroInstructionType.KeyboardRelease, 16)); }

        //            compiled.Add(new MicroInstruction(MicroInstructionType.LeftClick));
        //            compiled.Add(new MicroInstruction(MicroInstructionType.CheckPlaceSuccess));

        //            break;
        //        case ActionTypes.UpgradeMonkey: // 升级猴子

        //            if (!IfLast(index))
        //            {
        //                compiled.Add(new MicroInstruction(MicroInstructionType.MoveMouse, inst.Arguments[5], inst.Arguments[6]));
        //                compiled.Add(new MicroInstruction(MicroInstructionType.LeftClick));
        //            }

        //            if (arguments[2] == 0)
        //            {
        //                hotKey1 = settings.GetHotKey(HotkeyAction.UpgradeTopPath);
        //                miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //            }
        //            if (arguments[2] == 1)
        //            {
        //                hotKey1 = settings.GetHotKey(HotkeyAction.UpgradeMiddlePath);
        //                miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //            }
        //            if (arguments[2] == 2)
        //            {
        //                hotKey1 = settings.GetHotKey(HotkeyAction.UpgradeBottomPath);
        //                miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //            }
        //            if (!IfNext(i))
        //            {
        //                // move 
        //                miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
        //                // click
        //                if (!ifFast)
        //                {
        //                    miniDirective.Add("2");
        //                }
        //            }

        //            break;
        //        case ActionTypes.SwitchMonkeyTarget: // 切换目标
        //            hotKey1 = settings.GetHotKey(HotkeyAction.SwitchTarget);
        //            hotKey2 = settings.GetHotKey(HotkeyAction.ReverseSwitchTarget);
        //            // move
        //            miniDirective.Add(GetMoveInstruction(objectList[arguments[1]].coordinates.Item1, objectList[arguments[1]].coordinates.Item2, ifFast));
        //            // click
        //            if (!ifFast)
        //            {
        //                miniDirective.Add("2");
        //            }
        //            if (arguments[2] >= 0 && arguments[2] <= 2) // 右改1，2，3次
        //            {
        //                if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
        //                if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
        //                if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
        //                for (count = 0; count <= arguments[2]; count++)
        //                {
        //                    miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //                }
        //                if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
        //                if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
        //                if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }
        //            }
        //            else // 左改1，2，3次
        //            {
        //                if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
        //                if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
        //                if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
        //                for (count = 0; count <= arguments[2] - 3; count++)
        //                    miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
        //                if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
        //                if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
        //                if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }
        //            }

        //            // move
        //            miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
        //            // click
        //            if (!ifFast)
        //            {
        //                miniDirective.Add("2");
        //            }
        //            break;
        //        case ActionTypes.UseAbility: // 使用技能
        //            if (arguments[1] < 9)
        //            {
        //                miniDirective.Add("24 " + (arguments[1]).ToString() + " " + (arguments[1] + 49).ToString());
        //                miniDirective.Add("25 " + (arguments[1]).ToString());
        //            }
        //            else if (arguments[1] == 9)
        //            {
        //                miniDirective.Add("24 9 48");
        //                miniDirective.Add("25 9");
        //            }
        //            else if (arguments[1] == 10)
        //            {
        //                miniDirective.Add("24 10 189");
        //                miniDirective.Add("25 10");
        //            }
        //            else if (arguments[1] == 11)
        //            {
        //                miniDirective.Add("24 11 187");
        //                miniDirective.Add("25 11");
        //            }
        //            // 选择释放坐标
        //            if (arguments.Count > 2)
        //            {
        //                // move
        //                miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], ifFast));
        //                // click
        //                if (!ifFast)
        //                {
        //                    miniDirective.Add("2");
        //                }
        //            }
        //            break;
        //        case ActionTypes.SwitchSpeed: // 切换倍速
        //            hotKey1 = settings.GetHotKey(HotkeyAction.ChangeSpeed);
        //            hotKey2 = settings.GetHotKey(HotkeyAction.NextRound);
        //            if (arguments[1] == 0) // 快/慢切换
        //            {
        //                if (!IfLast(i))
        //                {
        //                    if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
        //                    if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
        //                    if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
        //                }
        //                miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //                if (!IfNext(i))
        //                {
        //                    if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
        //                    if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
        //                    if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }
        //                }
        //            }
        //            if (arguments[1] == 1) // 竞速下一回合
        //            {
        //                if (!IfLast(i))
        //                {
        //                    if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
        //                    if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
        //                    if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
        //                }
        //                miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
        //                if (!IfNext(i))
        //                {
        //                    if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
        //                    if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
        //                    if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }
        //                }
        //            }

        //            break;
        //        case ActionTypes.SellMonkey: // 出售猴子
        //            hotKey1 = settings.GetHotKey(HotkeyAction.Sell);
        //            // move
        //            miniDirective.Add(GetMoveInstruction(objectList[arguments[1]].coordinates.Item1, objectList[arguments[1]].coordinates.Item2, ifFast));
        //            // click
        //            if (!ifFast)
        //            {
        //                miniDirective.Add("2");
        //            }

        //            if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
        //            if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
        //            miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //            if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
        //            if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

        //            break;
        //        case ActionTypes.SetMonkeyFunction: // 设置猴子功能
        //            hotKey1 = settings.GetHotKey(HotkeyAction.SetFunction1);
        //            // move
        //            miniDirective.Add(GetMoveInstruction(objectList[arguments[1]].coordinates.Item1, objectList[arguments[1]].coordinates.Item2, ifFast));
        //            // click
        //            if (!ifFast)
        //            {
        //                miniDirective.Add("2");
        //            }

        //            if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
        //            if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
        //            miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //            if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); };
        //            if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

        //            if (arguments.Count > 2)
        //            {
        //                // move
        //                miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], ifFast));
        //                // click
        //                if (!ifFast)
        //                {
        //                    miniDirective.Add("2");
        //                }
        //            }
        //            // move
        //            miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
        //            // click
        //            if (!ifFast)
        //            {
        //                miniDirective.Add("2");
        //            }

        //            break;
        //        case ActionTypes.PlaceHero: // 放置英雄
        //            miniDirective.Add("23");

        //            miniDirective.Add(GetMoveInstruction(1715, 230, false));

        //            miniDirective.Add("2");

        //            // move
        //            miniDirective.Add(GetMoveInstruction(arguments[1], arguments[2], false));

        //            // click
        //            miniDirective.Add("2");

        //            miniDirective.Add("26");


        //            break;
        //        case ActionTypes.UpgradeHero: // 升级英雄
        //            hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
        //            hotKey2 = settings.GetHotKey(HotkeyAction.UpgradeTopPath);
        //            if (!IfLast(i))
        //            {
        //                if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
        //                if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
        //                if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
        //                miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //                if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
        //                if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
        //                if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }
        //            }

        //            if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
        //            if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
        //            if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
        //            miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
        //            if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
        //            if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
        //            if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }

        //            if (!IfNext(i))
        //            {
        //                // move
        //                miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
        //                // click
        //                if (!ifFast)
        //                {
        //                    miniDirective.Add("2");
        //                }
        //            }

        //            break;
        //        case ActionTypes.PlaceHeroItem: // 放置英雄物品
        //            // 点击英雄
        //            hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
        //            if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
        //            if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
        //            miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //            if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
        //            if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

        //            hotKey2 = settings.GetHotKey((HeroObjectTypes)arguments[1]);
        //            if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
        //            if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
        //            if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
        //            miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
        //            if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
        //            if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
        //            if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }

        //            if (arguments.Count > 2)
        //            {
        //                // move
        //                miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], ifFast));
        //                // click
        //                if (!ifFast)
        //                {
        //                    miniDirective.Add("2");
        //                }
        //            }
        //            // move
        //            miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
        //            // click
        //            if (!ifFast)
        //            {
        //                miniDirective.Add("2");
        //            }
        //            break;
        //        case ActionTypes.SwitchHeroTarget: // 切换英雄目标
        //            // 点击英雄
        //            hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
        //            hotKey2 = settings.GetHotKey(HotkeyAction.SwitchTarget);
        //            hotKey3 = settings.GetHotKey(HotkeyAction.ReverseSwitchTarget);
        //            if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
        //            if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
        //            miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //            if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
        //            if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

        //            if (arguments[1] >= 0 && arguments[1] <= 2) // 右改1，2，3次
        //            {
        //                if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
        //                if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
        //                if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
        //                for (count = 0; count <= arguments[1]; count++)
        //                {
        //                    miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
        //                }
        //                if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
        //                if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
        //                if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }
        //            }
        //            else // 左改1，2，3次
        //            {
        //                if (hotKey3.Alt) { miniDirective.Add("6 " + "18"); }
        //                if (hotKey3.Control) { miniDirective.Add("6 " + "17"); }
        //                if (hotKey3.Shift) { miniDirective.Add("6 " + "16"); }
        //                for (count = 0; count <= arguments[1] - 3; count++)
        //                    miniDirective.Add("8 " + ((int)hotKey3.MainKey).ToString());
        //                if (hotKey3.Alt) { miniDirective.Add("7 " + "18"); }
        //                if (hotKey3.Control) { miniDirective.Add("7 " + "17"); }
        //                if (hotKey3.Shift) { miniDirective.Add("7 " + "16"); }
        //            }

        //            // move
        //            miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
        //            // click
        //            if (!ifFast)
        //            {
        //                miniDirective.Add("2");
        //            }
        //            break;
        //        case ActionTypes.SetHeroFunction: // 设置英雄功能
        //            // 点击英雄
        //            hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
        //            hotKey2 = settings.GetHotKey(HotkeyAction.SetFunction1);
        //            hotKey3 = settings.GetHotKey(HotkeyAction.SetFunction2);
        //            if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
        //            if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
        //            miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //            if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
        //            if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

        //            if (arguments[1] == 0)
        //            {
        //                if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
        //                if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
        //                if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
        //                miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
        //                if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
        //                if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
        //                if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }
        //            }
        //            else
        //            {
        //                if (hotKey3.Alt) { miniDirective.Add("6 " + "18"); }
        //                if (hotKey3.Control) { miniDirective.Add("6 " + "17"); }
        //                if (hotKey3.Shift) { miniDirective.Add("6 " + "16"); }
        //                miniDirective.Add("8 " + ((int)hotKey3.MainKey).ToString());
        //                if (hotKey3.Alt) { miniDirective.Add("7 " + "18"); }
        //                if (hotKey3.Control) { miniDirective.Add("7 " + "17"); }
        //                if (hotKey3.Shift) { miniDirective.Add("7 " + "16"); }
        //            }

        //            if (arguments.Count > 2)
        //            {
        //                // move
        //                miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], ifFast));
        //                // click
        //                if (!ifFast)
        //                {
        //                    miniDirective.Add("2");
        //                }
        //            }
        //            // move
        //            miniDirective.Add(GetMoveInstruction(AnchorCoords.Item1, AnchorCoords.Item2, ifFast));
        //            // click
        //            if (!ifFast)
        //            {
        //                miniDirective.Add("2");
        //            }
        //            break;
        //        case ActionTypes.SellHero: // 出售英雄
        //            hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
        //            hotKey2 = settings.GetHotKey(HotkeyAction.Sell);
        //            if (hotKey1.Alt) { miniDirective.Add("6 " + "18"); }
        //            if (hotKey1.Control) { miniDirective.Add("6 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("6 " + "16"); }
        //            miniDirective.Add("8 " + ((int)hotKey1.MainKey).ToString());
        //            if (hotKey1.Alt) { miniDirective.Add("7 " + "18"); }
        //            if (hotKey1.Control) { miniDirective.Add("7 " + "17"); }
        //            if (hotKey1.Shift) { miniDirective.Add("7 " + "16"); }

        //            if (hotKey2.Alt) { miniDirective.Add("6 " + "18"); }
        //            if (hotKey2.Control) { miniDirective.Add("6 " + "17"); }
        //            if (hotKey2.Shift) { miniDirective.Add("6 " + "16"); }
        //            miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
        //            miniDirective.Add("8 " + ((int)hotKey2.MainKey).ToString());
        //            if (hotKey2.Alt) { miniDirective.Add("7 " + "18"); }
        //            if (hotKey2.Control) { miniDirective.Add("7 " + "17"); }
        //            if (hotKey2.Shift) { miniDirective.Add("7 " + "16"); }
        //            break;
        //        case ActionTypes.MouseClick: // 鼠标点击指令
        //            // move
        //            miniDirective.Add(GetMoveInstruction(arguments[2], arguments[3], false));
        //            for (int j = 0; j < arguments[1]; j++)
        //            {
        //                miniDirective.Add("2");
        //            }
        //            // click
        //            break;
        //        case ActionTypes.AdjustMonkeyCoordinates:
        //            miniDirective.Add("2");
        //            objectList[arguments[1]].SetCoordinates((arguments[2], arguments[3]));
        //            break;
        //        case ActionTypes.WaitMilliseconds:
        //            int times = (arguments[1] / settings.OperationInterval > 1 ? arguments[1] / settings.OperationInterval : 1);
        //            for (int k = 0; k < times; k++)
        //            {
        //                miniDirective.Add("10");
        //            }
        //            break;
        //        case ActionTypes.Jump:
        //            miniDirective.Add("16 " + arguments[1].ToString());
        //            break;
        //        case ActionTypes.StartFreeplay:
        //            miniDirective.Add("21");
        //            times = (3000 / settings.OperationInterval > 1 ? 3000 / settings.OperationInterval : 1);
        //            for (int m = 0; m < times; m++)
        //            {
        //                miniDirective.Add("10");
        //            }
        //            break;
        //        case ActionTypes.EndFreeplay:
        //            times = (4500 / settings.OperationInterval > 1 ? 4500 / settings.OperationInterval : 1);
        //            for (int m = 0; m < times; m++)
        //            {
        //                miniDirective.Add("10");
        //            }

        //            miniDirective.Add("1 " + "1600 " + "45");
        //            miniDirective.Add("2");

        //            for (int m = 0; m < times; m++)
        //            {
        //                miniDirective.Add("10");
        //            }
        //            //miniDirective.Add("20");
        //            break;
        //    }

        //    return compiled;
        //}

        // 编译优化辅助：当前指令是否与下一条重复（用于合并/省略优化）
        

        
        private bool IfNext(int index)
        {
            if (index < 0 || index >= instructions.Count - 1) return false;
            return CompareCore(instructions[index], instructions[index + 1]);
        }

        private bool IfLast(int index)
        {
            if (index <= 0 || index >= instructions.Count) return false;
            return CompareCore(instructions[index], instructions[index - 1]);
        }

        private bool CompareCore(Instruction a, Instruction b)
        {
            if (a.IsMonkeyInstruction() && b.IsMonkeyInstruction() && a.Arguments[0] == b.Arguments[0]) return true;
            if (a.IsHeroInstruction() && b.IsHeroInstruction()) return true;
            if (a.Type == ActionTypes.SwitchSpeed && b.Type == ActionTypes.SwitchSpeed && a.Arguments[0] == b.Arguments[0]) return true;
            return false;
        }
    }
}

