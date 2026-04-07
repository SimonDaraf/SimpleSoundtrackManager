using SimpleSoundtrackManager.MVVM.View.CustomWindows.SettingsWindow;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleSoundtrackManager.MVVM.View.Components
{
    /// <summary>
    /// AI generated slop :)
    /// </summary>
    public partial class ColorPickerWindow : SettingsApplicationWindow
    {
        private Color? _initialColor;
        private bool _initialized = false;

        public SKColor SelectedColor { get; private set; } = SKColors.White;
        public Color WpfColor { get => Color.FromRgb(SelectedColor.Red, SelectedColor.Green, SelectedColor.Blue); }

        private bool _isDragging;
        private SKPoint _cursorPos = new(100, 100);

        public ColorPickerWindow(Color? initialColor)
        {
            InitializeComponent();
            _initialColor = initialColor;
            
        }

        private void ColorCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var width = e.Info.Width;
            var height = e.Info.Height;

            canvas.Clear();

            // Draw hue gradient (left to right)
            DrawGradient(canvas, width, height);

            if (!_initialized && _initialColor.HasValue)
            {
                _initialized = true;
                _cursorPos = FindColorPosition(_initialColor.Value, width, height);
            }

            // Sample color at cursor
            using var bmp = new SKBitmap(width, height);
            using var offscreen = new SKCanvas(bmp);
            DrawGradient(offscreen, width, height);
            int px = Math.Clamp((int)_cursorPos.X, 0, width - 1);
            int py = Math.Clamp((int)_cursorPos.Y, 0, height - 1);
            SelectedColor = bmp.GetPixel(px, py);

            // 4. Draw cursor circle
            using var circlePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White,
                StrokeWidth = 2,
                IsAntialias = true
            };
            canvas.DrawCircle(_cursorPos.X, _cursorPos.Y, 8, circlePaint);
            circlePaint.Color = SKColors.Black;
            circlePaint.StrokeWidth = 1;
            canvas.DrawCircle(_cursorPos.X, _cursorPos.Y, 9, circlePaint);

            PreviewBorder.Background = new SolidColorBrush(WpfColor);
        }

        private SKPoint FindColorPosition(Color color, int width, int height)
        {
            using var bmp = new SKBitmap(width, height);
            // render gradient to bitmap and find closest match
            using var offscreen = new SKCanvas(bmp);
            DrawGradient(offscreen, width, height);

            float bestDist = float.MaxValue;
            SKPoint bestPos = new(width / 2f, height / 2f);

            // Sample a grid to find closest color
            for (int x = 0; x < width; x += 2)
            {
                for (int y = 0; y < height; y += 2)
                {
                    var pixel = bmp.GetPixel(x, y);
                    float dist = ColorDistance(pixel, color);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestPos = new SKPoint(x, y);
                    }
                }
            }
            return bestPos;
        }

        private float ColorDistance(SKColor a, Color b)
        {
            float dr = a.Red - b.R;
            float dg = a.Green - b.G;
            float db = a.Blue - b.B;
            return dr * dr + dg * dg + db * db;
        }

        private void DrawGradient(SKCanvas canvas, int width, int height)
        {
            using var hueShader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0), new SKPoint(width, 0),
                new[]
                {
                    SKColor.FromHsl(0, 100, 50),
                    SKColor.FromHsl(60, 100, 50),
                    SKColor.FromHsl(120, 100, 50),
                    SKColor.FromHsl(180, 100, 50),
                    SKColor.FromHsl(240, 100, 50),
                    SKColor.FromHsl(300, 100, 50),
                    SKColor.FromHsl(360, 100, 50),
                },
                SKShaderTileMode.Clamp);

            using var whiteShader = SKShader.CreateLinearGradient(
                new SKPoint(width, 0), new SKPoint(width, height / 4),
                new[] { SKColors.White, new SKColor(255, 255, 255, 0) },
                SKShaderTileMode.Clamp);

            using var blackShader = SKShader.CreateLinearGradient(
                new SKPoint(0, height / 2), new SKPoint(0, height),
                new[] { new SKColor(0, 0, 0, 0), SKColors.Black },
                SKShaderTileMode.Clamp);

            using var huePaint = new SKPaint { Shader = hueShader };
            using var whitePaint = new SKPaint { Shader = whiteShader };
            using var blackPaint = new SKPaint { Shader = blackShader };

            canvas.DrawRect(0, 0, width, height, huePaint);
            canvas.DrawRect(0, 0, width, height, whitePaint);
            canvas.DrawRect(0, 0, width, height, blackPaint);
        }

        private void ColorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            UpdateCursor(e.GetPosition(ColorCanvas));
            ((UIElement)sender).CaptureMouse();
        }

        private void ColorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            UpdateCursor(e.GetPosition(ColorCanvas));
        }

        private void ColorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }

        private void UpdateCursor(System.Windows.Point pos)
        {
            float x = Math.Clamp((float)pos.X, 0, (float)ColorCanvas.ActualWidth);
            float y = Math.Clamp((float)pos.Y, 0, (float)ColorCanvas.ActualHeight);
            _cursorPos = new SKPoint(x, y);
            ColorCanvas.InvalidateVisual();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
