using BTD6AutoCommunity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace BTD6AutoCommunity.ScriptEngine
{
    public class ScriptFileManager
    {
        private readonly string basePath;

        public ScriptFileManager()
        {
            basePath = $@"data\我的脚本\";
        }

        public bool ScriptExists(string mapName, string difficultyName, string scriptName)
        {
            string fullPath = GetScriptPath(mapName, difficultyName, scriptName);
            return File.Exists(fullPath);
        }

        public string GetScriptPath(string mapName, string difficultyName, string scriptName)
        {
            return Path.Combine(basePath, mapName, difficultyName, $"{scriptName}.json");
        }

        public string SaveScript(ScriptModel script)
        {
            string mapName = Constants.GetTypeName(script.Metadata.SelectedMap);
            string difficultyName = Constants.GetTypeName(script.Metadata.SelectedDifficulty);
            string dir = Path.Combine(basePath, mapName, difficultyName);
            Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, $"{script.Metadata.ScriptName}.btd6");

            string json = JsonConvert.SerializeObject(script, Formatting.None);
            File.WriteAllText(path, json);
            return path;
        }

        public ScriptModel LoadScript(string fullPath)
        {
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"找不到脚本文件: {fullPath}");

            string json = File.ReadAllText(fullPath);

            // 兼容旧格式处理（你原来的 RepairScript）
            ScriptModel model = JsonConvert.DeserializeObject<ScriptModel>(json);
            string scriptName = Path.GetFileNameWithoutExtension(fullPath);
            
            return RepairScript(model, json, scriptName); ;
        }

        private ScriptModel RepairScript(ScriptModel model, string json, string scriptName)
        {
            ScriptModel newModel = model;
            if (model.Metadata == null)
            {
                ScriptModelOld modelOld = JsonConvert.DeserializeObject<ScriptModelOld>(json);
                newModel = ScriptModel.Convert(modelOld, scriptName);
                SaveScript(newModel);
            }
            return newModel;
        }
    }
}

