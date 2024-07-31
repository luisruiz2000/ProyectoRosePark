using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class Persona
{
    public int IdPersonas { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string TipoDocumento { get; set; } = null!;

    public string NroDocumento { get; set; } = null!;

    public int Edad { get; set; }

    public string Celular { get; set; } = null!;

    public DateOnly FrechaNacimiento { get; set; }

    public int? IdRol { get; set; }

    public virtual Role? IdRolNavigation { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
