using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Dialogs;

partial class ConfirmDialog
{
    private void InitializeComponent()
    {
        pnlSurface = new GlassPanel();
        tbl = new TableLayoutPanel();
        lblTitle = new Label();
        lblMessage = new Label();
        flpButtons = new FlowLayoutPanel();
        btnConfirm = new RoundButton();
        btnKeep = new RoundButton();
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
        pnlSurface.Padding = new Padding(24);
        pnlSurface.Name = "pnlSurface";
        //
        // tbl
        //
        tbl.ColumnCount = 1;
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tbl.Controls.Add(lblTitle, 0, 0);
        tbl.Controls.Add(lblMessage, 0, 1);
        tbl.Controls.Add(flpButtons, 0, 2);
        tbl.Dock = DockStyle.Fill;
        tbl.RowCount = 3;
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
        lblTitle.Text = "Confirm";
        lblTitle.TextAlign = ContentAlignment.MiddleLeft;
        //
        // lblMessage
        //
        lblMessage.Dock = DockStyle.Fill;
        lblMessage.Font = new Font("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point);
        lblMessage.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblMessage.Name = "lblMessage";
        lblMessage.Text = "Are you sure?";
        //
        // flpButtons
        //
        flpButtons.Controls.Add(btnConfirm);
        flpButtons.Controls.Add(btnKeep);
        flpButtons.Dock = DockStyle.Fill;
        flpButtons.FlowDirection = FlowDirection.RightToLeft;
        flpButtons.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        flpButtons.Name = "flpButtons";
        flpButtons.WrapContents = false;
        //
        // btnConfirm
        //
        btnConfirm.FillColor = Color.FromArgb(0xEF, 0x44, 0x44);
        btnConfirm.IsPrimary = true;
        btnConfirm.Name = "btnConfirm";
        btnConfirm.Size = new Size(140, 44);
        btnConfirm.Text = "Yes";
        //
        // btnKeep
        //
        btnKeep.BorderColor = Color.FromArgb(0x2E, 0x29, 0x45);
        btnKeep.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnKeep.Name = "btnKeep";
        btnKeep.Size = new Size(120, 44);
        btnKeep.Text = "Keep it";
        //
        // ConfirmDialog
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(420, 220);
        Controls.Add(pnlSurface);
        Name = "ConfirmDialog";
        Text = "Confirm";
        pnlSurface.ResumeLayout(false);
        tbl.ResumeLayout(false);
        tbl.PerformLayout();
        flpButtons.ResumeLayout(false);
        ResumeLayout(false);
    }

    private GlassPanel pnlSurface;
    private TableLayoutPanel tbl;
    private Label lblTitle;
    private Label lblMessage;
    private FlowLayoutPanel flpButtons;
    private RoundButton btnConfirm;
    private RoundButton btnKeep;
}