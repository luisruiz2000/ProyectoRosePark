using System;
using System.ComponentModel.DataAnnotations;

namespace RosePark.Models.ViewModels
{
    public class UsuarioViewModel
    {
        public int IdUsuario { get; set; }

        [Required]
        [EmailAddress]
        public string CorreoUsuario { get; set; }

        public string ClaveUsuario { get; set; }

        public int? IdPersonas { get; set; }

        [Required]
        public string NombrePersona { get; set; }

        [Required]
        public string ApellidosPersona { get; set; }

        [Required]
        public string TipoDocumentoPersona { get; set; }

        [Required]
        public string NroDocumentoPersona { get; set; }

        [Required]
        public int EdadPersona { get; set; }

        [Required]
        public string CelularPersona { get; set; }

        [Required]
        public DateOnly FechaNacimientoPersona { get; set; }

        public int IdRol { get; set; }

        public string? NombreRol { get; set; }

    }
}
