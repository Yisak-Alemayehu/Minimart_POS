using AbayPOS.Domain.Models;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Infrastructure;
using AbayPOS.UI.Pages;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Forms;

/// <summary>Manager shell. Pages are UserControls swapped into the content panel.</summary>
public sealed partial class ManagerForm : ThemedForm
{
    private readonly Session _session;
    private readonly Dictionary<string, UserControl> _pages = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<UserControl>> _factories = new(StringComparer.Ordinal);
    private readonly Dictionary<string, RoundButton> _nav = new(StringComparer.Ordinal);
    private readonly System.Windows.Forms.Timer tmrClock = new();

    public ManagerForm(Session session)
    {
        _session = session;
        InitializeComponent();
        Text = "AbayPOS – Manager";
        MinimumSize = new Size(Theme.MinimumWidth, Theme.MinimumHeight);
        ClientSize = new Size(1366, 768);
        RegisterNav();
        RegisterFactories();
        Shown += (_, _) => ShowPage("Dashboard");
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
        lblUser.Text = _session.User.FullName;
        lblClock.Text = DateTime.Now.ToString("hh:mm tt");
        tmrClock.Interval = 1000;
        tmrClock.Tick += (_, _) => lblClock.Text = DateTime.Now.ToString("hh:mm tt");
        tmrClock.Start();
        WindowState = FormWindowState.Maximized;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        tmrClock.Dispose();
        base.OnFormClosed(e);
    }

    private void RegisterNav()
    {
        _nav["Dashboard"] = btnNavDashboard;
        _nav["Products"] = btnNavProducts;
        _nav["Receive Stock"] = btnNavReceiveStock;
        _nav["Suppliers"] = btnNavSuppliers;
        _nav["Reports"] = btnNavReports;
        _nav["Sales History"] = btnNavSalesHistory;
        _nav["Low Stock"] = btnNavLowStock;
        _nav["Users"] = btnNavUsers;
        btnNavDashboard.Click += (_, _) => ShowPage("Dashboard");
        btnNavProducts.Click += (_, _) => ShowPage("Products");
        btnNavReceiveStock.Click += (_, _) => ShowPage("Receive Stock");
        btnNavSuppliers.Click += (_, _) => ShowPage("Suppliers");
        btnNavReports.Click += (_, _) => ShowPage("Reports");
        btnNavSalesHistory.Click += (_, _) => ShowPage("Sales History");
        btnNavLowStock.Click += (_, _) => ShowPage("Low Stock");
        btnNavUsers.Click += (_, _) => ShowPage("Users");
        btnNavLogout.Click += (_, _) =>
        {
            DialogResult = DialogResult.Retry;
            Close();
        };
        foreach (var pair in _nav)
            ApplyNavStyle(pair.Value, pair.Key == "Dashboard");
    }

    private void RegisterFactories()
    {
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
    }

    private UserControl Page(string key)
    {
        if (_pages.TryGetValue(key, out var existing))
            return existing;

        var page = _factories[key]();
        page.Dock = DockStyle.Fill;
        page.Visible = false;
        _pages[key] = page;
        pnlContent.Controls.Add(page);
        return page;
    }

    private void ShowPage(string key)
    {
        lblTitle.Text = key;
        pnlContent.SuspendLayout();
        var page = Page(key);
        foreach (var pair in _pages)
            pair.Value.Visible = pair.Key == key;
        page.BringToFront();
        pnlContent.ResumeLayout(true);
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

    private static void ApplyNavStyle(RoundButton button, bool active)
    {
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(0, 2, 0, 2);
        button.FillColor = active ? Theme.Accent : Theme.Card;
        button.BorderColor = Color.Empty;
        button.LabelAlign = ContentAlignment.MiddleLeft;
        button.Font = Theme.Font(10, active ? FontStyle.Bold : FontStyle.Regular);
    }

}