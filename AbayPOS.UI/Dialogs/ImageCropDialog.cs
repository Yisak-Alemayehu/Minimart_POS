using System.Drawing.Drawing2D;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Imaging;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>Square crop with zoom, rotation and a live tile preview.</summary>
public sealed class ImageCropDialog : DialogForm
{
    private readonly CropSurface _surface;
    private readonly CropPreview _preview = new();
    private readonly TrackBar _zoom = new() { Minimum = 100, Maximum = 400, Value = 100, TickFrequency = 50, AutoSize = false, Height = Theme.Scale(36), Width = Theme.Scale(180) };

    public ImageCropDialog(Bitmap source)
    {
        Text = "AbayPOS – Crop image";
        ClientSize = new Size(Theme.Scale(860), Theme.Scale(560));
        MinimumSize = new Size(Theme.Scale(720), Theme.Scale(480));
        _surface = new CropSurface(source) { Dock = DockStyle.Fill };
        _surface.CropChanged += (_, _) => _preview.Invalidate();
        _preview.Source = _surface;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Theme.Base, Padding = new Padding(Theme.Scale(12)) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Theme.Scale(220)));
        root.Controls.Add(_surface, 0, 0);

        var side = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, BackColor = Theme.Base };
        side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        side.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _preview.Size = new Size(Theme.Scale(160), Theme.Scale(160));
        _preview.Margin = new Padding(Theme.Scale(16), Theme.Scale(8), 0, Theme.Scale(8));
        side.Controls.Add(new Label { Text = "Tile preview", AutoSize = true, ForeColor = Theme.TextSecondary, Font = Theme.FontHint() }, 0, 0);
        side.Controls.Add(_preview, 0, 1);
        var zoomRow = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, BackColor = Theme.Base, WrapContents = false };
        zoomRow.Controls.Add(new Label { Text = "Zoom", AutoSize = true, ForeColor = Theme.TextSecondary, Font = Theme.FontHint() });
        _zoom.ValueChanged += (_, _) => _surface.SetZoom(_zoom.Value / 100f);
        zoomRow.Controls.Add(_zoom);
        side.Controls.Add(zoomRow, 0, 2);

        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, BackColor = Theme.Base, WrapContents = false };
        buttons.Controls.Add(Action("Rotate left", () => _surface.Rotate(false)));
        buttons.Controls.Add(Action("Rotate right", () => _surface.Rotate(true)));
        buttons.Controls.Add(Action("Reset", _surface.ResetView));
        var apply = new RoundButton { Text = "Apply", Width = Theme.Scale(160), Height = Theme.Scale(40), IsPrimary = true, Margin = new Padding(0, Theme.Scale(12), 0, Theme.Scale(6)) };
        apply.Click += (_, _) => Apply();
        var cancel = new RoundButton { Text = "Cancel", Width = Theme.Scale(160), Height = Theme.Scale(40), FillColor = Theme.CancelGlass, BorderColor = Theme.GlassBorder };
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.Add(apply);
        buttons.Controls.Add(cancel);
        side.Controls.Add(buttons, 0, 3);
        root.Controls.Add(side, 1, 0);
        Controls.Add(root);
        Surface.Adopt(this, Theme.Base);
        AcceptButton = apply;
        CancelButton = cancel;
    }

    public byte[]? Jpeg { get; private set; }

    private void Apply()
    {
        using var cropped = _surface.Render();
        Jpeg = ProductImageCodec.ToJpeg(cropped);
        DialogResult = DialogResult.OK;
        Close();
    }

    private RoundButton Action(string text, Action click)
    {
        var button = new RoundButton { Text = text, Width = Theme.Scale(160), Height = Theme.Scale(36), Margin = new Padding(0, 0, 0, Theme.Scale(6)), FillColor = Theme.NavRaised, BorderColor = Theme.GlassBorder };
        button.Click += (_, _) =>
        {
            click();
            _zoom.Value = (int)Math.Round(_surface.Zoom * 100f);
            _preview.Invalidate();
        };
        return button;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _surface.DisposeSource();
        base.Dispose(disposing);
    }

    private sealed class CropPreview : Control
    {
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public CropSurface? Source { get; set; }

        public CropPreview()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Theme.Card;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.Card);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using var clip = Gfx.Round(bounds, Theme.PanelRadius);
            e.Graphics.SetClip(clip);
            if (Source?.SourceImage is { } image)
                ProductImageCodec.DrawCover(e.Graphics, image, Source.CropPixels, bounds);
            e.Graphics.ResetClip();
            e.Graphics.DrawPath(Theme.Stroke(Theme.GlassBorder), clip);
        }
    }

    private sealed class CropSurface : Control
    {
        private Bitmap _source;
        private RectangleF _crop;
        private float _zoom = 1f;
        private Drag _drag = Drag.None;
        private Point _last;
        private RectangleF _cropAtDrag;

        public CropSurface(Bitmap source)
        {
            _source = source;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Base;
            ResetView();
        }

        public event EventHandler? CropChanged;

        public Bitmap SourceImage => _source;
        public float Zoom => _zoom;
        public Rectangle CropPixels => Rectangle.Round(_crop);

        public void SetZoom(float zoom)
        {
            _zoom = Math.Clamp(zoom, 1f, 4f);
            Invalidate();
            CropChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ResetView()
        {
            var side = Math.Min(_source.Width, _source.Height);
            _crop = new RectangleF((_source.Width - side) / 2f, (_source.Height - side) / 2f, side, side);
            _zoom = 1f;
            Invalidate();
            CropChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Rotate(bool clockwise)
        {
            var next = ProductImageCodec.Rotate(_source, clockwise);
            _source.Dispose();
            _source = next;
            ResetView();
        }

        public Bitmap Render() => ProductImageCodec.CropToSquare(_source, CropPixels);

        public void DisposeSource() => _source.Dispose();

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.Base);
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var view = ImageView();
            e.Graphics.DrawImage(_source, view);
            var frame = CropView();
            using var dim = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
            e.Graphics.FillRectangle(dim, new RectangleF(0, 0, Width, frame.Top));
            e.Graphics.FillRectangle(dim, new RectangleF(0, frame.Bottom, Width, Height - frame.Bottom));
            e.Graphics.FillRectangle(dim, new RectangleF(0, frame.Top, frame.Left, frame.Height));
            e.Graphics.FillRectangle(dim, new RectangleF(frame.Right, frame.Top, Width - frame.Right, frame.Height));
            e.Graphics.DrawRectangle(Theme.Stroke(Theme.Accent, 2), Rectangle.Round(frame));
            var third = Theme.Stroke(Color.FromArgb(160, 255, 255, 255));
            for (var i = 1; i <= 2; i++)
            {
                var x = frame.Left + frame.Width * i / 3f;
                var y = frame.Top + frame.Height * i / 3f;
                e.Graphics.DrawLine(third, x, frame.Top, x, frame.Bottom);
                e.Graphics.DrawLine(third, frame.Left, y, frame.Right, y);
            }

            foreach (var corner in Corners(frame))
                e.Graphics.FillRectangle(Theme.Brush(Theme.OnAccent), corner);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            var frame = CropView();
            _drag = Hit(frame, e.Location);
            _last = e.Location;
            _cropAtDrag = _crop;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _drag = Drag.None;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_drag == Drag.None || e.Button != MouseButtons.Left)
                return;
            var scale = ViewScale();
            var dx = (e.X - _last.X) / scale;
            var dy = (e.Y - _last.Y) / scale;
            if (_drag == Drag.Move)
            {
                _crop = Clamp(new RectangleF(_cropAtDrag.X + (e.X - PointToStart().X) / scale, _cropAtDrag.Y + (e.Y - PointToStart().Y) / scale, _crop.Width, _crop.Height));
            }
            else
            {
                var grow = _drag is Drag.East or Drag.South or Drag.SouthEast ? Math.Max(dx, dy) : -Math.Max(dx, dy);
                if (_drag is Drag.NorthEast)
                    grow = Math.Max(dx, -dy);
                if (_drag is Drag.SouthWest)
                    grow = Math.Max(-dx, dy);
                var side = Math.Max(32f, _cropAtDrag.Width + grow);
                var origin = _cropAtDrag.Location;
                if (_drag is Drag.NorthWest or Drag.North or Drag.West or Drag.NorthEast or Drag.SouthWest)
                {
                    origin = _drag switch
                    {
                        Drag.NorthWest => new PointF(_cropAtDrag.Right - side, _cropAtDrag.Bottom - side),
                        Drag.NorthEast => new PointF(_cropAtDrag.Left, _cropAtDrag.Bottom - side),
                        Drag.SouthWest => new PointF(_cropAtDrag.Right - side, _cropAtDrag.Top),
                        Drag.North => new PointF(_cropAtDrag.Left, _cropAtDrag.Bottom - side),
                        Drag.West => new PointF(_cropAtDrag.Right - side, _cropAtDrag.Top),
                        _ => origin
                    };
                }

                _crop = Clamp(new RectangleF(origin, new SizeF(side, side)));
            }

            _last = e.Location;
            _cropAtDrag = _crop;
            Invalidate();
            CropChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            SetZoom(_zoom * (e.Delta > 0 ? 1.1f : 1f / 1.1f));
        }

        private Point PointToStart() => _last;

        private RectangleF Clamp(RectangleF crop)
        {
            var side = Math.Min(crop.Width, Math.Min(_source.Width, _source.Height));
            var x = Math.Clamp(crop.X, 0, _source.Width - side);
            var y = Math.Clamp(crop.Y, 0, _source.Height - side);
            return new RectangleF(x, y, side, side);
        }

        private float ViewScale()
        {
            var fit = Math.Min((Width - Theme.Scale(24)) / (float)_source.Width, (Height - Theme.Scale(24)) / (float)_source.Height);
            return Math.Max(0.01f, fit) * _zoom;
        }

        private RectangleF ImageView()
        {
            var scale = ViewScale();
            var width = _source.Width * scale;
            var height = _source.Height * scale;
            return new RectangleF((Width - width) / 2f, (Height - height) / 2f, width, height);
        }

        private RectangleF CropView()
        {
            var image = ImageView();
            var scale = ViewScale();
            return new RectangleF(image.X + _crop.X * scale, image.Y + _crop.Y * scale, _crop.Width * scale, _crop.Height * scale);
        }

        private static RectangleF[] Corners(RectangleF frame)
        {
            var s = 10f;
            return
            [
                new RectangleF(frame.Left - s / 2, frame.Top - s / 2, s, s),
                new RectangleF(frame.Right - s / 2, frame.Top - s / 2, s, s),
                new RectangleF(frame.Left - s / 2, frame.Bottom - s / 2, s, s),
                new RectangleF(frame.Right - s / 2, frame.Bottom - s / 2, s, s)
            ];
        }

        private static Drag Hit(RectangleF frame, Point point)
        {
            var corners = Corners(frame);
            if (corners[0].Contains(point))
                return Drag.NorthWest;
            if (corners[1].Contains(point))
                return Drag.NorthEast;
            if (corners[2].Contains(point))
                return Drag.SouthWest;
            if (corners[3].Contains(point))
                return Drag.SouthEast;
            return frame.Contains(point) ? Drag.Move : Drag.None;
        }

        private enum Drag
        {
            None,
            Move,
            NorthWest,
            NorthEast,
            SouthWest,
            SouthEast,
            North,
            South,
            East,
            West
        }
    }
}
