namespace AbayPOS.UI.Infrastructure;

/// <summary>Composition root populated from <c>Program.cs</c>. Forms resolve services here instead of new-ing repositories.</summary>
public static class ServiceLocator
{
    /// <summary>True after <see cref="Configure"/> runs.</summary>
    public static bool IsConfigured { get; private set; }

    /// <summary>Marks the locator as ready. Phase 2 fills this with real services.</summary>
    public static void Configure() => IsConfigured = true;

    public static void Reset() => IsConfigured = false;
}
