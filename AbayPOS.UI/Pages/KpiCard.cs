using System.ComponentModel;
using AbayPOS.UI.Controls;
using AbayPOS.UI.Theming;

namespace AbayPOS.UI.Pages;

internal sealed partial class KpiCard : GlassPanel
{
    private ToolTip _titleTip = new();

    public KpiCard()
    {
        InitializeComponent();
    }

    public KpiCard(string title) : this() => Title = title;

    [DefaultValue("KPI")]
    public string Title
    {
        get => lblTitle.Text;
        set
        {
            lblTitle.Text = value;
            _titleTip.SetToolTip(lblTitle, value);
        }
    }

    public void SetValue(string value) => txtValue.SetValue(value);

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignTime.IsActive(this))
            return;
        Theme.ApplyRuntime(this);
    }
}