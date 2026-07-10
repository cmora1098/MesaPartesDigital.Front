using System.Net.Http.Json;

namespace MesaPartesDigital.Services;

public abstract class ApiClientBase
{
    protected readonly HttpClient Http;
    protected ApiClientBase(HttpClient http) => Http = http;

    protected async Task<T> GetAsync<T>(string url)
        => await Http.GetFromJsonAsync<T>(url) ?? throw new InvalidOperationException($"La API no devolvió datos para {url}.");

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request)
    {
        using var response = await Http.PostAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>()
            ?? throw new InvalidOperationException($"La API no devolvió una respuesta válida para {url}.");
    }
}
