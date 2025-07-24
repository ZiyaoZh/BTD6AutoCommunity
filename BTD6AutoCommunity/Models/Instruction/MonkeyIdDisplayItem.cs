using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.Models.Instruction
{
    public class MonkeyIdDisplayItem
    {
        public int Id { get; set; }

        public string Name => Constants.GetTypeName((Monkeys)(Id % 100)) + (Id / 100).ToString();

        public override string ToString() => Name;
    }
}
