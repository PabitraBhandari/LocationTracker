using Microsoft.Maui.Graphics;

namespace LocationTracker.Views
{
    public class HeatDrawable : IDrawable
    {
        public readonly List<(double lat, double lng)> Points = new();
        public Func<(double lat, double lng), PointF>? GetProjection { get; set; }
        public Action? Invalidate { get; set; }

        public void SetPoints(IEnumerable<(double lat, double lng)> pts)
        {
            Points.Clear();
            Points.AddRange(pts);
            Invalidate?.Invoke();
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (GetProjection == null || Points.Count == 0) return;

            foreach (var p in Points)
            {
                var pt = GetProjection(p);
                if (pt.X < 0 || pt.Y < 0) continue;

                const float radius = 16f;
                // Define gradient stops as an array; no "Add" calls
                var stops = new PaintGradientStop[]
                {
                    new PaintGradientStop(0f, Colors.Red.WithAlpha(0.35f)),
                    new PaintGradientStop(0.5f, Colors.Orange.WithAlpha(0.20f)),
                    new PaintGradientStop(1f, Colors.Yellow.WithAlpha(0.05f))
                };

                var gradient = new RadialGradientPaint
                {
                    Center = pt,
                    Radius = radius,
                    GradientStops = stops
                };

                canvas.SetFillPaint(gradient, new RectF(pt.X - radius, pt.Y - radius, radius * 2, radius * 2));
                canvas.FillCircle(pt, radius);
            }
        }
    }
}