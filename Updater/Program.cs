using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Updater
{
    internal class Program
    {
        // ====== 配置区 ======
        static readonly string BaseUrl =
            "https://gitee.com/ldcz1037/BTD6AutoCommunity.Updater/raw/main/update/";

        static readonly string VersionJsonName = "version.json";
        static readonly string MainExeName = "BTD6AutoCommunity.exe"; // 主程序 exe 名

        // ===================

        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length == 0 || !int.TryParse(args[0], out int pid))
                {
                    Console.WriteLine("Missing main process id.");
                    return;
                }

                WaitMainProcessExit(pid);

                Console.WriteLine("Downloading version.json...");
                var versionInfo = await DownloadAndParseVersion();
                //Thread.Sleep(5000);

                await UpdateFiles(versionInfo);
                DeleteFiles(versionInfo);
                //Thread.Sleep(5000);

                Console.WriteLine("Update finished, restarting app...");
                Process.Start(new ProcessStartInfo
                {
                    FileName = MainExeName,
                    UseShellExecute = false
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Updater failed:");
                Console.WriteLine(ex);
                //Thread.Sleep(5000);

            }
        }

        // ================== 核心逻辑 ==================

        static void WaitMainProcessExit(int pid)
        {
            try
            {
                var p = Process.GetProcessById(pid);
                Console.WriteLine("Waiting for main process exit...");
                p.WaitForExit();
            }
            catch
            {
                // already exited
            }
        }

        static async Task<VersionInfo> DownloadAndParseVersion()
        {
            using (var client = CreateHttpClient())
            {
                var json = await client.GetStringAsync(BaseUrl + VersionJsonName);
                //Console.WriteLine("Downloaded version.json:" + json);
                var info = JsonConvert.DeserializeObject<VersionInfo>(json) ?? throw new Exception("Failed to parse version.json");

                if (!Directory.Exists("config"))
                    Directory.CreateDirectory("config");
                File.WriteAllText(Path.Combine("config", "version.json"), json);

                return info;
            }
        }

        static async Task UpdateFiles(VersionInfo info)
        {
            using (var client = CreateHttpClient())
            {
                foreach (var file in (info.Files ?? new List<UpdateFile>()))
                {
                    string localPath = Path.Combine(AppContext.BaseDirectory, file.Path);
                    string url = BaseUrl + GetUrlEncodedPath(file.Path);
                    //Console.WriteLine($"url:{url}");

                    if (!NeedUpdate(localPath, file.Sha256))
                    {
                        Console.WriteLine($"Skip: {file.Path}");
                        continue;
                    }

                    Console.WriteLine($"Updating: {file.Path}");

                    // 使用 GetAsync 以便检查状态码和内容类型
                    using (var resp = await client.GetAsync(url))
                    {
                        Console.WriteLine($"HTTP { (int)resp.StatusCode } { resp.ReasonPhrase }");
                        if (!resp.IsSuccessStatusCode)
                        {
                            // 记录一段响应文本，帮助判断是否为 HTML 错误页
                            string text = "";
                            try
                            {
                                text = await resp.Content.ReadAsStringAsync();
                                Console.WriteLine("Response preview:");
                                Console.WriteLine(text.Length > 512 ? text.Substring(0, 512) : text);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Failed reading response text: " + ex.Message);
                            }
                            throw new Exception($"Failed to download {file.Path}: HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");
                        }

                        //// 可选：比较 Content-Length 与 version.json 中的 Size（如果有的话）
                        //if (file.Size.HasValue && resp.Content.Headers.ContentLength.HasValue)
                        //{
                        //    if (file.Size.Value != resp.Content.Headers.ContentLength.Value)
                        //    {
                        //        Console.WriteLine($"Warning: expected size {file.Size.Value}, actual {resp.Content.Headers.ContentLength.Value}");
                        //    }
                        //}

                        var bytes = await resp.Content.ReadAsByteArrayAsync();
                        // 输出为字符串
                        Console.WriteLine($"Downloaded {bytes.Length} bytes.");

                        if (!string.IsNullOrEmpty(file.Sha256))
                        {
                            var hash = CalcSha256(bytes);
                            Console.WriteLine("Downloaded SHA256: " + hash);
                            if (!hash.Equals(file.Sha256, StringComparison.OrdinalIgnoreCase))
                                throw new Exception($"Hash mismatch: {file.Path}");
                        }

                        WriteAtomic(localPath, bytes);
                    }
                }
            }
        }

        static void DeleteFiles(VersionInfo info)
        {
            foreach (var path in (info.Delete ?? new List<string>()))
            {
                string fullPath = Path.Combine(AppContext.BaseDirectory, path);
                if (File.Exists(fullPath))
                {
                    Console.WriteLine($"Delete: {path}");
                    File.Delete(fullPath);
                }
            }
        }

        // ================== 工具方法 ==================

        static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Updater");
            return client;
        }

        static bool NeedUpdate(string path, string remoteHash)
        {
            if (!File.Exists(path)) return true;
            if (string.IsNullOrEmpty(remoteHash)) return true;

            var localHash = CalcSha256(File.ReadAllBytes(path));
            return !localHash.Equals(remoteHash, StringComparison.OrdinalIgnoreCase);
        }

        static void WriteAtomic(string path, byte[] bytes)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string tmp = path + ".tmp";
            File.WriteAllBytes(tmp, bytes);

            if (File.Exists(path))
                File.Delete(path);

            File.Move(tmp, path);
        }

        static string CalcSha256(byte[] data)
        {
            var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }

        // 将特殊字符进行 URL 编码
        static string GetUrlEncodedPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path ?? "";

            // 统一分隔符为 '/'
            var normalized = path.Replace("\\", "/");

            // 对每个路径段进行编码，保留 '/' 分隔
            var segments = normalized.Split(new[] { '/' }, StringSplitOptions.None);
            for (int i = 0; i < segments.Length; i++)
            {
                var seg = segments[i] ?? "";
                if (seg.Length == 0)
                {
                    // 保持空段（以保留连续或前导/末尾斜杠）
                    segments[i] = "";
                    continue;
                }

                try
                {
                    // 先解码防止二次编码，然后再进行标准的 URL 编码
                    var decoded = Uri.UnescapeDataString(seg);
                    segments[i] = Uri.EscapeDataString(decoded);
                }
                catch
                {
                    // 如果 Unescape 出错，退而直接编码原始段
                    segments[i] = Uri.EscapeDataString(seg);
                }
            }

            return string.Join("/", segments);
        }
    }

    // ================== 数据结构 ==================

    class VersionInfo
    {
        public string Version { get; set; } = "";
        public bool Force { get; set; }
        public string Changelog { get; set; } = "";
        public List<UpdateFile> Files { get; set; }
        public List<string> Delete { get; set; }
    }

    class UpdateFile
    {
        public string Path { get; set; } = "";
        public string Sha256 { get; set; }
        public long? Size { get; set; }
    }
}
