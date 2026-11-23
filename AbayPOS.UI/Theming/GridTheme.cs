namespace AbayPOS.UI.Theming;

internal static class GridTheme
{
    public static void Apply(DataGridView grid)
    {
        grid.BackgroundColor = Theme.CartSurface;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.GlassSolid;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextSecondary;
        grid.ColumnHeadersDefaultCellStyle.Font = Theme.Font(10, FontStyle.Bold);
        grid.ColumnHeadersHeight = 36;
        grid.DefaultCellStyle.BackColor = Theme.CartSurface;
        grid.DefaultCellStyle.ForeColor = Theme.Text;
        grid.DefaultCellStyle.SelectionBackColor = Theme.Selection;
        grid.DefaultCellStyle.SelectionForeColor = Theme.Text;
        grid.DefaultCellStyle.Font = Theme.FontBody();
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowTemplate.Height = Theme.TouchTarget;
        grid.GridColor = Color.FromArgb(40, 255, 255, 255);
    }
}
