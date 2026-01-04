using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.Services.Interfaces;
using BTD6AutoCommunity.Views;
using System;
using System.Drawing;
using System.Windows.Forms;

public class MouseCoordinateDisplayService : IMouseCoordinateDisplayService
{
    private Action<(double, double)> _onEnterPressed;
    private readonly IntPtr _windowHandle;
    private readonly object _lock = new object();
    private bool _isRunning = false;

    public MouseCoordinateDisplayService(IntPtr mainWindowHandle)
    {
        _windowHandle = mainWindowHandle;
    }

    public void StartDisplay(Action<(double, double)> onEnterPressed)
    {
        lock (_lock)
        {
            if (_isRunning) return;
            _isRunning = true;
        }

        _onEnterPressed = onEnterPressed;

        // 注册回车键热键
        WindowApiWrapper.RegisterHotKey(_windowHandle, 13, 0, Keys.Enter);

        // 确保 MaskWindow 已打开并显示坐标
        var context = new GameContext();
        if (context.IsValid)
        {
            MaskWindow.Open(context);
            MaskWindow.Instance.ToggleMouseCoordinateDisplay(true);
        }
        // 如果游戏窗口未找到，MaskWindow 不会打开，用户也不会看到任何东西
    }

    public void StopDisplay()
    {
        lock (_lock)
        {
            if (!_isRunning) return;
            _isRunning = false;

            // 隐藏 MaskWindow 上的坐标
            if (MaskWindow.Instance != null && !MaskWindow.Instance.IsDisposed)
            {
                MaskWindow.Instance.ToggleMouseCoordinateDisplay(false);
            }
            // 注意：这里不关闭 MaskWindow，因为它可能被其他功能（如显示策略步骤）使用。
            // 坐标显示只是其上的一个图层。

            // 注销热键
            WindowApiWrapper.UnregisterHotKey(_windowHandle, 13);
        }
    }

    public void HandleHotKeyPressed()
    {
        lock (_lock)
        {
            if (_isRunning)
            {
                var context = new GameContext();
                if (context.IsValid && MaskWindow.Instance != null && !MaskWindow.Instance.IsDisposed)
                {
                    // 从 MaskWindow 获取当前的游戏内坐标
                    (double, double) gamePosition = MaskWindow.Instance.GetMousePositionInGame(context);
                    _onEnterPressed?.Invoke(gamePosition);
                }
            }
        }
    }
}
