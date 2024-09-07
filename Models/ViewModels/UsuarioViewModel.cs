namespace RosePark.Models.ViewModels
{
    public class UsuarioViewModel
    {
        public int IdUsuario { get; set; }
        public string CorreoUsuario { get; set; } = null!;
        public string ClaveUsuario { get; set; } = null!;
        public int? IdPersonas { get; set; }
        public string NombrePersona { get; set; } = null!;
        public string ApellidosPersona { get; set; } = null!;
        public string TipoDocumentoPersona { get; set; } = null!;
        public string NroDocumentoPersona { get; set; } = null!;
        public int EdadPersona { get; set; }
        public string CelularPersona { get; set; } = null!;
        public DateOnly FechaNacimientoPersona { get; set; }
        public int? IdRol { get; set; }
        public string NombreRol { get; set; } = null!;  // Cambia esto a NombreRol
    }

}