using MesaPartesDigital.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace MesaPartesDigital.Services;

public class SessionService
{
    private readonly IJSRuntime _js;
    public LoginResultDto? UserSession { get; private set; }
    public event Action? OnSessionChanged;

    public SessionService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        if (UserSession == null)
        {
            try
            {
                var json = await _js.InvokeAsync<string>("localStorage.getItem", "user_session");
                if (!string.IsNullOrEmpty(json))
                {
                    UserSession = JsonSerializer.Deserialize<LoginResultDto>(json);
                    Notify();
                }
            }
            catch
            {
                // Ignorar errores de JS interop durante prerender
            }
        }
    }

    public void SetSession(LoginResultDto session)
    {
        UserSession = session;
        Notify();
    }

    public async Task ClearSessionAsync()
    {
        UserSession = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", "user_session");
        Notify();
    }

    public void Notify() => OnSessionChanged?.Invoke();
}