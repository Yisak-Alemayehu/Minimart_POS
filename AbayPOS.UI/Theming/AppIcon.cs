namespace AbayPOS.UI.Theming;

/// <summary>Loads the application icon embedded beside the executable.</summary>
public static class AppIcon
{
    private static Icon? _icon;

    public static Icon Load()
    {
        if (_icon is not null)
            return _icon;

        var path = Path.Combine(AppContext.BaseDirectory, "Resources", "abaypos.ico");
        _icon = File.Exists(path) ? new Icon(path) : SystemIcons.Application;
        return _icon;
    }
}
