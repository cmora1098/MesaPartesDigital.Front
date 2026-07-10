using MesaPartesDigital.Models;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MesaPartesDigital.Services;

public sealed class DocumentoService(HttpClient http) : ApiClientBase(http)
{
    public async Task<string> GuardarArchivoEnPCAsync(IBrowserFile archivo)
    {
        await using var stream = archivo.OpenReadStream(50 * 1024 * 1024);
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType ?? "application/octet-stream");
        content.Add(fileContent, "archivo", archivo.Name);
        using var response = await Http.PostAsync("api/archivos/subir", content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
        return result?.Ruta ?? throw new InvalidOperationException("No se recibió la ruta del archivo.");
    }

    public async Task<string> GuardarArchivoEnPCAsync(string nombreArchivo, string base64Data)
    {
        var raw = base64Data.Contains(',') ? base64Data[(base64Data.IndexOf(',') + 1)..] : base64Data;
        var bytes = Convert.FromBase64String(raw);
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(bytes);
        content.Add(fileContent, "archivo", nombreArchivo);
        using var response = await Http.PostAsync("api/archivos/subir", content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
        return result?.Ruta ?? throw new InvalidOperationException("No se recibió la ruta del archivo.");
    }

    public Task<RegistroDocumentoResponse> RegistroPersonaNatural_Home(RegistroDocumentoRequest request)
        => PostAsync<RegistroDocumentoRequest, RegistroDocumentoResponse>("api/documentos/registro-natural-externo", request);

    public Task<RegistroDocumentoResponse> RegistroTramiteInterno_Home(RegistroDocumentoRequest request)
        => PostAsync<RegistroDocumentoRequest, RegistroDocumentoResponse>("api/documentos/registro-natural-interno", request);

    public Task<RegistroDocumentoResponse> RegistrarPersonaJuridicaAsync(RegistroDocumentoRequest request, string rucEmpresa, string razonSocial)
        => PostAsync<object, RegistroDocumentoResponse>("api/documentos/registro-juridico", new { documento = request, rucEmpresa, razonSocial });

    public Task<List<TramiteDto>> ObtenerHistorialTramitesAsync(int iCodPer)
        => GetAsync<List<TramiteDto>>($"api/documentos/historial/{iCodPer}");

    private sealed class UploadResponse { public string Ruta { get; set; } = string.Empty; }
}
