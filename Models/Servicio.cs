using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class Servicio
{
    public int IdServicio { get; set; }

    public string NombreServicio { get; set; } = null!;

    public string DescripcionServicio { get; set; } = null!;

    public decimal PrecioServicio { get; set; }

    public string EstadoServicio { get; set; } = null!;

    public virtual ICollection<PaquetesServicio> PaquetesServicios { get; set; } = new List<PaquetesServicio>();

    public virtual ICollection<ReservasServicio> ReservasServicios { get; set; } = new List<ReservasServicio>();
}
