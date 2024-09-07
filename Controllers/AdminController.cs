using Microsoft.AspNetCore.Mvc;
using RosePark.Models;
using System.Linq;
using RosePark.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc.Rendering;


namespace RosePark.Controllers
{
    public class AdminController : Controller
    {
        private readonly RoseParkDbContext _context;

        public AdminController(RoseParkDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            // 1. Ocupación de habitaciones
            var ocupadas = _context.Habitaciones.Count(h => h.EstadoHabitacion == "Ocupada");
            var disponibles = _context.Habitaciones.Count(h => h.EstadoHabitacion == "Disponible");
            var totalHabitaciones = ocupadas + disponibles;
            var porcentajeOcupacion = (ocupadas / (double)totalHabitaciones) * 100;

            // 2. Reservas por mes
            var reservasPorMes = _context.Reservas
                .GroupBy(r => new { r.FechaReserva.Year, r.FechaReserva.Month })
                .Select(g => new
                {
                    Mes = g.Key.Month,
                    Anio = g.Key.Year,
                    CantidadReservas = g.Count()
                }).ToList();

            // 3. Paquetes populares
            var paquetesPopulares = _context.Reservas
                .Where(r => r.IdPaquete != null)
                .GroupBy(r => r.IdPaqueteNavigation.NombrePaquete)
                .Select(g => new
                {
                    Paquete = g.Key,
                    CantidadReservas = g.Count()
                }).OrderByDescending(p => p.CantidadReservas).ToList();

            // 4. Ingresos generados (solo abonos)
            var ingresosTotales = _context.Reservas.Sum(r => r.Abono ?? 0);

            // Pasamos los datos a la vista usando ViewData o ViewBag
            ViewBag.Ocupacion = porcentajeOcupacion;
            ViewBag.ReservasPorMes = reservasPorMes.Select(r => new ReservasPorMesViewModel
            {
                Mes = r.Mes,
                CantidadReservas = r.CantidadReservas
            }).ToList();
            ViewBag.PaquetesPopulares = paquetesPopulares.Select(p => new PaqueteViewModel
            {
                Paquete = p.Paquete,
                CantidadReservas = p.CantidadReservas
            }).ToList();
            ViewBag.IngresosTotales = ingresosTotales;

            return View("Dashboard"); // Mantén la vista en la raíz de Admin
        }








        public IActionResult Usuarios()
        {
            var usuarios = _context.Usuarios
                .Include(u => u.IdPersonasNavigation) // Incluye los datos de Persona
                .Include(u => u.IdRolNavigation) // Incluye los datos del Rol
                .Select(u => new UsuarioViewModel
                {
                    IdUsuario = u.IdUsuario,
                    CorreoUsuario = u.CorreoUsuario,
                    NombrePersona = u.IdPersonasNavigation.Nombres,
                    ApellidosPersona = u.IdPersonasNavigation.Apellidos,
                    TipoDocumentoPersona = u.IdPersonasNavigation.TipoDocumento,
                    NroDocumentoPersona = u.IdPersonasNavigation.NroDocumento,
                    EdadPersona = u.IdPersonasNavigation.Edad,
                    CelularPersona = u.IdPersonasNavigation.Celular,
                    FechaNacimientoPersona = u.IdPersonasNavigation.FechaNacimiento,
                    NombreRol = u.IdRolNavigation.Nombre
                }).ToList();

            return View("Usuarios/Usuarios", usuarios); // Especifica la vista dentro de la carpeta Usuarios
        }


