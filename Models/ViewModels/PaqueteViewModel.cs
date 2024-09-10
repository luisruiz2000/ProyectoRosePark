using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RosePark.Models.ViewModels
{
    public class PaqueteViewModel
    {
        public int IdPaquete { get; set; }

        [Required(ErrorMessage = "El nombre del paquete es obligatorio.")]
        public string NombrePaquete { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El precio total es obligatorio.")]
        public decimal PrecioTotal { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una habitación.")]
        public int IdHabitacion { get; set; }

        public string Estado { get; set; }

        public List<SelectListItem> Habitaciones { get; set; }

        [Required(ErrorMessage = "Debe seleccionar al menos un servicio.")]
        public List<int> ServiciosSeleccionados { get; set; } = new List<int>(); // Inicialización aquí

        public List<SelectListItem> Servicios { get; set; }

        // Propiedades no obligatorias usadas en Dashboard
        public string Paquete { get; set; }
        public int? CantidadReservas { get; set; }
    }

}
