using BTD6AutoCommunity.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
namespace BTD6AutoCommunity.Views
{
    public class MaskWindow : Form
    {
        private static MaskWindow _instance;
        private readonly List<DrawableShape> _shapes = new List<DrawableShape>();
        private readonly object _shapeLock = new object();

        // Win32 API 常量，用于设置窗口样式
        private const int WS_EX_TRANSPARENT = 0x20;

        private GameContext _gameContext;
        private Timer _mousePollTimer;
        private Point _currentMousePosition;
        private bool _isMouseInBounds = false;
        private bool _showMouseCoordinates = false;
        private bool _showShapes = true; // 新增：控制是否绘制图形


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
            Instance._gameContext = context;
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
        /// 更新遮罩窗口的位置以匹配游戏窗口。
        /// 如果游戏窗口移动或大小改变，可以调用此方法。
        /// </summary>
        public void UpdatePosition()
        {
            if (_gameContext == null) return;

            // 在UI线程上执行更新
            Action updateAction = () =>
            {
                _gameContext.Update(); // 刷新游戏窗口的上下文信息
                if (_gameContext.IsValid)
                {
                    // 使用更新后的上下文重新设置窗口边界
                    Bounds = new Rectangle(
                        _gameContext.OriginalClientTopLeft.X,
                        _gameContext.OriginalClientTopLeft.Y,
                        _gameContext.ClientRect.Right - _gameContext.ClientRect.Left,
                        _gameContext.ClientRect.Bottom - _gameContext.ClientRect.Top
                    );
                }
                else
                {
                    // 如果游戏窗口找不到了，则关闭遮罩窗口
                    Close();
                }
            };

            if (InvokeRequired)
            {
                Invoke(updateAction);
            }
            else
            {
                updateAction();
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
            // 确保坐标显示计时器被停止和释放
            ToggleMouseCoordinateDisplay(false);
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
            // 在添加新形状之前，确保窗口位置是最新的
            UpdatePosition();

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
        /// 获取鼠标在窗口内的相对位置。
        /// </summary>
        /// <returns>相对于窗口左上角的坐标点。</returns>
        public Point GetMousePositionInWindow()
        {
            // 使用 PointToClient 将屏幕坐标转换为窗口客户端坐标
            return PointToClient(MousePosition);
        }

        /// <summary>
        /// 获取鼠标在游戏内的坐标。
        /// </summary>
        /// <param name="context">游戏上下文，用于坐标转换。</param>
        /// <returns>游戏内的坐标点。</returns>
        public (double, double) GetMousePositionInGame(GameContext context)
        {
            // 获取鼠标在窗口中的相对位置
            Point windowPosition = GetMousePositionInWindow();

            // 将窗口坐标转换回游戏坐标
            // 这是 ShowCrosshair/ShowRectangle 中坐标转换的逆过程
            double gameX = windowPosition.X * context.DpiScale / context.ResolutionScale;
            double gameY = windowPosition.Y * context.DpiScale / context.ResolutionScale;

            return (gameX, gameY);
        }

        /// <summary>
        /// 切换图形的显示状态。
        /// </summary>
        /// <param name="show">为 true 则显示图形，为 false 则隐藏。</param>
        public void ToggleShapeDisplay(bool show)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ToggleShapeDisplay(show)));
                return;
            }
            _showShapes = show;
            Invalidate(); // 触发重绘以立即应用更改
        }

        /// <summary>
        /// 切换鼠标坐标的显示状态。
        /// </summary>
        /// <param name="show">为 true 则显示坐标，为 false 则隐藏。</param>
        public void ToggleMouseCoordinateDisplay(bool show)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ToggleMouseCoordinateDisplay(show)));
                return;
            }

            _showMouseCoordinates = show;

            if (_showMouseCoordinates)
            {
                if (_mousePollTimer == null)
                {
                    _mousePollTimer = new Timer { Interval = 50 }; // 每 50ms 更新一次
                    _mousePollTimer.Tick += PollMousePosition;
                    _mousePollTimer.Start();
                }
            }
            else
            {
                if (_mousePollTimer != null)
                {
                    _mousePollTimer.Stop();
                    _mousePollTimer.Dispose();
                    _mousePollTimer = null;
                }
                // 如果之前鼠标在窗口内，触发一次重绘来清除坐标
                if (_isMouseInBounds)
                {
                    _isMouseInBounds = false;
                    Invalidate();
                }
            }
        }

        private void PollMousePosition(object sender, EventArgs e)
        {
            if (_gameContext == null) return;

            // 每次轮询时都更新窗口位置，以防游戏窗口移动
            UpdatePosition();

            Point screenPos = MousePosition;
            bool isCurrentlyInBounds = Bounds.Contains(screenPos);

            // 只有当鼠标进入/离开窗口或在窗口内移动时才触发重绘
            if (isCurrentlyInBounds || _isMouseInBounds != isCurrentlyInBounds)
            {
                _isMouseInBounds = isCurrentlyInBounds;
                _currentMousePosition = PointToClient(screenPos);
                Invalidate(); // 触发 OnPaint
            }
        }


        /// <summary>
        /// 重写 OnPaint 方法以绘制所有活动的形状
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 如果启用了图形显示，则绘制所有图形
            if (_showShapes)
            {
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

            // 如果启用了坐标显示并且鼠标在窗口内
            if (_showMouseCoordinates && _isMouseInBounds && _gameContext != null)
            {
                (double X, double Y) gamePos = GetMousePositionInGame(_gameContext);
                // 添加新的一行文本
                string text = $"({gamePos.X}, {gamePos.Y})\nEnter自动输入";

                using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
                using (var backgroundBrush = new SolidBrush(Color.FromArgb(200, 30, 30, 30))) // 半透明深灰背景
                using (var textBrush = new SolidBrush(Color.White)) // 白色文字
                {
                    const int offset = 15; // 文本框与鼠标指针的距离
                    SizeF textSize = e.Graphics.MeasureString(text, font);

                    // --- 智能定位逻辑 ---
                    // 默认位置在右下
                    float x = _currentMousePosition.X + offset;
                    float y = _currentMousePosition.Y + offset;

                    // 如果右侧空间不足，则移动到左侧
                    if (x + textSize.Width > ClientSize.Width)
                    {
                        x = _currentMousePosition.X - textSize.Width - offset;
                    }

                    // 如果下方空间不足，则移动到上方
                    if (y + textSize.Height > ClientSize.Height)
                    {
                        y = _currentMousePosition.Y - textSize.Height - offset;
                    }
                    
                    PointF textPosition = new PointF(x, y);
                    RectangleF backgroundRect = new RectangleF(textPosition, new SizeF(textSize.Width + 4, textSize.Height + 4));

                    e.Graphics.FillRectangle(backgroundBrush, backgroundRect);
                    e.Graphics.DrawString(text, font, textBrush, textPosition.X + 2, textPosition.Y + 2);
                }
            }
        }
    }
}