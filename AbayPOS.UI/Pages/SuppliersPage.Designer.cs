#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Pages;

partial class SuppliersPage
{
    private void InitializeComponent()
    {
        pnlGlass = new GlassPanel();
        tbl = new TableLayoutPanel();
        lblHeading = new Label();
        flpToolbar = new FlowLayoutPanel();
        btnAdd = new RoundButton();
        btnEdit = new RoundButton();
        dgvSuppliers = new DataGridView();
        pnlGlass.SuspendLayout();
        tbl.SuspendLayout();
        flpToolbar.SuspendLayout();
        ((ISupportInitialize)dgvSuppliers).BeginInit();
        SuspendLayout();
        pnlGlass.Controls.Add(tbl);
        pnlGlass.Dock = DockStyle.Fill;
        pnlGlass.FillColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        pnlGlass.Name = "pnlGlass";
        pnlGlass.Padding = new Padding(16);
        tbl.ColumnCount = 1;
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tbl.Controls.Add(lblHeading, 0, 0);
        tbl.Controls.Add(flpToolbar, 0, 1);
        tbl.Controls.Add(dgvSuppliers, 0, 2);
        tbl.Dock = DockStyle.Fill;
        tbl.Name = "tbl";
        tbl.RowCount = 3;
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tbl.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        lblHeading.Dock = DockStyle.Fill;
        lblHeading.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
        lblHeading.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        lblHeading.Name = "lblHeading";
        lblHeading.Text = "Suppliers";
        flpToolbar.Controls.Add(btnAdd);
        flpToolbar.Controls.Add(btnEdit);
        flpToolbar.Dock = DockStyle.Fill;
        flpToolbar.Name = "flpToolbar";
        btnAdd.Height = 40;
        btnAdd.IsPrimary = true;
        btnAdd.Name = "btnAdd";
        btnAdd.Text = "Add";
        btnAdd.Width = 100;
        btnEdit.Height = 40;
        btnEdit.Name = "btnEdit";
        btnEdit.Text = "Edit";
        btnEdit.Width = 100;
        dgvSuppliers.Dock = DockStyle.Fill;
        dgvSuppliers.Name = "dgvSuppliers";
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        Controls.Add(pnlGlass);
        Dock = DockStyle.Fill;
        Name = "SuppliersPage";
        Size = new Size(800, 500);
        pnlGlass.ResumeLayout(false);
        tbl.ResumeLayout(false);
        flpToolbar.ResumeLayout(false);
        ((ISupportInitialize)dgvSuppliers).EndInit();
        ResumeLayout(false);
    }

    private GlassPanel pnlGlass = null!;
    private TableLayoutPanel tbl = null!;
    private Label lblHeading = null!;
    private FlowLayoutPanel flpToolbar = null!;
    private RoundButton btnAdd = null!;
    private RoundButton btnEdit = null!;
    private DataGridView dgvSuppliers = null!;
}