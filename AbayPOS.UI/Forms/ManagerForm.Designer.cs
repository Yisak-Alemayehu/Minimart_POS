#nullable enable
using AbayPOS.UI.Controls;

namespace AbayPOS.UI.Forms;

partial class ManagerForm
{
    private void InitializeComponent()
    {
        tlpRoot = new TableLayoutPanel();
        pnlSidebar = new GlassPanel();
        tlpSidebar = new TableLayoutPanel();
        pnlLogo = new Panel();
        lblLogo = new Label();
        lblManagerRole = new Label();
        btnNavDashboard = new RoundButton();
        btnNavProducts = new RoundButton();
        btnNavReceiveStock = new RoundButton();
        btnNavSuppliers = new RoundButton();
        btnNavReports = new RoundButton();
        btnNavSalesHistory = new RoundButton();
        btnNavLowStock = new RoundButton();
        btnNavUsers = new RoundButton();
        btnNavLogout = new RoundButton();
        tlpMain = new TableLayoutPanel();
        tlpTopBar = new TableLayoutPanel();
        lblTitle = new Label();
        lblUser = new Label();
        lblClock = new Label();
        pnlContent = new Panel();
        tlpRoot.SuspendLayout();
        pnlSidebar.SuspendLayout();
        tlpSidebar.SuspendLayout();
        pnlLogo.SuspendLayout();
        tlpMain.SuspendLayout();
        tlpTopBar.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.ColumnCount = 2;
        tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
        tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpRoot.Controls.Add(pnlSidebar, 0, 0);
        tlpRoot.Controls.Add(tlpMain, 1, 0);
        tlpRoot.Dock = DockStyle.Fill;
        tlpRoot.Padding = new Padding(12);
        //
        // pnlSidebar
        //
        pnlSidebar.Controls.Add(tlpSidebar);
        pnlSidebar.Dock = DockStyle.Fill;
        pnlSidebar.Margin = new Padding(0, 0, 12, 0);
        pnlSidebar.Padding = new Padding(12, 16, 12, 12);
        //
        // tlpSidebar
        //
        tlpSidebar.ColumnCount = 1;
        tlpSidebar.Controls.Add(pnlLogo, 0, 0);
        tlpSidebar.Controls.Add(btnNavDashboard, 0, 1);
        tlpSidebar.Controls.Add(btnNavProducts, 0, 2);
        tlpSidebar.Controls.Add(btnNavReceiveStock, 0, 3);
        tlpSidebar.Controls.Add(btnNavSuppliers, 0, 4);
        tlpSidebar.Controls.Add(btnNavReports, 0, 5);
        tlpSidebar.Controls.Add(btnNavSalesHistory, 0, 6);
        tlpSidebar.Controls.Add(btnNavLowStock, 0, 7);
        tlpSidebar.Controls.Add(btnNavUsers, 0, 8);
        tlpSidebar.Controls.Add(btnNavLogout, 0, 10);
        tlpSidebar.Dock = DockStyle.Fill;
        tlpSidebar.RowCount = 11;
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
        for (var i = 1; i <= 8; i++)
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        //
        // pnlLogo
        //
        pnlLogo.Controls.Add(lblManagerRole);
        pnlLogo.Controls.Add(lblLogo);
        pnlLogo.Dock = DockStyle.Fill;
        //
        // lblLogo
        //
        lblLogo.AutoSize = true;
        lblLogo.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        lblLogo.Location = new Point(4, 6);
        lblLogo.Text = "AbayPOS";
        //
        // lblManagerRole
        //
        lblManagerRole.AutoSize = true;
        lblManagerRole.Font = new Font("Segoe UI", 9F);
        lblManagerRole.Location = new Point(6, 42);
        lblManagerRole.Text = "Manager";
        //
        // btnNavDashboard
        //
        btnNavDashboard.Icon = "\uE80F";
        btnNavDashboard.Text = "Dashboard";
        //
        // btnNavProducts
        //
        btnNavProducts.Icon = "\uE7B8";
        btnNavProducts.Text = "Products";
        //
        // btnNavReceiveStock
        //
        btnNavReceiveStock.Icon = "\uE8F1";
        btnNavReceiveStock.Text = "Receive Stock";
        //
        // btnNavSuppliers
        //
        btnNavSuppliers.Icon = "\uE716";
        btnNavSuppliers.Text = "Suppliers";
        //
        // btnNavReports
        //
        btnNavReports.Icon = "\uE9D2";
        btnNavReports.Text = "Reports";
        //
        // btnNavSalesHistory
        //
        btnNavSalesHistory.Icon = "\uE81C";
        btnNavSalesHistory.Text = "Sales History";
        //
        // btnNavLowStock
        //
        btnNavLowStock.Icon = "\uE7BA";
        btnNavLowStock.Text = "Low Stock";
        //
        // btnNavUsers
        //
        btnNavUsers.Icon = "\uE77B";
        btnNavUsers.Text = "Users";
        //
        // btnNavLogout
        //
        btnNavLogout.Icon = "\uE8AC";
        btnNavLogout.Text = "Log out";
        //
        // tlpMain
        //
        tlpMain.ColumnCount = 1;
        tlpMain.Controls.Add(tlpTopBar, 0, 0);
        tlpMain.Controls.Add(pnlContent, 0, 1);
        tlpMain.Dock = DockStyle.Fill;
        tlpMain.RowCount = 2;
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // tlpTopBar
        //
        tlpTopBar.ColumnCount = 3;
        tlpTopBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpTopBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
        tlpTopBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
        tlpTopBar.Controls.Add(lblTitle, 0, 0);
        tlpTopBar.Controls.Add(lblUser, 1, 0);
        tlpTopBar.Controls.Add(lblClock, 2, 0);
        tlpTopBar.Dock = DockStyle.Fill;
        //
        // lblTitle
        //
        lblTitle.Dock = DockStyle.Fill;
        lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        lblTitle.Text = "Dashboard";
        lblTitle.TextAlign = ContentAlignment.MiddleLeft;
        //
        // lblUser
        //
        lblUser.Dock = DockStyle.Fill;
        lblUser.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblUser.TextAlign = ContentAlignment.MiddleRight;
        //
        // lblClock
        //
        lblClock.Dock = DockStyle.Fill;
        lblClock.Font = new Font("Segoe UI", 9F);
        lblClock.TextAlign = ContentAlignment.MiddleRight;
        //
        // pnlContent
        //
        pnlContent.Dock = DockStyle.Fill;
        //
        // ManagerForm
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(tlpRoot);
        Name = "ManagerForm";
        tlpRoot.ResumeLayout(false);
        pnlSidebar.ResumeLayout(false);
        tlpSidebar.ResumeLayout(false);
        pnlLogo.ResumeLayout(false);
        pnlLogo.PerformLayout();
        tlpMain.ResumeLayout(false);
        tlpTopBar.ResumeLayout(false);
        ResumeLayout(false);
    }

    private TableLayoutPanel tlpRoot = null!;
    private GlassPanel pnlSidebar = null!;
    private TableLayoutPanel tlpSidebar = null!;
    private Panel pnlLogo = null!;
    private Label lblLogo = null!;
    private Label lblManagerRole = null!;
    private RoundButton btnNavDashboard = null!;
    private RoundButton btnNavProducts = null!;
    private RoundButton btnNavReceiveStock = null!;
    private RoundButton btnNavSuppliers = null!;
    private RoundButton btnNavReports = null!;
    private RoundButton btnNavSalesHistory = null!;
    private RoundButton btnNavLowStock = null!;
    private RoundButton btnNavUsers = null!;
    private RoundButton btnNavLogout = null!;
    private TableLayoutPanel tlpMain = null!;
    private TableLayoutPanel tlpTopBar = null!;
    private Label lblTitle = null!;
    private Label lblUser = null!;
    private Label lblClock = null!;
    private Panel pnlContent = null!;
}