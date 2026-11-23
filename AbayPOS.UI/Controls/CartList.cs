using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using AbayPOS.Domain;
using AbayPOS.Domain.Models;
using AbayPOS.UI.Imaging;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Owner-drawn cart. Only visible rows are painted. Hit-tests − / + / ×.</summary>
public sealed class CartList : Control
{
    private readonly ToolTip _tip = new();
    private int _tipRow = -1;
    private readonly VScrollBar _scroll = new();
    private IReadOnlyList<CartLine> _lines = [];
    private int _selected;
    private int _flashIndex = -1;
    private readonly System.Windows.Forms.Timer _flash = new() { Interval = 400 };

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Func<int, string?>? ImageName { get; set; }

    public CartList()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        BackColor = Theme.CartSurface;
        TabStop = true;
        AccessibleName = "Cart";
        _scroll.Dock = DockStyle.Right;
        _scroll.Width = 14;
        _scroll.SmallChange = Theme.Scale(64);
        _scroll.LargeChange = Theme.Scale(64) * 3;
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
        _scroll.LargeChange = Math.Max(RowHeight, Height);
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
        using var g = CreateGraphics();
        var slots = Slots(g, _lines[index], row);
        if (Hit(e.Location, slots.Minus))
            QuantityMinus?.Invoke(this, index);
        else if (Hit(e.Location, slots.Plus))
            QuantityPlus?.Invoke(this, index);
        else if (Hit(e.Location, slots.Remove))
            LineRemoved?.Invoke(this, index);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        e.Graphics.Clear(Theme.CartSurface);
        if (_lines.Count == 0)
        {
            TextKit.DrawCenter(e.Graphics, "Scan a barcode or tap a product to start", Theme.FontBody(), ClientRectangle, Theme.TextSecondary);
            return;
        }

        var first = Math.Max(0, _scroll.Value / RowHeight);
        var last = Math.Min(_lines.Count - 1, first + Height / RowHeight + 1);
        for (var i = first; i <= last; i++)
            DrawRow(e.Graphics, i);
    }

    private int RowHeight => Theme.Scale(64);

    private void DrawRow(Graphics g, int index)
    {
        var line = _lines[index];
        var row = RowBounds(index);
        var slots = Slots(g, line, row);
        if (index == _flashIndex || index == _selected)
            g.FillRectangle(Theme.Brush(Theme.Selection), row);

        DrawThumb(g, line, slots.Thumb);
        TextKit.DrawFit(g, line.ProductName, Theme.Font(12, FontStyle.Bold), slots.Name, Theme.Text, wrap: true);
        TextKit.DrawFit(g, Money.Format(line.UnitPrice) + " each", Theme.Font(11), slots.Unit, Theme.TextSecondary);
        DrawStepper(g, slots.Minus, "−");
        TextKit.DrawCenter(g, line.Qty.ToString(), Theme.Font(11, FontStyle.Bold), slots.Qty, Theme.Text);
        DrawStepper(g, slots.Plus, "+");
        var totalText = Money.Format(line.LineTotal);
        var totalFont = TextKit.FitFont(g, totalText, Theme.Font(12, FontStyle.Bold), slots.Total.Width, 10f);
        TextKit.DrawMoney(g, totalText, totalFont, slots.Total, Theme.Accent);
        DrawStepper(g, slots.Remove, "×");
    }

    private readonly record struct RowSlots(Rectangle Thumb, Rectangle Name, Rectangle Unit, Rectangle Minus, Rectangle Qty, Rectangle Plus, Rectangle Total, Rectangle Remove);

    private RowSlots Slots(Graphics g, CartLine line, Rectangle row)
    {
        var pad = Theme.Scale(8);
        var button = Theme.Scale(28);
        var qtyFont = Theme.Font(11, FontStyle.Bold);
        var qtyWidth = Math.Max(Theme.Scale(28), TextKit.Measure(g, "9999", qtyFont).Width + pad);
        var money = TextKit.FitFont(g, "ETB 99,999.00", Theme.Font(12, FontStyle.Bold), Theme.Scale(140), 10f);
        var moneyWidth = TextKit.Measure(g, Money.Format(line.LineTotal), money).Width + pad;
        var remove = new Rectangle(row.Right - pad - button, row.Y + (row.Height - button) / 2, button, button);
        var total = new Rectangle(remove.Left - pad - moneyWidth, row.Y, moneyWidth, row.Height);
        var plus = new Rectangle(total.Left - pad - button, remove.Y, button, button);
        var qty = new Rectangle(plus.Left - qtyWidth, row.Y, qtyWidth, row.Height);
        var minus = new Rectangle(qty.Left - button, remove.Y, button, button);
        var thumbSize = Theme.Scale(40);
        var thumb = new Rectangle(row.X + pad, row.Y + (row.Height - thumbSize) / 2, thumbSize, thumbSize);
        var nameX = thumb.Right + pad;
        var name = new Rectangle(nameX, row.Y + Theme.Scale(4), Math.Max(Theme.Scale(40), minus.Left - pad - nameX), row.Height / 2);
        var unit = new Rectangle(name.X, name.Bottom, name.Width, row.Bottom - name.Bottom - Theme.Scale(4));
        return new RowSlots(thumb, name, unit, minus, qty, plus, total, remove);
    }

    private void DrawThumb(Graphics g, CartLine line, Rectangle bounds)
    {
        var relative = ImageName?.Invoke(line.ProductId);
        var size = Math.Max(bounds.Width, bounds.Height);
        var photo = string.IsNullOrWhiteSpace(relative) ? null : ThumbnailCache.Shared.Get(relative, size);
        if (photo is null && !string.IsNullOrWhiteSpace(relative))
            ThumbnailCache.Shared.Request(this, relative, size, Invalidate);
        var state = g.Save();
        using var clip = Gfx.Round(bounds, Theme.Scale(8));
        g.SetClip(clip);
        if (photo is not null)
            ProductImageCodec.DrawCover(g, photo, bounds);
        else
        {
            g.FillRectangle(Theme.Brush(Theme.NavRaised), bounds);
            var emoji = string.IsNullOrWhiteSpace(line.Emoji) ? "\uE7BF" : line.Emoji;
            var font = string.IsNullOrWhiteSpace(line.Emoji) ? Theme.IconFont(16) : Theme.Font(14);
            TextKit.DrawCenter(g, emoji, font, bounds, Theme.Text);
        }

        g.Restore(state);
    }

    private static void DrawStepper(Graphics g, Rectangle rect, string glyph)
    {
        using var path = Gfx.Round(rect, 8);
        g.FillPath(Theme.Brush(Theme.NavGlass), path);
        TextKit.DrawCenter(g, glyph, Theme.Font(12, FontStyle.Bold), Rectangle.Round(rect), Theme.Text);
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

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var index = HitRow(e.Y);
        if (index == _tipRow)
            return;
        _tipRow = index;
        _tip.SetToolTip(this, index >= 0 ? _lines[index].ProductName : string.Empty);
    }

    private static bool Hit(Point p, Rectangle r) => r.Contains(p);

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
        {
            _flash.Dispose();
            _tip.Dispose();
        }
        base.Dispose(disposing);
    }
}
