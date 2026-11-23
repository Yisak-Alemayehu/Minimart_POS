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
        grid.ColumnHeadersHeight = Theme.Scale(44);
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(Theme.Scale(8), 0, Theme.Scale(8), 0);
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
        grid.ShowCellToolTips = true;
        grid.RowTemplate.Height = Theme.Scale(40);
        grid.GridColor = Theme.GridLine;
    }

    public static void Balance(DataGridView grid, string? nameColumn, params string[] measured)
    {
        foreach (DataGridViewColumn column in grid.Columns)
        {
            column.MinimumWidth = Theme.Scale(72);
            column.FillWeight = 100;
        }

        if (nameColumn is not null && grid.Columns[nameColumn] is { } name)
        {
            name.FillWeight = 240;
            name.MinimumWidth = Theme.Scale(160);
        }

        foreach (var columnName in measured)
        {
            if (grid.Columns[columnName] is not { } column)
                continue;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            column.MinimumWidth = Theme.Scale(96);
        }
    }

    public static DataGridViewTextBoxColumn TextColumn(string name, string header, float weight, int min) => new()
    {
        Name = name,
        HeaderText = header,
        FillWeight = weight,
        MinimumWidth = Theme.Scale(min)
    };

    public static DataGridViewTextBoxColumn MoneyColumn(string name, string header) => new()
    {
        Name = name,
        HeaderText = header,
        AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
        MinimumWidth = Theme.Scale(110),
        DefaultCellStyle = new DataGridViewCellStyle
        {
            Alignment = DataGridViewContentAlignment.MiddleRight,
            Format = "'ETB '0.00"
        }
    };

    public static DataGridViewTextBoxColumn QtyColumn(string name, string header) => new()
    {
        Name = name,
        HeaderText = header,
        AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
        MinimumWidth = Theme.Scale(64),
        DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
    };
}
