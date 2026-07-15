namespace MesaPartesDigital.Models
{
    public class InicioPersJuridicaDto
    {
        public int? ICodPer { get; set; }

        // Datos de Empresa (Cambiamos nombres para que coincidan con los parámetros del SP)
        public string VRUC { get; set; } // Ajustado de VRucEmpresa
        public string VRazonSocial { get; set; }

        // Datos de Representante (Correctos)
        public int? ICodTipoDocPer { get; set; }
        public string VDocPer { get; set; }
        public string VNombres { get; set; }
        public string VApellidoPaterno { get; set; }
        public string VApellidoMaterno { get; set; }
        public string VEmail { get; set; }
        public string VTelefono { get; set; }
        public string VDireccion { get; set; }
        public string VCodDistrito { get; set; }

        // Datos de Documento (Correctos)
        public int? ICodTipoDoc { get; set; }
        public string VNroDoc { get; set; }
        public DateTime? DFecDoc { get; set; } = DateTime.Today;
        public string VNombreAsunto { get; set; }
        public string VReferencia { get; set; }
        public string VNroPagFolios { get; set; }

        // Lista de archivos
        public List<ArchivoRequestIPJ> Archivos { get; set; } = new();
    }  
    public class ArchivoRequestIPJ
        {
            public string VRutaDoc { get; set; } = string.Empty;
            public bool BTipo { get; set; } // true (1) = Principal, false (0) = Anexo
        }
    public class RegistroInicioPerJuridicaResponse
    {
        public int ICodDoc { get; set; }
        public int ICodAsunto { get; set; }
        public int ICodPer { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MailSeguimiento { get; set; } = string.Empty;
        public string? VAutoGenerado { get; set; } = string.Empty; // Puede ser null en caso de Anexos (Hijos)
    }
}
