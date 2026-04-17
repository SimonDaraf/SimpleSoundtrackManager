using CommunityToolkit.Mvvm.Messaging;
using SimpleSoundtrackManager.MVVM.Model.Audio;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;
using SkiaSharp;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

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

        public static readonly DependencyProperty ChannelCountProperty =
        DependencyProperty.Register(nameof(ChannelCount), typeof(int),
            typeof(WaveformReactor), new PropertyMetadata(1));

        public int ChannelCount
        {
            get => (int)GetValue(ChannelCountProperty);
            set => SetValue(ChannelCountProperty, value);
        }

        public static readonly DependencyProperty TrackProperty =
        DependencyProperty.Register(nameof(Track), typeof(Track),
            typeof(WaveformReactor), new PropertyMetadata(default(Track)));

        public Track Track
        {
            get => (Track)GetValue(TrackProperty);
            set => SetValue(TrackProperty, value);
        }

        public static readonly DependencyProperty SampleRateProperty =
        DependencyProperty.Register(nameof(SampleRate), typeof(int),
            typeof(WaveformReactor), new PropertyMetadata(44100));

        public int SampleRate
        {
            get => (int)GetValue(SampleRateProperty);
            set => SetValue(SampleRateProperty, value);
        }

        private const int FftSize = 8192;
        private float[] barHeights = [];

        private readonly double[] fftReal = new double[FftSize];
        private readonly double[] fftImag = new double[FftSize];
        private readonly float[] fftMagnitudes = new float[FftSize / 2];

        private float highestPeak = 1;
        private const float DecayPerFrame = 0.95f;
        float[] lastFrame = [];
        private readonly Queue<float[]> bufferQueue;
        private readonly DispatcherTimer renderTimer;
        private readonly Stopwatch stopwatch;
        private long lastElaspedTicks;

        public WaveformReactor()
        {
            InitializeComponent();
            stopwatch = Stopwatch.StartNew();
            renderTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromSeconds(1.0 / 60.0)
            };
            renderTimer.Tick += RenderTimer_Tick;
            renderTimer.Start();
            
            bufferQueue = new Queue<float[]>();
            WeakReferenceMessenger.Default.Register<TrackBufferUpdatedEventArgs>(this, (o, m) =>
            {   
                Dispatcher.InvokeAsync(() =>
                {
                    // We want to constantly provide newer samples, if for some reason we are behind, clear and start with newer ones.
                    bufferQueue.Clear();

                    if (!m.TrackName.Equals(Track.Name) && !m.TrackName.Equals(Track.FilePath))
                        return;

                    int samplesPerSecond = SampleRate / 60 * ChannelCount;

                    int i = 0;
                    while (i + samplesPerSecond <= m.Buffer.Length)
                    {
                        float[] frameBuffer = new float[samplesPerSecond];
                        Array.Copy(m.Buffer, i, frameBuffer, 0, samplesPerSecond);
                        bufferQueue.Enqueue(frameBuffer);
                        i += samplesPerSecond;
                    }
                });
            });

            Unloaded += WaveformReactor_Unloaded;
        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            long now = stopwatch.ElapsedTicks;
            double elapsed = (double)(now - lastElaspedTicks) / Stopwatch.Frequency;
            lastElaspedTicks = now;

            int framesToConsume = (int)Math.Round(elapsed * 60);

            for (int i = 0; i < framesToConsume; i++)
            {
                if (bufferQueue.TryDequeue(out float[]? buffer) && buffer is not null)
                    lastFrame = buffer;
                else lastFrame = [];
            }

            CanvasView.InvalidateVisual();
        }

        private void WaveformReactor_Unloaded(object sender, RoutedEventArgs e)
        {
            renderTimer.Stop();
            WeakReferenceMessenger.Default.Unregister<float[]>(this);
            Unloaded -= WaveformReactor_Unloaded;
            renderTimer.Tick -= RenderTimer_Tick;
        }

        private SKColor WpfToSkiasharpColor(Color color)
        {
            return new SKColor(color.R, color.G, color.B);
        }

        private void CanvasView_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();

            int width = e.Info.Width;
            if (barHeights.Length != width)
                barHeights = new float[width];

            bool hasNewAudio = lastFrame.Length > 0;
            if (hasNewAudio)
            {
                float[] fft = RunFft(lastFrame);
                int usableBins = fft.Length;
                float binsPerBar = (float)usableBins / width;

                for (int i = 0; i < width; i++)
                {
                    int binStart = (int)(i * binsPerBar);
                    int binEnd = Math.Min((int)((i + 1) * binsPerBar), usableBins);
                    if (binEnd <= binStart) binEnd = binStart + 1;

                    float sum = 0;
                    for (int j = binStart; j < binEnd; j++)
                        sum += fft[j];

                    float target = sum / (binEnd - binStart);
                    float val = (barHeights[i] * 0.4f) + (target * 0.6f);
                    barHeights[i] = val;
                    if (val > highestPeak) highestPeak = val;
                }

                DrawBars(canvas, e.Info);
            }
            else
            {
                for (int i = 0; i < width; i++)
                {
                    float val = MathF.Max(barHeights[i] * DecayPerFrame, 0.0001f);
                    barHeights[i] = val;
                    if (val > highestPeak) highestPeak = val;
                }

                DrawBars(canvas, e.Info);
            }

            lastFrame = [];
        }

        private void DrawBars(SKCanvas canvas, SkiaSharp.SKImageInfo info)
        {
            if (highestPeak == 0)
                return;

            float canvasWidth = info.Width;
            float canvasHeight = info.Height;

            const int desiredBarWidth = 1;
            const int gap = 3;
            const float barStep = desiredBarWidth + gap;
            int effectiveBars = Math.Min((int)(canvasWidth / barStep), barHeights.Length);

            using SKPaint paint = new SKPaint
            {
                Color = WpfToSkiasharpColor(WaveformColor),
                Style = SKPaintStyle.StrokeAndFill,
                StrokeCap = SKStrokeCap.Square,
                StrokeWidth = 1,
                IsAntialias = false,
            };

            for (int i = 0; i < effectiveBars; i++)
            {
                float barHeight = canvasHeight * (barHeights[i] / highestPeak);
                float x = i * barStep;
                float y = (canvasHeight - barHeight) / 2f;
                canvas.DrawRect(SKRect.Create(x, y, desiredBarWidth, barHeight), paint);
            }
        }

        private float[] RunFft(float[] samples)
        {
            int channels = Math.Max(1, ChannelCount);
            int availableFrames = samples.Length / channels;
            int frames = Math.Min(availableFrames, FftSize);

            for (int i = 0; i < frames; i++)
            {
                double window = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (frames - 1)));
                fftReal[i] = samples[i * channels] * window;
                fftImag[i] = 0.0;
            }
            for (int i = frames; i < FftSize; i++)
            {
                fftReal[i] = 0.0;
                fftImag[i] = 0.0;
            }

            FftIterative(fftReal, fftImag);

            for (int i = 0; i < FftSize / 2; i++)
                fftMagnitudes[i] = (float)Math.Sqrt(fftReal[i] * fftReal[i] + fftImag[i] * fftImag[i]);

            return fftMagnitudes;
        }

        private static void FftIterative(double[] re, double[] im)
        {
            int n = re.Length;
            int j = 0;

            for (int i = 1; i < n; i++)
            {
                int bit = n >> 1;
                for (; (j & bit) != 0; bit >>= 1) j ^= bit;
                j ^= bit;

                if (i < j)
                {
                    (re[i], re[j]) = (re[j], re[i]);
                    (im[i], im[j]) = (im[j], im[i]);
                }
            }

            for (int len = 2; len <= n; len <<= 1)
            {
                double angle = -2.0 * Math.PI / len;
                double wRe = Math.Cos(angle);
                double wIm = Math.Sin(angle);

                for (int i = 0; i < n; i += len)
                {
                    double curRe = 1.0, curIm = 0.0;
                    int half = len / 2;

                    for (int k = 0; k < half; k++)
                    {
                        double uRe = re[i + k], uIm = im[i + k];
                        double tRe = curRe * re[i + k + half] - curIm * im[i + k + half];
                        double tIm = curRe * im[i + k + half] + curIm * re[i + k + half];

                        re[i + k] = uRe + tRe;
                        im[i + k] = uIm + tIm;
                        re[i + k + half] = uRe - tRe;
                        im[i + k + half] = uIm - tIm;

                        double newCurRe = curRe * wRe - curIm * wIm;
                        curIm = curRe * wIm + curIm * wRe;
                        curRe = newCurRe;
                    }
                }
            }
        }
    }
}
