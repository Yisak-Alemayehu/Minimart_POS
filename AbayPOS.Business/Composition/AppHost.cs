using AbayPOS.Business.Logging;
using AbayPOS.Business.Security;
using AbayPOS.Business.Services;
using AbayPOS.Data;
using AbayPOS.Data.Sqlite;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Logging;

namespace AbayPOS.Business.Composition;

/// <summary>Composition root: connection factory, unit of work, hasher, and services.</summary>
public sealed class AppHost
{
    public AppHost(
        IAuthService auth,
        IProductService products,
        ISalesService sales,
        IInventoryService inventory,
        ISupplierService suppliers,
        IUserService users,
        IReportService reports,
        IAppLogger logger,
        string databasePath)
    {
        Auth = auth;
        Products = products;
        Sales = sales;
        Inventory = inventory;
        Suppliers = suppliers;
        Users = users;
        Reports = reports;
        Logger = logger;
        DatabasePath = databasePath;
    }

    public IAuthService Auth { get; }
    public IProductService Products { get; }
    public ISalesService Sales { get; }
    public IInventoryService Inventory { get; }
    public ISupplierService Suppliers { get; }
    public IUserService Users { get; }
    public IReportService Reports { get; }
    public IAppLogger Logger { get; }
    public string DatabasePath { get; }

    /// <summary>Database file used when the UI does not pass a path.</summary>
    public static string DefaultDatabasePath =>
        Path.Combine(AppContext.BaseDirectory, "abaypos.db");

    /// <summary>Builds the runtime, creates the SQLite file, and seeds default users.</summary>
    public static async Task<AppHost> StartAsync(string? databasePath = null, IAppLogger? logger = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(databasePath) ? DefaultDatabasePath : databasePath;
        logger ??= new FileLogger();
        var connectionFactory = new SqliteConnectionFactory(path);
        var initializer = new DatabaseInitializer(connectionFactory);
        await initializer.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        IUnitOfWorkFactory uowFactory = new SqliteUnitOfWorkFactory(connectionFactory);
        IPasswordHasher hasher = new PasswordHasher();
        await SeedUsersAsync(uowFactory, hasher, cancellationToken).ConfigureAwait(false);

        return new AppHost(
            new AuthService(uowFactory, hasher, logger),
            new ProductService(uowFactory, logger),
            new SalesService(uowFactory, logger),
            new InventoryService(uowFactory, logger),
            new SupplierService(uowFactory),
            new UserService(uowFactory, hasher),
            new ReportService(uowFactory),
            logger,
            path);
    }

    private static async Task SeedUsersAsync(IUnitOfWorkFactory factory, IPasswordHasher hasher, CancellationToken cancellationToken)
    {
        await using var uow = factory.Create();
        await uow.BeginAsync(cancellationToken).ConfigureAwait(false);
        if (await uow.Users.CountAsync(cancellationToken).ConfigureAwait(false) > 0)
        {
            await uow.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await InsertUserAsync(uow, hasher, "admin", "Store Manager", "admin123", UserRole.Manager, cancellationToken).ConfigureAwait(false);
        await InsertUserAsync(uow, hasher, "cashier", "Abebe Kebede", "cashier123", UserRole.Cashier, cancellationToken).ConfigureAwait(false);
        await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task InsertUserAsync(
        IUnitOfWork uow,
        IPasswordHasher hasher,
        string username,
        string fullName,
        string password,
        UserRole role,
        CancellationToken cancellationToken)
    {
        var (hash, salt) = hasher.Hash(password);
        await uow.Users.InsertAsync(new User
        {
            Username = username,
            FullName = fullName,
            PasswordHash = hash,
            Salt = salt,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.Now
        }, cancellationToken).ConfigureAwait(false);
    }
}
