using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Dialogs;

partial class ImageCropDialog
{
    private void InitializeComponent()
    {
        tlpRoot = new TableLayoutPanel();
        cropSurface = new CropSurface();
        tlpSide = new TableLayoutPanel();
        lblPreviewCaption = new Label();
        cropPreview = new CropPreview();
        flpZoom = new FlowLayoutPanel();
        lblZoom = new Label();
        trkZoom = new TrackBar();
        flpActions = new FlowLayoutPanel();
        btnRotateLeft = new RoundButton();
        btnRotateRight = new RoundButton();
        btnReset = new RoundButton();
        btnApply = new RoundButton();
        btnCancel = new RoundButton();
        tlpRoot.SuspendLayout();
        tlpSide.SuspendLayout();
        flpZoom.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)trkZoom).BeginInit();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.ColumnCount = 2;
        tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
        tlpRoot.Controls.Add(cropSurface, 0, 0);
        tlpRoot.Controls.Add(tlpSide, 1, 0);
        tlpRoot.Dock = DockStyle.Fill;
        tlpRoot.Padding = new Padding(12);
        tlpRoot.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        tlpRoot.Name = "tlpRoot";
        //
        // cropSurface
        //
        cropSurface.Dock = DockStyle.Fill;
        cropSurface.Name = "cropSurface";
        //
        // tlpSide
        //
        tlpSide.ColumnCount = 1;
        tlpSide.Controls.Add(lblPreviewCaption, 0, 0);
        tlpSide.Controls.Add(cropPreview, 0, 1);
        tlpSide.Controls.Add(flpZoom, 0, 2);
        tlpSide.Controls.Add(flpActions, 0, 3);
        tlpSide.Dock = DockStyle.Fill;
        tlpSide.RowCount = 4;
        tlpSide.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpSide.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpSide.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpSide.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpSide.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        tlpSide.Name = "tlpSide";
        //
        // lblPreviewCaption
        //
        lblPreviewCaption.AutoSize = true;
        lblPreviewCaption.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblPreviewCaption.Name = "lblPreviewCaption";
        lblPreviewCaption.Text = "Tile preview";
        //
        // cropPreview
        //
        cropPreview.Margin = new Padding(16, 8, 0, 8);
        cropPreview.Name = "cropPreview";
        cropPreview.Size = new Size(160, 160);
        //
        // flpZoom
        //
        flpZoom.AutoSize = true;
        flpZoom.Controls.Add(lblZoom);
        flpZoom.Controls.Add(trkZoom);
        flpZoom.FlowDirection = FlowDirection.TopDown;
        flpZoom.WrapContents = false;
        flpZoom.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        flpZoom.Name = "flpZoom";
        //
        // lblZoom
        //
        lblZoom.AutoSize = true;
        lblZoom.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblZoom.Name = "lblZoom";
        lblZoom.Text = "Zoom";
        //
        // trkZoom
        //
        trkZoom.AutoSize = false;
        trkZoom.Height = 36;
        trkZoom.Maximum = 400;
        trkZoom.Minimum = 100;
        trkZoom.Name = "trkZoom";
        trkZoom.TickFrequency = 50;
        trkZoom.Value = 100;
        trkZoom.Width = 180;
        //
        // flpActions
        //
        flpActions.AutoSize = true;
        flpActions.Controls.Add(btnRotateLeft);
        flpActions.Controls.Add(btnRotateRight);
        flpActions.Controls.Add(btnReset);
        flpActions.Controls.Add(btnApply);
        flpActions.Controls.Add(btnCancel);
        flpActions.FlowDirection = FlowDirection.TopDown;
        flpActions.WrapContents = false;
        flpActions.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        flpActions.Name = "flpActions";
        //
        // btnRotateLeft
        //
        btnRotateLeft.BorderColor = Color.FromArgb(0x2E, 0x29, 0x45);
        btnRotateLeft.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnRotateLeft.Margin = new Padding(0, 0, 0, 6);
        btnRotateLeft.Name = "btnRotateLeft";
        btnRotateLeft.Size = new Size(160, 36);
        btnRotateLeft.Text = "Rotate left";
        //
        // btnRotateRight
        //
        btnRotateRight.BorderColor = Color.FromArgb(0x2E, 0x29, 0x45);
        btnRotateRight.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnRotateRight.Margin = new Padding(0, 0, 0, 6);
        btnRotateRight.Name = "btnRotateRight";
        btnRotateRight.Size = new Size(160, 36);
        btnRotateRight.Text = "Rotate right";
        //
        // btnReset
        //
        btnReset.BorderColor = Color.FromArgb(0x2E, 0x29, 0x45);
        btnReset.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnReset.Margin = new Padding(0, 0, 0, 6);
        btnReset.Name = "btnReset";
        btnReset.Size = new Size(160, 36);
        btnReset.Text = "Reset";
        //
        // btnApply
        //
        btnApply.IsPrimary = true;
        btnApply.Margin = new Padding(0, 12, 0, 6);
        btnApply.Name = "btnApply";
        btnApply.Size = new Size(160, 40);
        btnApply.Text = "Apply";
        //
        // btnCancel
        //
        btnCancel.BorderColor = Color.FromArgb(0x2E, 0x29, 0x45);
        btnCancel.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(160, 40);
        btnCancel.Text = "Cancel";
        //
        // ImageCropDialog
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(860, 560);
        Controls.Add(tlpRoot);
        MinimumSize = new Size(720, 480);
        Name = "ImageCropDialog";
        Text = "AbayPOS – Crop image";
        tlpRoot.ResumeLayout(false);
        tlpSide.ResumeLayout(false);
        tlpSide.PerformLayout();
        flpZoom.ResumeLayout(false);
        flpZoom.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)trkZoom).EndInit();
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    private TableLayoutPanel tlpRoot = null!;
    private CropSurface cropSurface = null!;
    private TableLayoutPanel tlpSide = null!;
    private Label lblPreviewCaption = null!;
    private CropPreview cropPreview = null!;
    private FlowLayoutPanel flpZoom = null!;
    private Label lblZoom = null!;
    private TrackBar trkZoom = null!;
    private FlowLayoutPanel flpActions = null!;
    private RoundButton btnRotateLeft = null!;
    private RoundButton btnRotateRight = null!;
    private RoundButton btnReset = null!;
    private RoundButton btnApply = null!;
    private RoundButton btnCancel = null!;
}