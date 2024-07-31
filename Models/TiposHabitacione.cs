using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class TiposHabitacione
{
    public int IdTipoHabitacion { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Habitacione> Habitaciones { get; set; } = new List<Habitacione>();
}
