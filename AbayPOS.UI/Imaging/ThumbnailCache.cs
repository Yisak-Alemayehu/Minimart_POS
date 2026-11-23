using AbayPOS.UI.Infrastructure;

namespace AbayPOS.UI.Imaging;

/// <summary>Small decoded bitmaps keyed by file name and pixel size. The cache owns the bitmaps.</summary>
public sealed class ThumbnailCache
{
    public static ThumbnailCache Shared { get; } = new(200);

    private readonly int _max;
    private readonly object _gate = new();
    private readonly Dictionary<string, LinkedListNode<Entry>> _map = new();
    private readonly LinkedList<Entry> _order = new();

    public ThumbnailCache(int max) => _max = Math.Max(8, max);

    public Bitmap? Get(string? relativeName, int size)
    {
        if (string.IsNullOrWhiteSpace(relativeName))
            return null;
        lock (_gate)
        {
            if (!_map.TryGetValue(Key(relativeName, size), out var node))
                return null;
            _order.Remove(node);
            _order.AddFirst(node);
            return node.Value.Bitmap;
        }
    }

    public void Request(Control owner, string relativeName, int size, Action ready)
    {
        if (Get(relativeName, size) is not null)
        {
            ready();
            return;
        }

        _ = Task.Run(() =>
        {
            try
            {
                var bytes = ServiceLocator.Images.Load(relativeName);
                if (bytes is null)
                    return;
                using var full = ProductImageCodec.FromBytes(bytes);
                var thumb = ProductImageCodec.Thumbnail(full, size);
                Put(relativeName, size, thumb);
                if (!owner.IsDisposed && owner.IsHandleCreated)
                    owner.BeginInvoke(ready);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        });
    }

    private void Put(string relativeName, int size, Bitmap bitmap)
    {
        var key = Key(relativeName, size);
        lock (_gate)
        {
            if (_map.TryGetValue(key, out var existing))
            {
                existing.Value.Bitmap.Dispose();
                _order.Remove(existing);
                _map.Remove(key);
            }

            var node = _order.AddFirst(new Entry(key, bitmap));
            _map[key] = node;
            while (_order.Count > _max)
            {
                var last = _order.Last!;
                _order.RemoveLast();
                _map.Remove(last.Value.Key);
                last.Value.Bitmap.Dispose();
            }
        }
    }

    private static string Key(string relativeName, int size) => relativeName + "|" + size;

    private readonly record struct Entry(string Key, Bitmap Bitmap);
}
