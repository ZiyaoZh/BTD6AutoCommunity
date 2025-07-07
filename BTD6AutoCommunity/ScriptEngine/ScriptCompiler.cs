using BTD6AutoCommunity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BTD6AutoCommunity.ScriptEngine
{
    public class ScriptCompiler
    {
        private readonly ScriptSettings settings;
        private InstructionSequence instructions;
        private ScriptMetadata metadata;

        private Dictionary<ActionTypes, Func<int, ExecutableInstruction>> compileHandlers;

        public ScriptCompiler(ScriptSettings scriptSettings)
        {
            settings = scriptSettings;
            InitCompileHandler();
        }

        public List<ExecutableInstruction> Compile(InstructionSequence insts, ScriptMetadata data)
        {
            instructions = insts;
            metadata = data;

            var compiled = new List<ExecutableInstruction>();

            for (int i = 0; i < instructions.Count; i++)
            {
                var compiledLine = CompileInstruction(i);
                if (compiledLine != null)
                {
                    compiled.Add(compiledLine);
                }
            }

            return compiled;
        }

        private void InitCompileHandler()
        {
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
                { ActionTypes.Jump, CompileJump }
            };
        }

        private ExecutableInstruction CompilePlaceMonkey(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);

            compiled.Add(new MicroInstruction(MicroInstructionType.MouseMove, inst.Coordinates.X, inst.Coordinates.Y));
            compiled.Add(settings.GetHotKey((Monkeys)inst.Arguments[0]));

            compiled.Add(new MicroInstruction(MicroInstructionType.LeftClick));
            compiled.Add(new MicroInstruction(MicroInstructionType.CheckPlaceSuccess));

            return compiled;
        }

        private ExecutableInstruction CompileUpgradeMonkey(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            if (!IfLast(index))
            {
                compiled.Add(inst.Arguments[5], inst.Arguments[6]);
            }

            if (inst.Arguments[1] == 0) // 上路
            {
                compiled.Add(settings.GetHotKey(HotkeyAction.UpgradeTopPath));
            }
            if (inst.Arguments[1] == 1) // 中路
            {
                compiled.Add(settings.GetHotKey(HotkeyAction.UpgradeMiddlePath));
            }
            if (inst.Arguments[1] == 2) // 下路
            {
                compiled.Add(settings.GetHotKey(HotkeyAction.UpgradeBottomPath));
            }
            if (!IfNext(index))
            {
                compiled.Add(metadata.AnchorCoords.X, metadata.AnchorCoords.Y);
            }
            return compiled;
        }

        private ExecutableInstruction CompileSwitchMonkeyTarget(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            HotKey hotKey1 = settings.GetHotKey(HotkeyAction.SwitchTarget);
            HotKey hotKey2 = settings.GetHotKey(HotkeyAction.ReverseSwitchTarget);
            if (!IfLast(index))
            {
                compiled.Add(inst.Arguments[5], inst.Arguments[6]);
            }
            if (inst.Arguments[1] >= 0 && inst.Arguments[1] <= 2) // 右改1，2，3次
            {
                compiled.Add(hotKey1, inst.Arguments[1] + 1);
            }
            else // 左改1，2，3次
            {
                compiled.Add(hotKey2, inst.Arguments[1] - 2);
            }
            if (!IfNext(index))
            {
                compiled.Add(metadata.AnchorCoords.X, metadata.AnchorCoords.Y);
            }
            return compiled;
        }

        private ExecutableInstruction CompileSetMonkeyFunction(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            HotKey hotKey1 = settings.GetHotKey(HotkeyAction.SetFunction1);
            HotKey hotKey2 = settings.GetHotKey(HotkeyAction.SetFunction2);
            if (!IfLast(index))
            {
                compiled.Add(inst.Arguments[5], inst.Arguments[6]);
            }
            if (inst.Arguments[1] == 0 || inst.Arguments[1] == 1)
            {
                compiled.Add(hotKey1, 1);
                if (inst.Coordinates != (-1, -1))
                {
                    compiled.Add(inst.Coordinates.X, inst.Coordinates.Y);
                }
            }
            if (inst.Arguments[1] == 2 || inst.Arguments[1] == 3)
            {
                compiled.Add(hotKey2, 1);
                if (inst.Coordinates != (-1, -1))
                {
                    compiled.Add(inst.Coordinates.X, inst.Coordinates.Y);
                }
            }
            if (!IfNext(index))
            {
                compiled.Add(metadata.AnchorCoords.X, metadata.AnchorCoords.Y);
            }
            return compiled;
        }

        private ExecutableInstruction CompileAdjustMonkeyCoordinates(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            compiled.Add(new MicroInstruction(MicroInstructionType.Empty));
            return compiled;
        }

        private ExecutableInstruction CompileSellMonkey(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);

            if (!IfLast(index))
            {
                compiled.Add(inst.Arguments[5], inst.Arguments[6]);
            }
            compiled.Add(settings.GetHotKey(HotkeyAction.Sell), 1);
            return compiled;
        }

        private ExecutableInstruction CompilePlaceHero(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            compiled.Add(new MicroInstruction(MicroInstructionType.IsHeroCanPlace));
            compiled.Add(1715, 230); 
            compiled.Add(inst.Coordinates.X, inst.Coordinates.Y);
            compiled.Add(new MicroInstruction(MicroInstructionType.CheckPlaceSuccess));
            return compiled;
        }

        private ExecutableInstruction CompileUpgradeHero(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            HotKey hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
            HotKey hotKey2 = settings.GetHotKey(HotkeyAction.UpgradeTopPath);
            if (!IfLast(index))
            {
                compiled.Add(hotKey1, 1);
            }

            compiled.Add(hotKey2, 1);

            if (!IfNext(index))
            {
                compiled.Add(metadata.AnchorCoords.X, metadata.AnchorCoords.Y);
            }
            return compiled;
        }

        private ExecutableInstruction CompilePlaceHeroItem(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);

            HotKey hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
            HotKey hotKey2 = settings.GetHotKey((HeroObjectTypes)inst.Arguments[0]);

            if (!IfLast(index))
            {
                compiled.Add(hotKey1, 1);
            }
            compiled.Add(hotKey2, 1);

            if (inst.Coordinates != (-1, -1))
            {
                compiled.Add(inst.Coordinates.X, inst.Coordinates.Y);
            }

            compiled.Add(metadata.AnchorCoords.X, metadata.AnchorCoords.Y);
            return compiled;
        }

        private ExecutableInstruction CompileSwitchHeroTarget(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            HotKey hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
            HotKey hotKey2 = settings.GetHotKey(HotkeyAction.SwitchTarget);
            HotKey hotKey3 = settings.GetHotKey(HotkeyAction.ReverseSwitchTarget);

            if (!IfLast(index))
            {
                compiled.Add(hotKey1, 1);
            }
            if (inst.Arguments[0] >= 0 && inst.Arguments[0] <= 2) // 右改1，2，3次
            {
                compiled.Add(hotKey2, inst.Arguments[0] + 1);
            }
            else // 左改1，2，3次
            {
                compiled.Add(hotKey3, inst.Arguments[0] - 2);
            }
            if (!IfNext(index))
            {
                compiled.Add(metadata.AnchorCoords.X, metadata.AnchorCoords.Y);
            }
            return compiled;
        }

        private ExecutableInstruction CompileSetHeroFunction(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            HotKey hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
            HotKey hotKey2 = settings.GetHotKey(HotkeyAction.SetFunction1);
            HotKey hotKey3 = settings.GetHotKey(HotkeyAction.SetFunction2);

            if (!IfLast(index))
            {
                compiled.Add(hotKey1, 1);
            }
            if (inst.Arguments[0] == 0)
            {
                compiled.Add(hotKey2, 1);
            }
            else
            {
                compiled.Add(hotKey3, 1);
            }
            if (inst.Coordinates != (-1, -1))
            {
                compiled.Add(inst.Coordinates.X, inst.Coordinates.Y);
            }
            if (!IfNext(index))
            {
                compiled.Add(metadata.AnchorCoords.X, metadata.AnchorCoords.Y);
            }
            return compiled;
        }

        private ExecutableInstruction CompileSellHero(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            HotKey hotKey1 = settings.GetHotKey(HotkeyAction.Hero);
            HotKey hotKey2 = settings.GetHotKey(HotkeyAction.Sell);

            if (!IfLast(index))
            {
                compiled.Add(hotKey1, 1);
            }
            compiled.Add(hotKey2, 2);
            return compiled;
        }

        private ExecutableInstruction CompileUseAbility(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            compiled.Add(settings.GetHotKey((SkillTypes)inst.Arguments[0]));
            // 选择释放坐标
            if (inst.Coordinates != (-1, -1))
            {
                compiled.Add(inst.Coordinates.X, inst.Coordinates.Y);
            }
            return compiled;
        }

        private ExecutableInstruction CompileSwitchSpeed(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            HotKey hotKey1 = settings.GetHotKey(HotkeyAction.ChangeSpeed);
            HotKey hotKey2 = settings.GetHotKey(HotkeyAction.NextRound);
            if (inst.Arguments[0] == 0) // 快/慢切换
            {
                compiled.Add(hotKey1, 1, IfLast(index), IfNext(index));
            }
            if (inst.Arguments[0] == 1) // 竞速下一回合
            {
                compiled.Add(hotKey2, 1, IfLast(index), IfNext(index));
            }
            return compiled;
        }

        private ExecutableInstruction CompileMouseClick(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            if (inst.Coordinates != (-1, -1))
            {
                compiled.Add(inst.Coordinates.X, inst.Coordinates.Y, inst.Arguments[0]);
            }
            return compiled;
        }

        private ExecutableInstruction CompileWaitMilliseconds(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            int times = (inst.Arguments[0] / settings.OperationInterval > 1 ? inst.Arguments[0] / settings.OperationInterval : 1);
            for (int i = 0; i < times; i++)
            {
                compiled.Add(new MicroInstruction(MicroInstructionType.Empty));
            }
            return compiled;
        }

        private ExecutableInstruction CompileStartFreeplay(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            compiled.Add(new MicroInstruction(MicroInstructionType.StartAutoRound));
            int times = (3000 / settings.OperationInterval > 1 ? 3000 / settings.OperationInterval : 1);
            for (int m = 0; m < times; m++)
            {
                compiled.Add(new MicroInstruction(MicroInstructionType.Empty));
            }
            return compiled;
        }

        private ExecutableInstruction CompileEndFreeplay(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            int times = (4500 / settings.OperationInterval > 1 ? 4500 / settings.OperationInterval : 1);
            for (int m = 0; m < times; m++)
            {
                compiled.Add(new MicroInstruction(MicroInstructionType.Empty));
            }
            compiled.Add(1600, 45); // 点设置
            for (int m = 0; m < times; m++)
            {
                compiled.Add(new MicroInstruction(MicroInstructionType.Empty));
            }

            return compiled;
        }

        private ExecutableInstruction CompileJump(int index)
        {
            var inst = instructions[index];
            ExecutableInstruction compiled = new ExecutableInstruction(inst);
            if (inst.Arguments[0] < 1) inst.Arguments[0] = 1;
            if (inst.Arguments[0] > instructions.Count) inst.Arguments[0] = instructions.Count;
            compiled.Add(new MicroInstruction(MicroInstructionType.JumpTo, inst.Arguments[0]));
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

        private bool IfNext(int index)
        {
            if (index < 0 || index >= instructions.Count - 1) return false;
            return CompareCore(instructions[index], instructions[index + 1]);
        }

        private bool IfLast(int index)
        {
            if (index <= 0 || index >= instructions.Count) return false;
            return CompareCore(instructions[index - 1], instructions[index]);
        }

        private bool CompareCore(Instruction a, Instruction b)
        {
            if (a.CoinTrigger != b.CoinTrigger || b.RoundTrigger != b.RoundTrigger) return false;
            if (a.IsMonkeyInstruction() && b.IsMonkeyInstruction() && a.Arguments[0] == b.Arguments[0]) return true;
            if (a.IsHeroInstruction() && b.IsHeroInstruction()) return true;
            if (a.Type == ActionTypes.SwitchSpeed && b.Type == ActionTypes.SwitchSpeed && a.Arguments[0] == b.Arguments[0]) return true;
            return false;
        }
    }
}

