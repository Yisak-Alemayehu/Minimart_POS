using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

internal sealed class KpiCard : GlassPanel
{
    private readonly Label _value = new();

    public KpiCard(string title)
    {
        Padding = new Padding(16);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Theme.Base };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        layout.Controls.Add(new Label { Text = title, Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, Font = Theme.FontHint(), TextAlign = ContentAlignment.BottomLeft }, 0, 0);
        _value.Dock = DockStyle.Fill;
        _value.ForeColor = Theme.Text;
        _value.Font = Theme.Font(18, FontStyle.Bold);
        _value.TextAlign = ContentAlignment.TopLeft;
        layout.Controls.Add(_value, 0, 1);
        Controls.Add(layout);
    }

    public void SetValue(string value) => _value.Text = value;
}
