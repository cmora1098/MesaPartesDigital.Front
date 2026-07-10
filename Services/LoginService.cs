namespace MesaPartesDigital.Services;

public sealed class LoginService(HttpClient http) : ApiClientBase(http), ILoginService
{
    public Task<LoginResultDto> ValidarCredencialesAsync(string email, string password)
        => PostAsync<object, LoginResultDto>("api/autenticacion/login", new { documento = email, password });
    public Task<GestionCredencialesResultDto> GestionarCredencialesAsync(string email, string? codigoOtp, int tipoAccion)
        => PostAsync<object, GestionCredencialesResultDto>("api/autenticacion/credenciales", new { documento = email, codigoOtp, tipoAccion });
}
