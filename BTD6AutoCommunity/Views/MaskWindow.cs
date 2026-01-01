using BTD6AutoCommunity.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using POINT = BTD6AutoCommunity.Core.WindowApiWrapper.POINT;
namespace BTD6AutoCommunity.Views
{
    public class MaskWindow : Form
    {
        private static MaskWindow _instance;
        private readonly List<DrawableShape> _shapes = new List<DrawableShape>();
        private readonly object _shapeLock = new object();

        // Win32 API 常量，用于设置窗口样式
        private const int WS_EX_TRANSPARENT = 0x20;

        #region Drawable Shapes

        /// <summary>
        /// 可绘制形状的抽象基类
        /// </summary>
        private abstract class DrawableShape
        {
            public Timer Timer { get; }

            protected DrawableShape(Timer timer)
            {
                Timer = timer;
            }

            public abstract void Draw(Graphics g, Pen pen);
        }

        /// <summary>
        /// 用于管理单个十字准星信息的内部类
        /// </summary>
        private class Crosshair : DrawableShape
        {
            public Point Position { get; }

            public Crosshair(Point position, Timer timer) : base(timer)
            {
                Position = position;
            }

            public override void Draw(Graphics g, Pen pen)
            {
                int size = 12; // 十字准星的大小
                int gap = 2;   // 中心空隙的大小

                // 绘制左上到右下的对角线 (带中心间隙)
                g.DrawLine(pen, Position.X - size, Position.Y - size, Position.X - gap, Position.Y - gap); // 左上部分
                g.DrawLine(pen, Position.X + gap, Position.Y + gap, Position.X + size, Position.Y + size); // 右下部分

                // 绘制右上到左下的对角线 (带中心间隙)
                g.DrawLine(pen, Position.X + size, Position.Y - size, Position.X + gap, Position.Y - gap); // 右上部分
                g.DrawLine(pen, Position.X - gap, Position.Y + gap, Position.X - size, Position.Y + size); // 左下部分
            }
        }

        /// <summary>
        /// 用于管理单个矩形框信息的内部类
        /// </summary>
        private class RectangleBox : DrawableShape
        {
            public Rectangle Bounds { get; }

            public RectangleBox(Rectangle bounds, Timer timer) : base(timer)
            {
                Bounds = bounds;
            }

            public override void Draw(Graphics g, Pen pen)
            {
                g.DrawRectangle(pen, Bounds);
            }
        }

        #endregion

        // 私有构造函数，防止外部直接实例化
        private MaskWindow()
        {
            // 设置窗口样式
            FormBorderStyle = FormBorderStyle.None; // 无边框
            ShowInTaskbar = false;                  // 不在任务栏显示
            TopMost = true;                         // 始终保持在最顶层
            BackColor = Color.Wheat;                // 设置一个将要被透明化的背景色
            TransparencyKey = Color.Wheat;          // 将此颜色设置为透明
            StartPosition = FormStartPosition.Manual; // 手动设置位置

            // 启用双缓冲以减少闪烁
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// 重写 CreateParams 属性，添加 WS_EX_TRANSPARENT 样式
        /// 这使得窗口对鼠标事件透明，允许点击穿透
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TRANSPARENT;
                return cp;
            }
        }

