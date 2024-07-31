using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class PaquetesServicio
{
    public int IdPaquetesServicios { get; set; }

    public int IdServicio { get; set; }

    public int IdPaquete { get; set; }

    public virtual Paquete IdPaqueteNavigation { get; set; } = null!;

    public virtual Servicio IdServicioNavigation { get; set; } = null!;
}
