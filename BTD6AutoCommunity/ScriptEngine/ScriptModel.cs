using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine
{
    public class ScriptModel
    {
        public ScriptMetadata Metadata { get; set; }

        public List<List<int>> InstructionsList { get; set; }

        public List<int> MonkeyCount { get; set; }

        public List<(int, int)> MonkeyId { get; set; }

        public ScriptModel() { }

        public ScriptModel(ScriptMetadata metadata, List<List<int>> sequence, List<int> monkeyCount, List<(int, int)> monkeyId)
        {
            Metadata = metadata;
            InstructionsList = sequence;
            MonkeyCount = monkeyCount;
            MonkeyId = monkeyId;
        }
    }
}

