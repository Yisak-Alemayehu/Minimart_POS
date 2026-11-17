using System.Runtime.InteropServices;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Margins
        {
            public int Left, Right, Top, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyFrostedGlass();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AddGlassOutlines(this);
        }

        private void ApplyFrostedGlass()
        {
            int dark = 1;
            DwmSetWindowAttribute(Handle, 20, ref dark, sizeof(int));
            int backdrop = 1; // no system backdrop (avoids gray tint)
            DwmSetWindowAttribute(Handle, 38, ref backdrop, sizeof(int));
            var margins = new Margins { Left = -1, Right = -1, Top = -1, Bottom = -1 };
            DwmExtendFrameIntoClientArea(Handle, ref margins);

            var accent = new AccentPolicy
            {
                AccentState = 4, // acrylic blur behind
                GradientColor = unchecked((int)0x28101010) // AABBGGRR: low alpha = more see-through
            };
            int size = Marshal.SizeOf(accent);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(accent, ptr, false);
                var data = new WindowCompositionAttributeData { Attribute = 19, Data = ptr, SizeOfData = size };
                SetWindowCompositionAttribute(Handle, ref data);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        private void AddGlassOutlines(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Panel p)
                {
                    p.Paint += (s, pe) =>
                    {
                        using var pen = new Pen(Color.FromArgb(90, 255, 255, 255));
                        pe.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                    };
                }
                AddGlassOutlines(c);
            }
        }

        private void lvItems_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            ListViewItem listViewItem1 = new ListViewItem(new string[] { "Coca-Cola 500ml", "2", "ETB 90" }, -1);
            ListViewItem listViewItem2 = new ListViewItem(new string[] { "Bread", "1", "ETB 20" }, -1);
            ListViewItem listViewItem3 = new ListViewItem(new string[] { "Milk 1L", "1", "ETB 85" }, -1);
            ListViewItem listViewItem4 = new ListViewItem(new string[] { "Rice 5kg", "1", "ETB 650" }, -1);
            ListViewItem listViewItem5 = new ListViewItem(new string[] { "Sugar 1kg", "1", "ETB 120" }, -1);
            ListViewItem listViewItem6 = new ListViewItem(new string[] { "Soap", "1", "ETB 40" }, -1);
            pnlSidebar = new Panel();
            lblLogo = new Label();
            btnSales = new Button();
            btnHistory = new Button();
            btnLogout = new Button();
            pnlHeader = new Panel();
            lblBrand = new Label();
            txtSearch = new TextBox();
            lblCashier = new Label();
            lblTime = new Label();
            pnlProducts = new Panel();
            txtBarcode = new TextBox();
            card1 = new Panel();
            img1 = new Label();
            name1 = new Label();
            price1 = new Label();
            card2 = new Panel();
            img2 = new Label();
            name2 = new Label();
            price2 = new Label();
            card3 = new Panel();
            img3 = new Label();
            name3 = new Label();
            price3 = new Label();
            card4 = new Panel();
            img4 = new Label();
            name4 = new Label();
            price4 = new Label();
            card5 = new Panel();
            img5 = new Label();
            name5 = new Label();
            price5 = new Label();
            card6 = new Panel();
            img6 = new Label();
            name6 = new Label();
            price6 = new Label();
            pnlSale = new Panel();
            lblSaleTitle = new Label();
            lvItems = new ListView();
            colItem = new ColumnHeader();
            colQty = new ColumnHeader();
            colTotal = new ColumnHeader();
            lblSubtotalText = new Label();
            lblSubtotal = new Label();
            lblDiscountText = new Label();
            lblDiscount = new Label();
            lblTotalText = new Label();
            lblTotal = new Label();
            btnPay = new Button();
            btnCancel = new Button();
            pnlSidebar.SuspendLayout();
            pnlHeader.SuspendLayout();
            pnlProducts.SuspendLayout();
            card1.SuspendLayout();
            card2.SuspendLayout();
            card3.SuspendLayout();
            card4.SuspendLayout();
            card5.SuspendLayout();
            card6.SuspendLayout();
            pnlSale.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.Black;
            pnlSidebar.Controls.Add(lblLogo);
            pnlSidebar.Controls.Add(btnSales);
            pnlSidebar.Controls.Add(btnHistory);
            pnlSidebar.Controls.Add(btnLogout);
            pnlSidebar.Location = new Point(10, 12);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(170, 674);
            pnlSidebar.TabIndex = 0;
            // 
            // lblLogo
            // 
            lblLogo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblLogo.ForeColor = Color.White;
            lblLogo.Location = new Point(10, 15);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(150, 40);
            lblLogo.TabIndex = 0;
            lblLogo.Text = "🛍 AbayPOS";
            lblLogo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnSales
            // 
            btnSales.BackColor = Color.FromArgb(255, 106, 0);
            btnSales.FlatAppearance.BorderSize = 0;
            btnSales.FlatStyle = FlatStyle.Flat;
            btnSales.Font = new Font("Segoe UI", 12F);
            btnSales.ForeColor = Color.White;
            btnSales.Location = new Point(8, 90);
            btnSales.Name = "btnSales";
            btnSales.Size = new Size(154, 64);
            btnSales.TabIndex = 1;
            btnSales.Text = "\U0001f6d2  Sales";
            btnSales.UseVisualStyleBackColor = false;
            // 
            // btnHistory
            // 
            btnHistory.FlatAppearance.BorderSize = 0;
            btnHistory.FlatStyle = FlatStyle.Flat;
            btnHistory.Font = new Font("Segoe UI", 12F);
            btnHistory.ForeColor = Color.Gainsboro;
            btnHistory.Location = new Point(8, 170);
            btnHistory.Name = "btnHistory";
            btnHistory.Size = new Size(154, 56);
            btnHistory.TabIndex = 2;
            btnHistory.Text = "🕑  History";
            // 
            // btnLogout
            // 
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Font = new Font("Segoe UI", 12F);
            btnLogout.ForeColor = Color.Gainsboro;
            btnLogout.Location = new Point(8, 242);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(154, 56);
            btnLogout.TabIndex = 3;
            btnLogout.Text = "↪  Log out";
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.Black;
            pnlHeader.Controls.Add(lblBrand);
            pnlHeader.Controls.Add(txtSearch);
            pnlHeader.Controls.Add(lblCashier);
            pnlHeader.Controls.Add(lblTime);
            pnlHeader.Location = new Point(198, 12);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1050, 68);
            pnlHeader.TabIndex = 1;
            // 
            // lblBrand
            // 
            lblBrand.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblBrand.ForeColor = Color.White;
            lblBrand.Location = new Point(20, 10);
            lblBrand.Name = "lblBrand";
            lblBrand.Size = new Size(200, 48);
            lblBrand.TabIndex = 0;
            lblBrand.Text = "AbayPOS";
            lblBrand.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSearch
            // 
            txtSearch.BackColor = Color.Black;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = new Font("Segoe UI", 12F);
            txtSearch.ForeColor = Color.Silver;
            txtSearch.Location = new Point(255, 18);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search... Ctrl+K";
            txtSearch.Size = new Size(370, 29);
            txtSearch.TabIndex = 1;
            // 
            // lblCashier
            // 
            lblCashier.Font = new Font("Segoe UI", 11F);
            lblCashier.ForeColor = Color.Gainsboro;
            lblCashier.Location = new Point(720, 20);
            lblCashier.Name = "lblCashier";
            lblCashier.Size = new Size(170, 28);
            lblCashier.TabIndex = 2;
            lblCashier.Text = "Cashier: Abebe";
            lblCashier.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTime
            // 
            lblTime.Font = new Font("Segoe UI", 11F);
            lblTime.ForeColor = Color.Gainsboro;
            lblTime.Location = new Point(910, 20);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(125, 28);
            lblTime.TabIndex = 3;
            lblTime.Text = "🕑 10:24 AM";
            lblTime.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlProducts
            // 
            pnlProducts.BackColor = Color.Black;
            pnlProducts.Controls.Add(txtBarcode);
            pnlProducts.Controls.Add(card1);
            pnlProducts.Controls.Add(card2);
            pnlProducts.Controls.Add(card3);
            pnlProducts.Controls.Add(card4);
            pnlProducts.Controls.Add(card5);
            pnlProducts.Controls.Add(card6);
            pnlProducts.Location = new Point(198, 93);
            pnlProducts.Name = "pnlProducts";
            pnlProducts.Size = new Size(627, 593);
            pnlProducts.TabIndex = 2;
            // 
            // txtBarcode
            // 
            txtBarcode.BackColor = Color.Black;
            txtBarcode.BorderStyle = BorderStyle.FixedSingle;
            txtBarcode.Font = new Font("Segoe UI", 12F);
            txtBarcode.ForeColor = Color.Silver;
            txtBarcode.Location = new Point(20, 18);
            txtBarcode.Name = "txtBarcode";
            txtBarcode.PlaceholderText = "Scan barcode or search product   (Ctrl+K)";
            txtBarcode.Size = new Size(587, 29);
            txtBarcode.TabIndex = 0;
            // 
            // card1
            // 
            card1.BackColor = Color.Black;
            card1.Controls.Add(img1);
            card1.Controls.Add(name1);
            card1.Controls.Add(price1);
            card1.Location = new Point(20, 70);
            card1.Name = "card1";
            card1.Size = new Size(190, 245);
            card1.TabIndex = 1;
            // 
            // img1
            // 
            img1.Font = new Font("Segoe UI Emoji", 48F);
            img1.ForeColor = Color.White;
            img1.Location = new Point(0, 10);
            img1.Name = "img1";
            img1.Size = new Size(190, 130);
            img1.TabIndex = 0;
            img1.Text = "\U0001f964";
            img1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // name1
            // 
            name1.Font = new Font("Segoe UI", 11F);
            name1.ForeColor = Color.White;
            name1.Location = new Point(10, 165);
            name1.Name = "name1";
            name1.Size = new Size(175, 25);
            name1.TabIndex = 1;
            name1.Text = "Coca-Cola 500ml";
            // 
            // price1
            // 
            price1.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            price1.ForeColor = Color.FromArgb(255, 106, 0);
            price1.Location = new Point(10, 195);
            price1.Name = "price1";
            price1.Size = new Size(175, 28);
            price1.TabIndex = 2;
            price1.Text = "ETB 45";
            // 
            // card2
            // 
            card2.BackColor = Color.Black;
            card2.Controls.Add(img2);
            card2.Controls.Add(name2);
            card2.Controls.Add(price2);
            card2.Location = new Point(222, 70);
            card2.Name = "card2";
            card2.Size = new Size(190, 245);
            card2.TabIndex = 2;
            // 
            // img2
            // 
            img2.Font = new Font("Segoe UI Emoji", 48F);
            img2.ForeColor = Color.White;
            img2.Location = new Point(0, 10);
            img2.Name = "img2";
            img2.Size = new Size(190, 130);
            img2.TabIndex = 0;
            img2.Text = "🍞";
            img2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // name2
            // 
            name2.Font = new Font("Segoe UI", 11F);
            name2.ForeColor = Color.White;
            name2.Location = new Point(10, 165);
            name2.Name = "name2";
            name2.Size = new Size(175, 25);
            name2.TabIndex = 1;
            name2.Text = "Bread";
            // 
            // price2
            // 
            price2.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            price2.ForeColor = Color.White;
            price2.Location = new Point(10, 195);
            price2.Name = "price2";
            price2.Size = new Size(175, 28);
            price2.TabIndex = 2;
            price2.Text = "ETB 20";
            // 
            // card3
            // 
            card3.BackColor = Color.Black;
            card3.Controls.Add(img3);
            card3.Controls.Add(name3);
            card3.Controls.Add(price3);
            card3.Location = new Point(424, 70);
            card3.Name = "card3";
            card3.Size = new Size(190, 245);
            card3.TabIndex = 3;
            // 
            // img3
            // 
            img3.Font = new Font("Segoe UI Emoji", 48F);
            img3.ForeColor = Color.White;
            img3.Location = new Point(0, 0);
            img3.Name = "img3";
            img3.Size = new Size(190, 130);
            img3.TabIndex = 0;
            img3.Text = "\U0001f95b";
            img3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // name3
            // 
            name3.Font = new Font("Segoe UI", 11F);
            name3.ForeColor = Color.White;
            name3.Location = new Point(10, 165);
            name3.Name = "name3";
            name3.Size = new Size(175, 25);
            name3.TabIndex = 1;
            name3.Text = "Milk 1L";
            // 
            // price3
            // 
            price3.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            price3.ForeColor = Color.White;
            price3.Location = new Point(10, 195);
            price3.Name = "price3";
            price3.Size = new Size(175, 28);
            price3.TabIndex = 2;
            price3.Text = "ETB 85";
            // 
            // card4
            // 
            card4.BackColor = Color.Black;
            card4.Controls.Add(img4);
            card4.Controls.Add(name4);
            card4.Controls.Add(price4);
            card4.Location = new Point(20, 330);
            card4.Name = "card4";
            card4.Size = new Size(190, 245);
            card4.TabIndex = 4;
            // 
            // img4
            // 
            img4.Font = new Font("Segoe UI Emoji", 48F);
            img4.ForeColor = Color.White;
            img4.Location = new Point(0, 10);
            img4.Name = "img4";
            img4.Size = new Size(190, 130);
            img4.TabIndex = 0;
            img4.Text = "🌾";
            img4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // name4
            // 
            name4.Font = new Font("Segoe UI", 11F);
            name4.ForeColor = Color.White;
            name4.Location = new Point(10, 165);
            name4.Name = "name4";
            name4.Size = new Size(175, 25);
            name4.TabIndex = 1;
            name4.Text = "Rice 5kg";
            // 
            // price4
            // 
            price4.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            price4.ForeColor = Color.White;
            price4.Location = new Point(10, 195);
            price4.Name = "price4";
            price4.Size = new Size(175, 28);
            price4.TabIndex = 2;
            price4.Text = "ETB 650";
            // 
            // card5
            // 
            card5.BackColor = Color.Black;
            card5.Controls.Add(img5);
            card5.Controls.Add(name5);
            card5.Controls.Add(price5);
            card5.Location = new Point(222, 330);
            card5.Name = "card5";
            card5.Size = new Size(190, 245);
            card5.TabIndex = 5;
            // 
            // img5
            // 
            img5.Font = new Font("Segoe UI Emoji", 48F);
            img5.ForeColor = Color.White;
            img5.Location = new Point(0, 10);
            img5.Name = "img5";
            img5.Size = new Size(190, 130);
            img5.TabIndex = 0;
            img5.Text = "\U0001f963";
            img5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // name5
            // 
            name5.Font = new Font("Segoe UI", 11F);
            name5.ForeColor = Color.White;
            name5.Location = new Point(10, 165);
            name5.Name = "name5";
            name5.Size = new Size(175, 25);
            name5.TabIndex = 1;
            name5.Text = "Sugar 1kg";
            // 
            // price5
            // 
            price5.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            price5.ForeColor = Color.White;
            price5.Location = new Point(10, 195);
            price5.Name = "price5";
            price5.Size = new Size(175, 28);
            price5.TabIndex = 2;
            price5.Text = "ETB 120";
            // 
            // card6
            // 
            card6.BackColor = Color.Black;
            card6.Controls.Add(img6);
            card6.Controls.Add(name6);
            card6.Controls.Add(price6);
            card6.Location = new Point(424, 330);
            card6.Name = "card6";
            card6.Size = new Size(190, 245);
            card6.TabIndex = 6;
            // 
            // img6
            // 
            img6.Font = new Font("Segoe UI Emoji", 48F);
            img6.ForeColor = Color.White;
            img6.Location = new Point(0, 10);
            img6.Name = "img6";
            img6.Size = new Size(190, 130);
            img6.TabIndex = 0;
            img6.Text = "\U0001f9fc";
            img6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // name6
            // 
            name6.Font = new Font("Segoe UI", 11F);
            name6.ForeColor = Color.White;
            name6.Location = new Point(10, 165);
            name6.Name = "name6";
            name6.Size = new Size(175, 25);
            name6.TabIndex = 1;
            name6.Text = "Soap";
            // 
            // price6
            // 
            price6.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            price6.ForeColor = Color.White;
            price6.Location = new Point(10, 195);
            price6.Name = "price6";
            price6.Size = new Size(175, 28);
            price6.TabIndex = 2;
            price6.Text = "ETB 40";
            // 
            // pnlSale
            // 
            pnlSale.BackColor = Color.Black;
            pnlSale.Controls.Add(lblSaleTitle);
            pnlSale.Controls.Add(lvItems);
            pnlSale.Controls.Add(lblSubtotalText);
            pnlSale.Controls.Add(lblSubtotal);
            pnlSale.Controls.Add(lblDiscountText);
            pnlSale.Controls.Add(lblDiscount);
            pnlSale.Controls.Add(lblTotalText);
            pnlSale.Controls.Add(lblTotal);
            pnlSale.Controls.Add(btnPay);
            pnlSale.Controls.Add(btnCancel);
            pnlSale.Location = new Point(839, 93);
            pnlSale.Name = "pnlSale";
            pnlSale.Size = new Size(408, 593);
            pnlSale.TabIndex = 3;
            // 
            // lblSaleTitle
            // 
            lblSaleTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblSaleTitle.ForeColor = Color.White;
            lblSaleTitle.Location = new Point(18, 15);
            lblSaleTitle.Name = "lblSaleTitle";
            lblSaleTitle.Size = new Size(300, 30);
            lblSaleTitle.TabIndex = 0;
            lblSaleTitle.Text = "Current Sale #S-1024";
            // 
            // lvItems
            // 
            lvItems.BackColor = Color.Black;
            lvItems.BorderStyle = BorderStyle.None;
            lvItems.Columns.AddRange(new ColumnHeader[] { colItem, colQty, colTotal });
            lvItems.Font = new Font("Segoe UI", 11F);
            lvItems.ForeColor = Color.White;
            lvItems.FullRowSelect = true;
            lvItems.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2, listViewItem3, listViewItem4, listViewItem5, listViewItem6 });
            lvItems.Location = new Point(18, 48);
            lvItems.Name = "lvItems";
            lvItems.Size = new Size(372, 250);
            lvItems.TabIndex = 1;
            lvItems.UseCompatibleStateImageBehavior = false;
            lvItems.View = View.Details;
            lvItems.SelectedIndexChanged += lvItems_SelectedIndexChanged;
            // 
            // colItem
            // 
            colItem.Text = "Item";
            colItem.Width = 190;
            // 
            // colQty
            // 
            colQty.Text = "Qty";
            colQty.Width = 70;
            // 
            // colTotal
            // 
            colTotal.Text = "Total";
            colTotal.Width = 100;
            // 
            // lblSubtotalText
            // 
            lblSubtotalText.ForeColor = Color.Silver;
            lblSubtotalText.Location = new Point(18, 325);
            lblSubtotalText.Name = "lblSubtotalText";
            lblSubtotalText.Size = new Size(150, 24);
            lblSubtotalText.TabIndex = 2;
            lblSubtotalText.Text = "Subtotal";
            // 
            // lblSubtotal
            // 
            lblSubtotal.ForeColor = Color.Silver;
            lblSubtotal.Location = new Point(220, 325);
            lblSubtotal.Name = "lblSubtotal";
            lblSubtotal.Size = new Size(170, 24);
            lblSubtotal.TabIndex = 3;
            lblSubtotal.Text = "ETB 1,005.00";
            lblSubtotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblDiscountText
            // 
            lblDiscountText.ForeColor = Color.Silver;
            lblDiscountText.Location = new Point(18, 352);
            lblDiscountText.Name = "lblDiscountText";
            lblDiscountText.Size = new Size(150, 24);
            lblDiscountText.TabIndex = 4;
            lblDiscountText.Text = "Discount";
            // 
            // lblDiscount
            // 
            lblDiscount.ForeColor = Color.FromArgb(255, 140, 0);
            lblDiscount.Location = new Point(220, 352);
            lblDiscount.Name = "lblDiscount";
            lblDiscount.Size = new Size(170, 24);
            lblDiscount.TabIndex = 5;
            lblDiscount.Text = "- ETB 5.00";
            lblDiscount.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotalText
            // 
            lblTotalText.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTotalText.ForeColor = Color.White;
            lblTotalText.Location = new Point(18, 405);
            lblTotalText.Name = "lblTotalText";
            lblTotalText.Size = new Size(120, 45);
            lblTotalText.TabIndex = 6;
            lblTotalText.Text = "Total";
            lblTotalText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTotal
            // 
            lblTotal.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTotal.ForeColor = Color.White;
            lblTotal.Location = new Point(140, 405);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(250, 45);
            lblTotal.TabIndex = 7;
            lblTotal.Text = "ETB 1,000.00";
            lblTotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnPay
            // 
            btnPay.BackColor = Color.FromArgb(255, 106, 0);
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.FlatStyle = FlatStyle.Flat;
            btnPay.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            btnPay.ForeColor = Color.White;
            btnPay.Location = new Point(18, 465);
            btnPay.Name = "btnPay";
            btnPay.Size = new Size(372, 65);
            btnPay.TabIndex = 8;
            btnPay.Text = "💳  Pay      [Enter]";
            btnPay.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            btnCancel.FlatAppearance.BorderColor = Color.Gray;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 12F);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(18, 540);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(372, 40);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "✕  Cancel";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1264, 700);
            Controls.Add(pnlSidebar);
            Controls.Add(pnlHeader);
            Controls.Add(pnlProducts);
            Controls.Add(pnlSale);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AbayPOS";
            pnlSidebar.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlProducts.ResumeLayout(false);
            pnlProducts.PerformLayout();
            card1.ResumeLayout(false);
            card2.ResumeLayout(false);
            card3.ResumeLayout(false);
            card4.ResumeLayout(false);
            card5.ResumeLayout(false);
            card6.ResumeLayout(false);
            pnlSale.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlSidebar;
        private Label lblLogo;
        private Button btnSales;
        private Button btnHistory;
        private Button btnLogout;
        private Panel pnlHeader;
        private Label lblBrand;
        private TextBox txtSearch;
        private Label lblCashier;
        private Label lblTime;
        private Panel pnlProducts;
        private TextBox txtBarcode;
        private Panel card1, card2, card3, card4, card5, card6;
        private Label img1, img2, img3, img4, img5, img6;
        private Label name1, name2, name3, name4, name5, name6;
        private Label price1, price2, price3, price4, price5, price6;
        private Panel pnlSale;
        private Label lblSaleTitle;
        private ListView lvItems;
        private ColumnHeader colItem;
        private ColumnHeader colQty;
        private ColumnHeader colTotal;
        private Label lblSubtotalText;
        private Label lblSubtotal;
        private Label lblDiscountText;
        private Label lblDiscount;
        private Label lblTotalText;
        private Label lblTotal;
        private Button btnPay;
        private Button btnCancel;
    }
}
