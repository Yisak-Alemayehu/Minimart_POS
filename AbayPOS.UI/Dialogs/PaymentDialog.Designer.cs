using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Dialogs;

partial class PaymentDialog
{
    private void InitializeComponent()
    {
        pnlSurface = new GlassPanel();
        tbl = new TableLayoutPanel();
        lblTitle = new Label();
        txtDue = new FittedText();
        flpTabs = new FlowLayoutPanel();
        btnCashTab = new RoundButton();
        btnMobileTab = new RoundButton();
        numAmount = new NumericUpDown();
        flpQuick = new FlowLayoutPanel();
        lblChangeCaption = new Label();
        lblChange = new Label();
        flpButtons = new FlowLayoutPanel();
        btnPay = new RoundButton();
        btnBack = new RoundButton();
        pnlSurface.SuspendLayout();
        tbl.SuspendLayout();
        flpTabs.SuspendLayout();
        ((ISupportInitialize)numAmount).BeginInit();
        flpButtons.SuspendLayout();
        SuspendLayout();
        //
        // pnlSurface
        //
        pnlSurface.Controls.Add(tbl);
        pnlSurface.Dock = DockStyle.Fill;
        pnlSurface.FillColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        pnlSurface.Padding = new Padding(26);
        pnlSurface.Name = "pnlSurface";
        //
        // tbl
        //
        tbl.ColumnCount = 1;
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tbl.Controls.Add(lblTitle, 0, 0);
        tbl.Controls.Add(txtDue, 0, 1);
        tbl.Controls.Add(flpTabs, 0, 2);
        tbl.Controls.Add(numAmount, 0, 3);
        tbl.Controls.Add(flpQuick, 0, 4);
        tbl.Controls.Add(lblChangeCaption, 0, 5);
        tbl.Controls.Add(lblChange, 0, 6);
        tbl.Controls.Add(flpButtons, 0, 7);
        tbl.Dock = DockStyle.Fill;
        tbl.RowCount = 8;
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        tbl.Name = "tbl";
        //
        // lblTitle
        //
        lblTitle.Dock = DockStyle.Fill;
        lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
        lblTitle.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        lblTitle.Name = "lblTitle";
        lblTitle.Text = "Take payment";
        lblTitle.TextAlign = ContentAlignment.MiddleLeft;
        //
        // txtDue
        //
        txtDue.Dock = DockStyle.Fill;
        txtDue.ForeColor = Color.FromArgb(0xF5, 0x7C, 0x1F);
        txtDue.MinSize = 20F;
        txtDue.MinimumSize = new Size(160, 40);
        txtDue.Money = true;
        txtDue.Name = "txtDue";
        txtDue.StartSize = 32F;
        txtDue.Text = "ETB 0.00";
        //
        // flpTabs
        //
        flpTabs.Controls.Add(btnCashTab);
        flpTabs.Controls.Add(btnMobileTab);
        flpTabs.Dock = DockStyle.Fill;
        flpTabs.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        flpTabs.Name = "flpTabs";
        //
        // btnCashTab
        //
        btnCashTab.FillColor = Color.FromArgb(0xF5, 0x7C, 0x1F);
        btnCashTab.Margin = new Padding(0, 4, 8, 4);
        btnCashTab.Name = "btnCashTab";
        btnCashTab.Size = new Size(140, 40);
        btnCashTab.Text = "Cash";
        //
        // btnMobileTab
        //
        btnMobileTab.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnMobileTab.Margin = new Padding(0, 4, 8, 4);
        btnMobileTab.Name = "btnMobileTab";
        btnMobileTab.Size = new Size(140, 40);
        btnMobileTab.Text = "Mobile Money";
        //
        // numAmount
        //
        numAmount.BackColor = Color.FromArgb(0x25, 0x20, 0x38);
        numAmount.BorderStyle = BorderStyle.None;
        numAmount.DecimalPlaces = 2;
        numAmount.Dock = DockStyle.Fill;
        numAmount.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
        numAmount.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        numAmount.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
        numAmount.Name = "numAmount";
        numAmount.ThousandsSeparator = true;
        //
        // flpQuick
        //
        flpQuick.Dock = DockStyle.Fill;
        flpQuick.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        flpQuick.Name = "flpQuick";
        //
        // lblChangeCaption
        //
        lblChangeCaption.Dock = DockStyle.Fill;
        lblChangeCaption.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblChangeCaption.Name = "lblChangeCaption";
        lblChangeCaption.Text = "Change";
        lblChangeCaption.TextAlign = ContentAlignment.BottomLeft;
        //
        // lblChange
        //
        lblChange.Dock = DockStyle.Fill;
        lblChange.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
        lblChange.ForeColor = Color.FromArgb(0x22, 0xC5, 0x5E);
        lblChange.Name = "lblChange";
        lblChange.Text = "ETB 0.00";
        lblChange.TextAlign = ContentAlignment.MiddleLeft;
        //
        // flpButtons
        //
        flpButtons.Controls.Add(btnPay);
        flpButtons.Controls.Add(btnBack);
        flpButtons.Dock = DockStyle.Fill;
        flpButtons.FlowDirection = FlowDirection.RightToLeft;
        flpButtons.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        flpButtons.Name = "flpButtons";
        flpButtons.WrapContents = false;
        //
        // btnPay
        //
        btnPay.Badge = "Enter";
        btnPay.FillColor = Color.FromArgb(0xF5, 0x7C, 0x1F);
        btnPay.IsPrimary = true;
        btnPay.Name = "btnPay";
        btnPay.Size = new Size(150, 44);
        btnPay.Text = "Pay";
        //
        // btnBack
        //
        btnBack.BorderColor = Color.FromArgb(0x2E, 0x29, 0x45);
        btnBack.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnBack.Name = "btnBack";
        btnBack.Size = new Size(110, 44);
        btnBack.Text = "Back";
        //
        // PaymentDialog
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(480, 460);
        Controls.Add(pnlSurface);
        Name = "PaymentDialog";
        Text = "AbayPOS – Payment";
        pnlSurface.ResumeLayout(false);
        tbl.ResumeLayout(false);
        tbl.PerformLayout();
        flpTabs.ResumeLayout(false);
        ((ISupportInitialize)numAmount).EndInit();
        flpButtons.ResumeLayout(false);
        ResumeLayout(false);
    }

    private GlassPanel pnlSurface = null!;
    private TableLayoutPanel tbl = null!;
    private Label lblTitle = null!;
    private FittedText txtDue = null!;
    private FlowLayoutPanel flpTabs = null!;
    private RoundButton btnCashTab = null!;
    private RoundButton btnMobileTab = null!;
    private NumericUpDown numAmount = null!;
    private FlowLayoutPanel flpQuick = null!;
    private Label lblChangeCaption = null!;
    private Label lblChange = null!;
    private FlowLayoutPanel flpButtons = null!;
    private RoundButton btnPay = null!;
    private RoundButton btnBack = null!;
}