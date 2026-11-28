using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Dialogs;

partial class DiscountDialog
{
    private void InitializeComponent()
    {
        pnlSurface = new GlassPanel();
        tbl = new TableLayoutPanel();
        lblTitle = new Label();
        numValue = new NumericUpDown();
        chkPercent = new CheckBox();
        flpButtons = new FlowLayoutPanel();
        btnApply = new RoundButton();
        btnCancel = new RoundButton();
        pnlSurface.SuspendLayout();
        tbl.SuspendLayout();
        ((ISupportInitialize)numValue).BeginInit();
        flpButtons.SuspendLayout();
        SuspendLayout();
        //
        // pnlSurface
        //
        pnlSurface.Controls.Add(tbl);
        pnlSurface.Dock = DockStyle.Fill;
        pnlSurface.FillColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        pnlSurface.Padding = new Padding(24);
        pnlSurface.Name = "pnlSurface";
        //
        // tbl
        //
        tbl.ColumnCount = 1;
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tbl.Controls.Add(lblTitle, 0, 0);
        tbl.Controls.Add(numValue, 0, 1);
        tbl.Controls.Add(chkPercent, 0, 2);
        tbl.Controls.Add(flpButtons, 0, 3);
        tbl.Dock = DockStyle.Fill;
        tbl.RowCount = 4;
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tbl.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        tbl.Name = "tbl";
        //
        // lblTitle
        //
        lblTitle.Dock = DockStyle.Fill;
        lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
        lblTitle.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        lblTitle.Name = "lblTitle";
        lblTitle.Text = "Discount";
        //
        // numValue
        //
        numValue.BackColor = Color.FromArgb(0x25, 0x20, 0x38);
        numValue.DecimalPlaces = 2;
        numValue.Dock = DockStyle.Fill;
        numValue.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Point);
        numValue.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        numValue.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
        numValue.Name = "numValue";
        numValue.ThousandsSeparator = true;
        //
        // chkPercent
        //
        chkPercent.Dock = DockStyle.Fill;
        chkPercent.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        chkPercent.Name = "chkPercent";
        chkPercent.Text = "Percent (%)  —  cashiers max 10%";
        //
        // flpButtons
        //
        flpButtons.Controls.Add(btnApply);
        flpButtons.Controls.Add(btnCancel);
        flpButtons.Dock = DockStyle.Fill;
        flpButtons.FlowDirection = FlowDirection.RightToLeft;
        flpButtons.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        flpButtons.Name = "flpButtons";
        flpButtons.WrapContents = false;
        //
        // btnApply
        //
        btnApply.FillColor = Color.FromArgb(0xF5, 0x7C, 0x1F);
        btnApply.IsPrimary = true;
        btnApply.Name = "btnApply";
        btnApply.Size = new Size(130, 44);
        btnApply.Text = "Apply  F4";
        //
        // btnCancel
        //
        btnCancel.BorderColor = Color.FromArgb(0x2E, 0x29, 0x45);
        btnCancel.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(110, 44);
        btnCancel.Text = "Cancel";
        //
        // DiscountDialog
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(380, 240);
        Controls.Add(pnlSurface);
        Name = "DiscountDialog";
        Text = "AbayPOS – Discount";
        pnlSurface.ResumeLayout(false);
        tbl.ResumeLayout(false);
        tbl.PerformLayout();
        ((ISupportInitialize)numValue).EndInit();
        flpButtons.ResumeLayout(false);
        ResumeLayout(false);
    }

    private GlassPanel pnlSurface;
    private TableLayoutPanel tbl;
    private Label lblTitle;
    private NumericUpDown numValue;
    private CheckBox chkPercent;
    private FlowLayoutPanel flpButtons;
    private RoundButton btnApply;
    private RoundButton btnCancel;
}