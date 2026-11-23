using AbayPOS.Domain.Entities;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Printing;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>UC-05 receipt preview with print or skip.</summary>
public sealed class ReceiptPreviewDialog : DialogForm
{
    public ReceiptPreviewDialog(Sale sale)
    {
        Text = "AbayPOS – Receipt";
        ClientSize = new Size(520, 640);
        BackColor = Theme.Base;
        var printer = new ReceiptPrinter();
        var document = printer.CreateDocument(sale);
        var surface = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Theme.Base };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var preview = new PrintPreviewControl
        {
            Dock = DockStyle.Fill,
            Document = document,
            Zoom = 1.2,
            BackColor = Color.White
        };
        layout.Controls.Add(preview, 0, 0);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Base };
        var print = new RoundButton { Text = "Print", Width = 120, Height = 44, IsPrimary = true };
        print.Click += (_, _) =>
        {
            try
            {
                printer.Print(sale);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ToastHost.Error("Printer is not ready. You can skip printing. " + ex.Message);
            }
        };
        var skip = new RoundButton { Text = "Skip", Width = 110, Height = 44, FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        skip.Click += (_, _) => { DialogResult = DialogResult.Ignore; Close(); };
        buttons.Controls.Add(print);
        buttons.Controls.Add(skip);
        layout.Controls.Add(buttons, 0, 1);
        surface.Controls.Add(layout);
        Controls.Add(surface);
        Surface.Adopt(this, Theme.Base);
        CancelButton = skip;
        AcceptButton = print;
    }
}
