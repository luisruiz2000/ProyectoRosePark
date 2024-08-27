namespace RosePark.Models
{
    public class ResumenReservaViewModel
    {
        public int IdPaquete { get; set; }
        public string NombrePaquete { get; set; }
        public string Descripcion { get; set; }
        public string NorHabitacion { get; set; }
        public decimal PrecioTotal { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int NumeroPersonas { get; set; }
        public List<Servicio> ServiciosAdicionales { get; set; } // Servicios incluidos en el paquete
        public List<Servicio> ServiciosDisponibles { get; set; } // Todos los servicios disponibles
    }

}