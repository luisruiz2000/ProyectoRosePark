using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RosePark.Models;

public partial class RoseParkDbContext : DbContext
{
    public RoseParkDbContext()
    {
    }

    public RoseParkDbContext(DbContextOptions<RoseParkDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Habitacione> Habitaciones { get; set; }

    public virtual DbSet<Paquete> Paquetes { get; set; }

    public virtual DbSet<PaquetesServicio> PaquetesServicios { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<ReservasServicio> ReservasServicios { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolesPermiso> RolesPermisos { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<TiposHabitacione> TiposHabitaciones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=mrxxo; Initial Catalog=RoseParkDb;User ID=sa; Password=Morocho1006372989.; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habitacione>(entity =>
        {
            entity.HasKey(e => e.IdHabitacion).HasName("PK__Habitaci__8BBBF90135355A80");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.EstadoHabitacion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NorHabitacion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PrecioHabitacion).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdTipoHabitacionNavigation).WithMany(p => p.Habitaciones)
                .HasForeignKey(d => d.IdTipoHabitacion)
                .HasConstraintName("FK__Habitacio__IdTip__40C49C62");
        });

        modelBuilder.Entity<Paquete>(entity =>
        {
            entity.HasKey(e => e.IdPaquete).HasName("PK__Paquetes__DE278F8B3C633C0E");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NombrePaquete)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PrecioTotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.Paquetes)
                .HasForeignKey(d => d.IdHabitacion)
                .HasConstraintName("FK__Paquetes__IdHabi__43A1090D");
        });

        modelBuilder.Entity<PaquetesServicio>(entity =>
        {
            entity.HasKey(e => e.IdPaquetesServicios).HasName("PK__Paquetes__12550DDE56B46214");

            entity.HasOne(d => d.IdPaqueteNavigation).WithMany(p => p.PaquetesServicios)
                .HasForeignKey(d => d.IdPaquete)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PaquetesS__IdPaq__5B78929E");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.PaquetesServicios)
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PaquetesS__IdSer__5A846E65");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.IdPermisos).HasName("PK__Permisos__CE7ED38D33939B5A");

            entity.Property(e => e.EstadoPermisos)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NombrePermiso)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.HasKey(e => e.IdPersonas).HasName("PK__Personas__05A9201249DCEBE4");

            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Nombres)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NroDocumento)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Personas)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__Personas__IdRol__36470DEF");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__Reservas__0E49C69DC063F3AE");

            entity.Property(e => e.Abono).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EstadoReserva)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.FechaReserva).HasColumnType("datetime");
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdPaqueteNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdPaquete)
                .HasConstraintName("FK__Reservas__IdPaqu__4A4E069C");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Reservas__IdUsua__4959E263");
        });

        modelBuilder.Entity<ReservasServicio>(entity =>
        {
            entity.HasKey(e => e.IdReservasServicios).HasName("PK__Reservas__CFBB9AECBBCE5D9C");

            entity.HasOne(d => d.IdReservaNavigation).WithMany(p => p.ReservasServicios)
                .HasForeignKey(d => d.IdReserva)
                .HasConstraintName("FK__ReservasS__IdRes__68D28DBC");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.ReservasServicios)
                .HasForeignKey(d => d.IdServicio)
                .HasConstraintName("FK__ReservasS__IdSer__67DE6983");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__2A49584C97EB5004");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RolesPermiso>(entity =>
        {
            entity.HasKey(e => e.IdRolesPermisos).HasName("PK__RolesPer__AAB7C825BDE4691D");

            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdPermisosNavigation).WithMany(p => p.RolesPermisos)
                .HasForeignKey(d => d.IdPermisos)
                .HasConstraintName("FK__RolesPerm__IdPer__336AA144");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.RolesPermisos)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__RolesPerm__IdRol__32767D0B");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.IdServicio).HasName("PK__Servicio__2DCCF9A2D071E765");

            entity.Property(e => e.DescripcionServicio)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.EstadoServicio)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NombreServicio)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PrecioServicio).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<TiposHabitacione>(entity =>
        {
            entity.HasKey(e => e.IdTipoHabitacion).HasName("PK__TipoHabi__AB75E87CE4CA9061");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__EAEBAC8F26C4982E");

            entity.Property(e => e.ClaveUsuario).HasMaxLength(50);
            entity.Property(e => e.CorreoUsuario).HasMaxLength(50);

            entity.HasOne(d => d.IdPersonasNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdPersonas)
                .HasConstraintName("FK__Usuarios__IdPers__39237A9A");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK_Usuarios_IdRol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
