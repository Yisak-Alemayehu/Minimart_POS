using System.ComponentModel;
using AbayPOS.Domain.Entities;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Imaging;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>Add/edit product with inline validation and an optional photo.</summary>
public sealed partial class ProductEditDialog : DialogForm
{
    private Bitmap? _working;

    public ProductEditDialog()
    {
        InitializeComponent();
        Product = new Product { IsActive = true, Price = 1, Category = "Staples", Name = "Sample product", Barcode = "000000" };
        WireButtons();
    }

    public ProductEditDialog(Product product, IReadOnlyList<Supplier> suppliers) : this()
    {
        Product = product;
        ApplyProduct(product, suppliers);
    }

    public Product Product { get; private set; } = new();
    public byte[]? ReplacementJpeg { get; private set; }
    public bool ImageRemoved { get; private set; }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
        WirePhotoDrop();
        LoadStored(Product.ImagePath);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (DesignTime.IsActive(this))
            return;
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

    private void WireButtons()
    {
        btnSave.Click += (_, _) => TrySave();
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        btnUpload.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.webp", Title = "Product image" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                LoadFile(dialog.FileName);
        };
        btnRemove.Click += (_, _) => ClearImage();
        btnCrop.Click += (_, _) =>
        {
            if (_working is not null)
                OpenCrop(new Bitmap(_working));
        };
        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    private void ApplyProduct(Product product, IReadOnlyList<Supplier> suppliers)
    {
        Text = product.Id == 0 ? "AbayPOS – Add product" : "AbayPOS – Edit product";
        var screenHeight = Screen.PrimaryScreen?.WorkingArea.Height ?? 900;
        MaximumSize = new Size(Theme.Scale(980), Math.Max(Theme.Scale(640), (int)(screenHeight * 0.85)));

        txtName.Text = product.Name;
        txtBarcode.Text = product.Barcode;
        txtCategory.Text = product.Category;
        txtEmoji.Text = product.Emoji;
        numPrice.Value = Math.Max(0.01m, product.Price == 0 ? 1 : product.Price);
        numCost.Value = Math.Max(0, product.CostPrice);
        numStock.Value = product.StockQty;
        numReorder.Value = product.ReorderLevel;
        picPhoto.Emoji = product.Emoji;

        cboSupplier.Items.Clear();
        cboSupplier.Items.Add(new Supplier { Id = 0, Name = "(none)" });
        foreach (var supplier in suppliers)
            cboSupplier.Items.Add(supplier);
        cboSupplier.DisplayMember = nameof(Supplier.Name);
        cboSupplier.SelectedIndex = 0;
        if (product.SupplierId is int sid)
        {
            for (var i = 1; i < cboSupplier.Items.Count; i++)
            {
                if (cboSupplier.Items[i] is Supplier s && s.Id == sid)
                    cboSupplier.SelectedIndex = i;
            }
        }
    }

    private void WirePhotoDrop()
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            picPhoto.AllowDrop = true;
        picPhoto.DragEnter += (_, e) =>
        {
            e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;
        };
        picPhoto.DragDrop += (_, e) =>
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                LoadFile(files[0]);
        };
    }

    private void LoadStored(string? relativeName)
    {
        if (!ServiceLocator.IsConfigured || string.IsNullOrWhiteSpace(relativeName))
            return;
        try
        {
            var bytes = ServiceLocator.Images.Load(relativeName);
            if (bytes is null)
                return;
            _working = ProductImageCodec.FromBytes(bytes);
            picPhoto.Image = _working;
            btnCrop.Enabled = true;
        }
        catch (Exception ex)
        {
            lblError.Text = "The saved image could not be opened. " + ex.Message;
        }
    }

    private void LoadFile(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists)
            return;
        if (info.Length > 10 * 1024 * 1024)
        {
            lblError.Text = "Choose an image that is 10 MB or smaller.";
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
            lblError.Text = "That file could not be read as an image.";
        }
    }

    private void Paste()
    {
        try
        {
            if (!Clipboard.ContainsImage())
            {
                lblError.Text = "The clipboard does not contain an image.";
                return;
            }

            using var clip = Clipboard.GetImage();
            if (clip is null)
                return;
            OpenCrop(new Bitmap(clip));
        }
        catch (Exception)
        {
            lblError.Text = "The clipboard image could not be pasted.";
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
        picPhoto.Image = _working;
        ReplacementJpeg = jpeg;
        ImageRemoved = false;
        btnCrop.Enabled = true;
        lblError.Text = string.Empty;
        picPhoto.Invalidate();
    }

    private void ClearImage()
    {
        _working?.Dispose();
        _working = null;
        picPhoto.Image = null;
        ReplacementJpeg = null;
        ImageRemoved = true;
        btnCrop.Enabled = false;
        picPhoto.Invalidate();
    }

    private void TrySave()
    {
        Product.Name = txtName.Text ?? string.Empty;
        Product.Barcode = txtBarcode.Text ?? string.Empty;
        Product.Category = txtCategory.Text ?? string.Empty;
        Product.Emoji = string.IsNullOrWhiteSpace(txtEmoji.Text) ? null : txtEmoji.Text;
        Product.Price = numPrice.Value;
        Product.CostPrice = numCost.Value;
        Product.StockQty = (int)numStock.Value;
        Product.ReorderLevel = (int)numReorder.Value;
        Product.SupplierId = cboSupplier.SelectedItem is Supplier s && s.Id != 0 ? s.Id : null;
        if (string.IsNullOrWhiteSpace(Product.Name) || string.IsNullOrWhiteSpace(Product.Barcode) || Product.Price <= 0)
        {
            lblError.Text = "Name, barcode and a price above zero are required.";
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private sealed class PhotoBox : Control
    {
        public PhotoBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Bitmap? Image { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? Emoji { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.Clear(BackColor);
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = Gfx.Round(bounds, 12);
            e.Graphics.SetClip(path);
            if (Image is not null)
                ProductImageCodec.DrawCover(e.Graphics, Image, bounds);
            else
            {
                e.Graphics.Clear(Color.FromArgb(0x25, 0x20, 0x38));
                var caption = string.IsNullOrWhiteSpace(Emoji) ? "No image" : Emoji;
                var font = string.IsNullOrWhiteSpace(Emoji) ? new Font("Segoe UI", 12, FontStyle.Bold) : new Font("Segoe UI", 28, FontStyle.Bold);
                TextKit.DrawCenter(e.Graphics, caption, font, bounds, Color.FromArgb(0xA1, 0xA1, 0xAA));
            }

            e.Graphics.ResetClip();
            e.Graphics.DrawPath(new Pen(Color.FromArgb(0x2E, 0x29, 0x45)), path);
        }
    }
}