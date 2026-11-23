namespace AbayPOS.UI.Theming;

/// <summary>Paints each child with its parent's solid color so captions are drawn once.</summary>
public static class Surface
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, ColorBox> Hooks = new();

    public static void Adopt(Control control, Color surface)
    {
        Color childSurface;
        if (control is Controls.GlassPanel)
            childSurface = control.BackColor;
        else if (Skip(control))
            return;
        else
        {
            control.BackColor = Theme.Solid(surface);
            childSurface = control.BackColor;
        }

        Watch(control, childSurface);
        foreach (Control child in control.Controls)
            Adopt(child, childSurface);
    }

    private static void Watch(Control control, Color color)
    {
        if (!Hooks.TryGetValue(control, out var box))
        {
            box = new ColorBox();
            Hooks.Add(control, box);
            control.ControlAdded += (_, args) =>
            {
                if (args.Control is not null && Hooks.TryGetValue(control, out var current))
                    Adopt(args.Control, current.Color);
            };
        }

        box.Color = color;
    }

    private static bool Skip(Control control) =>
        control is Controls.RoundButton
            or Controls.ChipButton
            or Controls.WindowButton
            or Controls.ProductTile
            or Controls.GlassTextBox
            or Controls.LoginField
            or Controls.BarChart
            or Controls.CartList
            or DataGridView
            or NumericUpDown
            or TextBox
            or ComboBox
            or DateTimePicker
            or ProgressBar;

    private sealed class ColorBox
    {
        public Color Color { get; set; }
    }
}
