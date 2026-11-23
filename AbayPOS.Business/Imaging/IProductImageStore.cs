namespace AbayPOS.Business.Imaging;

/// <summary>Stores product photos as files. Callers pass JPEG bytes and receive only the file name.</summary>
public interface IProductImageStore
{
    /// <summary>Writes a JPEG and returns its file name. Deletes <paramref name="previousName"/> when it is different.</summary>
    string Save(int productId, ReadOnlySpan<byte> jpeg, string? previousName);

    /// <summary>Reads a previously saved file, or null when the name is empty or the file is gone.</summary>
    byte[]? Load(string? relativeName);

    /// <summary>Deletes a previously saved file. Missing files are ignored.</summary>
    void Delete(string? relativeName);
}
