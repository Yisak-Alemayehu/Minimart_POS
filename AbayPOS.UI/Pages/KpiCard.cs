using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

internal sealed class KpiCard : GlassPanel
{
    private readonly FittedText _value = new() { ForeColor = Theme.Text, StartSize = 24f, MinSize = 14f, Money = true };

    public KpiCard(string title)
    {
        Padding = new Padding(Theme.Scale(16));
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Theme.Card };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var heading = new Label
        {
            Text = title,
            AutoSize = true,
            AutoEllipsis = true,
            MaximumSize = new Size(Theme.Scale(220), 0),
            ForeColor = Theme.TextSecondary,
            Font = Theme.FontHint(),
            Dock = DockStyle.Fill
        };
        new ToolTip().SetToolTip(heading, title);
        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(_value, 0, 1);
        Controls.Add(layout);
    }

    public void SetValue(string value) => _value.SetValue(value);
}
