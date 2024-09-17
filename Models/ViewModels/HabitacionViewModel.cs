namespace RosePark.Models
{

    public class HabitacionViewModel
    {
        public int IdHabitacion { get; set; }
        public string NorHabitacion { get; set; }
        public string Descripcion { get; set; }
        public Habitacione.EstadoHabitacionEnum EstadoHabitacion { get; set; }
        public decimal PrecioHabitacion { get; set; }
    }
}
