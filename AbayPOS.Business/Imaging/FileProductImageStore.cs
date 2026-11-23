namespace AbayPOS.Business.Imaging;

/// <summary>File-system store under %LocalAppData%\AbayPOS\images\products.</summary>
public sealed class FileProductImageStore : IProductImageStore
{
    public FileProductImageStore(string? root = null)
    {
        Root = root ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AbayPOS",
            "images",
            "products");
    }

    public string Root { get; }

    public string Save(int productId, ReadOnlySpan<byte> jpeg, string? previousName)
    {
        if (jpeg.IsEmpty)
            throw new ArgumentException("Image data is empty.", nameof(jpeg));
        Directory.CreateDirectory(Root);
        var name = $"{productId}_{Guid.NewGuid():N}.jpg";
        File.WriteAllBytes(Path.Combine(Root, name), jpeg.ToArray());
        if (!string.IsNullOrWhiteSpace(previousName)
            && !string.Equals(previousName, name, StringComparison.OrdinalIgnoreCase))
            Delete(previousName);
        return name;
    }

    public byte[]? Load(string? relativeName)
    {
        var path = SafePath(relativeName);
        return path is null || !File.Exists(path) ? null : File.ReadAllBytes(path);
    }

    public void Delete(string? relativeName)
    {
        var path = SafePath(relativeName);
        if (path is not null && File.Exists(path))
            File.Delete(path);
    }

    private string? SafePath(string? relativeName)
    {
        if (string.IsNullOrWhiteSpace(relativeName))
            return null;
        var name = Path.GetFileName(relativeName);
        if (!string.Equals(name, relativeName.Trim(), StringComparison.OrdinalIgnoreCase))
            return null;
        return Path.Combine(Root, name);
    }
}
