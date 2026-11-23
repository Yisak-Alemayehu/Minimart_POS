using AbayPOS.Business.Composition;
using AbayPOS.Business.Imaging;
using AbayPOS.Business.Services;
using AbayPOS.Domain.Logging;

namespace AbayPOS.UI.Infrastructure;

/// <summary>Composition root populated from <c>Program.cs</c>. Forms resolve services here instead of new-ing repositories.</summary>
public static class ServiceLocator
{
    private static AppHost? _host;

    /// <summary>True after <see cref="Configure"/> runs.</summary>
    public static bool IsConfigured => _host is not null;

    public static AppHost Host => _host ?? throw new InvalidOperationException("AbayPOS has not started yet.");

    public static IAuthService Auth => Host.Auth;
    public static IProductService Products => Host.Products;
    public static ISalesService Sales => Host.Sales;
    public static IInventoryService Inventory => Host.Inventory;
    public static ISupplierService Suppliers => Host.Suppliers;
    public static IUserService Users => Host.Users;
    public static IReportService Reports => Host.Reports;
    public static IProductImageStore Images => Host.Images;
    public static IAppLogger Logger => Host.Logger;

    public static void Configure(AppHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        CrashGuard.Logger = host.Logger;
    }

    public static void Reset() => _host = null;
}
