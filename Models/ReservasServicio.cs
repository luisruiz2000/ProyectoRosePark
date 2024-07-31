using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class ReservasServicio
{
    public int IdReservasServicios { get; set; }

    public int? IdServicio { get; set; }

    public int? IdReserva { get; set; }

    public virtual Reserva? IdReservaNavigation { get; set; }

    public virtual Servicio? IdServicioNavigation { get; set; }
}
