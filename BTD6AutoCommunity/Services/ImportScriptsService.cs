using BTD6AutoCommunity.ScriptEngine;
using BTD6AutoCommunity.ScriptEngine.ScriptSystem;
using BTD6AutoCommunity.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTD6AutoCommunity.Services
{
    public class ImportScriptsService
    {
        private readonly MessageBoxService _messageBoxService = new MessageBoxService();
        
        public bool IsScriptPackage { get; set; } = false;

        public List<string> PackageList { get; set; } = new List<string>();

        public ImportScriptsService() 
        {

        }

        public ScriptModel ImportScripts()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "脚本文件 (*.btd6)|*.btd6|脚本包 (*.btd6s)|*.btd6s";

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return null;

                string sourceFilePath = openFileDialog.FileName;
                string extension = Path.GetExtension(sourceFilePath);

                if (extension == ".btd6")
                {
                    try
                    {
                        var scriptModel = ScriptFileManager.LoadScript(sourceFilePath);
                        ScriptService scriptService = new ScriptService();
                        scriptService.LoadScript(scriptModel);
                        scriptService.SaveScript();
                        IsScriptPackage = false;
                        return scriptModel;
                    }
                    catch
                    {
                        _messageBoxService.ShowError("脚本内容错误！");
                        return null;
                    }
                }
                else if (extension == ".btd6s")
                {
                    try
                    {
                        var scriptModels = ScriptFileManager.LoadScriptPackage(sourceFilePath);
                        PackageList = ScriptFileManager.GetScriptPackageList(scriptModels);
                        var sb = new StringBuilder();
                        foreach (var name in PackageList)
                        {
                            sb.Append(name);
                            sb.Append(" ");
                        }
                        _messageBoxService.ShowMessage("脚本包导入成功，包含以下脚本：\n" + sb.ToString());
                        ScriptService scriptService = new ScriptService();
                        foreach (var scriptModel in scriptModels)
                        {
                            scriptService.LoadScript(scriptModel);
                            scriptService.SaveScript();
                        }
                        IsScriptPackage = true;
                        // 对于脚本包，返回第一个脚本或null
                        return scriptModels.FirstOrDefault();
                    }
                    catch
                    {
                        _messageBoxService.ShowError("脚本包内容错误！");
                        return null;
                    }
                }
                else
                {
                    _messageBoxService.ShowError("不支持的文件格式！");
                    return null;
                }
            }
        }
    }
}
