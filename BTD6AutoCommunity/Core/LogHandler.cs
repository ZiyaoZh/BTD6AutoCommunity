using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.Core
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public class LogHandler
    {
        // 日志事件
        public event Action<string, LogLevel> OnLogMessage;
        public bool EnableInfoLog { get; set; } = true;

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (!EnableInfoLog) return;
            OnLogMessage?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}", level);
        }
    }
}
