namespace RosePark.Models
{
    public partial class Habitacione
    {
        public enum EstadoHabitacionEnum
        {
            Disponible,
            Mantenimiento
        }

        public int IdHabitacion { get; set; }

        public string NorHabitacion { get; set; } = null!;

        public string Descripcion { get; set; } = null!;

        public EstadoHabitacionEnum EstadoHabitacion { get; set; }

        public decimal PrecioHabitacion { get; set; }

        // Asegúrate de tener esta propiedad en la clase
        public string ImagenUrl { get; set; } = null!;

        public virtual ICollection<Paquete> Paquetes { get; set; } = new List<Paquete>();
    }
}
