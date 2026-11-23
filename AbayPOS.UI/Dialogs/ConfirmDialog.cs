using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>Frosted confirm used only for destructive actions.</summary>
public sealed class ConfirmDialog : DialogForm
{
    private ConfirmDialog(string title, string message, string confirmText, bool danger)
    {
        Text = title;
        ClientSize = new Size(420, 220);
        BackColor = Theme.Base;
        DoubleBuffered = true;
        KeyPreview = true;

        var surface = new GlassPanel { Dock = DockStyle.Fill, Padding = new Padding(24) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, BackColor = Theme.Base };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = title, Dock = DockStyle.Fill, Font = Theme.FontHeading(), ForeColor = Theme.Text, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        layout.Controls.Add(new Label { Text = message, Dock = DockStyle.Fill, Font = Theme.FontBody(), ForeColor = Theme.TextSecondary }, 0, 1);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Base };
        var ok = new RoundButton
        {
            Text = confirmText,
            Width = 140,
            Height = 44,
            FillColor = danger ? Theme.Danger : Theme.Accent,
            IsPrimary = true
        };
        ok.Click += (_, _) => { DialogResult = DialogResult.Yes; Close(); };
        var cancel = new RoundButton { Text = "Keep it", Width = 120, Height = 44, FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => { DialogResult = DialogResult.No; Close(); };
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        layout.Controls.Add(buttons, 0, 2);
        surface.Controls.Add(layout);
        Controls.Add(surface);
        Surface.Adopt(this, Theme.Base);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    public static bool Ask(IWin32Window owner, string title, string message, string confirmText = "Yes", bool danger = true)
    {
        using var overlay = CreateOverlay(owner);
        overlay?.Show();
        using var dialog = new ConfirmDialog(title, message, confirmText, danger);
        var result = dialog.ShowDialog(owner);
        overlay?.Close();
        return result == DialogResult.Yes;
    }

    internal static Form? CreateOverlay(IWin32Window owner)
    {
        if (owner is not Form form)
            return null;
        return new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Bounds = form.RectangleToScreen(form.ClientRectangle),
            BackColor = Color.Black,
            Opacity = 0.45,
            Owner = form
        };
    }
}
