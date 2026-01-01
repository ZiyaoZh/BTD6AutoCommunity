using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.Services.Interfaces;
using BTD6AutoCommunity;
using System.Timers;
using System.Windows.Forms;
using System;
using System.Drawing;

public class MouseCoordinateDisplayService : IMouseCoordinateDisplayService
{
    private OverlayForm _overlayForm;
    private System.Timers.Timer _timer;
    private Action<Point> _onEnterPressed;
    private readonly IntPtr _windowHandle;
    private Point position;
    private readonly object _lock = new object();
    private bool _isRunning = false;

    public MouseCoordinateDisplayService(IntPtr mainWindowHandle)
    {
        _windowHandle = mainWindowHandle;
    }

    public void StartDisplay(Action<Point> onEnterPressed)
    {
        lock (_lock)
        {
            if (_isRunning) return;
            _isRunning = true;
        }

        _onEnterPressed = onEnterPressed;

        WindowApiWrapper.RegisterHotKey(_windowHandle, 13, 0, Keys.Enter);

        _overlayForm = new OverlayForm();
        _overlayForm.Show();

        _timer = new System.Timers.Timer(100);
        _timer.Elapsed += TimerElapsed;
        _timer.Start();
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        lock (_lock)
        {
            if (!_isRunning) return;

            position = Cursor.Position;
            GameContext context = new GameContext();

            if (!context.IsValid)
            {
                _overlayForm?.UpdateLabelPosition(Cursor.Position, "未找到游戏窗口");
                return;
            }

            Point mousePoint = WindowApiWrapper.GetCursorPosition();
            (double x, double y) = context.ConvertScreenPosition(mousePoint.X * context.DpiScale, mousePoint.Y * context.DpiScale);
            position.X = (int)Math.Round(x);
            position.Y = (int)Math.Round(y);

            if (position.X >= 16000 || position.Y >= 9000)
            {
                _overlayForm?.UpdateLabelPosition(Cursor.Position, "无效的坐标");
            }
            else
            {
                _overlayForm?.UpdateLabelPosition(Cursor.Position, $"X: {position.X}, Y: {position.Y}");
            }
        }
    }

    public void StopDisplay()
    {
        lock (_lock)
        {
            if (!_isRunning) return;
            _isRunning = false;

            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;

            _overlayForm?.Hide();
            _overlayForm?.Dispose();
            _overlayForm = null;

            WindowApiWrapper.UnregisterHotKey(_windowHandle, 13);
        }
    }

    public void HandleHotKeyPressed()
    {
        lock (_lock)
        {
            if (_isRunning)
            {
                _onEnterPressed?.Invoke(position);
            }
        }
    }
}
