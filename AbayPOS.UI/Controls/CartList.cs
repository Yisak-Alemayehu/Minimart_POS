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
    private readonly System.Windows.Forms.Timer _flash = new() { Interval = 400 };

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
            var flashed = _flashIndex;
            _flashIndex = -1;
            _flash.Stop();
            InvalidateRow(flashed);
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
            var next = _lines.Count == 0 ? -1 : Math.Clamp(value, 0, _lines.Count - 1);
            if (_selected == next)
                return;
            var previous = _selected;
            _selected = next;
            EnsureVisible(_selected);
            SelectionChanged?.Invoke(this, _selected);
            InvalidateRow(previous);
            InvalidateRow(_selected);
        }
    }

    public void SetLines(IReadOnlyList<CartLine> lines, int? selectIndex = null, bool flash = false)
    {
        var previousCount = _lines.Count;
        var previousSelected = _selected;
        var previousFlash = _flashIndex;
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

        if (previousCount == lines.Count)
        {
            InvalidateRow(previousSelected);
            InvalidateRow(previousFlash);
            InvalidateRow(_selected);
            InvalidateRow(_flashIndex);
            return;
        }

        Invalidate();
    }

    private void InvalidateRow(int index)
    {
        if (index < 0)
            return;
        var row = RowBounds(index);
        Invalidate(new Rectangle(row.X, row.Y, Math.Max(1, row.Width), row.Height));
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
            e.Graphics.DrawString("Scan a barcode or tap a product to start", Theme.FontBody(), Theme.Brush(Theme.TextSecondary), ClientRectangle, Theme.CenterFormat);
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
        if (index == _flashIndex || index == _selected)
            g.FillRectangle(Theme.Brush(Theme.Selection), row);

        var nameFont = Theme.Font(12, FontStyle.Bold);
        var body = Theme.Font(11);
        g.DrawString(line.ProductName, nameFont, Theme.Brush(Theme.Text), new RectangleF(row.X + 8, row.Y + 6, row.Width * 0.42f, 22));
        g.DrawString(Money.Format(line.UnitPrice), body, Theme.Brush(Theme.TextSecondary), new RectangleF(row.X + 8, row.Y + 28, 120, 18));

        DrawStepper(g, MinusRect(row), "−");
        g.DrawString(line.Qty.ToString(), body, Theme.Brush(Theme.Text), QtyRect(row), Theme.CenterFormat);
        DrawStepper(g, PlusRect(row), "+");
        g.DrawString(Money.Format(line.LineTotal), nameFont, Theme.Brush(Theme.Accent), new RectangleF(row.Right - 108, row.Y, 72, row.Height), Theme.FarFormat);
        DrawStepper(g, RemoveRect(row), "×");
    }

    private static void DrawStepper(Graphics g, RectangleF rect, string glyph)
    {
        using var path = Gfx.Round(rect, 8);
        g.FillPath(Theme.Brush(Theme.NavGlass), path);
        g.DrawString(glyph, Theme.Font(12, FontStyle.Bold), Theme.Brush(Theme.Text), rect, Theme.CenterFormat);
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
