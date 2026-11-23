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

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovered = false;
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _pressed = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _pressed = false;
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
        using var fill = new SolidBrush(dim ? Theme.TileMuted : _hovered ? Theme.TileHover : Theme.Tile);
        using var border = new Pen(_hovered && !dim ? Theme.AccentHover : Theme.GlassBorder, _hovered && !dim ? 2 : 1);
        if (_pressed && !dim)
        {
            e.Graphics.TranslateTransform(Width * 0.01f, Height * 0.01f);
            e.Graphics.ScaleTransform(0.98f, 0.98f);
        }

        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);

        var emoji = string.IsNullOrWhiteSpace(Product.Emoji) ? "🛒" : Product.Emoji;
        using var emojiFont = new Font("Segoe UI Emoji", Math.Max(22, Width / 6f), FontStyle.Regular, GraphicsUnit.Point);
        using var white = new SolidBrush(dim ? Theme.Disabled : Theme.Text);
        using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
        e.Graphics.DrawString(emoji, emojiFont, white, new RectangleF(0, Height * 0.08f, Width, Height * 0.38f), center);

        using var nameFont = Theme.Font(Math.Max(10, Width / 15f), FontStyle.Bold);
        e.Graphics.DrawString(Product.Name, nameFont, white, new RectangleF(8, Height * 0.48f, Width - 16, Height * 0.22f), center);

        using var priceFont = Theme.Font(Math.Max(10, Width / 16f), FontStyle.Bold);
        using var orange = new SolidBrush(dim ? Theme.Disabled : Theme.Accent);
        e.Graphics.DrawString(AbayPOS.Domain.Money.Format(Product.Price), priceFont, orange, new RectangleF(0, Height * 0.72f, Width, Height * 0.16f), center);

        if (Product.IsOutOfStock)
            DrawBadge(e.Graphics, "Out of stock", Theme.Danger);
        else if (Product.IsLowStock)
            DrawBadge(e.Graphics, "Low", Theme.Warning);
    }

    private void DrawBadge(Graphics g, string text, Color color)
    {
        using var font = Theme.Font(8, FontStyle.Bold);
        var size = g.MeasureString(text, font);
        var rect = new RectangleF(Width - size.Width - 18, 10, size.Width + 10, 18);
        using var path = Gfx.Round(rect, Theme.ChipRadius);
        using var fill = new SolidBrush(color);
        using var brush = new SolidBrush(Color.White);
        using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.FillPath(fill, path);
        g.DrawString(text, font, brush, rect, format);
    }
}
