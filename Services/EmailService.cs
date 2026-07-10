namespace MesaPartesDigital.Services;

public interface IEmailService
{
    Task<bool> EnviarCodigoOtpAsync(string correoDestino, string codigoOtp);
    Task<bool> EnviarCargoDigitalAsync(string correoDestino, string codigoTramite);
    Task<bool> EnviarPasswordSistemaAsync(string correoDestino, string passwordTemporal);
    Task<bool> EnviarConfirmacionTramiteAsync(string correoDestino, string codigoTramite, string asuntoTramite);
}

public sealed class EmailService(HttpClient http) : ApiClientBase(http), IEmailService
{
    public Task<bool> EnviarCodigoOtpAsync(string correoDestino, string codigoOtp)
        => PostAsync<object, bool>("api/correos/otp", new { correo = correoDestino, codigo = codigoOtp });
    public Task<bool> EnviarCargoDigitalAsync(string correoDestino, string codigoTramite)
        => PostAsync<object, bool>("api/correos/cargo", new { correo = correoDestino, codigoTramite });
    public Task<bool> EnviarConfirmacionTramiteAsync(string correoDestino, string codigoTramite, string asuntoTramite)
        => PostAsync<object, bool>("api/correos/confirmacion", new { correo = correoDestino, codigoTramite, asunto = asuntoTramite });
    public Task<bool> EnviarPasswordSistemaAsync(string correoDestino, string passwordTemporal)
        => Task.FromResult(false); // El cambio/restablecimiento lo gestiona AutenticacionController.
}
