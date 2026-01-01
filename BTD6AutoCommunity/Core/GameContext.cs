using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BTD6AutoCommunity.Core.WindowApiWrapper;

namespace BTD6AutoCommunity.Core
{
    public class GameContext
    {
        // 窗口相关参数
        public IntPtr WindowHandle { get; set; }
        public RECT ClientRect { get; set; }

        // 原始窗口左上角屏幕坐标
        public POINT OriginalClientTopLeft;

        public POINT ClientTopLeft;

        // 分辨率相关
        public Size BaseResolution { get; } = new Size(1920, 1080);
        public Size CurrentResolution { get; set; }
        public double ResolutionScale => _preciseCurrentResolution.Width / BaseResolution.Width;

        public double ScreenWidth { get; set; }
        public double ScreenHeight { get; set; }

        // DPI相关
        public double SystemDpi { get; set; }
        public double DpiScale => SystemDpi / 96.0; // 标准DPI为96

        // 初始化状态
        public bool IsValid { get; set; }

        // 用于精确计算的内部变量
        private (double Width, double Height) _preciseCurrentResolution;
        private (double X, double Y) _preciseClientTopLeft;


        public GameContext()
        {
            Update();
        }

        public void Update()
        {
            try
            {
                // 自动查找游戏窗口
                WindowHandle = FindGameWindow();
                if (WindowHandle == IntPtr.Zero)
                {
                    IsValid = false;
                    return;
                }

                // 获取DPI
                SystemDpi = GetSystemDpi();

                ScreenWidth = GetSystemMetrics(0) * DpiScale;
                ScreenHeight = GetSystemMetrics(1) * DpiScale;

                // 获取窗口信息
                if (!GetClientRect(WindowHandle, out RECT clientRect))
                {
                    IsValid = false;
                    return;
                }

                ClientRect = clientRect;
                //Debug.WriteLine($"ClientRect: Left={clientRect.Left}, Top={clientRect.Top}, Right={clientRect.Right}, Bottom={clientRect.Bottom}");
                // 使用 double 存储精确的 DPI 缩放后尺寸
                _preciseCurrentResolution.Width = (clientRect.Right - clientRect.Left) * DpiScale;
                _preciseCurrentResolution.Height = (clientRect.Bottom - clientRect.Top) * DpiScale;
                Debug.WriteLine($"Precise CurrentResolution: Width={_preciseCurrentResolution.Width}, Height={_preciseCurrentResolution.Height}");
                CurrentResolution = new Size((int)Math.Round(_preciseCurrentResolution.Width), (int)Math.Round(_preciseCurrentResolution.Height));

                OriginalClientTopLeft = new POINT { X = clientRect.Left, Y = clientRect.Top };
                ClientToScreen(WindowHandle, ref OriginalClientTopLeft);
                Debug.WriteLine($"ClientTopLeft after ClientToScreen: X={OriginalClientTopLeft.X}, Y={OriginalClientTopLeft.Y}");
                // 使用 double 存储精确的 DPI 缩放后位置
                _preciseClientTopLeft.X = OriginalClientTopLeft.X * DpiScale;
                _preciseClientTopLeft.Y = OriginalClientTopLeft.Y * DpiScale;

                ClientTopLeft.X = (int)Math.Round(_preciseClientTopLeft.X);
                ClientTopLeft.Y = (int)Math.Round(_preciseClientTopLeft.Y);

                IsValid = true;
            }
            catch
            {
                IsValid = false;
            }
        }

        private IntPtr FindGameWindow()
        {
            // 支持多个可能的窗口标题
            string[] windowTitles = { "BloonsTD6", "BloonsTD6-Epic" };
            foreach (var title in windowTitles)
            {
                var hWnd = FindWindow(null, title);
                if (hWnd != IntPtr.Zero) return hWnd;
            }
            return IntPtr.Zero;
        }

        private double GetSystemDpi()
        {
            // 使用系统DPI
            //using (Graphics g = Graphics.FromHwnd(WindowHandle))
            //{
            //    return g.DpiX;
            //}
            return GetDpiForWindow(WindowHandle);
        }

        // 窗口坐标->屏幕坐标
        public Point ConvertGamePosition(Point basePoint)
        {
            var scaledX = basePoint.X * ResolutionScale;
            var scaledY = basePoint.Y * ResolutionScale;
            return new Point(
                (int)Math.Round(scaledX + _preciseClientTopLeft.X),
                (int)Math.Round(scaledY + _preciseClientTopLeft.Y)
            );
        }

        public (double x, double y) ConvertGamePosition(double x, double y)
        {
            double scaledX = x * ResolutionScale;
            double scaledY = y * ResolutionScale;
            return (scaledX + _preciseClientTopLeft.X, scaledY + _preciseClientTopLeft.Y);
        }

        // 屏幕坐标->窗口坐标
        public Point ConvertScreenPosition(Point screenPoint)
        {
            // 移除窗口左上角的偏移量
            double adjustedX = screenPoint.X - _preciseClientTopLeft.X;
            double adjustedY = screenPoint.Y - _preciseClientTopLeft.Y;

            // 根据分辨率缩放比例反向缩放
            double originalX = adjustedX / ResolutionScale;
            double originalY = adjustedY / ResolutionScale;

            return new Point((int)Math.Round(originalX), (int)Math.Round(originalY));
        }

        public (double x, double y) ConvertScreenPosition(double x, double y)
        {
            double adjustedX = x - _preciseClientTopLeft.X;
            double adjustedY = y - _preciseClientTopLeft.Y;
            double originalX = adjustedX / ResolutionScale;
            double originalY = adjustedY / ResolutionScale;
            return (originalX, originalY);
        }

        public Point ConvertToAbsolute(Point basePoint)
        {
            (double x, double y) = ConvertGamePosition(basePoint.X, basePoint.Y);

            return new Point(
                (int)Math.Round(x * 65535 / ScreenWidth),
                (int)Math.Round(y * 65535 / ScreenHeight)
            );
        }


        public override string ToString()
        {
            return $"游戏窗口上下文[Valid={IsValid} | Handle={WindowHandle} " +
                   $"| DPI={SystemDpi}({DpiScale:0.00}x) " +
                   $"| Res={CurrentResolution.Width}x{CurrentResolution.Height}({ResolutionScale:0.00}x) " +
                   $"| Pos=({ClientTopLeft.X},{ClientTopLeft.Y})]";
        }
    }
}
