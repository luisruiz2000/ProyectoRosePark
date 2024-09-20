using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class Reserva
{
    public enum EstadoReservaEnum
    {
        Pendiente = 0,
        Confirmada = 1,
        CheckIn = 2,
        CheckOut = 3,
        Cancelada = 4
    }
    public int IdReserva { get; set; }

    public DateTime FechaReserva { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public int NroPersonas { get; set; }

    public decimal? MontoTotal { get; set; }

    public decimal? Abono { get; set; }

    public EstadoReservaEnum EstadoReserva { get; set; }  // Cambiado a Enum

    public int? IdUsuario { get; set; }

    public int? IdPaquete { get; set; }

    public virtual Paquete? IdPaqueteNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<ReservasServicio> ReservasServicios { get; set; } = new List<ReservasServicio>();
}
