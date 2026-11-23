using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace AbayPOS.UI.Imaging;

/// <summary>Loads, orients, crops and encodes product photos. Callers own every returned bitmap.</summary>
public static class ProductImageCodec
{
    public const int TileSize = 512;
    private const int OrientationId = 0x0112;

    public static Bitmap FromFile(string path)
    {
        using var stream = File.OpenRead(path);
        return FromStream(stream);
    }

    public static Bitmap FromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes, false);
        return FromStream(stream);
    }

    public static Bitmap FromStream(Stream stream)
    {
        using var decoded = new Bitmap(stream);
        var copy = new Bitmap(decoded.Width, decoded.Height, PixelFormat.Format24bppRgb);
        using (var graphics = Graphics.FromImage(copy))
            graphics.DrawImage(decoded, 0, 0, decoded.Width, decoded.Height);
        ApplyExif(decoded, copy);
        return copy;
    }

    public static Bitmap Rotate(Bitmap source, bool clockwise)
    {
        var copy = new Bitmap(source);
        copy.RotateFlip(clockwise ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate270FlipNone);
        return copy;
    }

    public static Bitmap CropToSquare(Bitmap source, Rectangle crop)
    {
        var bounds = new Rectangle(0, 0, source.Width, source.Height);
        var safe = Rectangle.Intersect(crop, bounds);
        if (safe.Width < 1 || safe.Height < 1)
        {
            var side = Math.Min(source.Width, source.Height);
            safe = new Rectangle((source.Width - side) / 2, (source.Height - side) / 2, side, side);
        }

        var edge = Math.Min(safe.Width, safe.Height);
        var output = new Bitmap(TileSize, TileSize, PixelFormat.Format24bppRgb);
        using var graphics = Graphics.FromImage(output);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.DrawImage(source, new Rectangle(0, 0, TileSize, TileSize), new Rectangle(safe.X, safe.Y, edge, edge), GraphicsUnit.Pixel);
        return output;
    }

    public static Bitmap Thumbnail(Bitmap source, int size)
    {
        var output = new Bitmap(Math.Max(1, size), Math.Max(1, size), PixelFormat.Format24bppRgb);
        using var graphics = Graphics.FromImage(output);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.Clear(Color.FromArgb(28, 24, 47));
        DrawCover(graphics, source, new Rectangle(0, 0, output.Width, output.Height));
        return output;
    }

    public static void DrawCover(Graphics graphics, Bitmap bitmap, Rectangle source, Rectangle dest) =>
        graphics.DrawImage(bitmap, dest, source, GraphicsUnit.Pixel);

    public static void DrawCover(Graphics graphics, Bitmap bitmap, Rectangle dest)
    {
        var scale = Math.Max(dest.Width / (float)bitmap.Width, dest.Height / (float)bitmap.Height);
        var width = bitmap.Width * scale;
        var height = bitmap.Height * scale;
        graphics.DrawImage(bitmap, dest.X + (dest.Width - width) / 2f, dest.Y + (dest.Height - height) / 2f, width, height);
    }

    public static byte[] ToJpeg(Bitmap bitmap, long quality = 85)
    {
        var codec = ImageCodecInfo.GetImageEncoders().First(item => item.FormatID == ImageFormat.Jpeg.Guid);
        using var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
        using var stream = new MemoryStream();
        bitmap.Save(stream, codec, parameters);
        return stream.ToArray();
    }

    private static void ApplyExif(Bitmap decoded, Bitmap copy)
    {
        if (!decoded.PropertyIdList.Contains(OrientationId))
            return;
        var item = decoded.GetPropertyItem(OrientationId);
        if (item?.Value is not byte[] raw || raw.Length < 2)
            return;
        var flip = BitConverter.ToUInt16(raw, 0) switch
        {
            2 => RotateFlipType.RotateNoneFlipX,
            3 => RotateFlipType.Rotate180FlipNone,
            4 => RotateFlipType.Rotate180FlipX,
            5 => RotateFlipType.Rotate90FlipX,
            6 => RotateFlipType.Rotate90FlipNone,
            7 => RotateFlipType.Rotate270FlipX,
            8 => RotateFlipType.Rotate270FlipNone,
            _ => RotateFlipType.RotateNoneFlipNone
        };
        if (flip != RotateFlipType.RotateNoneFlipNone)
            copy.RotateFlip(flip);
    }
}
