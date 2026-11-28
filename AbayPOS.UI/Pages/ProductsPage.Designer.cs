using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Pages;

partial class ProductsPage
{
    private void InitializeComponent()
    {
        pnlGlass = new GlassPanel();
        tlpMain = new TableLayoutPanel();
        lblHeading = new Label();
        flpToolbar = new FlowLayoutPanel();
        txtSearch = new GlassTextBox();
        btnAdd = new RoundButton();
        btnEdit = new RoundButton();
        btnDeactivate = new RoundButton();
        dgvProducts = new DataGridView();
        pnlGlass.SuspendLayout();
        tlpMain.SuspendLayout();
        flpToolbar.SuspendLayout();
        ((ISupportInitialize)dgvProducts).BeginInit();
        SuspendLayout();
        pnlGlass.Controls.Add(tlpMain);
        pnlGlass.Dock = DockStyle.Fill;
        pnlGlass.Padding = new Padding(16);
        pnlGlass.Name = "pnlGlass";
        tlpMain.ColumnCount = 1;
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpMain.Controls.Add(lblHeading, 0, 0);
        tlpMain.Controls.Add(flpToolbar, 0, 1);
        tlpMain.Controls.Add(dgvProducts, 0, 2);
        tlpMain.Dock = DockStyle.Fill;
        tlpMain.RowCount = 3;
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpMain.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        tlpMain.Name = "tlpMain";
        lblHeading.Dock = DockStyle.Fill;
        lblHeading.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold);
        lblHeading.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        lblHeading.Text = "Products";
        lblHeading.Name = "lblHeading";
        flpToolbar.Controls.Add(txtSearch);
        flpToolbar.Controls.Add(btnAdd);
        flpToolbar.Controls.Add(btnEdit);
        flpToolbar.Controls.Add(btnDeactivate);
        flpToolbar.Dock = DockStyle.Fill;
        flpToolbar.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        flpToolbar.Name = "flpToolbar";
        txtSearch.Name = "txtSearch";
        txtSearch.Placeholder = "Search name or barcode";
        txtSearch.Size = new Size(280, 40);
        btnAdd.IsPrimary = true;
        btnAdd.Name = "btnAdd";
        btnAdd.Size = new Size(100, 40);
        btnAdd.Text = "Add";
        btnEdit.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnEdit.Name = "btnEdit";
        btnEdit.Size = new Size(100, 40);
        btnEdit.Text = "Edit";
        btnDeactivate.BorderColor = Color.FromArgb(0x2E, 0x29, 0x45);
        btnDeactivate.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnDeactivate.Name = "btnDeactivate";
        btnDeactivate.Size = new Size(130, 40);
        btnDeactivate.Text = "Deactivate";
        dgvProducts.Dock = DockStyle.Fill;
        dgvProducts.Name = "dgvProducts";
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        Controls.Add(pnlGlass);
        Name = "ProductsPage";
        Size = new Size(900, 600);
        pnlGlass.ResumeLayout(false);
        tlpMain.ResumeLayout(false);
        flpToolbar.ResumeLayout(false);
        ((ISupportInitialize)dgvProducts).EndInit();
        ResumeLayout(false);
    }

    private GlassPanel pnlGlass = null!;
    private TableLayoutPanel tlpMain = null!;
    private Label lblHeading = null!;
    private FlowLayoutPanel flpToolbar = null!;
    private GlassTextBox txtSearch = null!;
    private RoundButton btnAdd = null!;
    private RoundButton btnEdit = null!;
    private RoundButton btnDeactivate = null!;
    private DataGridView dgvProducts = null!;
}