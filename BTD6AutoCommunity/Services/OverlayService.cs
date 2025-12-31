using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace BTD6AutoCommunity.Services
{
    public sealed class OverlayService : IDisposable
    {
        // --- Singleton Implementation ---
        private static readonly Lazy<OverlayService> lazy = new Lazy<OverlayService>(() => new OverlayService());
        public static OverlayService Instance => lazy.Value;

        public static bool IsEnabled { get; private set; } = false;

        private OverlayForm _overlayForm;
        private Thread _overlayThread;
        private CancellationTokenSource _cts;
        private readonly ConcurrentDictionary<Guid, TimedElement> _elements = new ConcurrentDictionary<Guid, TimedElement>();

        // Private constructor to prevent direct instantiation
        private OverlayService() { }

        // --- Public Static Control Methods ---
        public static void Enable()
        {
            if (IsEnabled) return;
            IsEnabled = true;
            Instance.Start();
        }

        public static void Disable()
        {
            if (!IsEnabled) return;
            IsEnabled = false;
            Instance.Stop();
        }

        // --- Public Static Drawing Methods ---
        public static void DrawCrosshair(Point center, Color color, int size, int gap, int durationMs)
        {
            if (!IsEnabled) return;
            Instance.InternalDrawCrosshair(center, color, size, gap, durationMs);
        }

        // --- Instance Methods (Internal Logic) ---
        private void Start()
        {
            if (_overlayThread != null) return;

            _cts = new CancellationTokenSource();
            _overlayThread = new Thread(() =>
            {
                _overlayForm = new OverlayForm();
                _overlayForm.Show();

                while (!_cts.Token.IsCancellationRequested)
                {
                    var now = DateTime.UtcNow;
                    var expiredIds = _elements.Where(kvp => kvp.Value.Expiry < now).Select(kvp => kvp.Key).ToList();

                    if (expiredIds.Any())
                    {
                        foreach (var id in expiredIds)
                        {
                            _elements.TryRemove(id, out _);
                        }
                        _overlayForm.UpdateElements(_elements.Values.ToList());
                    }

                    Thread.Sleep(50); // Refresh rate
                }

                _overlayForm.Close();
                _overlayForm.Dispose();
            });

            _overlayThread.SetApartmentState(ApartmentState.STA);
            _overlayThread.Start();
        }

        private void Stop()
        {
            Clear();
            _cts?.Cancel();
            _overlayThread?.Join();
            _overlayThread = null;
            _cts?.Dispose();
            _cts = null;
        }

        private void Clear()
        {
            _elements.Clear();
            _overlayForm?.UpdateElements(new List<TimedElement>());
        }

        private void InternalDrawCrosshair(Point center, Color color, int size, int gap, int durationMs)
        {
            var expiry = DateTime.UtcNow.AddMilliseconds(durationMs);

            // Horizontal line (left part)
            _elements.TryAdd(Guid.NewGuid(), new TimedElement(ElementType.Line, new Point(center.X - size, center.Y), new Point(center.X - gap, center.Y), color, 1, expiry));
            // Horizontal line (right part)
            _elements.TryAdd(Guid.NewGuid(), new TimedElement(ElementType.Line, new Point(center.X + gap, center.Y), new Point(center.X + size, center.Y), color, 1, expiry));
            // Vertical line (top part)
            _elements.TryAdd(Guid.NewGuid(), new TimedElement(ElementType.Line, new Point(center.X, center.Y - size), new Point(center.X, center.Y - gap), color, 1, expiry));
            // Vertical line (bottom part)
            _elements.TryAdd(Guid.NewGuid(), new TimedElement(ElementType.Line, new Point(center.X, center.Y + gap), new Point(center.X, center.Y + size), color, 1, expiry));

            _overlayForm?.UpdateElements(_elements.Values.ToList());
        }

        public void Dispose()
        {
            if (IsEnabled)
            {
                Stop();
            }
        }

        // --- Nested Types ---
        internal class TimedElement
        {
            public ElementType Type { get; }
            public Point Location { get; }
            public Point EndLocation { get; }
            public Color Color { get; }
            public int Thickness { get; }
            public DateTime Expiry { get; }

            public TimedElement(ElementType type, Point start, Point end, Color color, int thickness, DateTime expiry)
            {
                Type = type;
                Location = start;
                EndLocation = end;
                Color = color;
                Thickness = thickness;
                Expiry = expiry;
            }
        }

        internal enum ElementType
        {
            Line
        }

        private class OverlayForm : Form
        {
            private List<TimedElement> _currentElements = new List<TimedElement>();

            public OverlayForm()
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.TopMost = true;
                this.BackColor = Color.Magenta;
                this.TransparencyKey = Color.Magenta;
                this.DoubleBuffered = true;
            }

            public void UpdateElements(List<TimedElement> elements)
            {
                _currentElements = elements;
                this.Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (_currentElements == null || !_currentElements.Any()) return;

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                foreach (var element in _currentElements)
                {
                    using (var pen = new Pen(element.Color, element.Thickness))
                    {
                        if (element.Type == ElementType.Line)
                        {
                            e.Graphics.DrawLine(pen, element.Location, element.EndLocation);
                        }
                    }
                }
            }
        }
    }
}