using System.Text.Json.Serialization;

namespace MesaPartesDigital.Models
{
    public class LoginResultDto
    {
        [JsonPropertyName("exitoso")]
        public bool Exitoso { get; set; }

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; } = "Error no especificado";

        [JsonPropertyName("iCodPer")]
        public int ICodPer { get; set; }

        [JsonPropertyName("vNombreCompleto")]
        public string VNombreCompleto { get; set; } = string.Empty;

        [JsonPropertyName("vEmail")]
        public string VEmail { get; set; } = string.Empty;

        // Si el JSON no trae vDocPer, no fallará, pero si lo trae, ya lo mapeará
        [JsonPropertyName("vDocPer")]
        public string VDocPer { get; set; } = string.Empty;
    }
}