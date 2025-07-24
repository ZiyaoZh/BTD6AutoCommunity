using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine.InstructionSystem
{
    public interface IInstructionArgument
    {
        string Name { get; set; }
        Type ValueType { get; }
        bool IsVisible { get; set; }
        bool IsSelectable { get; set; }
        string Placeholder { get; set; }

        object GetValue();  // 获取最终值
    }
}
