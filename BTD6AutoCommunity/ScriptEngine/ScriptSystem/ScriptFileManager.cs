using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine.InstructionSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace BTD6AutoCommunity.ScriptEngine.ScriptSystem
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
            string fullPath = (GetScriptFullPath(mapName, difficultyName, scriptName));
            return File.Exists(fullPath);
        }

        public string GetScriptFullPath(string mapName, string difficultyName, string scriptName)
        {
            return Path.Combine(basePath, mapName, difficultyName, $"{scriptName}.btd6");
        }

        public string SaveScript(ScriptModel script)
        {
            string mapName = Constants.GetTypeName(script.Metadata.SelectedMap);
            string difficultyName = Constants.GetTypeName(script.Metadata.SelectedDifficulty);
            string dir = Path.Combine(basePath, mapName, difficultyName);
            Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, $"{script.Metadata.ScriptName}.btd6");

            string json = JsonConvert.SerializeObject(script, Formatting.None);
            if (File.Exists(path)) DeleteScript(path);

            File.WriteAllText(path, json);
            return path;
        }

        public void DeleteScript(string fullpath)
        {
            // 将文件移入data/最近删除
            string recentDeleteDir = Path.Combine(basePath, "最近删除");
            Directory.CreateDirectory(recentDeleteDir);
            // 名称加上日期事件信息
            string fileName = Path.GetFileName(fullpath) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string newPath = Path.Combine(recentDeleteDir, fileName);
            File.Move(fullpath, newPath);
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

