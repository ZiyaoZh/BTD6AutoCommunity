using BTD6AutoCommunity.ScriptEngine.ScriptSystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine.InstructionSystem
{
    public class InstructionsBundle
    {
        public List<string> BundleNames;

        private List<InstructionSequence> InstructionSequences;

        private const string fileName = @"config\InstructionsBundle.json";

        public InstructionsBundle() 
        {
            LoadBundle();
        }

        public void LoadBundle()
        {
            if (!File.Exists(fileName))
            {
                // 创建一个文件
                File.Create(fileName).Close();
            }
            var json = File.ReadAllText(fileName);
            var bundle = JsonConvert.DeserializeObject<InstructionsBundleModel>(json);
            if (bundle == null)
            {
                BundleNames = new List<string>();
                InstructionSequences = new List<InstructionSequence>();
            }
            else
            {
                BundleNames = bundle.BundleName ?? new List<string>();
                InstructionSequences = bundle.ScriptModels != null ? bundle.ScriptModels.Select(s => InstructionSequence.BuildByScriptModel(s)).ToList() : new List<InstructionSequence>();
            }
        }

        public void SaveBundle()
        {
            InstructionsBundleModel model = new InstructionsBundleModel()
            {
                BundleName = BundleNames,
                ScriptModels = InstructionSequences.Select(s => ScriptModel.Create(null, s)).ToList()
            };
            var json = JsonConvert.SerializeObject(model, Formatting.None);
            File.WriteAllText(fileName, json);
        }

        public bool AddBundle(string bundleName, ScriptModel scriptModel)
        {
            if (BundleNames.Contains(bundleName))
            {
                return false;
            }
            BundleNames.Add(bundleName);
            InstructionSequences.Add(InstructionSequence.BuildByScriptModel(scriptModel));
            return true;
        }

        public void RemoveBundle(string bundleName)
        {
            var index = BundleNames.IndexOf(bundleName);
            if (index == -1)
            {
                return;
            }
            BundleNames.RemoveAt(index);
            InstructionSequences.RemoveAt(index);
        }

        public InstructionSequence GetInstructionSequence(string bundleName)
        {
            var index = BundleNames.IndexOf(bundleName);
            if (index == -1)
            {
                return null;
            }
            return InstructionSequences[index];
        }
    }

    public class InstructionsBundleModel
    {
        public List<string> BundleName { get; set; }
        public List<ScriptModel> ScriptModels { get; set; }
    }
}
