using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Dialogs;

partial class UserEditDialog
{
    private void InitializeComponent()
    {
        pnlSurface = new GlassPanel();
        tbl = new TableLayoutPanel();
        lblUsernameCaption = new Label();
        txtUsername = new GlassTextBox();
        lblFullNameCaption = new Label();
        txtFullName = new GlassTextBox();
        lblRoleCaption = new Label();
        cmbRole = new ComboBox();
        lblPasswordCaption = new Label();
        txtPassword = new GlassTextBox();
        lblError = new Label();
        flpButtons = new FlowLayoutPanel();
        btnSave = new RoundButton();
        btnCancel = new RoundButton();
        pnlSurface.SuspendLayout();
        tbl.SuspendLayout();
        flpButtons.SuspendLayout();
        SuspendLayout();
        //
        // pnlSurface
        //
        pnlSurface.AutoSize = true;
        pnlSurface.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        pnlSurface.Controls.Add(tbl);
        pnlSurface.Dock = DockStyle.Top;
        pnlSurface.FillColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        pnlSurface.Padding = new Padding(24);
        pnlSurface.Name = "pnlSurface";
        //
        // tbl
        //
        tbl.AutoSize = true;
        tbl.ColumnCount = 1;
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tbl.Controls.Add(lblUsernameCaption, 0, 0);
        tbl.Controls.Add(txtUsername, 0, 1);
        tbl.Controls.Add(lblFullNameCaption, 0, 2);
        tbl.Controls.Add(txtFullName, 0, 3);
        tbl.Controls.Add(lblRoleCaption, 0, 4);
        tbl.Controls.Add(cmbRole, 0, 5);
        tbl.Controls.Add(lblPasswordCaption, 0, 6);
        tbl.Controls.Add(txtPassword, 0, 7);
        tbl.Controls.Add(lblError, 0, 8);
        tbl.Controls.Add(flpButtons, 0, 9);
        tbl.Dock = DockStyle.Top;
        tbl.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        tbl.RowCount = 10;
        tbl.RowStyles.Clear();
        for (var i = 0; i < 10; i++)
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.Name = "tbl";
        //
        // lblUsernameCaption
        //
        lblUsernameCaption.AutoSize = true;
        lblUsernameCaption.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblUsernameCaption.Margin = new Padding(0, 8, 0, 2);
        lblUsernameCaption.Name = "lblUsernameCaption";
        lblUsernameCaption.Text = "Username";
        //
        // txtUsername
        //
        txtUsername.Dock = DockStyle.Top;
        txtUsername.Margin = new Padding(0, 0, 0, 4);
        txtUsername.MinimumSize = new Size(280, 32);
        txtUsername.Name = "txtUsername";
        txtUsername.Placeholder = "Username";
        //
        // lblFullNameCaption
        //
        lblFullNameCaption.AutoSize = true;
        lblFullNameCaption.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblFullNameCaption.Margin = new Padding(0, 8, 0, 2);
        lblFullNameCaption.Name = "lblFullNameCaption";
        lblFullNameCaption.Text = "Full name";
        //
        // txtFullName
        //
        txtFullName.Dock = DockStyle.Top;
        txtFullName.Margin = new Padding(0, 0, 0, 4);
        txtFullName.MinimumSize = new Size(280, 32);
        txtFullName.Name = "txtFullName";
        txtFullName.Placeholder = "Full name";
        //
        // lblRoleCaption
        //
        lblRoleCaption.AutoSize = true;
        lblRoleCaption.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblRoleCaption.Margin = new Padding(0, 8, 0, 2);
        lblRoleCaption.Name = "lblRoleCaption";
        lblRoleCaption.Text = "Role";
        //
        // cmbRole
        //
        cmbRole.BackColor = Color.FromArgb(0x25, 0x20, 0x38);
        cmbRole.Dock = DockStyle.Top;
        cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbRole.FlatStyle = FlatStyle.Flat;
        cmbRole.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        cmbRole.Margin = new Padding(0, 0, 0, 4);
        cmbRole.MinimumSize = new Size(280, 32);
        cmbRole.Name = "cmbRole";
        //
        // lblPasswordCaption
        //
        lblPasswordCaption.AutoSize = true;
        lblPasswordCaption.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblPasswordCaption.Margin = new Padding(0, 8, 0, 2);
        lblPasswordCaption.Name = "lblPasswordCaption";
        lblPasswordCaption.Text = "Password";
        //
        // txtPassword
        //
        txtPassword.Dock = DockStyle.Top;
        txtPassword.Margin = new Padding(0, 0, 0, 4);
        txtPassword.MinimumSize = new Size(280, 32);
        txtPassword.Name = "txtPassword";
        txtPassword.Placeholder = "Password";
        txtPassword.UseSystemPasswordChar = true;
        //
        // lblError
        //
        lblError.AutoSize = true;
        lblError.ForeColor = Color.FromArgb(0xEF, 0x44, 0x44);
        lblError.MaximumSize = new Size(360, 0);
        lblError.Name = "lblError";
        //
        // flpButtons
        //
        flpButtons.AutoSize = true;
        flpButtons.Controls.Add(btnSave);
        flpButtons.Controls.Add(btnCancel);
        flpButtons.FlowDirection = FlowDirection.RightToLeft;
        flpButtons.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        flpButtons.Name = "flpButtons";
        flpButtons.WrapContents = false;
        //
        // btnSave
        //
        btnSave.FillColor = Color.FromArgb(0xF5, 0x7C, 0x1F);
        btnSave.IsPrimary = true;
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(120, 44);
        btnSave.Text = "Save";
        //
        // btnCancel
        //
        btnCancel.BorderColor = Color.FromArgb(0x2E, 0x29, 0x45);
        btnCancel.FillColor = Color.FromArgb(0x25, 0x20, 0x38);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(110, 44);
        btnCancel.Text = "Cancel";
        //
        // UserEditDialog
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Controls.Add(pnlSurface);
        MinimumSize = new Size(420, 320);
        Name = "UserEditDialog";
        Text = "AbayPOS – Add user";
        pnlSurface.ResumeLayout(false);
        pnlSurface.PerformLayout();
        tbl.ResumeLayout(false);
        tbl.PerformLayout();
        flpButtons.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private GlassPanel pnlSurface;
    private TableLayoutPanel tbl;
    private Label lblUsernameCaption;
    private GlassTextBox txtUsername;
    private Label lblFullNameCaption;
    private GlassTextBox txtFullName;
    private Label lblRoleCaption;
    private ComboBox cmbRole;
    private Label lblPasswordCaption;
    private GlassTextBox txtPassword;
    private Label lblError;
    private FlowLayoutPanel flpButtons;
    private RoundButton btnSave;
    private RoundButton btnCancel;
}