using Microsoft.Extensions.Logging;
using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Windows.Forms.AxHost;

namespace SimpleSoundtrackManager.MVVM.View.Components
{
    /// <summary>
    /// Interaction logic for TrackPlayer.xaml
    /// </summary>
    public partial class TrackPlayer : UserControl
    {
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

        // Private properties.
        private int channels = 1;
        private float[] audioBuffer = [];
        private int bucketCount;
        private float[] peakMin = [];
        private float[] peakMax = [];
        private int width = 0;
        private int height = 0;
        private Point mousePos;
        private bool isDraggingStart;
        private bool isDraggingEnd;
        private float startCoordinate;
        private float endCoordinate;
        private int handleSize = 10;

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

            // Only perform rebucket if necessary.
            if (e.Info.Height != height || e.Info.Width != width 
                || peakMin.Length == 0 || peakMax.Length == 0)
            {
                height = e.Info.Height;
                width = e.Info.Width;
                Rebucket(width);
            }
            canvas.Clear();

            if (Track is null || width == 0 || height == 0 || peakMin.Length == 0) return;

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
                Color = new SKColor(brushColor.Red, brushColor.Green, brushColor.Blue, 25),
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Square,
                StrokeWidth = 1,
                IsAntialias = false,
            };

            float startX = (float)((double)Track.StartPoint / Track.TrackLength * width);
            float endX = (float)((double)Track.LoopPoint / Track.TrackLength * width) - 1;

            if (isDraggingStart)
            {
                startX = MathF.Max(0, Math.Min((float)mousePos.X, endX - handleSize - 1));
                Track.StartPoint = (long)((double)startX / width * Track.TrackLength);
            }
            else if (isDraggingEnd)
            {
                endX = MathF.Min(width - 1, MathF.Max((float)mousePos.X, startX + handleSize + 1));
                Track.LoopPoint = (long)((double)endX / width * Track.TrackLength);
            }

            float midY = height / 2f;

            // Draw Waveform
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

            SKPaint skMoverStyle = new SKPaint()
            {
                Color = SKColor.Parse("#FFFFFF").WithAlpha(75),
                Style = SKPaintStyle.StrokeAndFill,
                StrokeCap = SKStrokeCap.Square,
                StrokeWidth = 1,
                IsAntialias = false,
            };

            SKPaint skMoverHoverStyle = new SKPaint()
            {
                Color = SKColor.Parse("#FFFFFF"),
                Style = SKPaintStyle.StrokeAndFill,
                StrokeCap = SKStrokeCap.Square,
                StrokeWidth = 1,
                IsAntialias = false,
            };

            float maxEndOffset = endX == width ? endX : endX - 1;

            startCoordinate = startX;
            endCoordinate = maxEndOffset;

            float rectStartPos = startX + 1;
            float rectEndPos = maxEndOffset - handleSize - 1;

            SKPaint startPaint = IsMouseWithinBounds(startX, 0, handleSize, handleSize) || isDraggingStart ? skMoverHoverStyle : skMoverStyle;
            SKPaint endPaint = IsMouseWithinBounds(rectEndPos, 0, handleSize, handleSize) || isDraggingEnd ? skMoverHoverStyle : skMoverStyle;

            // Draw Loop Points
            canvas.DrawLine(new SKPoint(startX, 0), new SKPoint(startX, height), startPaint);
            canvas.DrawRect(SKRect.Create(rectStartPos, 0, handleSize, handleSize), startPaint);

            canvas.DrawLine(new SKPoint(maxEndOffset, 0), new SKPoint(maxEndOffset, height), endPaint);
            canvas.DrawRect(SKRect.Create(rectEndPos, 0, handleSize, handleSize), endPaint);
        }

        private bool IsMouseWithinBounds(float left, float top, float width, float height)
        {
            return mousePos.X >= left && mousePos.X <= left + width && mousePos.Y > top && mousePos.Y <= top + height;
        }

        private void CanvasView_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!isDraggingEnd && !isDraggingStart)
            {
                if (IsMouseWithinBounds(startCoordinate, 0, handleSize, handleSize)) isDraggingStart = true;
                else if (IsMouseWithinBounds(endCoordinate - handleSize - 1, 0, handleSize, handleSize)) isDraggingEnd = true;

                if (isDraggingStart || isDraggingEnd)
                {
                    Mouse.Capture((IInputElement)sender);
                }
            }
        }

        private void CanvasView_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isDraggingStart) isDraggingStart = false;
            else if (isDraggingEnd) isDraggingEnd = false;
            Mouse.Capture(null);
        }

        private void CanvasView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            mousePos = e.GetPosition(CanvasView);
            CanvasView.InvalidateVisual();
        }
    }
}
