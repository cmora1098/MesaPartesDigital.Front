using MesaPartesDigital.Models;

namespace MesaPartesDigital.Services;

public sealed class TipoPersonaService(HttpClient http) : ApiClientBase(http)
{
    public Task<List<TipoPersona>> ObtenerTiposPersonaAsync() => GetAsync<List<TipoPersona>>("api/catalogos/tipos-persona");
}
public sealed class TipoDocumentoService(HttpClient http) : ApiClientBase(http)
{
    public Task<List<TipoDocumento>> ObtenerTiposDocumentoAsync() => GetAsync<List<TipoDocumento>>("api/catalogos/tipos-documento");
}
public sealed class EstadoService(HttpClient http) : ApiClientBase(http)
{
    public Task<List<Estado>> ObtenerEstadosAsync() => GetAsync<List<Estado>>("api/catalogos/estados");
}
public sealed class TipoDocPerService(HttpClient http) : ApiClientBase(http)
{
    public Task<List<TipoDocPer>> ObtenerTiposDocPerAsync() => GetAsync<List<TipoDocPer>>("api/catalogos/tipos-documento-persona");
    public Task<PersonaNaturalDto?> BuscarPersonaPorDocumentoAsync(int tipo, string documento)
        => GetAsync<PersonaNaturalDto?>($"api/personas/natural?tipoDocumento={tipo}&documento={Uri.EscapeDataString(documento)}");
}
