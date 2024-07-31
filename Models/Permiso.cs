using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class Permiso
{
    public int IdPermisos { get; set; }

    public string NombrePermiso { get; set; } = null!;

    public string? EstadoPermisos { get; set; }

    public virtual ICollection<RolesPermiso> RolesPermisos { get; set; } = new List<RolesPermiso>();
}
