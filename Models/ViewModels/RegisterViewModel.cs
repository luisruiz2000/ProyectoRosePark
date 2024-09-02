using System;
using System.ComponentModel.DataAnnotations;

namespace RosePark.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string CorreoUsuario { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string ClaveUsuario { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("ClaveUsuario", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarClave { get; set; }

        [Required]
        public string Nombres { get; set; }

        [Required]
        public string Apellidos { get; set; }

        [Required]
        public string TipoDocumento { get; set; }

        [Required]
        public string NroDocumento { get; set; }

        [Required]
        public int Edad { get; set; }

        [Required]
        public string Celular { get; set; }

        [Required]
        public DateOnly FechaNacimiento { get; set; }

        public int? IdRol { get; set; }
    }
}
