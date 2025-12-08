using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.Core
{
    /// <summary>
    /// 负责与屏幕截取相关的功能：按游戏窗口区域或基于游戏坐标的区域截屏，
    /// 提供线程安全的内部缓存并负责释放旧 Bitmap，支持返回 Bitmap 副本或 OpenCvSharp.Mat。
    /// </summary>
    public class ScreenCapturer : IDisposable
    {
        private readonly GameContext _context;
        // 私有缓存，使用 Interlocked.Exchange 原子替换并释放旧对象
        private Bitmap _screenshot;
        private bool _disposed;

        public ScreenCapturer(GameContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// 当前缓存的截图（只读）。注意：不要直接对该实例调用 Dispose/修改，使用 GetScreenshotClone 安全获取副本。
        /// </summary>
        public Bitmap CurrentScreenshot => _screenshot;

        /// <summary>
        /// 捕获整个游戏窗口（基于 GameContext.CurrentResolution / ClientTopLeft）。
        /// 返回一个 Bitmap 副本（调用者负责 Dispose）。
        /// 内部也会原子替换保存最新截图并释放旧的缓存。
        /// </summary>
        public Bitmap CaptureFullAndGetClone()
        {
            EnsureNotDisposed();

            if (!_context.IsValid) return null;
            var screenArea = new Rectangle(
                _context.ClientTopLeft.X,
                _context.ClientTopLeft.Y,
                _context.CurrentResolution.Width,
                _context.CurrentResolution.Height
            );

            return CaptureAndSwap(screenArea);
        }

        /// <summary>
        /// 捕获基于游戏逻辑坐标（base coordinates）的矩形区域（坐标以 BaseResolution 为基准）。
        /// 返回 Bitmap 副本（调用者负责 Dispose）。
        /// </summary>
        public Bitmap CaptureGameRegionAndGetClone(Rectangle baseRect)
        {
            EnsureNotDisposed();

            if (!_context.IsValid) return null;

            // 将 baseRect（游戏坐标）转换为屏幕坐标区域
            var topLeft = _context.ConvertGamePosition(new System.Drawing.Point(baseRect.X, baseRect.Y));
            var scaledWidth = (int)(baseRect.Width * _context.ResolutionScale);
            var scaledHeight = (int)(baseRect.Height * _context.ResolutionScale);
            var screenArea = new Rectangle(topLeft.X, topLeft.Y, scaledWidth, scaledHeight);

            return CaptureAndSwap(screenArea);
        }

        /// <summary>
        /// 捕获整个游戏窗口并返回 OpenCvSharp.Mat（调用者负责 Dispose Mat）。
        /// </summary>
        public Mat CaptureFullAsMat()
        {
            var bmp = CaptureFullAndGetClone();
            if (bmp == null) return null;
            try
            {
                return BitmapConverter.ToMat(bmp);
            }
            finally
            {
                bmp.Dispose();
            }
        }

        /// <summary>
        /// 捕获基于游戏逻辑坐标的区域并返回 Mat（调用者负责 Dispose）。
        /// </summary>
        public Mat CaptureGameRegionAsMat(Rectangle baseRect)
        {
            var bmp = CaptureGameRegionAndGetClone(baseRect);
            if (bmp == null) return null;
            try
            {
                return BitmapConverter.ToMat(bmp);
            }
            finally
            {
                bmp.Dispose();
            }
        }

        /// <summary>
        /// 获取当前缓存截图的副本（调用者负责 Dispose）。
        /// </summary>
        public Bitmap GetScreenshotClone()
        {
            var bmp = _screenshot;
            if (bmp == null) return null;
            try
            {
                return (Bitmap)bmp.Clone();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 执行实际截屏并原子替换内部缓存，返回一个 Bitmap 副本（调用者负责 Dispose）。
        /// </summary>
        private Bitmap CaptureAndSwap(Rectangle screenArea)
        {
            // 防御性检查
            if (screenArea.Width <= 0 || screenArea.Height <= 0) return null;

            Bitmap newBmp = null;
            try
            {
                newBmp = new Bitmap(screenArea.Width, screenArea.Height);
                using (var g = Graphics.FromImage(newBmp))
                {
                    g.CopyFromScreen(screenArea.Location, System.Drawing.Point.Empty, screenArea.Size);
                }

                // 原子替换缓存并释放旧的 bitmap
                var old = Interlocked.Exchange(ref _screenshot, newBmp);
                if (old != null)
                {
                    try { old.Dispose(); } catch { /* 忽略释放异常 */ }
                }

                // 返回副本，避免调用者误释放内部缓存
                return (Bitmap)newBmp.Clone();
            }
            catch
            {
                // 若创建或截取过程中抛出，确保释放新建但未缓存的 bitmap
                try { newBmp?.Dispose(); } catch { }
                throw;
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ScreenCapturer));
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                var old = Interlocked.Exchange(ref _screenshot, null);
                try { old?.Dispose(); } catch { }
            }
            _disposed = true;
        }

        ~ScreenCapturer()
        {
            Dispose(false);
        }
        #endregion
    }
}
