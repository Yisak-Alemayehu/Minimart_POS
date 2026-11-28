#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Pages;

partial class LowStockPage
{
    private void InitializeComponent()
    {
        pnlGlass = new GlassPanel();
        tbl = new TableLayoutPanel();
        lblHeading = new Label();
        dgvLowStock = new DataGridView();
        pnlGlass.SuspendLayout();
        tbl.SuspendLayout();
        ((ISupportInitialize)dgvLowStock).BeginInit();
        SuspendLayout();
        pnlGlass.Controls.Add(tbl);
        pnlGlass.Dock = DockStyle.Fill;
        pnlGlass.FillColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        pnlGlass.Name = "pnlGlass";
        pnlGlass.Padding = new Padding(16);
        tbl.ColumnCount = 1;
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tbl.Controls.Add(lblHeading, 0, 0);
        tbl.Controls.Add(dgvLowStock, 0, 1);
        tbl.Dock = DockStyle.Fill;
        tbl.Name = "tbl";
        tbl.RowCount = 2;
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tbl.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        lblHeading.Dock = DockStyle.Fill;
        lblHeading.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
        lblHeading.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        lblHeading.Name = "lblHeading";
        lblHeading.Text = "Low stock";
        dgvLowStock.Dock = DockStyle.Fill;
        dgvLowStock.Name = "dgvLowStock";
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        Controls.Add(pnlGlass);
        Dock = DockStyle.Fill;
        Name = "LowStockPage";
        Size = new Size(800, 500);
        pnlGlass.ResumeLayout(false);
        tbl.ResumeLayout(false);
        ((ISupportInitialize)dgvLowStock).EndInit();
        ResumeLayout(false);
    }

    private GlassPanel pnlGlass = null!;
    private TableLayoutPanel tbl = null!;
    private Label lblHeading = null!;
    private DataGridView dgvLowStock = null!;
}
