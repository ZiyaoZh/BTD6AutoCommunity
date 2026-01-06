using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine.InstructionSystem;
using BTD6AutoCommunity.ScriptEngine.ScriptSystem;
using BTD6AutoCommunity.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTD6AutoCommunity.ScriptEngine
{
    public class ScriptService : IScriptService
    {
        private ScriptMetadata metadata;

        private InstructionSequence instructions;

        private InstructionsBundle bundle;

        private readonly InstructionFactory factory;


        public ScriptService()
        {
            metadata = new ScriptMetadata();
            instructions = new InstructionSequence();
            bundle = new InstructionsBundle();
            factory = new InstructionFactory();
        }

        public void SetMetadata(ScriptMetadata metadata)
        {
            this.metadata = metadata;
        }

        public ScriptMetadata GetMetadata()
        {
            return metadata;
        }

        public InstructionSequence GetInstructions()
        {
            return instructions;
        }

        public InstructionSequence GetInstructionsCopy()
        {
            return instructions.Clone();
        }

        public Instruction AddInstruction(ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (double, double) coords)
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

        public int AddInstructionBundle(string bundleName, int times)
        {
            bundle = new InstructionsBundle();
            return instructions.InsertBundle(instructions.Count, bundle.GetInstructionSequence(bundleName), times);
        }

        public Instruction InsertInstruction(int index, ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (double, double) coords)
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

        public int InsertInstructionBundle(int index, string bundleName, int times)
        {
            return instructions.InsertBundle(index, bundle.GetInstructionSequence(bundleName), times);
        }

        public Instruction ModifyInstruction(int index, ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (double, double) coords)
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

        public bool TryModifyInstruction(int index, ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (double, double) coords)
        {
            var inst = factory.Create(type, args, roundTrigger, coinTrigger, coords);
            if (instructions.TryModify(index, inst)) return true;
            return false;
        }

        public void RemoveInstruction(int index)
        {
            instructions.Delete(index);
        }

        public void RemoveInstructions(List<int> indices)
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

        public Instruction GetInstruction(int index)
        {
            return instructions[index];
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

        public string GetScriptPath(string seletedMap, string selectedDifficulty, string scriptName)
        {
            return ScriptFileManager.GetScriptFullPath(
                seletedMap,
                selectedDifficulty,
                scriptName
                );
        }

        public string GetScriptPath()
        {
            return ScriptFileManager.GetScriptFullPath(
                Constants.GetTypeName(metadata.SelectedMap),
                Constants.GetTypeName(metadata.SelectedDifficulty),
                metadata.ScriptName
                );
        }

        public string SaveScript()
        {
            BuildInstructions();
            var script = new ScriptModel(metadata, instructions.GetInstructionList(), instructions.GetMonkeyCounts(), instructions.GetMonkeyIds());
            return ScriptFileManager.SaveScript(script);
        }

        public bool LoadScript(string scriptPath)
        {
            var script = ScriptFileManager.LoadScript(scriptPath);
            if (script == null)
            {
                return false;
            }
            metadata = script.Metadata;
            instructions = InstructionSequence.BuildByScriptModel(script);
            return true;
        }

        public bool LoadScript(ScriptModel scriptModel)
        {
            if (scriptModel == null)
            {
                return false;
            }
            metadata = scriptModel.Metadata;
            instructions = InstructionSequence.BuildByScriptModel(scriptModel);
            return true;
        }

        public List<string> GetPreview()
        {
            if (instructions.Count == 0) return new List<string>();
            return instructions.InstructionsList
                       .Select(inst => inst.ToString()) // 或自定义展示格式
                       .ToList();
        }
    }
}

