using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using BTD6AutoCommunity.Services.Interfaces;

namespace BTD6AutoCommunity.Services
{
    public class UpdateService
    {
        private const string LatestVersionUrl = "https://gitee.com/ldcz1037/BTD6AutoCommunity.Updater/raw/main/update/version.json";
        private const string LocalVersionFilePath = @"config/version.json";
        private const string UpdaterExePath = "Updater.exe";
        private readonly IMessageBoxService messageBoxService;

        public UpdateService(IMessageBoxService messageBoxService)
        {
            this.messageBoxService = messageBoxService;
        }

        public async Task CheckUpdateAsync(bool IsPassive = false)
        {
            try
            {
                VersionInfo VersionInfo = await GetLatestVersionAsync();
                string latestVersion = VersionInfo?.Version ?? "Unknown";
                string currentVersion = GetCurrentVersion();
                string changelog = VersionInfo?.Changelog ?? "未获取到更新内容";
                if (latestVersion != currentVersion)
                {
                    // 有新版本，执行相应逻辑
                    if (messageBoxService.ShowConfirmation($"发现新版本{latestVersion}，当前版本{currentVersion}。\n更新内容：{changelog}。\n是否前往下载？", "更新提示") == DialogResult.Yes)
                    {
                        if (!System.IO.File.Exists(UpdaterExePath))
                        {
                            messageBoxService.ShowError("更新程序不存在，无法更新。", "错误");
                            return;
                        }
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = UpdaterExePath,
                            Arguments = Process.GetCurrentProcess().Id.ToString(),
                            UseShellExecute = true
                        });
                        Application.Exit();
                    }
                }
                else
                {
                    if (IsPassive)
                        messageBoxService.ShowMessage("当前已是最新版！");
                }
            }
            catch (Exception ex)
            {
                messageBoxService.ShowError($"检查更新时发生错误: {ex.Message}", "错误");
                return;
            }
        }

        public async Task<VersionInfo> GetLatestVersionAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync(LatestVersionUrl);
                var versionInfo = JsonConvert.DeserializeObject<VersionInfo>(response);
                return versionInfo;
            }
        }

        public string GetCurrentVersion()
        {
            if (!System.IO.File.Exists(LocalVersionFilePath))
            {
                return "Unknown";
            }
            var versionJson = System.IO.File.ReadAllText(LocalVersionFilePath);
            var versionInfo = JsonConvert.DeserializeObject<VersionInfo>(versionJson);
            return versionInfo?.Version ?? "Unknown";
        }
    }

    public class VersionInfo
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        public string Changelog { get; set; }
    }
}
