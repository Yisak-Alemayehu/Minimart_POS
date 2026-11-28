using AbayPOS.Domain.Entities;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

public sealed partial class SupplierEditDialog : DialogForm
{
    public SupplierEditDialog()
    {
        InitializeComponent();
        btnSave.Click += (_, _) => TrySave();
        btnCancel.Click += (_, _) => Close();
        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    public SupplierEditDialog(Supplier supplier) : this()
    {
        Supplier = supplier;
        Text = supplier.Id == 0 ? "AbayPOS – Add supplier" : "AbayPOS – Edit supplier";
        txtName.Text = supplier.Name;
        txtPhone.Text = supplier.Phone;
        txtEmail.Text = supplier.Email;
        txtAddress.Text = supplier.Address;
        chkActive.Checked = supplier.IsActive;
    }

    public Supplier Supplier { get; private set; } = new();

    private void TrySave()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            lblError.Text = "Enter a supplier name.";
            return;
        }

        Supplier.Name = txtName.Text!.Trim();
        Supplier.Phone = txtPhone.Text;
        Supplier.Email = txtEmail.Text;
        Supplier.Address = txtAddress.Text;
        Supplier.IsActive = chkActive.Checked;
        DialogResult = DialogResult.OK;
        Close();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
    }
}