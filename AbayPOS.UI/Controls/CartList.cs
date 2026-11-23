using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using AbayPOS.Domain;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Owner-drawn cart. Only visible rows are painted. Hit-tests − / + / ×.</summary>
public sealed class CartList : Control
{
    public const int RowHeight = 52;
    private readonly VScrollBar _scroll = new();
    private IReadOnlyList<CartLine> _lines = [];
    private int _selected;
    private int _flashIndex = -1;
    private readonly System.Windows.Forms.Timer _flash = new() { Interval = 180 };

    public CartList()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        BackColor = Theme.CartSurface;
        TabStop = true;
        AccessibleName = "Cart";
        _scroll.Dock = DockStyle.Right;
        _scroll.Width = 14;
        _scroll.SmallChange = RowHeight;
        _scroll.LargeChange = RowHeight * 3;
        _scroll.Scroll += (_, _) => Invalidate();
        Controls.Add(_scroll);
        _flash.Tick += (_, _) =>
        {
            _flashIndex = -1;
            _flash.Stop();
            Invalidate();
        };
    }

    public event EventHandler<int>? QuantityPlus;
    public event EventHandler<int>? QuantityMinus;
    public event EventHandler<int>? LineRemoved;
    public event EventHandler<int>? SelectionChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex
    {
        get => _selected;
        set
        {
            _selected = _lines.Count == 0 ? -1 : Math.Clamp(value, 0, _lines.Count - 1);
            EnsureVisible(_selected);
            SelectionChanged?.Invoke(this, _selected);
            Invalidate();
        }
    }

    public void SetLines(IReadOnlyList<CartLine> lines, int? selectIndex = null, bool flash = false)
    {
        _lines = lines;
        _scroll.Maximum = Math.Max(0, lines.Count * RowHeight);
        _scroll.Visible = lines.Count * RowHeight > Height;
        if (selectIndex is not null)
            SelectedIndex = selectIndex.Value;
        else if (_selected >= lines.Count)
            SelectedIndex = lines.Count - 1;
        if (flash && lines.Count > 0)
        {
            _flashIndex = SelectedIndex;
            _flash.Stop();
            _flash.Start();
        }

        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (!_scroll.Visible)
            return;
        _scroll.Value = Math.Clamp(_scroll.Value - Math.Sign(e.Delta) * RowHeight, _scroll.Minimum, Math.Max(_scroll.Minimum, _scroll.Maximum - Height));
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        var index = HitRow(e.Y);
        if (index < 0)
            return;
        SelectedIndex = index;
        var row = RowBounds(index);
        if (Hit(e.Location, MinusRect(row)))
            QuantityMinus?.Invoke(this, index);
        else if (Hit(e.Location, PlusRect(row)))
            QuantityPlus?.Invoke(this, index);
        else if (Hit(e.Location, RemoveRect(row)))
            LineRemoved?.Invoke(this, index);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        e.Graphics.Clear(Theme.CartSurface);
        if (_lines.Count == 0)
        {
            using var muted = new SolidBrush(Theme.TextSecondary);
            using var font = Theme.FontBody();
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString("Cart is empty. Scan a barcode or tap a product.", font, muted, ClientRectangle, format);
            return;
        }

        var first = Math.Max(0, _scroll.Value / RowHeight);
        var last = Math.Min(_lines.Count - 1, first + Height / RowHeight + 1);
        for (var i = first; i <= last; i++)
            DrawRow(e.Graphics, i);
    }

    private void DrawRow(Graphics g, int index)
    {
        var line = _lines[index];
        var row = RowBounds(index);
        if (index == _flashIndex)
        {
            using var flash = new SolidBrush(Color.FromArgb(70, Theme.Accent));
            g.FillRectangle(flash, row);
        }
        else if (index == _selected)
        {
            using var sel = new SolidBrush(Theme.Selection);
            g.FillRectangle(sel, row);
        }

        using var nameFont = Theme.Font(12, FontStyle.Bold);
        using var body = Theme.Font(11);
        using var text = new SolidBrush(Theme.Text);
        using var muted = new SolidBrush(Theme.TextSecondary);
        using var accent = new SolidBrush(Theme.Accent);
        g.DrawString(line.ProductName, nameFont, text, new RectangleF(row.X + 8, row.Y + 6, row.Width * 0.42f, 22));
        g.DrawString(Money.Format(line.UnitPrice), body, muted, new RectangleF(row.X + 8, row.Y + 28, 120, 18));

        DrawStepper(g, MinusRect(row), "−");
        using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(line.Qty.ToString(), body, text, QtyRect(row), center);
        DrawStepper(g, PlusRect(row), "+");
        using var right = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
        g.DrawString(Money.Format(line.LineTotal), nameFont, accent, new RectangleF(row.Right - 108, row.Y, 72, row.Height), right);
        DrawStepper(g, RemoveRect(row), "×");
    }

    private static void DrawStepper(Graphics g, RectangleF rect, string glyph)
    {
        using var path = Gfx.Round(rect, 8);
        using var fill = new SolidBrush(Theme.NavGlass);
        using var brush = new SolidBrush(Theme.Text);
        using var font = Theme.Font(12, FontStyle.Bold);
        using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.FillPath(fill, path);
        g.DrawString(glyph, font, brush, rect, center);
    }

    private Rectangle RowBounds(int index)
    {
        var y = index * RowHeight - _scroll.Value;
        return new Rectangle(0, y, Width - (_scroll.Visible ? _scroll.Width : 0), RowHeight);
    }

    private int HitRow(int y)
    {
        var index = (_scroll.Value + y) / RowHeight;
        return index >= 0 && index < _lines.Count ? index : -1;
    }

    private static RectangleF MinusRect(Rectangle row) => new(row.Width * 0.48f, row.Y + 12, 28, 28);
    private static RectangleF QtyRect(Rectangle row) => new(row.Width * 0.48f + 28, row.Y + 12, 28, 28);
    private static RectangleF PlusRect(Rectangle row) => new(row.Width * 0.48f + 56, row.Y + 12, 28, 28);
    private static RectangleF RemoveRect(Rectangle row) => new(row.Right - 32, row.Y + 12, 28, 28);
    private static bool Hit(Point p, RectangleF r) => r.Contains(p);

    private void EnsureVisible(int index)
    {
        if (index < 0)
            return;
        var top = index * RowHeight;
        if (top < _scroll.Value)
            _scroll.Value = top;
        else if (top + RowHeight > _scroll.Value + Height)
            _scroll.Value = Math.Max(0, top + RowHeight - Height);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _flash.Dispose();
        base.Dispose(disposing);
    }
}
