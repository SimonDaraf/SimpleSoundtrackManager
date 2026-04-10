using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Windows.Forms.AxHost;

namespace SimpleSoundtrackManager.MVVM.View.Components
{
    /// <summary>
    /// Interaction logic for TrackPlayer.xaml
    /// </summary>
    public partial class TrackPlayer : UserControl
    {
        public static readonly DependencyProperty LoopStartProperty =
            DependencyProperty.Register(nameof(LoopStart), typeof(long), 
                typeof(TrackPlayer), new PropertyMetadata(default(long), OnPropertyChanged));

        public long LoopStart
        {
            get => (long)GetValue(LoopStartProperty);
            set => SetValue(LoopStartProperty, value);
        }

        public static readonly DependencyProperty LoopEndProperty =
            DependencyProperty.Register(nameof(LoopEnd), typeof(long),
                typeof(TrackPlayer), new PropertyMetadata(default(long), OnPropertyChanged));

        public long LoopEnd
        {
            get => (long)GetValue(LoopEndProperty);
            set => SetValue(LoopEndProperty, value);
        }

        public static readonly DependencyProperty WaveformColorProperty =
            DependencyProperty.Register(nameof(WaveformColor), typeof(Color),
                typeof(TrackPlayer), new PropertyMetadata(new Color(), OnPropertyChanged));

        public Color WaveformColor
        {
            get => (Color)GetValue(WaveformColorProperty);
            set => SetValue(WaveformColorProperty, value);
        }

        public static readonly DependencyProperty TrackProperty =
            DependencyProperty.Register(nameof(Track), typeof(Track),
                typeof(TrackPlayer), new PropertyMetadata(null, OnPropertyChanged));

        public Track? Track
        {
            get => (Track?)GetValue(TrackProperty);
            set => SetValue(TrackProperty, value);
        }

        private int channels = 1;
        private float[] audioBuffer = [];
        private float[] monoBuffer = [];
        private int bucketCount;
        private float[] peakMin = [];
        private float[] peakMax = [];

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TrackPlayer tp)
            {
                if (e.Property.Equals(TrackProperty) && e.NewValue is Track t)
                {
                    tp.audioBuffer = ReadAllSamplesNormalized(t.FilePath, out int channels);
                    tp.channels = channels;
                }

                tp.CanvasView.InvalidateVisual();
            }
        }

        private static float[] ReadAllSamplesNormalized(string filePath, out int channels)
        {
            using AudioFileReader reader = new AudioFileReader(filePath);
            channels = reader.WaveFormat.Channels;

            // Read in chunks
            float[] buffer = new float[reader.WaveFormat.SampleRate * channels]; // 1 s at a time
            List<float> allFrames = new List<float>();

            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    allFrames.Add(buffer[i]);
                }
            }

            return [.. allFrames];
        }

        public TrackPlayer()
        {
            InitializeComponent();
        }

        private SKColor WpfToSkiasharpColor(Color color)
        {
            return new SKColor(color.R, color.G, color.B);
        }

        private void Rebucket(int buckets)
        {
            if (buckets <= 0 || audioBuffer.Length == 0)
            {
                peakMin = [];
                peakMax = [];
                return;
            }

            // Work with mono frames
            int frames = audioBuffer.Length / channels;
            peakMin = new float[buckets];
            peakMax = new float[buckets];

            double framesPerBucket = (double)frames / buckets;

            for (int b = 0; b < buckets; b++)
            {
                int start = (int)(b * framesPerBucket);
                int end = (int)((b + 1) * framesPerBucket);
                end = Math.Min(end, frames);

                float mn = 1f;
                float mx = -1f;

                for (int f = start; f < end; f++)
                {
                    // Average channels into a mono sample
                    float mono = 0f;
                    for (int c = 0; c < channels; c++)
                        mono += audioBuffer[f * channels + c];
                    mono /= channels;

                    if (mono < mn) mn = mono;
                    if (mono > mx) mx = mono;
                }

                peakMin[b] = mn;
                peakMax[b] = mx;
            }

            bucketCount = buckets;
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            int height = e.Info.Height;
            int width = e.Info.Width;

            canvas.Clear();
            if (Track is null) return;
            Rebucket(width);

            SKColor brushColor = WpfToSkiasharpColor(WaveformColor);
            SKPaint skBrushSolid = new SKPaint()
            {
                Color = brushColor,
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Square,
                StrokeWidth = 1,
                IsAntialias = false,
            };
            SKPaint skBrushTranslucent = new SKPaint()
            {
                Color = brushColor,
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Square,
                StrokeWidth = 1,
                IsAntialias = false,
                BlendMode = SKBlendMode.Overlay
            };

            float startX = (float)(LoopStart / Track.TrackLength * width);
            float endX = (float)(LoopEnd / Track.TrackLength * width);
            float midY = height / 2f;

            int buckets = peakMin.Length;
            float barWidth = width / buckets;

            for (int b = 0; b < buckets; b++)
            {
                float x = b * barWidth;
                float barMid = x + barWidth * 0.5f;
                float top = midY + peakMin[b] * midY;
                float bot = midY + peakMax[b] * midY;
                float barH = Math.Max(1f, bot - top);

                // Colour: active region vs. outside
                bool inRegion = barMid >= startX && barMid <= endX;

                canvas.DrawRect(SKRect.Create(x, top, barWidth - 0.5f, barH), inRegion ? skBrushSolid : skBrushTranslucent);
            }
        }
    }
}
