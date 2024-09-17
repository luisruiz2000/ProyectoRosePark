using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using RosePark.Models; // Asegúrate de importar el espacio de nombres donde está definido el Enum

namespace RosePark.Models.ViewModels
{
    public class PaqueteViewModel
    {
        public int IdPaquete { get; set; }

        [Required(ErrorMessage = "El nombre del paquete es obligatorio.")]
        public string NombrePaquete { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una habitación.")]
        public int IdHabitacion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el estado del paquete.")]
        public Paquete.EstadoEnum Estado { get; set; } // Cambiado a Enum

        public List<SelectListItem> Habitaciones { get; set; }

        [Required(ErrorMessage = "Debe seleccionar al menos un servicio.")]
        public List<int> ServiciosSeleccionados { get; set; } = new List<int>(); // Inicialización aquí

        public List<SelectListItem> Servicios { get; set; }

        // Propiedades no obligatorias usadas en Dashboard
        public string Paquete { get; set; }
        public int? CantidadReservas { get; set; }
        // Nueva propiedad para la URL de la imagen de la habitación
        public string ImagenUrl { get; set; } // Asegúrate de asignar esta propiedad cuando cargues los datos

    }
}
