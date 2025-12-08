using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Drawing;

namespace BTD6AutoCommunity.Strategies
{
    public class CustomStrategy : Base.BaseStrategy
    {
        protected int startIndex = 0;

        public CustomStrategy(LogHandler logHandler, UserSelection userSelection)
            : base(logHandler)
        {
            DefaultDataReadInterval = _settings.DataReadInterval;
            DefaultOperationInterval = _settings.OperationInterval;
            startIndex = userSelection.selectedIndex;
            InitializeStateHandlers();
            GetExecutableInstructions(userSelection);
        }

        protected override void InitializeStateHandlers()
        {
            stateHandlers = new Dictionary<GameState, Action>
            {
                // 添加更多状态处理...
            };
        }

        protected override void OnPreStart()
        {
            _logs.Log($"开始自定义策略...", LogLevel.Info);
            screenShotCaptureTimer?.Dispose();
            screenShotCaptureTimer = null;
        }

        protected override void OnPostStart()
        {
            StartLevelTimer(startIndex, false);
        }

        protected override void OnPostStop()
        {
            _logs.Log("自定义策略已停止!", LogLevel.Info);
        }

        protected override void OnGameActionFinished()
        {
            if (IsStrategyExecutionCompleted == false)
            {
                IsStrategyExecutionCompleted = true;
                Stop();
                _logs.Log("策略执行完毕!", LogLevel.Info);
            }
        }
    }
}
