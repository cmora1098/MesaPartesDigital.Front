using MesaPartesDigital.Models;

namespace MesaPartesDigital.Services;

public sealed class UbigeoService(HttpClient http) : ApiClientBase(http)
{
    public Task<List<Departamento>> ObtenerDepartamentosAsync() => GetAsync<List<Departamento>>("api/ubigeo/departamentos");
    public Task<List<Provincia>> ObtenerProvinciasAsync(string codigo) => GetAsync<List<Provincia>>($"api/ubigeo/provincias/{Uri.EscapeDataString(codigo)}");
    public Task<List<Distrito>> ObtenerDistritosAsync(string codigo) => GetAsync<List<Distrito>>($"api/ubigeo/distritos/{Uri.EscapeDataString(codigo)}");
}
