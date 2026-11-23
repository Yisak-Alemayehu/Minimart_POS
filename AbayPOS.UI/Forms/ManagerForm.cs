using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Pages;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Forms;

/// <summary>Manager shell. Pages are UserControls swapped into the content panel.</summary>
public sealed class ManagerForm : ThemedForm
{
    private readonly Session _session;
    private readonly Panel _content = new() { Dock = DockStyle.Fill, BackColor = Color.Transparent };
    private readonly Dictionary<string, UserControl> _pages = new(StringComparer.Ordinal);
    private readonly Dictionary<string, RoundButton> _nav = new(StringComparer.Ordinal);
    private readonly Label _clock = new();
    private readonly Label _lowBadge = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
    private string _current = "Dashboard";

    public ManagerForm(Session session)
    {
        PreferMica = true;
        _session = session;
        Text = "AbayPOS | Manager";
        ClientSize = new Size(1366, 768);
        MaximizedBounds = Screen.PrimaryScreen?.WorkingArea ?? Rectangle.Empty;
        WindowState = FormWindowState.Maximized;
        Build();
        _timer.Tick += (_, _) => _clock.Text = DateTime.Now.ToString("hh:mm tt");
        _timer.Start();
        Shown += (_, _) => ShowPage("Dashboard");
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Dispose();
        base.OnFormClosed(e);
    }

    private void Build()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12), BackColor = Color.Transparent };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Theme.SidebarWidth));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.Controls.Add(BuildSidebar(), 0, 0);
        var main = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.Transparent };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.TopBarHeight));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.Controls.Add(BuildTopBar(), 0, 0);
        main.Controls.Add(_content, 0, 1);
        root.Controls.Add(main, 1, 0);
        Controls.Add(root);

        Register("Dashboard", new DashboardPage(_session));
        Register("Products", new ProductsPage(_session));
        Register("Receive Stock", new ReceiveStockPage(_session));
        Register("Suppliers", new SuppliersPage(_session));
        Register("Reports", new ReportsPage(_session));
        Register("Sales History", new SalesHistoryPage(_session));
        var low = new LowStockPage();
        low.CountChanged += (_, count) => _lowBadge.Text = count > 0 ? count.ToString() : string.Empty;
        Register("Low Stock", low);
        Register("Users", new UsersPage(_session));
    }

    private Control BuildSidebar()
    {
        var sidebar = new GlassPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(12, 16, 12, 12), FillColor = Theme.SidebarGlass };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        var keys = new[] { "Dashboard", "Products", "Receive Stock", "Suppliers", "Reports", "Sales History", "Low Stock", "Users" };
        foreach (var _ in keys)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        var logo = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        logo.Controls.Add(new Label { Text = "Abay", Font = Theme.Font(22, FontStyle.Bold), ForeColor = Theme.Text, AutoSize = true, Location = new Point(4, 8) });
        logo.Controls.Add(new Label { Text = "Manager", Font = Theme.FontHint(), ForeColor = Theme.Accent, AutoSize = true, Location = new Point(6, 44) });
        layout.Controls.Add(logo, 0, 0);
        var row = 1;
        foreach (var key in keys)
        {
            var button = CreateNav(key, key == "Dashboard");
            if (key == "Low Stock")
            {
                _lowBadge.AutoSize = true;
                _lowBadge.ForeColor = Theme.Warning;
                _lowBadge.Font = Theme.Font(9, FontStyle.Bold);
                _lowBadge.Location = new Point(150, 14);
                button.Controls.Add(_lowBadge);
            }

            button.Click += (_, _) => ShowPage(key);
            _nav[key] = button;
            layout.Controls.Add(button, 0, row++);
        }

        var logout = CreateNav("Log out", false);
        logout.Click += (_, _) =>
        {
            DialogResult = DialogResult.Retry;
            Close();
        };
        layout.Controls.Add(logout, 0, row + 1);
        sidebar.Controls.Add(layout);
        return sidebar;
    }

    private Control BuildTopBar()
    {
        var bar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Color.Transparent };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        var title = new Label { Text = "AbayPOS", Dock = DockStyle.Fill, Font = Theme.Font(15, FontStyle.Bold), ForeColor = Theme.Text, TextAlign = ContentAlignment.MiddleLeft };
        title.MouseDown += DragWindow;
        bar.Controls.Add(title, 0, 0);
        var right = CreateWindowButtons(this);
        _clock.AutoSize = true;
        _clock.ForeColor = Theme.TextSecondary;
        _clock.Font = Theme.FontHint();
        _clock.Margin = new Padding(12, 21, 0, 0);
        var name = new Label { Text = _session.User.FullName, AutoSize = true, ForeColor = Theme.Text, Font = Theme.Font(9, FontStyle.Bold), Margin = new Padding(8, 21, 0, 0) };
        right.Controls.Add(_clock);
        right.Controls.Add(name);
        bar.Controls.Add(right, 1, 0);
        return bar;
    }

    private void Register(string key, UserControl page)
    {
        page.Dock = DockStyle.Fill;
        page.Visible = false;
        _pages[key] = page;
        _content.Controls.Add(page);
    }

    private void ShowPage(string key)
    {
        _current = key;
        foreach (var pair in _pages)
            pair.Value.Visible = pair.Key == key;
        foreach (var pair in _nav)
            pair.Value.FillColor = pair.Key == key ? Theme.Accent : Theme.NavGlass;
        if (_pages[key] is DashboardPage dashboard)
            dashboard.Reload();
        if (_pages[key] is LowStockPage low)
            low.Reload();
        if (_pages[key] is ProductsPage products)
            products.Reload();
    }

    private static RoundButton CreateNav(string text, bool active) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 2, 0, 2),
        FillColor = active ? Theme.Accent : Theme.NavGlass,
        LabelAlign = ContentAlignment.MiddleLeft,
        Font = Theme.Font(10, active ? FontStyle.Bold : FontStyle.Regular)
    };
}
