using System.ComponentModel;
using AbayPOS.Domain.Entities;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Imaging;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>Add/edit product with inline validation and an optional photo.</summary>
public sealed class ProductEditDialog : DialogForm
{
    private readonly GlassTextBox _name = new() { Placeholder = "Name" };
    private readonly GlassTextBox _barcode = new() { Placeholder = "Barcode" };
    private readonly GlassTextBox _category = new() { Placeholder = "Category" };
    private readonly GlassTextBox _emoji = new() { Placeholder = "Emoji" };
    private readonly NumericUpDown _price = MoneyBox();
    private readonly NumericUpDown _cost = MoneyBox();
    private readonly NumericUpDown _stock = IntBox();
    private readonly NumericUpDown _reorder = IntBox();
    private readonly ComboBox _supplier = new();
    private readonly Label _error = new() { ForeColor = Theme.Danger, AutoSize = true, Font = Theme.FontHint(), MaximumSize = new Size(Theme.Scale(420), 0) };
    private readonly PhotoBox _photo = new();
    private readonly RoundButton _crop = new() { Text = "Crop…", Width = Theme.Scale(200), Height = Theme.Scale(36), Enabled = false, FillColor = Theme.NavRaised, BorderColor = Theme.GlassBorder };
    private Bitmap? _working;

