using AbayPOS.Business.Imaging;

namespace AbayPOS.Tests;

public sealed class ProductImageStoreTests
{
    [Fact]
    public void Save_replace_and_delete_keep_only_the_current_file()
    {
        var root = Path.Combine(Path.GetTempPath(), "abaypos-images-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new FileProductImageStore(root);
            var first = store.Save(7, [1, 2, 3, 4], null);
            Assert.True(File.Exists(Path.Combine(root, first)));
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, store.Load(first));

            var second = store.Save(7, [9, 8, 7], first);
            Assert.NotEqual(first, second);
            Assert.False(File.Exists(Path.Combine(root, first)));
            Assert.Equal(new byte[] { 9, 8, 7 }, store.Load(second));
            Assert.Null(store.Load(@"..\secret.txt"));

            store.Delete(second);
            Assert.False(File.Exists(Path.Combine(root, second)));
            Assert.Null(store.Load(second));
            store.Delete(second);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }
}
