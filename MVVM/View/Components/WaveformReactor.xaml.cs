using CommunityToolkit.Mvvm.Messaging;
using SimpleSoundtrackManager.MVVM.Model.Services;
using SkiaSharp;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleSoundtrackManager.MVVM.View.Components
{
    /// <summary>
    /// Interaction logic for WaveformReactor.xaml
    /// </summary>
    public partial class WaveformReactor : UserControl
    {
        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register(nameof(IsPlaying), typeof(bool),
                typeof(WaveformReactor), new PropertyMetadata(false));

        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            set => SetValue(IsPlayingProperty, value);
        }

        public static readonly DependencyProperty WaveformColorProperty =
            DependencyProperty.Register(nameof(WaveformColor), typeof(Color),
                typeof(WaveformReactor), new PropertyMetadata(new Color()));

        public Color WaveformColor
        {
            get => (Color)GetValue(WaveformColorProperty);
            set => SetValue(WaveformColorProperty, value);
        }

        private object _lock = new object();
        private float[] barHeights = [];
        private float[] buffer = [];

        public WaveformReactor()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<float[]>(this, (o, m) =>
            {
                float[] copy = [.. m];
                
                Dispatcher.InvokeAsync(() =>
                {
                    if (!IsPlaying)
                        return;
                    buffer = copy;
                    CanvasView.InvalidateVisual();
                });
            });

            Unloaded += WaveformReactor_Unloaded;
        }

        private void WaveformReactor_Unloaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Unregister<float[]>(this);
            Unloaded -= WaveformReactor_Unloaded;
        }

        private SKColor WpfToSkiasharpColor(Color color)
        {
            return new SKColor(color.R, color.G, color.B);
        }

        private void CanvasView_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();

            // If not playing, do not repaint.
            if (!IsPlaying)
                return;
            lock (_lock)
            {
                int width = e.Info.Width;
                float[] fft = FastFourierTransform(buffer);

                if (barHeights.Length != width)
                {
                    barHeights = new float[width];
                }

                int binsPerBar = fft.Length / 2 / width;
                float[] targetHeights = new float[width];

                for (int i = 0; i < width; i++)
                {
                    float sum = 0;
                    for (int j = 0; j < binsPerBar; j++)
                    {
                        int binIndex = i * binsPerBar + j;
                        sum += fft[binIndex];
                    }
                    targetHeights[i] = sum / binsPerBar;
                }

                float maxSample = 0;
                for (int i = 0; i < width; i++)
                {
                    float val = (barHeights[i] * 0.8f) + (targetHeights[i] * 0.2f);
                    barHeights[i] = val;
                    if (val > maxSample)
                        maxSample = val;
                }

                // Draw
                float canvasWidth = e.Info.Width;
                float canvasHeight = e.Info.Height;

                int desiredBarWidth = 1;
                int gap = 3;
                float barStep = desiredBarWidth + gap;
                int effectiveBars = (int)(canvasWidth / barStep);

                using SKPaint skBrushSolid = new SKPaint()
                {
                    Color = WpfToSkiasharpColor(WaveformColor),
                    Style = SKPaintStyle.StrokeAndFill,
                    StrokeCap = SKStrokeCap.Square,
                    StrokeWidth = 1,
                    IsAntialias = false,
                };

                for (int i = 0; i < effectiveBars; i++)
                {
                    float barHeight = e.Info.Height * (barHeights[i] / maxSample);
                    float x = i * barStep;
                    float y = (canvasHeight - barHeight) / 2f;

                    canvas.DrawRect(SKRect.Create(x, y, desiredBarWidth, barHeight), skBrushSolid);
                }
            }
        }

        private float[] FastFourierTransform(float[] samples)
        {
            int n = samples.Length;
            Complex[] complex = new Complex[n];

            // Apply Hann window to reduce spectral leakage
            for (int i = 0; i < n; i++)
            {
                double window = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (n - 1)));
                complex[i] = new Complex(samples[i] * window, 0);
            }

            FFTRecursive(complex);

            // Return magnitudes
            return [.. complex.Take(n / 2).Select(c => (float)c.Magnitude)];
        }

        private void FFTRecursive(Complex[] x)
        {
            int n = x.Length;
            if (n <= 1) return;

            Complex[] even = x.Where((_, i) => i % 2 == 0).ToArray();
            Complex[] odd = x.Where((_, i) => i % 2 != 0).ToArray();

            FFTRecursive(even);
            FFTRecursive(odd);

            for (int k = 0; k < n / 2; k++)
            {
                Complex t = Complex.FromPolarCoordinates(1, -2 * Math.PI * k / n) * odd[k];
                x[k] = even[k] + t;
                x[k + (n / 2)] = even[k] - t;
            }
        }
    }
}
