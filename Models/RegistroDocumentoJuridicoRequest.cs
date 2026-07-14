namespace MesaPartesDigital.Models // Usa el namespace que prefieras
{
    public class RegistroDocumentoJuridicoRequest
    {
        // Datos de Empresa
        public string VRucEmpresa { get; set; }
        public string VRazonSocial { get; set; }
        // Datos de Representante
        public int ICodTipoDocPer { get; set; }
        public string VDocPer { get; set; } = string.Empty;
        public string VNombres { get; set; } = string.Empty;
        public string VApellidoPaterno { get; set; } = string.Empty;
        public string VApellidoMaterno { get; set; } = string.Empty;
        public string VEmail { get; set; } = string.Empty;
        public string VTelefono { get; set; } = string.Empty;
        public string VDireccion { get; set; } = string.Empty;
        public string VCodDistrito { get; set; } = string.Empty;
        
        // Datos de Documento
        public int ICodTipoDoc { get; set; }
        public string VNroDoc { get; set; }
        public DateTime DFecDoc { get; set; }
        public string VReferencia { get; set; }
        public string VNroPagFolios { get; set; }
        public string VLink { get; set; }
        public List<ArchivoRequest> Archivos { get; set; } = new();
    }
}