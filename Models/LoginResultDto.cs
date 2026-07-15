namespace MesaPartesDigital.Models
{
    public class LoginResultDto
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = "Error no especificado";

        // Estos campos deben coincidir con los alias de tu SP:
        // [iCodPer], [vNombreCompleto], [vEmail]
        public int ICodPer { get; set; }
        public string VNombreCompleto { get; set; } = string.Empty;
        public string VEmail { get; set; } = string.Empty;

        // Agregamos el DNI por si lo necesitas en el front después del login
        public string VDocPer { get; set; } = string.Empty;
    }
}
