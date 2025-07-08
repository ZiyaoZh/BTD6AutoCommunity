using BTD6AutoCommunity.Core;
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
        public InstructionSequence Instructions { get; set; }

        private readonly InstructionFactory factory;

        private readonly ScriptFileManager fileManager;

        public ScriptEditorCore()
        {
            Instructions = new InstructionSequence();
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
                Instructions.Add(inst);
            }
            catch 
            {
                return null;
            }
            return Instructions.Last;
        }

        public Instruction InsertInstruction(int index, ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (int, int) coords)
        {
            try
            {
                var inst = factory.Create(type, args, roundTrigger, coinTrigger, coords);
                Instructions.Insert(index, inst);

            }
            catch
            {
                return null;
            }
            return Instructions[index];
        }

        public Instruction ModifyInstruction(int index, ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (int, int) coords)
        {
            try
            {
                var inst = factory.Create(type, args, roundTrigger, coinTrigger, coords);
                if (Instructions.Modify(index, inst))
                {
                    return Instructions[index];
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public void DeleteInstruction(int index)
        {
            Instructions.Delete(index);
        }

        public void MoveInstruction(int index, bool up)
        {
            Instructions.Move(index, up);
        }

        public List<int> GetAllArguments(int index)
        {
            return Instructions.GetAllArguments(index);
        }

        public void ClearInstructions()
        {
            Instructions.Clear();
        }

        public void BuildInstructions()
        {
            Instructions.Build();
        }

        public string SaveInstructionsToFile()
        {
            var script = new ScriptModel(Metadata, Instructions.GetInstructionList(), Instructions.GetMonkeyCount(), Instructions.GetMonkeyIds());
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
            Instructions = InstructionSequence.BuildByScriptModel(script);
        }
    }
}

