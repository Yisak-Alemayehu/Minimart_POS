#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Pages;

partial class SalesHistoryPage
{
    private void InitializeComponent()
    {
        pnlGlass = new GlassPanel();
        tbl = new TableLayoutPanel();
        lblHeading = new Label();
        flpToolbar = new FlowLayoutPanel();
        dtpFrom = new DateTimePicker();
        dtpTo = new DateTimePicker();
        txtSaleNo = new GlassTextBox();
        cmbCashier = new ComboBox();
        btnFilter = new RoundButton();
        btnReprint = new RoundButton();
        btnVoid = new RoundButton();
        dgvSales = new DataGridView();
        dgvItems = new DataGridView();
        pnlGlass.SuspendLayout();
        tbl.SuspendLayout();
        flpToolbar.SuspendLayout();
        ((ISupportInitialize)dgvSales).BeginInit();
        ((ISupportInitialize)dgvItems).BeginInit();
        SuspendLayout();
        pnlGlass.Controls.Add(tbl);
        pnlGlass.Dock = DockStyle.Fill;
        pnlGlass.FillColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        pnlGlass.Padding = new Padding(16);
        tbl.ColumnCount = 1;
        tbl.RowCount = 4;
        tbl.Dock = DockStyle.Fill;
        tbl.Controls.Add(lblHeading, 0, 0);
        tbl.Controls.Add(flpToolbar, 0, 1);
        tbl.Controls.Add(dgvSales, 0, 2);
        tbl.Controls.Add(dgvItems, 0, 3);
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
        tbl.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        lblHeading.Dock = DockStyle.Fill;
        lblHeading.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold);
        lblHeading.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        lblHeading.Text = "Sales history";
        flpToolbar.Dock = DockStyle.Fill;
        flpToolbar.Controls.AddRange([dtpFrom, dtpTo, txtSaleNo, cmbCashier, btnFilter, btnReprint, btnVoid]);
        txtSaleNo.Width = 140; txtSaleNo.Height = 36; txtSaleNo.Placeholder = "Sale no";
        cmbCashier.Width = 160; cmbCashier.DropDownStyle = ComboBoxStyle.DropDownList; cmbCashier.FlatStyle = FlatStyle.Flat;
        btnFilter.Text = "Filter"; btnFilter.Width = 90; btnFilter.Height = 36; btnFilter.IsPrimary = true;
        btnReprint.Text = "Reprint"; btnReprint.Width = 100; btnReprint.Height = 36;
        btnVoid.Text = "Void"; btnVoid.Width = 90; btnVoid.Height = 36;
        dgvSales.Dock = DockStyle.Fill;
        dgvItems.Dock = DockStyle.Fill;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        Controls.Add(pnlGlass);
        Dock = DockStyle.Fill;
        Size = new Size(900, 600);
        pnlGlass.ResumeLayout(false);
        tbl.ResumeLayout(false);
        flpToolbar.ResumeLayout(false);
        ((ISupportInitialize)dgvSales).EndInit();
        ((ISupportInitialize)dgvItems).EndInit();
        ResumeLayout(false);
    }

    private GlassPanel pnlGlass = null!;
    private TableLayoutPanel tbl = null!;
    private Label lblHeading = null!;
    private FlowLayoutPanel flpToolbar = null!;
    private DateTimePicker dtpFrom = null!;
    private DateTimePicker dtpTo = null!;
    private GlassTextBox txtSaleNo = null!;
    private ComboBox cmbCashier = null!;
    private RoundButton btnFilter = null!;
    private RoundButton btnReprint = null!;
    private RoundButton btnVoid = null!;
    private DataGridView dgvSales = null!;
    private DataGridView dgvItems = null!;
}
