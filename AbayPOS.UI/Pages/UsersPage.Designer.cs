using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Pages;

partial class UsersPage
{
    private void InitializeComponent()
    {
        pnlGlass = new GlassPanel();
        tbl = new TableLayoutPanel();
        lblHeading = new Label();
        flpToolbar = new FlowLayoutPanel();
        btnAdd = new RoundButton();
        btnEdit = new RoundButton();
        btnDeactivate = new RoundButton();
        btnUnlock = new RoundButton();
        btnResetPassword = new RoundButton();
        dgvUsers = new DataGridView();
        pnlGlass.SuspendLayout();
        tbl.SuspendLayout();
        flpToolbar.SuspendLayout();
        ((ISupportInitialize)dgvUsers).BeginInit();
        SuspendLayout();
        pnlGlass.Controls.Add(tbl);
        pnlGlass.Dock = DockStyle.Fill;
        pnlGlass.FillColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        pnlGlass.Padding = new Padding(16);
        tbl.ColumnCount = 1;
        tbl.RowCount = 3;
        tbl.Dock = DockStyle.Fill;
        tbl.Controls.Add(lblHeading, 0, 0);
        tbl.Controls.Add(flpToolbar, 0, 1);
        tbl.Controls.Add(dgvUsers, 0, 2);
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tbl.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        lblHeading.Dock = DockStyle.Fill;
        lblHeading.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold);
        lblHeading.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        lblHeading.Text = "Users";
        flpToolbar.Dock = DockStyle.Fill;
        flpToolbar.Controls.Add(btnAdd);
        flpToolbar.Controls.Add(btnEdit);
        flpToolbar.Controls.Add(btnDeactivate);
        flpToolbar.Controls.Add(btnUnlock);
        flpToolbar.Controls.Add(btnResetPassword);
        btnAdd.Text = "Add"; btnAdd.Width = 100; btnAdd.Height = 40; btnAdd.IsPrimary = true;
        btnEdit.Text = "Edit"; btnEdit.Width = 100; btnEdit.Height = 40;
        btnDeactivate.Text = "Deactivate"; btnDeactivate.Width = 130; btnDeactivate.Height = 40;
        btnUnlock.Text = "Unlock"; btnUnlock.Width = 100; btnUnlock.Height = 40;
        btnResetPassword.Text = "Reset password"; btnResetPassword.Width = 140; btnResetPassword.Height = 40;
        dgvUsers.Dock = DockStyle.Fill;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        Controls.Add(pnlGlass);
        Dock = DockStyle.Fill;
        pnlGlass.ResumeLayout(false);
        tbl.ResumeLayout(false);
        flpToolbar.ResumeLayout(false);
        ((ISupportInitialize)dgvUsers).EndInit();
        ResumeLayout(false);
    }

    private GlassPanel pnlGlass = null!;
    private TableLayoutPanel tbl = null!;
    private Label lblHeading = null!;
    private FlowLayoutPanel flpToolbar = null!;
    private RoundButton btnAdd = null!;
    private RoundButton btnEdit = null!;
    private RoundButton btnDeactivate = null!;
    private RoundButton btnUnlock = null!;
    private RoundButton btnResetPassword = null!;
    private DataGridView dgvUsers = null!;
}