using System.Drawing.Drawing2D;

namespace WinFormsApp2
{
    static class Gfx
    {
        public static GraphicsPath Round(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    class RoundedPanel : Panel
    {
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)] public int Radius { get; set; } = 18;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)] public Color Fill { get; set; } = Color.FromArgb(22, 24, 30);
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)] public Color Border { get; set; } = Color.FromArgb(62, 66, 74);

        public RoundedPanel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = Gfx.Round(new Rectangle(0, 0, Width - 1, Height - 1), Radius);
            using var brush = new SolidBrush(Fill);
            using var pen = new Pen(Border);
            g.FillPath(brush, path);
            g.DrawPath(pen, path);
            base.OnPaint(e);
        }
    }

    class RoundButton : Control
    {
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)] public string Icon { get; set; } = "";
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)] public string Hint { get; set; } = "";
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)] public Color FillColor { get; set; } = Color.Empty;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)] public Color FillColor2 { get; set; } = Color.Empty;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)] public Color BorderColor { get; set; } = Color.Empty;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)] public bool LeftAlign { get; set; }
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)] public int Radius { get; set; } = 14;

        public RoundButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = Gfx.Round(rect, Radius);
            if (FillColor != Color.Empty)
            {
                using var fill = new LinearGradientBrush(rect, FillColor, FillColor2 == Color.Empty ? FillColor : FillColor2, 90f);
                g.FillPath(fill, path);
            }
            if (BorderColor != Color.Empty)
            {
                using var pen = new Pen(BorderColor);
                g.DrawPath(pen, path);
            }

            using var textBrush = new SolidBrush(ForeColor);
            var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var near = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

            if (Icon.Length > 0)
            {
                using var iconFont = new Font("Segoe MDL2 Assets", 16F);
                g.DrawString(Icon, iconFont, textBrush, new Rectangle(LeftAlign ? 16 : 22, 0, 40, Height), center);
            }
            if (LeftAlign)
                g.DrawString(Text, Font, textBrush, new Rectangle(62, 0, Width - 62, Height), near);
            else
                g.DrawString(Text, Font, textBrush, new Rectangle(0, 0, Width, Height), center);

            if (Hint.Length > 0)
            {
                var pill = new Rectangle(Width - 92, (Height - 32) / 2, 76, 32);
                using var pillPath = Gfx.Round(pill, 12);
                using var pillFill = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
                using var pillPen = new Pen(Color.FromArgb(90, 255, 255, 255));
                g.FillPath(pillFill, pillPath);
                g.DrawPath(pillPen, pillPath);
                using var hintFont = new Font("Segoe UI", 10F);
                g.DrawString(Hint, hintFont, textBrush, pill, center);
            }
        }
    }
}

