using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public DateTime FechaReserva { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public int NroPersonas { get; set; }

    public decimal? MontoTotal { get; set; }

    public decimal? Abono { get; set; }

    public string? EstadoReserva { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdPaquete { get; set; }

    public virtual Paquete? IdPaqueteNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<ReservasServicio> ReservasServicios { get; set; } = new List<ReservasServicio>();
}
