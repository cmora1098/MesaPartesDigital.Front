using Microsoft.AspNetCore.Components.Forms;

namespace MesaPartesDigital.Models  
{
    public class ArchivoAdjunto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Base64 { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Tamano { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
        public IBrowserFile? FileBrowser { get; set; }

    }
}