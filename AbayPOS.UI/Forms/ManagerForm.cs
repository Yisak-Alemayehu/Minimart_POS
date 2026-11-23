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
    private readonly Panel _content = new() { Dock = DockStyle.Fill, BackColor = Theme.Base };
    private readonly Dictionary<string, UserControl> _pages = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<UserControl>> _factories = new(StringComparer.Ordinal);
    private readonly Dictionary<string, RoundButton> _nav = new(StringComparer.Ordinal);
    private readonly Label _clock = new();
    private readonly Label _title = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
    private string _current = "Dashboard";

    public ManagerForm(Session session)
    {
        _session = session;
        Text = "AbayPOS – Manager";
        MinimumSize = new Size(Theme.MinimumWidth, Theme.MinimumHeight);
        ClientSize = new Size(1366, 768);
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
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12), BackColor = Theme.Base };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Theme.SidebarWidth));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.Controls.Add(BuildSidebar(), 0, 0);
        var main = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Theme.Base };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.TopBarHeight));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.Controls.Add(BuildTopBar(), 0, 0);
        main.Controls.Add(_content, 0, 1);
        root.Controls.Add(main, 1, 0);
        Controls.Add(root);

        _factories["Dashboard"] = () =>
        {
            var dashboard = new DashboardPage(_session);
            dashboard.ViewProducts += (_, _) => ShowPage("Products");
            return dashboard;
        };
        _factories["Products"] = () => new ProductsPage(_session);
        _factories["Receive Stock"] = () => new ReceiveStockPage(_session);
        _factories["Suppliers"] = () => new SuppliersPage(_session);
        _factories["Reports"] = () => new ReportsPage(_session);
        _factories["Sales History"] = () => new SalesHistoryPage(_session);
        _factories["Low Stock"] = () =>
        {
            var low = new LowStockPage();
            low.CountChanged += (_, count) =>
            {
                if (_nav.TryGetValue("Low Stock", out var button))
                    button.Badge = count > 0 ? count.ToString() : string.Empty;
            };
            return low;
        };
        _factories["Users"] = () => new UsersPage(_session);
        Surface.Adopt(this, Theme.Base);
    }

    private Control BuildSidebar()
    {
        var sidebar = new GlassPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(12, 16, 12, 12), FillColor = Theme.Sidebar };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, BackColor = Theme.Sidebar };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        var keys = new[] { "Dashboard", "Products", "Receive Stock", "Suppliers", "Reports", "Sales History", "Low Stock", "Users" };
        foreach (var _ in keys)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        var logo = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Sidebar };
        logo.Controls.Add(new Label { Text = "AbayPOS", Font = Theme.Font(20, FontStyle.Bold), ForeColor = Theme.Accent, BackColor = Theme.Sidebar, AutoSize = true, Location = new Point(4, 6) });
        logo.Controls.Add(new Label { Text = "Manager", Font = Theme.FontHint(), ForeColor = Theme.TextSecondary, BackColor = Theme.Sidebar, AutoSize = true, Location = new Point(6, 42) });
        layout.Controls.Add(logo, 0, 0);
        var row = 1;
        foreach (var key in keys)
        {
            var button = CreateNav(key, key == "Dashboard");
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
        var bar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, BackColor = Theme.Base };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        _title.Text = "Dashboard";
        _title.Dock = DockStyle.Fill;
        _title.Font = Theme.Font(18, FontStyle.Bold);
        _title.ForeColor = Theme.Text;
        _title.BackColor = Theme.Base;
        _title.TextAlign = ContentAlignment.MiddleLeft;
        bar.Controls.Add(_title, 0, 0);
        var name = new Label { Text = _session.User.FullName, Dock = DockStyle.Fill, ForeColor = Theme.Text, BackColor = Theme.Base, Font = Theme.Font(9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight };
        var clock = _clock;
        clock.Text = DateTime.Now.ToString("hh:mm tt");
        clock.Dock = DockStyle.Fill;
        clock.ForeColor = Theme.TextSecondary;
        clock.BackColor = Theme.Base;
        clock.Font = Theme.FontHint();
        clock.TextAlign = ContentAlignment.MiddleRight;
        bar.Controls.Add(name, 1, 0);
        bar.Controls.Add(clock, 2, 0);
        return bar;
    }

    private UserControl Page(string key)
    {
        if (_pages.TryGetValue(key, out var existing))
            return existing;

        var page = _factories[key]();
        page.Dock = DockStyle.Fill;
        page.Visible = false;
        _pages[key] = page;
        _content.Controls.Add(page);
        return page;
    }

    private void ShowPage(string key)
    {
        _current = key;
        _title.Text = key;
        _content.SuspendLayout();
        var page = Page(key);
        foreach (var pair in _pages)
            pair.Value.Visible = pair.Key == key;
        page.BringToFront();
        _content.ResumeLayout(true);
        foreach (var pair in _nav)
            pair.Value.FillColor = pair.Key == key ? Theme.Accent : Theme.Card;
        switch (page)
        {
            case DashboardPage dashboard:
                dashboard.Reload();
                break;
            case LowStockPage low:
                low.Reload();
                break;
            case ProductsPage products:
                products.Reload();
                break;
            case SuppliersPage suppliers:
                suppliers.Reload();
                break;
            case ReportsPage reports:
                reports.Reload();
                break;
            case SalesHistoryPage history:
                history.Reload();
                break;
            case UsersPage users:
                users.Reload();
                break;
            case ReceiveStockPage stock:
                stock.ReloadLookups();
                break;
        }
    }

    private static RoundButton CreateNav(string text, bool active) => new()
    {
        Text = text,
        Icon = NavIcon(text),
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 2, 0, 2),
        FillColor = active ? Theme.Accent : Theme.Card,
        BorderColor = Color.Empty,
        LabelAlign = ContentAlignment.MiddleLeft,
        Font = Theme.Font(10, active ? FontStyle.Bold : FontStyle.Regular)
    };

    private static string NavIcon(string text) => text switch
    {
        "Dashboard" => "\uE80F",
        "Products" => "\uE7B8",
        "Receive Stock" => "\uE8F1",
        "Suppliers" => "\uE716",
        "Reports" => "\uE9D2",
        "Sales History" => "\uE81C",
        "Low Stock" => "\uE7BA",
        "Users" => "\uE77B",
        "Log out" => "\uE8AC",
        _ => "\uE10F"
    };
}
