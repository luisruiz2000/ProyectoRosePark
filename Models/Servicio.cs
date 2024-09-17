using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class Servicio
{
    public enum EstadoServicioEnum
    {
        Inactivo,
        Activo
    }

    public int IdServicio { get; set; }
    public string NombreServicio { get; set; } = null!;
    public decimal PrecioServicio { get; set; }
    public EstadoServicioEnum EstadoServicio { get; set; }
    public string? UrlIcono { get; set; } // Añadida propiedad para la URL del ícono

    public virtual ICollection<PaquetesServicio> PaquetesServicios { get; set; } = new List<PaquetesServicio>();
    public virtual ICollection<ReservasServicio> ReservasServicios { get; set; } = new List<ReservasServicio>();
}

