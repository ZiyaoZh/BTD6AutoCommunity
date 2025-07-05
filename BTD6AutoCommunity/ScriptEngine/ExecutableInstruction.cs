using BTD6AutoCommunity.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine
{
    public class ExecutableInstruction
    {
        public int Index { get; set; }

        public ActionTypes Type { get; set; }

        public int RoundTrigger { get; set; }

        public int CoinTrigger { get; set; }

        public bool IsRoundMet { get; set; }

        public bool IsCoinMet { get; set; }

        public string Content { get; set; }

        public List<MicroInstruction> MicroInstructions { get; set; }

        public ExecutableInstruction(Instruction inst)
        {
            Type = inst.Type;
            RoundTrigger = inst.RoundTrigger;
            CoinTrigger = inst.CoinTrigger;
            Content = inst.ToString();
        }

        public void Add(MicroInstruction micro)
        {
            MicroInstructions.Add(micro);
        }

        public void Add(HotKey key)
        {
            if (key.Alt) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardPress, 18)); }
            if (key.Control) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardPress, 17)); }
            if (key.Shift) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardPress, 16)); }
            MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardPressAndRelease, (int)key.MainKey));
            if (key.Alt) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardRelease, 18)); }
            if (key.Control) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardRelease, 17)); }
            if (key.Shift) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardRelease, 16)); }
        }

        public MicroInstruction this[int index] => index >= 0 && index < MicroInstructions.Count? MicroInstructions[index] : null;

        public int Count => MicroInstructions.Count;

    }
}
