using Microsoft.AspNetCore.Components;

namespace MesaPartesDigital.Components.Base;

public abstract class PageBase : LayoutComponentBase
{
    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    public void Ir(string ruta, bool forceLoad = false)
    {
        Navigation.NavigateTo(ruta.Trim('/'), forceLoad);
    }
}