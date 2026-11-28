using System.ComponentModel;
using System.Drawing.Drawing2D;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Imaging;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Dialogs;

/// <summary>Square crop with zoom, rotation and a live tile preview.</summary>
public sealed partial class ImageCropDialog : DialogForm
{
    public ImageCropDialog()
    {
        InitializeComponent();
        cropPreview.Source = cropSurface;
        cropSurface.CropChanged += (_, _) => cropPreview.Invalidate();
        btnApply.Click += (_, _) => Apply();
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        btnRotateLeft.Click += (_, _) => Action(() => cropSurface.Rotate(false));
        btnRotateRight.Click += (_, _) => Action(() => cropSurface.Rotate(true));
        btnReset.Click += (_, _) => Action(cropSurface.ResetView);
        trkZoom.ValueChanged += (_, _) => cropSurface.SetZoom(trkZoom.Value / 100f);
        AcceptButton = btnApply;
        CancelButton = btnCancel;
    }

    public ImageCropDialog(Bitmap source) : this()
    {
        cropSurface.SetSource(source);
        trkZoom.Value = (int)Math.Round(cropSurface.Zoom * 100f);
    }

    public byte[]? Jpeg { get; private set; }

    private void Apply()
    {
        using var cropped = cropSurface.Render();
        Jpeg = ProductImageCodec.ToJpeg(cropped);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void Action(Action click)
    {
        click();
        trkZoom.Value = (int)Math.Round(cropSurface.Zoom * 100f);
        cropPreview.Invalidate();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            cropSurface.DisposeSource();
        base.Dispose(disposing);
    }

    private sealed class CropPreview : Control
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CropSurface? Source { get; set; }

        public CropPreview()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.FromArgb(0x1C, 0x18, 0x2F);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using var clip = Gfx.Round(bounds, 12);
            e.Graphics.SetClip(clip);
            if (Source?.SourceImage is { } image)
                ProductImageCodec.DrawCover(e.Graphics, image, Source.CropPixels, bounds);
            e.Graphics.ResetClip();
            e.Graphics.DrawPath(new Pen(Color.FromArgb(0x2E, 0x29, 0x45)), clip);
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

        public CropSurface()
        {
            _source = DesignTime.IsActive() ? CreatePlaceholder() : new Bitmap(64, 64);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(0x0D, 0x0B, 0x14);
            ResetView();
        }

        public event EventHandler? CropChanged;

        public Bitmap SourceImage => _source;
        public float Zoom => _zoom;
        public Rectangle CropPixels => Rectangle.Round(_crop);

        public void SetSource(Bitmap source)
        {
            _source.Dispose();
            _source = source;
            ResetView();
        }

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

        private static Bitmap CreatePlaceholder()
        {
            var bmp = new Bitmap(480, 360);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(0x1A, 0x18, 0x24));
            using var accent = new SolidBrush(Color.FromArgb(0xF5, 0x7C, 0x1F));
            g.FillEllipse(accent, 140, 80, 200, 200);
            using var text = new SolidBrush(Color.FromArgb(0xA1, 0xA1, 0xAA));
            g.DrawString("Sample", new Font("Segoe UI", 16, FontStyle.Bold), text, 200, 28);
            return bmp;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var view = ImageView();
            e.Graphics.DrawImage(_source, view);
            var frame = CropView();
            using var dim = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
            e.Graphics.FillRectangle(dim, new RectangleF(0, 0, Width, frame.Top));
            e.Graphics.FillRectangle(dim, new RectangleF(0, frame.Bottom, Width, Height - frame.Bottom));
            e.Graphics.FillRectangle(dim, new RectangleF(0, frame.Top, frame.Left, frame.Height));
            e.Graphics.FillRectangle(dim, new RectangleF(frame.Right, frame.Top, Width - frame.Right, frame.Height));
            e.Graphics.DrawRectangle(new Pen(Color.FromArgb(0xF5, 0x7C, 0x1F), 2), Rectangle.Round(frame));
            var third = new Pen(Color.FromArgb(160, 255, 255, 255));
            for (var i = 1; i <= 2; i++)
            {
                var x = frame.Left + frame.Width * i / 3f;
                var y = frame.Top + frame.Height * i / 3f;
                e.Graphics.DrawLine(third, x, frame.Top, x, frame.Bottom);
                e.Graphics.DrawLine(third, frame.Left, y, frame.Right, y);
            }

            foreach (var corner in Corners(frame))
                e.Graphics.FillRectangle(Brushes.White, corner);
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
                _crop = Clamp(new RectangleF(_cropAtDrag.X + (e.X - PointToStart().X) / scale, _cropAtDrag.Y + (e.Y - PointToStart().Y) / scale, _crop.Width, _crop.Height));
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
            _ = dy;
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
            var fit = Math.Min((Width - 24) / (float)_source.Width, (Height - 24) / (float)_source.Height);
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
            const float s = 10f;
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