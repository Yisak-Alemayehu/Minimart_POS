using System.ComponentModel;
using System.Drawing.Drawing2D;
using AbayPOS.Domain.Entities;
using AbayPOS.UI.Imaging;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Controls;

/// <summary>Product card used on the cashier grid. Out-of-stock tiles are dimmed and inert.</summary>
[ToolboxItem(true)]
[Category("AbayPOS")]
[Description("Product tile for the cashier grid.")]
public partial class ProductTile : UserControl
{
    private static Product SampleProduct => new()
    {
        Name = "Milk 1L",
        Price = 85m,
        StockQty = 12,
        Emoji = "🥛",
        Category = "Dairy",
        Barcode = "000000"
    };

    private bool _hovered;
    private bool _pressed;
    private readonly ToolTip _tip = new();
    private Product _product = SampleProduct;

    public ProductTile() : this(SampleProduct) { }

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
        _tip.SetToolTip(this, product.Name);
        TabStop = !product.IsOutOfStock;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Product Product
    {
        get => _product;
        set
        {
            _product = value;
            Cursor = value.IsOutOfStock ? Cursors.No : Cursors.Hand;
            Enabled = !value.IsOutOfStock;
            TabStop = !value.IsOutOfStock;
            AccessibleName = $"{value.Name}, {AbayPOS.Domain.Money.Format(value.Price)}";
            _tip.SetToolTip(this, value.Name);
            Invalidate();
        }
    }

    public void Bind(Product product) => Product = product;

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

    protected override void OnPaintBackground(PaintEventArgs e) { }

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

        var imageHeight = Math.Max(Theme.Scale(48), (int)(Height * 0.60f));
        var imageBounds = new Rectangle(Theme.Scale(1), Theme.Scale(1), Math.Max(1, Width - Theme.Scale(2)), imageHeight);
        var photo = string.IsNullOrWhiteSpace(Product.ImagePath) ? null : ThumbnailCache.Shared.Get(Product.ImagePath, imageBounds.Height);
        if (photo is null && !string.IsNullOrWhiteSpace(Product.ImagePath) && !DesignTime.IsActive(this))
            ThumbnailCache.Shared.Request(this, Product.ImagePath, imageBounds.Height, Invalidate);
        using (var clip = Gfx.Round(new RectangleF(1, 1, Width - 3, imageHeight + Theme.PanelRadius), Theme.PanelRadius))
        {
            var state = e.Graphics.Save();
            e.Graphics.SetClip(clip);
            if (photo is not null)
                ProductImageCodec.DrawCover(e.Graphics, photo, imageBounds);
            else
            {
                var emoji = string.IsNullOrWhiteSpace(Product.Emoji) ? "\uE7BF" : Product.Emoji;
                var emojiFont = string.IsNullOrWhiteSpace(Product.Emoji) ? Theme.IconFont(28) : Theme.Font(22, FontStyle.Bold);
                TextKit.DrawCenter(e.Graphics, emoji, emojiFont, imageBounds, dim ? Theme.Disabled : Theme.Text);
            }

            e.Graphics.Restore(state);
        }

        var nameHeight = Theme.Scale(36);
        var priceHeight = Theme.Scale(22);
        var nameBounds = new Rectangle(Theme.Scale(8), imageHeight, Math.Max(1, Width - Theme.Scale(16)), nameHeight);
        TextKit.DrawFit(e.Graphics, Product.Name, Theme.Font(13, FontStyle.Bold), nameBounds, dim ? Theme.Disabled : Theme.Text, wrap: true);

        var priceText = AbayPOS.Domain.Money.Format(Product.Price);
        var priceBounds = new Rectangle(Theme.Scale(8), nameBounds.Bottom, Math.Max(1, Width - Theme.Scale(16)), priceHeight);
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
        var rect = new RectangleF(Width - size.Width - Theme.Scale(18), Theme.Scale(8), size.Width + Theme.Scale(10), Theme.Scale(18));
        using var path = Gfx.Round(rect, Theme.ChipRadius);
        var fill = Theme.Brush(color);
        g.FillPath(fill, path);
        TextKit.DrawCenter(g, text, font, Rectangle.Round(rect), Color.White);
    }
}