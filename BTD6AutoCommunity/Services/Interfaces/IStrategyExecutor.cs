using BTD6AutoCommunity.ScriptEngine.InstructionSystem;
using BTD6AutoCommunity.ScriptEngine.ScriptSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.Services.Interfaces
{
    public interface IStrategyExecutor
    {
        bool ReadyToStart { get; }
        bool IsPaused { get; }
        event Action OnStopTriggered;
        event Action<List<string>> OnGameDataUpdated;
        event Action<ExecutableInstruction> OnCurrentInstructionCompleted;
        event Action<ScriptMetadata> OnScriptLoaded;
        void Start();

        void Pause();

        void Resume();

        void Stop();
    }
}
