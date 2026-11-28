using System.Drawing.Drawing2D;
using AbayPOS.Domain.Reporting;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Line chart of daily sales. No third-party chart library.</summary>
[ToolboxItem(true)]
[Category("AbayPOS")]
[Description("Daily sales line chart.")]
public sealed class BarChart : Control
{
    private readonly ToolTip _tip = new();
    private IReadOnlyList<DailySalesPoint> _points = [];
    private PointF[] _pts = [];

    public BarChart()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Theme.Card;
        AccessibleName = "Sales chart";
        if (DesignTime.IsActive)
        {
            var today = DateTime.Today;
            SetPoints(Enumerable.Range(0, 7)
                .Select(i => new DailySalesPoint
                {
                    Date = today.AddDays(-6 + i),
                    Total = 1200m + i * 350m,
                    Transactions = 8 + i
                })
                .ToList());
        }
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
            TextKit.DrawFit(e.Graphics, "No sales in this period.", Theme.FontBody(), new Rectangle(8, Height / 2, Width - 16, Theme.Scale(24)), Theme.TextSecondary);
            return;
        }

        var font = Theme.Font(8);
        var max = Math.Max(1m, _points.Max(p => p.Total));
        var yLabels = new[] { Compact(max), Compact(max / 2), Compact(0) };
        var left = yLabels.Max(label => TextKit.Measure(e.Graphics, label, font).Width) + Theme.Scale(10);
        var sample = _points[0].Date.ToString("ddd");
        var labelWidth = TextKit.Measure(e.Graphics, sample, font).Width;
        var bottom = Height - Theme.Scale(22);
        var top = Theme.Scale(12);
        var right = Theme.Scale(12);
        var step = _points.Count <= 1 ? 0 : (Width - left - right) / (_points.Count - 1);
        var stride = 1;
        if (step > 0 && labelWidth > step)
            stride = (int)Math.Ceiling(labelWidth / (double)step);
        var pts = new PointF[_points.Count];
        for (var i = 0; i < _points.Count; i++)
        {
            var h = (float)(_points[i].Total / max) * (bottom - top);
            pts[i] = new PointF(left + i * step, bottom - h);
        }

        _pts = pts;
        for (var tick = 0; tick < 4; tick++)
        {
            var y = top + (bottom - top) * tick / 3f;
            e.Graphics.DrawLine(Theme.Stroke(Theme.Line), left, y, Width - right, y);
            var value = max * (3 - tick) / 3m;
            TextKit.DrawMoney(e.Graphics, Compact(value), font, new Rectangle(0, (int)y - Theme.Scale(8), left - Theme.Scale(4), Theme.Scale(16)), Theme.TextSecondary);
        }

        using var area = new GraphicsPath();
        area.AddLine(pts[0].X, bottom, pts[0].X, pts[0].Y);
        if (pts.Length > 1)
            area.AddLines(pts);
        area.AddLine(pts[^1].X, pts[^1].Y, pts[^1].X, bottom);
        area.CloseFigure();
        e.Graphics.FillPath(Theme.Brush(Theme.Selection), area);

        var line = Theme.Stroke(Theme.Accent, 2.5f);
        if (pts.Length > 1)
            e.Graphics.DrawLines(line, pts);
        for (var i = 0; i < pts.Length; i++)
        {
            e.Graphics.FillEllipse(Theme.Brush(Theme.Accent), pts[i].X - 4, pts[i].Y - 4, 8, 8);
            e.Graphics.FillEllipse(Theme.Brush(Theme.Card), pts[i].X - 2, pts[i].Y - 2, 4, 4);
            if (i % stride != 0 && i != pts.Length - 1)
                continue;
            var label = _points[i].Date.ToString(_points.Count > 14 ? "d" : "ddd");
            var box = new Rectangle((int)pts[i].X - labelWidth / 2, (int)bottom + Theme.Scale(2), labelWidth, Theme.Scale(16));
            TextKit.DrawCenter(e.Graphics, label, font, box, Theme.TextSecondary);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var hit = -1;
        for (var i = 0; i < _pts.Length; i++)
        {
            if (Math.Abs(e.X - _pts[i].X) <= Theme.Scale(8))
                hit = i;
        }

        _tip.SetToolTip(this, hit >= 0 ? $"{_points[hit].Date:MMM d}: {AbayPOS.Domain.Money.Format(_points[hit].Total)}" : string.Empty);
    }

    private static string Compact(decimal value)
    {
        var abs = Math.Abs(value);
        if (abs >= 1_000_000m)
            return $"ETB {value / 1_000_000m:0.#}M";
        if (abs >= 1_000m)
            return $"ETB {value / 1_000m:0.#}K";
        return $"ETB {value:0}";
    }
}
