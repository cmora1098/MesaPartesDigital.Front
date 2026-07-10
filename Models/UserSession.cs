namespace MesaPartesDigital.Models
{
    public class UserSession
    {
        public int ICodPer { get; set; }
        public string VNombreCompleto { get; set; } = string.Empty;
        public string VEmail { get; set; } = string.Empty;

        // Propiedad calculada para saber si hay sesión activa sin revisar el ID
        public bool IsAuthenticated => ICodPer > 0;

        public void LimpiarSesion()
        {
            ICodPer = 0;
            VNombreCompleto = string.Empty;
            VEmail = string.Empty;
        }
    }
}