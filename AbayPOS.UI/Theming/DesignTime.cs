using System.ComponentModel;
using System.Diagnostics;

namespace AbayPOS.UI.Theming;

/// <summary>True while Visual Studio is creating or painting a control in the Designer.</summary>
public static class DesignTime
{
    public static bool IsActive
    {
        get
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;
            var name = Process.GetCurrentProcess().ProcessName;
            return name is "devenv" or "DesignToolsServer" or "XDesProc";
        }
    }

    public static bool IsActive(Control control) =>
        IsActive
        || control.Site?.DesignMode == true
        || control.DesignMode
        || (control.FindForm()?.Site?.DesignMode == true);
}