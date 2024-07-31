using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class RolesPermiso
{
    public int IdRolesPermisos { get; set; }

    public string Estado { get; set; } = null!;

    public int? IdRol { get; set; }

    public int? IdPermisos { get; set; }

    public virtual Permiso? IdPermisosNavigation { get; set; }

    public virtual Role? IdRolNavigation { get; set; }
}
