using AbayPOS.Domain.Entities;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Printing;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>UC-05 receipt preview with print or skip.</summary>
public sealed class ReceiptPreviewDialog : Form
{
    public ReceiptPreviewDialog(Sale sale)
    {
        Text = "Receipt";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        ClientSize = new Size(520, 640);
        BackColor = Theme.Base;
        var printer = new ReceiptPrinter();
        var document = printer.CreateDocument(sale);
        var surface = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        var preview = new PrintPreviewControl
        {
            Dock = DockStyle.Fill,
            Document = document,
            Zoom = 1.2,
            BackColor = Color.White
        };
        layout.Controls.Add(preview, 0, 0);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
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
        Load += (_, _) => GlassHelper.ApplyToWindow(this);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        GlassHelper.PaintFormBackground(this, e.Graphics);
}
