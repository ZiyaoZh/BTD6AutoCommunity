using BTD6AutoCommunity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine
{
    public class InstructionFactory
    {
        public Instruction Create(
            ActionTypes type,
            List<int> args,
            int roundTrigger,
            int coinTrigger,
            (int x, int y) coords)
        {
            return new Instruction(
                type,
                args,
                roundTrigger,
                coinTrigger,
                coords
            );
        }
    }
}
