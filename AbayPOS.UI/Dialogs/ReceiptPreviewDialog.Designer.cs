using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Dialogs;

partial class ReceiptPreviewDialog
{
    private void InitializeComponent()
    {
        pnlSurface = new GlassPanel();
        tbl = new TableLayoutPanel();
        previewReceipt = new PrintPreviewControl();
        flpButtons = new FlowLayoutPanel();
        btnPrint = new RoundButton();
        btnSkip = new RoundButton();
        pnlSurface.SuspendLayout();
        tbl.SuspendLayout();
        flpButtons.SuspendLayout();
        SuspendLayout();
        //
        // pnlSurface
        //
        pnlSurface.Controls.Add(tbl);
        pnlSurface.Dock = DockStyle.Fill;
        pnlSurface.FillColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        pnlSurface.Padding = new Padding(16);
        pnlSurface.Name = "pnlSurface";
        //
        // tbl
        //
        tbl.ColumnCount = 1;
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tbl.Controls.Add(previewReceipt, 0, 0);
        tbl.Controls.Add(flpButtons, 0, 1);
        tbl.Dock = DockStyle.Fill;
        tbl.RowCount = 2;
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        tbl.Name = "tbl";
        //
        // previewReceipt
        //
        previewReceipt.BackColor = Color.White;
        previewReceipt.Dock = DockStyle.Fill;
        previewReceipt.Name = "previewReceipt";
        previewReceipt.Zoom = 1.2;
        //
        // flpButtons
        //
        flpButtons.Controls.Add(btnPrint);
        flpButtons.Controls.Add(btnSkip);
        flpButtons.Dock = DockStyle.Fill;
        flpButtons.FlowDirection = FlowDirection.RightToLeft;
        flpButtons.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        flpButtons.Name = "flpButtons";
        flpButtons.WrapContents = false;
        //
        // btnPrint
        //
        btnPrint.FillColor = Color.FromArgb(0xF5, 0x7C, 0x1F);
        btnPrint.IsPrimary = true;
        btnPrint.Name = "btnPrint";
        btnPrint.Size = new Size(120, 44);
        btnPrint.Text = "Print";
        //
        // btnSkip
        //
        btnSkip.BorderColor = Color.FromArgb(0x2E, 0x29, 0x45);
        btnSkip.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnSkip.Name = "btnSkip";
        btnSkip.Size = new Size(110, 44);
        btnSkip.Text = "Skip";
        //
        // ReceiptPreviewDialog
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(520, 640);
        Controls.Add(pnlSurface);
        Name = "ReceiptPreviewDialog";
        Text = "AbayPOS – Receipt";
        pnlSurface.ResumeLayout(false);
        tbl.ResumeLayout(false);
        flpButtons.ResumeLayout(false);
        ResumeLayout(false);
    }

    private GlassPanel pnlSurface;
    private TableLayoutPanel tbl;
    private PrintPreviewControl previewReceipt;
    private FlowLayoutPanel flpButtons;
    private RoundButton btnPrint;
    private RoundButton btnSkip;
}