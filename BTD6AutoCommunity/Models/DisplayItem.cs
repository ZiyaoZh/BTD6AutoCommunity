using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.Models
{
    public class DisplayItem<T> : IDisplayItem
    {
        private readonly T _value;

        public object Value
        {
            get
            {
                if (_value is MonkeyId monkeyId)
                    return monkeyId.Id;

                return _value;
            }
        }

        public Type ValueType => typeof(T);

        public string Name => Constants.GetTypeName(_value, ValueType);

        public DisplayItem(T value)
        {
            _value = value;
        }

        public override string ToString() => Name;

    }
}
