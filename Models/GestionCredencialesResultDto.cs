namespace MesaPartesDigital.Models
{
    public class GestionCredencialesResultDto
    {
        // Indica si la operación en el SP fue exitosa (1 = Sí, 0 = No)
        public bool Exitoso { get; set; }

        // Mensaje de respuesta (ej. "Cuenta verificada", "Código incorrecto", etc.)
        public string Mensaje { get; set; } = string.Empty;

        // Contendrá la contraseña plana solo si el SP la generó exitosamente
        // Usamos string? porque puede ser null (si hubo error)
        public string? PasswordGenerado { get; set; }
        public string? EmailDestino { get; set; }   
    }
}