    public ProductEditDialog(Product product, IReadOnlyList<Supplier> suppliers)
    {
        Product = product;
        Text = product.Id == 0 ? "AbayPOS – Add product" : "AbayPOS – Edit product";
        AutoScroll = false;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        MinimumSize = new Size(Theme.Scale(860), Theme.Scale(520));
        var screenHeight = Screen.PrimaryScreen?.WorkingArea.Height ?? 900;
        MaximumSize = new Size(Theme.Scale(980), Math.Max(Theme.Scale(640), (int)(screenHeight * 0.85)));
        var surface = new GlassPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Theme.Scale(24)) };
        var grid = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Theme.Base };
        var row = 0;
        void Field(string caption, Control input)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.Controls.Add(new Label { Text = caption, AutoSize = true, ForeColor = Theme.TextSecondary, Font = Theme.FontHint(), Margin = new Padding(0, 8, 0, 2) }, 0, row++);
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(0, 0, 0, 4);
            input.MinimumSize = new Size(Theme.Scale(180), Theme.Scale(32));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.Controls.Add(input, 0, row++);
        }

        _name.Text = product.Name;
        _barcode.Text = product.Barcode;
        _category.Text = product.Category;
        _emoji.Text = product.Emoji;
        _price.Value = Math.Max(0.01m, product.Price == 0 ? 1 : product.Price);
        _cost.Value = Math.Max(0, product.CostPrice);
        _stock.Value = product.StockQty;
        _reorder.Value = product.ReorderLevel;
        _supplier.Dock = DockStyle.Fill;
        _supplier.DropDownStyle = ComboBoxStyle.DropDownList;
        _supplier.FlatStyle = FlatStyle.Flat;
        _supplier.BackColor = Theme.InputFill;
        _supplier.ForeColor = Theme.Text;
        _supplier.Items.Add(new Supplier { Id = 0, Name = "(none)" });
        foreach (var supplier in suppliers)
            _supplier.Items.Add(supplier);
        _supplier.DisplayMember = nameof(Supplier.Name);
        _supplier.SelectedIndex = 0;
        if (product.SupplierId is int sid)
        {
            for (var i = 1; i < _supplier.Items.Count; i++)
            {
                if (_supplier.Items[i] is Supplier s && s.Id == sid)
                    _supplier.SelectedIndex = i;
            }
        }

        Field("Name", _name);
        Field("Barcode", _barcode);
        Field("Category", _category);
        Field("Emoji", _emoji);
        Field("Price (ETB)", _price);
        Field("Cost (ETB)", _cost);
        Field("Stock", _stock);
        Field("Reorder level", _reorder);
        Field("Supplier", _supplier);
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(_error, 0, row++);
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Base, WrapContents = false };
        var save = new RoundButton { Text = "Save", Width = Theme.Scale(120), Height = Theme.Scale(44), IsPrimary = true };
        save.Click += (_, _) => TrySave();
        var cancel = new RoundButton { Text = "Cancel", Width = Theme.Scale(110), Height = Theme.Scale(44), FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(buttons, 0, row);

        var shell = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2, BackColor = Theme.Base };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Theme.Scale(220)));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        shell.Controls.Add(BuildPhotos(product), 0, 0);
        shell.Controls.Add(grid, 1, 0);
        surface.Controls.Add(shell);
        Controls.Add(surface);
        Surface.Adopt(this, Theme.Base);
        AcceptButton = save;
        CancelButton = cancel;
    }

    public Product Product { get; }
    public byte[]? ReplacementJpeg { get; private set; }
    public bool ImageRemoved { get; private set; }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Control && e.KeyCode == Keys.V)
        {
            Paste();
            e.Handled = true;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _working?.Dispose();
        base.Dispose(disposing);
    }

    private Control BuildPhotos(Product product)
    {
        var stack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            BackColor = Theme.Base,
            Padding = new Padding(0, Theme.Scale(8), Theme.Scale(12), 0)
        };
        _photo.Size = new Size(Theme.Scale(200), Theme.Scale(200));
        _photo.Emoji = product.Emoji;
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            _photo.AllowDrop = true;
        _photo.DragEnter += (_, e) =>
        {
            e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;
        };
        _photo.DragDrop += (_, e) =>
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                LoadFile(files[0]);
        };
        LoadStored(product.ImagePath);
        stack.Controls.Add(_photo);
        stack.Controls.Add(PhotoButton("Upload…", () =>
        {
            using var dialog = new OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.webp", Title = "Product image" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                LoadFile(dialog.FileName);
        }));
        _crop.Click += (_, _) => { if (_working is not null) OpenCrop(new Bitmap(_working)); };
        stack.Controls.Add(_crop);
        stack.Controls.Add(PhotoButton("Remove", ClearImage));
        return stack;
    }

    private void LoadStored(string? relativeName)
    {
        if (string.IsNullOrWhiteSpace(relativeName))
            return;
        try
        {
            var bytes = ServiceLocator.Images.Load(relativeName);
            if (bytes is null)
                return;
            _working = ProductImageCodec.FromBytes(bytes);
            _photo.Image = _working;
            _crop.Enabled = true;
        }
        catch (Exception ex)
        {
            _error.Text = "The saved image could not be opened. " + ex.Message;
        }
    }

    private void LoadFile(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists)
            return;
        if (info.Length > 10 * 1024 * 1024)
        {
            _error.Text = "Choose an image that is 10 MB or smaller.";
            return;
        }

        Bitmap? loaded = null;
        try
        {
            loaded = ProductImageCodec.FromFile(path);
            OpenCrop(loaded);
            loaded = null;
        }
        catch (Exception)
        {
            loaded?.Dispose();
            _error.Text = "That file could not be read as an image.";
        }
    }

    private void Paste()
    {
        try
        {
            if (!Clipboard.ContainsImage())
            {
                _error.Text = "The clipboard does not contain an image.";
                return;
            }

            using var clip = Clipboard.GetImage();
            if (clip is null)
                return;
            OpenCrop(new Bitmap(clip));
        }
        catch (Exception)
        {
            _error.Text = "The clipboard image could not be pasted.";
        }
    }

    private void OpenCrop(Bitmap source)
    {
        using var dialog = new ImageCropDialog(source);
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.Jpeg is { } jpeg)
            ApplyJpeg(jpeg);
    }

    private void ApplyJpeg(byte[] jpeg)
    {
        _working?.Dispose();
        _working = ProductImageCodec.FromBytes(jpeg);
        _photo.Image = _working;
        ReplacementJpeg = jpeg;
        ImageRemoved = false;
        _crop.Enabled = true;
        _error.Text = string.Empty;
        _photo.Invalidate();
    }

    private void ClearImage()
    {
        _working?.Dispose();
        _working = null;
        _photo.Image = null;
        ReplacementJpeg = null;
        ImageRemoved = true;
        _crop.Enabled = false;
        _photo.Invalidate();
    }

    private void TrySave()
    {
        Product.Name = _name.Text ?? string.Empty;
        Product.Barcode = _barcode.Text ?? string.Empty;
        Product.Category = _category.Text ?? string.Empty;
        Product.Emoji = string.IsNullOrWhiteSpace(_emoji.Text) ? null : _emoji.Text;
        Product.Price = _price.Value;
        Product.CostPrice = _cost.Value;
        Product.StockQty = (int)_stock.Value;
        Product.ReorderLevel = (int)_reorder.Value;
        Product.SupplierId = _supplier.SelectedItem is Supplier s && s.Id != 0 ? s.Id : null;
        if (string.IsNullOrWhiteSpace(Product.Name) || string.IsNullOrWhiteSpace(Product.Barcode) || Product.Price <= 0)
        {
            _error.Text = "Name, barcode and a price above zero are required.";
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private RoundButton PhotoButton(string text, Action click)
    {
        var button = new RoundButton { Text = text, Width = Theme.Scale(200), Height = Theme.Scale(36), Margin = new Padding(0, Theme.Scale(6), 0, 0), FillColor = Theme.NavRaised, BorderColor = Theme.GlassBorder };
        button.Click += (_, _) => click();
        return button;
    }

    private static NumericUpDown MoneyBox() => new()
    {
        DecimalPlaces = 2,
        ThousandsSeparator = true,
        Maximum = 10_000_000,
        Width = Theme.Scale(220),
        Minimum = 0,
        Dock = DockStyle.Fill,
        BackColor = Theme.InputFill,
        ForeColor = Theme.Text
    };

    private static NumericUpDown IntBox() => new()
    {
        DecimalPlaces = 0,
        Maximum = 1_000_000,
        Dock = DockStyle.Fill,
        BackColor = Theme.InputFill,
        ForeColor = Theme.Text
    };

    private sealed class PhotoBox : Control
    {
        public PhotoBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Theme.Card;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Bitmap? Image { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? Emoji { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.Clear(Theme.Card);
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = Gfx.Round(bounds, Theme.PanelRadius);
            e.Graphics.SetClip(path);
            if (Image is not null)
                ProductImageCodec.DrawCover(e.Graphics, Image, bounds);
            else
            {
                e.Graphics.Clear(Theme.NavRaised);
                var caption = string.IsNullOrWhiteSpace(Emoji) ? "No image" : Emoji;
                var font = string.IsNullOrWhiteSpace(Emoji) ? Theme.Font(12, FontStyle.Bold) : Theme.Font(28);
                TextKit.DrawCenter(e.Graphics, caption, font, bounds, Theme.TextSecondary);
            }

            e.Graphics.ResetClip();
            e.Graphics.DrawPath(Theme.Stroke(Theme.GlassBorder), path);
        }
    }
}
