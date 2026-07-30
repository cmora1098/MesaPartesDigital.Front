using Microsoft.AspNetCore.Components;

namespace MesaPartesDigital.Components.Base;

public abstract class PageBase : ComponentBase
{
    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    protected void Ir(string ruta, bool forceLoad = false)
    {
        Navigation.NavigateTo(ruta.Trim('/'), forceLoad);
    }
}