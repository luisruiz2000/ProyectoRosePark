namespace RosePark.Models
{
    public class ConfirmacionReservaViewModel
{
    public int IdReserva { get; set; }
    public Paquete Paquete { get; set; }
    public List<Servicio> ServiciosAdicionales { get; set; }
    public decimal PrecioTotal { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}

}
