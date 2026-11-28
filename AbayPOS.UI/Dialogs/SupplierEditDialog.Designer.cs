using System.Drawing;
using System.Windows.Forms;
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Dialogs;

partial class SupplierEditDialog
{
    private void InitializeComponent()
    {
        pnlSurface = new GlassPanel();
        tbl = new TableLayoutPanel();
        lblNameCaption = new Label();
        txtName = new GlassTextBox();
        lblPhoneCaption = new Label();
        txtPhone = new GlassTextBox();
        lblEmailCaption = new Label();
        txtEmail = new GlassTextBox();
        lblAddressCaption = new Label();
        txtAddress = new GlassTextBox();
        chkActive = new CheckBox();
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
        tbl.Controls.Add(lblNameCaption, 0, 0);
        tbl.Controls.Add(txtName, 0, 1);
        tbl.Controls.Add(lblPhoneCaption, 0, 2);
        tbl.Controls.Add(txtPhone, 0, 3);
        tbl.Controls.Add(lblEmailCaption, 0, 4);
        tbl.Controls.Add(txtEmail, 0, 5);
        tbl.Controls.Add(lblAddressCaption, 0, 6);
        tbl.Controls.Add(txtAddress, 0, 7);
        tbl.Controls.Add(chkActive, 0, 8);
        tbl.Controls.Add(lblError, 0, 9);
        tbl.Controls.Add(flpButtons, 0, 10);
        tbl.Dock = DockStyle.Top;
        tbl.BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
        tbl.RowCount = 11;
        tbl.RowStyles.Clear();
        for (var i = 0; i < 11; i++)
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.Name = "tbl";
        //
        // lblNameCaption
        //
        lblNameCaption.AutoSize = true;
        lblNameCaption.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblNameCaption.Margin = new Padding(0, 8, 0, 2);
        lblNameCaption.Name = "lblNameCaption";
        lblNameCaption.Text = "Name";
        //
        // txtName
        //
        txtName.Dock = DockStyle.Top;
        txtName.Margin = new Padding(0, 0, 0, 4);
        txtName.MinimumSize = new Size(280, 32);
        txtName.Name = "txtName";
        txtName.Placeholder = "Name";
        //
        // lblPhoneCaption
        //
        lblPhoneCaption.AutoSize = true;
        lblPhoneCaption.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblPhoneCaption.Margin = new Padding(0, 8, 0, 2);
        lblPhoneCaption.Name = "lblPhoneCaption";
        lblPhoneCaption.Text = "Phone";
        //
        // txtPhone
        //
        txtPhone.Dock = DockStyle.Top;
        txtPhone.Margin = new Padding(0, 0, 0, 4);
        txtPhone.MinimumSize = new Size(280, 32);
        txtPhone.Name = "txtPhone";
        txtPhone.Placeholder = "Phone";
        //
        // lblEmailCaption
        //
        lblEmailCaption.AutoSize = true;
        lblEmailCaption.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblEmailCaption.Margin = new Padding(0, 8, 0, 2);
        lblEmailCaption.Name = "lblEmailCaption";
        lblEmailCaption.Text = "Email";
        //
        // txtEmail
        //
        txtEmail.Dock = DockStyle.Top;
        txtEmail.Margin = new Padding(0, 0, 0, 4);
        txtEmail.MinimumSize = new Size(280, 32);
        txtEmail.Name = "txtEmail";
        txtEmail.Placeholder = "Email";
        //
        // lblAddressCaption
        //
        lblAddressCaption.AutoSize = true;
        lblAddressCaption.ForeColor = Color.FromArgb(0xA1, 0xA1, 0xAA);
        lblAddressCaption.Margin = new Padding(0, 8, 0, 2);
        lblAddressCaption.Name = "lblAddressCaption";
        lblAddressCaption.Text = "Address";
        //
        // txtAddress
        //
        txtAddress.Dock = DockStyle.Top;
        txtAddress.Margin = new Padding(0, 0, 0, 4);
        txtAddress.MinimumSize = new Size(280, 32);
        txtAddress.Name = "txtAddress";
        txtAddress.Placeholder = "Address";
        //
        // chkActive
        //
        chkActive.AutoSize = true;
        chkActive.Checked = true;
        chkActive.CheckState = CheckState.Checked;
        chkActive.ForeColor = Color.FromArgb(0xF5, 0xF5, 0xF7);
        chkActive.Name = "chkActive";
        chkActive.Text = "Active";
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
        // SupplierEditDialog
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Controls.Add(pnlSurface);
        MinimumSize = new Size(420, 360);
        Name = "SupplierEditDialog";
        Text = "AbayPOS – Add supplier";
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
    private Label lblNameCaption;
    private GlassTextBox txtName;
    private Label lblPhoneCaption;
    private GlassTextBox txtPhone;
    private Label lblEmailCaption;
    private GlassTextBox txtEmail;
    private Label lblAddressCaption;
    private GlassTextBox txtAddress;
    private CheckBox chkActive;
    private Label lblError;
    private FlowLayoutPanel flpButtons;
    private RoundButton btnSave;
    private RoundButton btnCancel;
}