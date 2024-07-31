using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class Paquete
{
    public int IdPaquete { get; set; }

    public string NombrePaquete { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public decimal PrecioTotal { get; set; }

    public string Estado { get; set; } = null!;

    public int? IdHabitacion { get; set; }

    public virtual Habitacione? IdHabitacionNavigation { get; set; }

    public virtual ICollection<PaquetesServicio> PaquetesServicios { get; set; } = new List<PaquetesServicio>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
