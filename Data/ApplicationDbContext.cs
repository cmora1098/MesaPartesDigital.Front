using MesaPartesDigital.Models;
using System.Net.Http.Json;

namespace MesaPartesDigital.Data;

// Adaptador temporal para conservar las páginas actuales sin acceso directo a SQL Server.
public sealed class ApplicationDbContext(HttpClient http)
{
    public async Task<List<PersonaJuridicaDto>> ObtenerPersonaJuridicaPorRucAsync(int tipo, string documento)
    {
        var item = await http.GetFromJsonAsync<PersonaJuridicaDto?>($"api/personas/juridica?tipoDocumento={tipo}&documento={Uri.EscapeDataString(documento)}");
        return item is null ? [] : [item];
    }
}
