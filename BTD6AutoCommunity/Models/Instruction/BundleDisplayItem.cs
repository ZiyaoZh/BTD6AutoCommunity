using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.Models.Instruction
{
    public class BundleDisplayItem
    {
        public string Value { get; set; }

        public string Name => Value;

        public override string ToString() => Value;
    }
}
