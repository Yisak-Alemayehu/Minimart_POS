namespace AbayPOS.UI.Theming;

internal static class GridTheme
{
    public static void Apply(DataGridView grid)
    {
        grid.BackgroundColor = Theme.Card;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Base;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextSecondary;
        grid.ColumnHeadersDefaultCellStyle.Font = Theme.Font(10, FontStyle.Bold);
        grid.ColumnHeadersHeight = 40;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        grid.DefaultCellStyle.BackColor = Theme.Card;
        grid.DefaultCellStyle.ForeColor = Theme.Text;
        grid.DefaultCellStyle.SelectionBackColor = Theme.Selection;
        grid.DefaultCellStyle.SelectionForeColor = Theme.Text;
        grid.DefaultCellStyle.Font = Theme.FontBody();
        grid.AlternatingRowsDefaultCellStyle.BackColor = Theme.NavRaised;
        grid.AlternatingRowsDefaultCellStyle.ForeColor = Theme.Text;
        grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Theme.Selection;
        grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = Theme.Text;
        grid.RowHeadersVisible = false;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowTemplate.Height = 40;
        grid.GridColor = Theme.GridLine;
    }
}