        /// <summary>
        /// 获取 MaskWindow 的单例实例
        /// </summary>
        public static MaskWindow Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                {
                    _instance = new MaskWindow();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 根据 GameContext 的信息打开并定位遮罩窗口
        /// </summary>
        /// <param name="context">游戏上下文，包含窗口位置和大小信息</param>
        public static void Open(GameContext context)
        {
            if (Instance.InvokeRequired)
            {
                Instance.Invoke(new Action(() => ShowAndPosition(context)));
            }
            else
            {
                ShowAndPosition(context);
            }
        }

        private static void ShowAndPosition(GameContext context)
        {
            // 使用 ClientRect 的原始值来设置遮罩窗口的位置和大小
            // ClientRect 是未缩放的，与 WinForms 的 Bounds 属性单位一致
            Instance.Bounds = new Rectangle(
                context.OriginalClientTopLeft.X,
                context.OriginalClientTopLeft.Y,
                context.ClientRect.Right - context.ClientRect.Left,
                context.ClientRect.Bottom - context.ClientRect.Top
            );

            if (!Instance.Visible)
            {
                Instance.Show();
            }
        }

        /// <summary>
        /// 关闭遮罩窗口并清理所有资源
        /// </summary>
        public static void CloseWindow()
        {
            if (_instance != null && !_instance.IsDisposed)
            {
                if (Instance.InvokeRequired)
                {
                    Instance.Invoke(new Action(() => _instance.Close()));
                }
                else
                {
                    _instance.Close();
                }
            }
        }

        /// <summary>
        /// 重写 OnFormClosed 以确保所有计时器都被正确释放
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            lock (_shapeLock)
            {
                foreach (var shape in _shapes)
                {
                    shape.Timer.Stop();
                    shape.Timer.Dispose();
                }
                _shapes.Clear();
            }
            _instance = null;
            base.OnFormClosed(e);
        }

        /// <summary>
        /// 在指定位置显示一个十字准星，持续一段时间
        /// </summary>
        /// <param name="basePosition">游戏内的坐标</param>
        /// <param name="context">游戏上下文</param>
        public void ShowCrosshair(Point basePosition, GameContext context)
        {
            // 将游戏坐标转换为窗口内的相对坐标
            var windowPosition = new Point(
                (int)Math.Round(basePosition.X * context.ResolutionScale / context.DpiScale),
                (int)Math.Round(basePosition.Y * context.ResolutionScale / context.DpiScale)
            );

            var timer = new Timer { Interval = 100 };
            var crosshair = new Crosshair(windowPosition, timer);
            AddShape(crosshair);
        }

        /// <summary>
        /// 在指定位置显示一个矩形框，持续一段时间
        /// </summary>
        /// <param name="gameRectangle">游戏内的矩形区域</param>
        /// <param name="context">游戏上下文</param>
        public void ShowRectangle(Rectangle gameRectangle, GameContext context)
        {
            // 将游戏坐标转换为窗口内的相对坐标
            var windowRectangle = new Rectangle(
                (int)Math.Round(gameRectangle.X * context.ResolutionScale / context.DpiScale),
                (int)Math.Round(gameRectangle.Y * context.ResolutionScale / context.DpiScale),
                (int)Math.Round(gameRectangle.Width * context.ResolutionScale / context.DpiScale),
                (int)Math.Round(gameRectangle.Height * context.ResolutionScale / context.DpiScale)
            );

            var timer = new Timer { Interval = 100 };
            var rectangleBox = new RectangleBox(windowRectangle, timer);
            AddShape(rectangleBox);
        }

        private void AddShape(DrawableShape shape)
        {
            // 确保在UI线程上创建和管理形状
            Action addShapeAction = () =>
            {
                shape.Timer.Tick += (sender, args) =>
                {
                    shape.Timer.Stop();
                    lock (_shapeLock)
                    {
                        _shapes.Remove(shape);
                    }
                    shape.Timer.Dispose();
                    Invalidate(); // 触发重绘以移除形状
                };

                lock (_shapeLock)
                {
                    _shapes.Add(shape);
                }
                shape.Timer.Start();
                Invalidate(); // 触发重绘以显示新形状
            };

            if (InvokeRequired)
            {
                Invoke(addShapeAction);
            }
            else
            {
                addShapeAction();
            }
        }

        /// <summary>
        /// 重写 OnPaint 方法以绘制所有活动的形状
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            List<DrawableShape> currentShapes;
            lock (_shapeLock)
            {
                // 创建一个副本以避免在迭代时集合被修改
                currentShapes = _shapes.ToList();
            }

            if (currentShapes.Any())
            {
                using (var pen = new Pen(Color.Red, 2))
                {
                    foreach (var shape in currentShapes)
                    {
                        shape.Draw(e.Graphics, pen);
                    }
                }
            }
        }
    }
}