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
            IsRoundMet = false;
            IsCoinMet = false;
            MicroInstructions = new List<MicroInstruction>();
        }

        public void Add(MicroInstruction micro)
        {
            MicroInstructions.Add(micro);
        }

        /// <summary>
        /// 添加按热键, 按键次数的指令，默认1次
        /// </summary>
        /// <param name="key"></param>
        /// <param name="times"></param>
        public void Add(HotKey key, int times = 1, bool isLast = false, bool isNext = false)
        {
            if (!isLast)
            {
                if (key.Alt) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardPress, 18)); }
                if (key.Control) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardPress, 17)); }
                if (key.Shift) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardPress, 16)); }
            }
            for (int i = 0; i < times; i++) MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardPressAndRelease, (int)key.MainKey));
            if (!isNext)
            {
                if (key.Alt) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardRelease, 18)); }
                if (key.Control) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardRelease, 17)); }
                if (key.Shift) { MicroInstructions.Add(new MicroInstruction(MicroInstructionType.KeyboardRelease, 16)); }
            }
        }

        /// <summary>
        /// 添加鼠标移动和点击指令
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="times"></param>
        public void Add(int X, int Y, int times = 1)
        {
            MicroInstructions.Add(new MicroInstruction(MicroInstructionType.MouseMove, X, Y));
            for (int i = 0; i < times; i++) MicroInstructions.Add(new MicroInstruction(MicroInstructionType.LeftClick));
        }


        public MicroInstruction this[int index] => index >= 0 && index < MicroInstructions.Count? MicroInstructions[index] : null;

        public int Count => MicroInstructions.Count;

    }
}
