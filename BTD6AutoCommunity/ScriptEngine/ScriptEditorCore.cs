using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine.InstructionSystem;
using BTD6AutoCommunity.ScriptEngine.ScriptSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTD6AutoCommunity.ScriptEngine
{
    public class ScriptEditorCore
    {
        public ScriptMetadata Metadata { get; set; }

        public InstructionSequence Instructions => instructions;

        private InstructionSequence instructions;

        private readonly InstructionFactory factory;

        private readonly ScriptFileManager fileManager;

        public ScriptEditorCore()
        {
            instructions = new InstructionSequence();
            factory = new InstructionFactory();
            fileManager = new ScriptFileManager();
        }

        public void SetMetadata(ScriptMetadata metadata)
        {
            Metadata = metadata;
        }

        public Instruction AddInstruction(ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (int, int) coords)
        {
            try
            {
                var inst = factory.Create(type, args, roundTrigger, coinTrigger, coords);
                instructions.Insert(instructions.Count, inst);
            }
            catch 
            {
                return null;
            }
            return instructions.Last;
        }

        public int AddInstructionBundle(InstructionSequence bundle, int times)
        {
            return instructions.InsertBundle(instructions.Count, bundle, times);
        }

        public Instruction InsertInstruction(int index, ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (int, int) coords)
        {
            try
            {
                var inst = factory.Create(type, args, roundTrigger, coinTrigger, coords);
                instructions.Insert(index, inst);

            }
            catch
            {
                return null;
            }
            return instructions[index];
        }

        public int InsertInstructionBundle(int index, InstructionSequence bundle, int times)
        {
            return instructions.InsertBundle(index, bundle, times);
        }

        public Instruction ModifyInstruction(int index, ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (int, int) coords)
        {
            try
            {
                var inst = factory.Create(type, args, roundTrigger, coinTrigger, coords);
                if (instructions.Modify(index, inst))
                {
                    return instructions[index];
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public bool TryModifyInstruction(int index, ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (int, int) coords)
        {
            var inst = factory.Create(type, args, roundTrigger, coinTrigger, coords);
            if (instructions.TryModify(index, inst)) return true;
            return false;
        }

        public void DeleteInstruction(int index)
        {
            instructions.Delete(index);
        }

        public void DeleteInstructions(List<int> indices)
        {
            for (int i = indices.Count - 1; i >= 0; i--)
            {
                instructions.Delete(indices[i]);
            }
        }

        public void MoveInstruction(int index, bool up)
        {
            instructions.Move(index, up);
        }

        public List<int> GetAllArguments(int index)
        {
            return instructions.GetAllArguments(index);
        }

        public List<int> GetInstructionsMonkeyIds()
        {
            return instructions.GetMonkeyIds();
        }

        public void ClearInstructions()
        {
            instructions.Clear();
        }

        public void BuildInstructions()
        {
            instructions.Build();
        }

        public string SaveInstructionsToFile()
        {
            var script = new ScriptModel(Metadata, instructions.GetInstructionList(), instructions.GetMonkeyCounts(), instructions.GetMonkeyIds());
            return fileManager.SaveScript(script);
        }

        public void LoadInstructionsFromFile(string filePath)
        {
            var script = fileManager.LoadScript(filePath);
            if (script == null)
            {
                MessageBox.Show("Failed to load script file.");
                return;
            }
            Metadata = script.Metadata;
            instructions = InstructionSequence.BuildByScriptModel(script);
        }
    }
}

