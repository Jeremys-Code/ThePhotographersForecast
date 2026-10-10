using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;
using ThePhotographersForecast.Services.Modules;

namespace ThePhotographersForecast.Services
{
    public class LottieParadeBackground
    {
        private SKCanvasView _canvasView;
        private ISkyColorEngine _skyEngine;
        private readonly Stopwatch _stopwatch = new();

        public bool RotatingSweepGradient(SKCanvasView canvasView, ISkyColorEngine skyEngine)
        {
            _skyEngine = skyEngine;
            _canvasView = canvasView;
            _canvasView.PaintSurface += OnPaintSurface;
            _stopwatch.Start();

            canvasView.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
            {
                _canvasView.InvalidateSurface();
                return true;
            });
            return false;
        }

        private async void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            SKImageInfo info = e.Info;

            canvas.Clear(SKColors.Black);

            SKPoint center = new SKPoint(info.Width / 2f, info.Height / 2f);

            // Rotate 90 degrees per second
            float angle = ((float)_stopwatch.Elapsed.TotalSeconds * 90f) % 360f;

            SKColor[] colors = new[]
            {
            SKColors.Cyan,
            SKColors.Magenta,
            SKColors.Yellow,
            SKColors.Cyan
        };

            // Rotate the canvas context around the center point
            canvas.Save();
            canvas.RotateDegrees(angle, center.X, center.Y);

            using var shader = SKShader.CreateSweepGradient(center, colors, null);
            using var paint = new SKPaint
            {
                Shader = shader,
                IsAntialias = true
            };

            canvas.DrawPaint(paint);
            canvas.Restore();
        }
    }
}
