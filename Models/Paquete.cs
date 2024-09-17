using System;
using System.Collections.Generic;

namespace RosePark.Models;

public partial class Paquete
{
    public enum EstadoEnum
    {
        Inactivo = 0,
        Activo = 1
    }

    public int IdPaquete { get; set; }

    public string NombrePaquete { get; set; } = null!;

    public string Descripcion { get; set; } = null!;


    public EstadoEnum Estado { get; set; }

    public int? IdHabitacion { get; set; }

    public virtual Habitacione? IdHabitacionNavigation { get; set; }

    public virtual ICollection<PaquetesServicio> PaquetesServicios { get; set; } = new List<PaquetesServicio>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
