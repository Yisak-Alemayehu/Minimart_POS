using System.Drawing.Drawing2D;
using AbayPOS.Domain.Reporting;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Line chart of daily sales. No third-party chart library.</summary>
public sealed class BarChart : Control
{
    private IReadOnlyList<DailySalesPoint> _points = [];

    public BarChart()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Theme.Card;
        AccessibleName = "Sales chart";
    }

    public void SetPoints(IReadOnlyList<DailySalesPoint> points)
    {
        _points = points;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Theme.Card);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        if (_points.Count == 0)
        {
            using var muted = new SolidBrush(Theme.TextSecondary);
            e.Graphics.DrawString("No sales in this period.", Theme.FontBody(), muted, 8, Height / 2f);
            return;
        }

        var left = 36f;
        var right = 16f;
        var top = 16f;
        var bottom = Height - 28f;
        var max = Math.Max(1m, _points.Max(p => p.Total));
        var step = (_points.Count == 1) ? 0 : (Width - left - right) / (_points.Count - 1);
        var pts = new PointF[_points.Count];
        for (var i = 0; i < _points.Count; i++)
        {
            var h = (float)(_points[i].Total / max) * (bottom - top);
            pts[i] = new PointF(left + i * step, bottom - h);
        }

        using var area = new GraphicsPath();
        area.AddLine(pts[0].X, bottom, pts[0].X, pts[0].Y);
        if (pts.Length > 1)
            area.AddLines(pts);
        area.AddLine(pts[^1].X, pts[^1].Y, pts[^1].X, bottom);
        area.CloseFigure();
        using var fade = new SolidBrush(Theme.Selection);
        e.Graphics.FillPath(fade, area);

        using var line = new Pen(Theme.Accent, 2.5f) { LineJoin = LineJoin.Round };
        if (pts.Length > 1)
            e.Graphics.DrawLines(line, pts);
        using var dot = new SolidBrush(Theme.Accent);
        using var hole = new SolidBrush(Theme.Card);
        using var font = Theme.Font(8);
        using var text = new SolidBrush(Theme.TextSecondary);
        var labelEvery = Math.Max(1, _points.Count / 7);
        for (var i = 0; i < pts.Length; i++)
        {
            e.Graphics.FillEllipse(dot, pts[i].X - 4, pts[i].Y - 4, 8, 8);
            e.Graphics.FillEllipse(hole, pts[i].X - 2, pts[i].Y - 2, 4, 4);
            if (i % labelEvery != 0 && i != pts.Length - 1)
                continue;
            var label = _points[i].Date.ToString(_points.Count > 14 ? "d" : "ddd");
            e.Graphics.DrawString(label, font, text, pts[i].X - 14, bottom + 6);
        }
    }
}
