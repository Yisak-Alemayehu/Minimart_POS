using AbayPOS.Domain.Entities;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Printing;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>UC-05 receipt preview with print or skip.</summary>
public sealed partial class ReceiptPreviewDialog : DialogForm
{
    private ReceiptPrinter? _printer;
    private Sale? _sale;

    public ReceiptPreviewDialog()
    {
        InitializeComponent();
        btnPrint.Click += (_, _) =>
        {
            if (_sale is null || _printer is null)
                return;
            try
            {
                _printer.Print(_sale);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ToastHost.Error("Printer is not ready. You can skip printing. " + ex.Message);
            }
        };
        btnSkip.Click += (_, _) => { DialogResult = DialogResult.Ignore; Close(); };
        CancelButton = btnSkip;
        AcceptButton = btnPrint;
    }

    public ReceiptPreviewDialog(Sale sale) : this()
    {
        _sale = sale;
        _printer = new ReceiptPrinter();
        previewReceipt.Document = _printer.CreateDocument(sale);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
    }
}