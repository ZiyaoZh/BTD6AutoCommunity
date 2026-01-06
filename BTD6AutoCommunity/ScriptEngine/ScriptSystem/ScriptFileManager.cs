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
using System.Security.Policy;

namespace BTD6AutoCommunity.ScriptEngine.ScriptSystem
{
    public class ScriptFileManager
    {
        private const string basePath = "data\\我的脚本\\";
        private const string deleteDir = "data\\最近删除\\";

        public static bool ScriptExists(string mapName, string difficultyName, string scriptName)
        {
            string fullPath = (GetScriptFullPath(mapName, difficultyName, scriptName));
            return File.Exists(fullPath);
        }

        public static string GetScriptFullPath(string mapName, string difficultyName, string scriptName)
        {
            return Path.GetFullPath(Path.Combine(basePath, mapName, difficultyName, $"{scriptName}.btd6"));
        }

        public static string GetScriptFullPath(ScriptModel scriptModel)
        {
            string mapName = Constants.GetTypeName(scriptModel.Metadata.SelectedMap);
            string difficultyName = Constants.GetTypeName(scriptModel.Metadata.SelectedDifficulty);
            string scriptName = scriptModel.Metadata.ScriptName;
            return GetScriptFullPath(mapName, difficultyName, scriptName);
        }

        public static string SaveScript(ScriptModel script)
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

        public static void DeleteScript(string fullpath)
        {
            // 将文件移入data/最近删除
            Directory.CreateDirectory(deleteDir);
            // 名称加上日期事件信息
            string fileName = Path.GetFileName(fullpath) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string newPath = Path.Combine(deleteDir, fileName);
            if (File.Exists(newPath))
            {
                File.Delete(newPath);
            }
            File.Move(fullpath, newPath);
        }

        public static ScriptMetadata GetScriptMetadata(string fullPath)
        {
            if (!File.Exists(fullPath))
                return null;
            string json = File.ReadAllText(fullPath);
            ScriptModel model = JsonConvert.DeserializeObject<ScriptModel>(json);
            return model.Metadata;
        }

        public static ScriptMetadata GetScriptMetadata(ScriptModel scriptModel)
        {
            return scriptModel.Metadata;
        }

        public static ScriptModel LoadScript(string fullPath)
        {
            if (!File.Exists(fullPath))
                return null;

            string json = File.ReadAllText(fullPath);

            // 兼容旧格式处理（你原来的 RepairScript）
            ScriptModel model = JsonConvert.DeserializeObject<ScriptModel>(json);
            string scriptName = Path.GetFileNameWithoutExtension(fullPath);
            
            return RepairScript(model, json, scriptName); ;
        }

        public static List<ScriptModel> LoadScriptPackage(string fullPath)
        {
            if (!File.Exists(fullPath))
                return null;
            string json = File.ReadAllText(fullPath);
            List<ScriptModel> models = JsonConvert.DeserializeObject<List<ScriptModel>>(json);
            return models;
        }

        public static List<string> GetScriptPackageList(string fullPath)
        {
            if (!File.Exists(fullPath))
                return null;
            string json = File.ReadAllText(fullPath);
            List<ScriptModel> models = JsonConvert.DeserializeObject<List<ScriptModel>>(json);
            return models.Select(m => m.Metadata.ScriptName).ToList();
        }

        public static List<string> GetScriptPackageList(List<ScriptModel> models)
        {
            return models.Select(m => Constants.GetTypeName(m.Metadata.SelectedMap) + '\\' + 
                                    Constants.GetTypeName(m.Metadata.SelectedDifficulty) + '\\' + 
                                    m.Metadata.ScriptName).ToList();
        }

        private static ScriptModel RepairScript(ScriptModel model, string json, string scriptName)
        {
            ScriptModel newModel = model;
            if (model.Metadata == null)
            {
                ScriptModelOld modelOld = JsonConvert.DeserializeObject<ScriptModelOld>(json);
                newModel = ScriptModel.Convert00_11(modelOld, scriptName);
                SaveScript(newModel);
            }
            else
            {
                if (model.Metadata.Version != "1.1")
                {
                    newModel = ScriptModel.Convert10_11(model);
                    SaveScript(newModel);
                }
            }
            return newModel;
        }
    }
}

