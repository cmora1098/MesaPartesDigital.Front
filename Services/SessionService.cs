using MesaPartesDigital.Models;

namespace MesaPartesDigital.Services;

public class SessionService
{
    public LoginResultDto? UserSession { get; set; }
    public event Action? OnSessionChanged;
    public void Notify() => OnSessionChanged?.Invoke();
}