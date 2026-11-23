using AbayPOS.Domain.Reporting;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Simple GDI+ bar chart of daily sales. No third-party chart library.</summary>
public sealed class BarChart : Control
{
    private IReadOnlyList<DailySalesPoint> _points = [];

    public BarChart()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
        AccessibleName = "Sales last 7 days";
    }

    public void SetPoints(IReadOnlyList<DailySalesPoint> points)
    {
        _points = points;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        if (_points.Count == 0)
        {
            using var muted = new SolidBrush(Theme.TextSecondary);
            e.Graphics.DrawString("No sales in the last 7 days.", Theme.FontBody(), muted, 8, Height / 2f);
            return;
        }

        var max = Math.Max(1m, _points.Max(p => p.Total));
        var barWidth = (Width - 20f) / _points.Count;
        using var fill = new SolidBrush(Theme.Accent);
        using var text = new SolidBrush(Theme.TextSecondary);
        using var font = Theme.Font(8);
        for (var i = 0; i < _points.Count; i++)
        {
            var h = (float)(_points[i].Total / max) * (Height - 36);
            var x = 10 + i * barWidth;
            var y = Height - 24 - h;
            e.Graphics.FillRectangle(fill, x + 8, y, Math.Max(8, barWidth - 16), Math.Max(2, h));
            var label = _points[i].Date.ToString("ddd");
            e.Graphics.DrawString(label, font, text, x + 6, Height - 20);
        }
    }
}
