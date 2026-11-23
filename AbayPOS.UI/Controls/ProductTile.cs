using System.Drawing.Drawing2D;
using AbayPOS.Domain.Entities;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Product card used on the cashier grid. Out-of-stock tiles are dimmed and inert.</summary>
public sealed class ProductTile : UserControl
{
    private bool _hovered;
    private bool _pressed;

    public ProductTile(Product product)
    {
        Product = product;
        Size = new Size(170, 170);
        Margin = new Padding(0, 0, 12, 12);
        BackColor = Theme.Tile;
        Cursor = product.IsOutOfStock ? Cursors.No : Cursors.Hand;
        Enabled = !product.IsOutOfStock;
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        AccessibleName = $"{product.Name}, {AbayPOS.Domain.Money.Format(product.Price)}";
        TabStop = !product.IsOutOfStock;
    }

    public Product Product { get; }

    public void SyncStock()
    {
        var blocked = Product.IsOutOfStock;
        Enabled = !blocked;
        Cursor = blocked ? Cursors.No : Cursors.Hand;
        TabStop = !blocked;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        SetFlag(ref _hovered, true);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        var changed = _hovered || _pressed;
        _hovered = false;
        _pressed = false;
        if (changed)
            Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        SetFlag(ref _pressed, true);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        SetFlag(ref _pressed, false);
    }

    private void SetFlag(ref bool field, bool value)
    {
        if (field == value)
            return;
        field = value;
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Theme.Solid(Parent?.BackColor ?? Theme.Card));
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        var dim = Product.IsOutOfStock;
        using var path = Gfx.Round(new RectangleF(1, 1, Width - 3, Height - 3), Theme.PanelRadius);
        var fill = Theme.Brush(dim ? Theme.TileMuted : _hovered ? Theme.TileHover : Theme.Tile);
        var border = Theme.Stroke(_hovered && !dim ? Theme.AccentHover : Theme.GlassBorder, _hovered && !dim ? 2 : 1);
        if (_pressed && !dim)
        {
            e.Graphics.TranslateTransform(Width * 0.01f, Height * 0.01f);
            e.Graphics.ScaleTransform(0.98f, 0.98f);
        }

        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);

        var emoji = string.IsNullOrWhiteSpace(Product.Emoji) ? "\uE7BF" : Product.Emoji;
        var emojiFont = string.IsNullOrWhiteSpace(Product.Emoji) ? Theme.IconFont(28) : Theme.Font(22, FontStyle.Bold);
        TextKit.DrawCenter(e.Graphics, emoji, emojiFont, new Rectangle(0, (int)(Height * 0.08f), Width, (int)(Height * 0.38f)), dim ? Theme.Disabled : Theme.Text);

        var nameFont = Theme.Font(13, FontStyle.Bold);
        TextKit.DrawFit(e.Graphics, Product.Name, nameFont, new Rectangle(8, (int)(Height * 0.48f), Math.Max(1, Width - 16), (int)(Height * 0.22f)), dim ? Theme.Disabled : Theme.Text, wrap: true);

        var priceText = AbayPOS.Domain.Money.Format(Product.Price);
        var priceBounds = new Rectangle(4, (int)(Height * 0.72f), Math.Max(1, Width - 8), (int)(Height * 0.16f));
        var priceFont = TextKit.FitFont(e.Graphics, priceText, Theme.Font(13, FontStyle.Bold), priceBounds.Width, 10f);
        TextKit.DrawCenter(e.Graphics, priceText, priceFont, priceBounds, dim ? Theme.Disabled : Theme.Accent);

        if (Product.IsOutOfStock)
            DrawBadge(e.Graphics, "Out of stock", Theme.Danger);
        else if (Product.IsLowStock)
            DrawBadge(e.Graphics, "Low", Theme.Warning);
    }

    private void DrawBadge(Graphics g, string text, Color color)
    {
        var font = Theme.Font(8, FontStyle.Bold);
        var size = TextKit.Measure(g, text, font);
        var rect = new RectangleF(Width - size.Width - 18, 10, size.Width + 10, 18);
        using var path = Gfx.Round(rect, Theme.ChipRadius);
        var fill = Theme.Brush(color);
        var brush = Theme.Brush(Color.White);
        g.FillPath(fill, path);
        TextKit.DrawCenter(g, text, font, Rectangle.Round(rect), Color.White);
    }
}
