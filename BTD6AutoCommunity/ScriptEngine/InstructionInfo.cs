using BTD6AutoCommunity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine
{
    public struct InstructionInfo
    {
        public int Index { get; set; }  // 指令索引

        public ActionTypes Type { get; set; }  // 指令类型

        public string Content { get; set; }  // 指令内容
        public List<int> Arguments { get; set; }    // 指令内容

        public int RoundTrigger { get; set; }  // 回合触发条件

        public int CashTrigger { get; set; }   // Cash触发条件

        public bool IsRoundMet { get; set; }  // 是否达到回合触发条件

        public bool IsCashMet { get; set; }   // 是否达到Cash触发条件

        public InstructionInfo(int index, ActionTypes type, List<int> arguments, string content, int round, int cash)
        {
            Index = index;
            Type = type;
            Arguments = arguments;
            Content = content;
            RoundTrigger = round;
            CashTrigger = cash;
            IsRoundMet = true;
            IsCashMet = true;
        }

        public override string ToString()
        {
            return Content;
        }
    }

}
