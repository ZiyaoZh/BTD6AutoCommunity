using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.Models
{
    public class HeroDisplayItem
    {
        public Heroes Value { get; set; }

        public string Name => Constants.GetTypeName(Value);

        public override string ToString() => Name;
    }
}
