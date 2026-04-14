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
                typeof(TrackPlayer), new PropertyMetadata(false));

        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            set => SetValue(IsPlayingProperty, value);
        }

        public static readonly DependencyProperty WaveformColorProperty =
            DependencyProperty.Register(nameof(WaveformColor), typeof(Color),
                typeof(TrackPlayer), new PropertyMetadata(new Color()));

        public Color WaveformColor
        {
            get => (Color)GetValue(WaveformColorProperty);
            set => SetValue(WaveformColorProperty, value);
        }

        public static readonly DependencyProperty BarCountProperty =
            DependencyProperty.Register(nameof(BarCount), typeof(int),
                typeof(TrackPlayer), new PropertyMetadata(48, OnBarCountChanged));

        public int BarCount
        {
            get => (int)GetValue(BarCountProperty);
            set => SetValue(BarCountProperty, value);
        }

        private static void OnBarCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WaveformReactor wr && e.NewValue is int v)
            {
                lock (wr._lock)
                {
                    wr.barHeights = new float[v];
                }
            }
        }

        private object _lock = new object();
        private float[] barHeights = [];
        private float[] buffer = [];

        public WaveformReactor()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<float[]>(this, (o, m) =>
            {
                if (!IsPlaying)
                    return;
                lock(_lock)
                {
                    m.CopyTo(buffer, 0);
                    Dispatcher.InvokeAsync(() => CanvasView.InvalidateVisual());
                }
            });
        }

        private SKColor WpfToSkiasharpColor(Color color)
        {
            return new SKColor(color.R, color.G, color.B);
        }

        private void CanvasView_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            lock (_lock)
            {
                SKCanvas canvas = e.Surface.Canvas;
                canvas.Clear();

                // If not playing, do not repaint.
                if (!IsPlaying)
                    return;

                float[] fft = FastFourierTransform(buffer);

                int binsPerBar = fft.Length / 2 / BarCount;
                float[] targetHeights = new float[BarCount];

                for (int i = 0; i < BarCount; i++)
                {
                    float sum = 0;
                    for (int j = 0; j < binsPerBar; j++)
                    {
                        int binIndex = i * binsPerBar + j;
                        sum += fft[binIndex];
                    }
                    targetHeights[i] = sum / binsPerBar;
                }

                for (int i = 0; i < BarCount; i++)
                {
                    barHeights[i] = (barHeights[i] * 0.7f) + (targetHeights[i] * 0.3f);
                }

                // Draw
                float canvasWidth = e.Info.Width;
                float canvasHeight = e.Info.Height;
                float barWidth = canvasWidth / BarCount;
                float gap = barWidth * 0.2f;

                SKPaint skBrushSolid = new SKPaint()
                {
                    Color = WpfToSkiasharpColor(WaveformColor),
                    Style = SKPaintStyle.StrokeAndFill,
                    StrokeCap = SKStrokeCap.Square,
                    StrokeWidth = 1,
                    IsAntialias = false,
                };

                for (int i = 0; i < BarCount; i++)
                {
                    float barHeight = Math.Clamp(barHeights[i] * canvasHeight * 3f, 2f, canvasHeight);
                    float x = (i * barWidth) + gap / 2;
                    float y = (canvasHeight - barHeight) / 2; // centered vertically

                    canvas.DrawRoundRect(x, y, barWidth - gap, barHeight, 4, 4, skBrushSolid);
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
