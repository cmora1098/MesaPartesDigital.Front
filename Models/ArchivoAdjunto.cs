using Microsoft.AspNetCore.Components.Forms;

namespace MesaPartesDigital.Models  
{
    public class ArchivoAdjunto
    {
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Tamano { get; set; } = "";
        public byte[] Contenido { get; set; } = Array.Empty<byte>(); // Aquí guardamos los bytes
        public IBrowserFile? FileBrowser { get; set; }
    }
}