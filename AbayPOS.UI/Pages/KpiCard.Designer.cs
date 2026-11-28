using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

partial class KpiCard
{
    private void InitializeComponent()
    {
        tbl = new TableLayoutPanel();
        lblTitle = new Label();
        txtValue = new FittedText();
        SuspendLayout();
        //
        // tbl
        //
        tbl.ColumnCount = 1;
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tbl.Controls.Add(lblTitle, 0, 0);
        tbl.Controls.Add(txtValue, 0, 1);
        tbl.Dock = DockStyle.Fill;
        tbl.RowCount = 2;
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tbl.BackColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        //
        // lblTitle
        //
        lblTitle.AutoEllipsis = true;
        lblTitle.AutoSize = true;
        lblTitle.Dock = DockStyle.Fill;
        lblTitle.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        lblTitle.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblTitle.Margin = new Padding(0);
        lblTitle.MaximumSize = new Size(220, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Text = "KPI";
        //
        // txtValue
        //
        txtValue.Dock = DockStyle.Fill;
        txtValue.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        txtValue.MinSize = 14F;
        txtValue.Money = true;
        txtValue.Name = "txtValue";
        txtValue.StartSize = 24F;
        txtValue.Text = "ETB 12,450.00";
        //
        // KpiCard
        //
        BackColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        Controls.Add(tbl);
        FillColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        Padding = new Padding(16);
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel tbl;
    private Label lblTitle;
    private FittedText txtValue;
}