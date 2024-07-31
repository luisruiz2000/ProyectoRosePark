using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class Habitacione
{
    public int IdHabitacion { get; set; }

    public string NorHabitacion { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string EstadoHabitacion { get; set; } = null!;

    public decimal PrecioHabitacion { get; set; }

    public int? IdTipoHabitacion { get; set; }

    public virtual TiposHabitacione? IdTipoHabitacionNavigation { get; set; }

    public virtual ICollection<Paquete> Paquetes { get; set; } = new List<Paquete>();
}