        public IActionResult EditarUsuario(int id)
        {
            try
            {
                var usuario = _context.Usuarios
                    .Include(u => u.IdPersonasNavigation)
                    .Include(u => u.IdRolNavigation)
                    .Where(u => u.IdUsuario == id)
                    .Select(u => new UsuarioViewModel
                    {
                        IdUsuario = u.IdUsuario,
                        CorreoUsuario = u.CorreoUsuario,
                        ClaveUsuario = u.ClaveUsuario,
                        IdPersonas = u.IdPersonas,
                        NombrePersona = u.IdPersonasNavigation.Nombres,
                        ApellidosPersona = u.IdPersonasNavigation.Apellidos,
                        TipoDocumentoPersona = u.IdPersonasNavigation.TipoDocumento,
                        NroDocumentoPersona = u.IdPersonasNavigation.NroDocumento,
                        EdadPersona = u.IdPersonasNavigation.Edad,
                        CelularPersona = u.IdPersonasNavigation.Celular,
                        FechaNacimientoPersona = u.IdPersonasNavigation.FechaNacimiento,
                        IdRol = u.IdRol,
                        NombreRol = u.IdRolNavigation.Nombre
                    }).FirstOrDefault();

                if (usuario == null)
                    return NotFound();

                // Cargar datos para los select
                ViewBag.Personas = new SelectList(_context.Personas, "IdPersonas", "NombreCompleto", usuario.IdPersonas);
                ViewBag.Roles = new SelectList(_context.Roles, "IdRol", "Nombre", usuario.IdRol);

                return View("~/Views/Admin/Usuarios/EditarUsuario.cshtml", usuario);
            }
            catch (Exception ex)
            {
                // Log the exception and return a generic error message
                Console.WriteLine(ex.Message); // Log the exception message
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPost]
        public async Task<IActionResult> EditarUsuario(UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Mostrar los errores de validación en la vista
                return View("~/Views/Admin/Usuarios/EditarUsuario.cshtml", model);
            }

            try
            {
                // Buscar el usuario en la base de datos
                var usuario = await _context.Usuarios
                    .Include(u => u.IdPersonasNavigation)
                    .FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario);

                if (usuario == null)
                {
                    return NotFound();
                }

                // Actualizar los datos del usuario
                usuario.CorreoUsuario = model.CorreoUsuario;

                // Si estás manejando la contraseña, asegúrate de hashearla
                if (!string.IsNullOrEmpty(model.ClaveUsuario))
                {
                    var passwordHasher = new PasswordHasher<Usuario>();
                    usuario.ClaveUsuario = passwordHasher.HashPassword(usuario, model.ClaveUsuario);
                }

                // Actualizar los datos de la persona asociada
                if (usuario.IdPersonasNavigation != null)
                {
                    usuario.IdPersonasNavigation.Nombres = model.NombrePersona;
                    usuario.IdPersonasNavigation.Apellidos = model.ApellidosPersona;
                    usuario.IdPersonasNavigation.TipoDocumento = model.TipoDocumentoPersona;
                    usuario.IdPersonasNavigation.NroDocumento = model.NroDocumentoPersona;
                    usuario.IdPersonasNavigation.Edad = model.EdadPersona;
                    usuario.IdPersonasNavigation.Celular = model.CelularPersona;
                    usuario.IdPersonasNavigation.FechaNacimiento = model.FechaNacimientoPersona;
                }

                // Guardar los cambios
                _context.Update(usuario);
                await _context.SaveChangesAsync();

                // Redirigir a la lista de usuarios
                return RedirectToAction("Usuarios");
            }
            catch (Exception ex)
            {
                // Manejar o registrar la excepción
                Console.WriteLine($"Error al guardar los cambios: {ex.Message}");
                ModelState.AddModelError("", "No se pudo guardar el usuario. Inténtalo de nuevo.");
                return View("~/Views/Admin/Usuarios/EditarUsuario.cshtml", model);
            }
        }










        public IActionResult EliminarUsuario(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Usuarios));
        }







        public IActionResult Paquetes()
        {
            var paquetes = _context.Paquetes.ToList();
            return View("Paquetes/Paquetes", paquetes); // Especifica la vista dentro de la carpeta Paquetes
        }

        public IActionResult EditarPaquete(int id)
        {
            var paquete = _context.Paquetes.Find(id);
            if (paquete == null)
                return NotFound();

            return View("Paquetes/EditarPaquete", paquete); // Especifica la vista dentro de la carpeta Paquetes
        }

        [HttpPost]
        public IActionResult EditarPaquete(Paquete paquete)
        {
            if (ModelState.IsValid)
            {
                _context.Paquetes.Update(paquete);
                _context.SaveChanges();
                return RedirectToAction(nameof(Paquetes));
            }
            return View("Paquetes/EditarPaquete", paquete); // Especifica la vista dentro de la carpeta Paquetes
        }

        public IActionResult EliminarPaquete(int id)
        {
            var paquete = _context.Paquetes.Find(id);
            if (paquete != null)
            {
                _context.Paquetes.Remove(paquete);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Paquetes));
        }

        public IActionResult Servicios()
        {
            var servicios = _context.Servicios.ToList();
            return View("Servicios/Servicios", servicios); // Especifica la vista dentro de la carpeta Servicios
        }

        public IActionResult EditarServicio(int id)
        {
            var servicio = _context.Servicios.Find(id);
            if (servicio == null)
                return NotFound();

            return View("~/Views/Admin/Servicios/EditarServicio.cshtml", servicio); // Ruta completa de la vista
        }

        [HttpPost]
        public IActionResult EditarServicio(Servicio servicio)
        {
            if (ModelState.IsValid)
            {
                var existingServicio = _context.Servicios.Find(servicio.IdServicio);
                if (existingServicio != null)
                {
                    existingServicio.NombreServicio = servicio.NombreServicio;
                    existingServicio.DescripcionServicio = servicio.DescripcionServicio;
                    existingServicio.PrecioServicio = servicio.PrecioServicio;
                    existingServicio.EstadoServicio = servicio.EstadoServicio;

                    _context.Servicios.Update(existingServicio);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Servicios));
                }
            }
            return View("~/Views/Admin/Servicios/EditarServicio.cshtml", servicio); // Ruta completa de la vista
        }








        public IActionResult EliminarServicio(int id)
        {
            var servicio = _context.Servicios.Find(id);
            if (servicio != null)
            {
                _context.Servicios.Remove(servicio);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Servicios));
        }
    }
}
