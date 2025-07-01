using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static BTD6AutoCommunity.Core.WindowApiWrapper;

namespace BTD6AutoCommunity.Core
{
    public class GameContext
    {
        // 窗口相关参数
        public IntPtr WindowHandle { get; set; }
        public RECT ClientRect { get; set; }
        public POINT ClientTopLeft;

        // 分辨率相关
        public Size BaseResolution { get; } = new Size(1920, 1080);
        public Size CurrentResolution { get; set; }
        public double ResolutionScale => CurrentResolution.Width / (double)BaseResolution.Width;

        // DPI相关
        public double SystemDpi { get; set; }
        public double DpiScale => SystemDpi / 96.0; // 标准DPI为96

        // 初始化状态
        public bool IsValid { get; set; }


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

                // 获取窗口信息
                if (!GetClientRect(WindowHandle, out RECT rect))
                {
                    IsValid = false;
                    return;
                }

                ClientRect = rect;
                CurrentResolution = new Size((int)((rect.Right - rect.Left) * DpiScale), (int)((rect.Bottom - rect.Top) * DpiScale));
                Debug.WriteLine($"CurrentResolution: {CurrentResolution.Width}");
                ClientTopLeft = new POINT { X = rect.Left, Y = rect.Top };
                ClientToScreen(WindowHandle, ref ClientTopLeft);
                ClientTopLeft.X = (int)(ClientTopLeft.X * DpiScale);
                ClientTopLeft.Y = (int)(ClientTopLeft.Y * DpiScale);
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
            var scaledX = (int)(basePoint.X * ResolutionScale);
            //Debug.WriteLine($"ResolutionScale: {ResolutionScale} DpiScale: {DpiScale}");
            var scaledY = (int)(basePoint.Y * ResolutionScale);
            return new Point(
                scaledX + ClientTopLeft.X,
                scaledY + ClientTopLeft.Y
            );
        }

        // 屏幕坐标->窗口坐标
        public Point ConvertScreenPosition(Point screenPoint)
        {
            // 移除窗口左上角的偏移量
            var adjustedX = screenPoint.X - ClientTopLeft.X;
            var adjustedY = screenPoint.Y - ClientTopLeft.Y;

            // 根据分辨率缩放比例反向缩放
            var originalX = (int)(adjustedX / ResolutionScale);
            var originalY = (int)(adjustedY / ResolutionScale);

            return new Point(originalX, originalY);
        }

        public Point ConvertToAbsolute(Point basePoint)
        {
            var point = ConvertGamePosition(basePoint);
            //Debug.WriteLine($"PointX: {point.X} PointY: {point.Y}");
            int screenWidth = (int)(GetSystemMetrics(0) * DpiScale);
            int screenHeight = (int)(GetSystemMetrics(1) * DpiScale);
            //Debug.WriteLine($"ScreenWidth: {screenWidth} ScreenHeight: {screenHeight}");
            return new Point(
                point.X * 65535 / screenWidth,
                point.Y * 65535 / screenHeight
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
