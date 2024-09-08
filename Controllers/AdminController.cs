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
                .Include(u => u.IdPersonasNavigation)
                .Include(u => u.IdRolNavigation)
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
                    FechaNacimientoPersona = u.IdPersonasNavigation.FechaNacimiento, // Asignar DateOnly directamente
                    NombreRol = u.IdRolNavigation.Nombre
                })
                .ToList();

            return View("Usuarios/Usuarios", usuarios);
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
                        FechaNacimientoPersona = u.IdPersonasNavigation.FechaNacimiento, // Convertir DateOnly a DateTime
                        IdRol = u.IdRol ?? 0, // Manejar valor nullable
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
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarUsuario(UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Registrar los errores del modelo para debug
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }

                // Cargar las listas nuevamente si la validación falla
                ViewBag.Roles = new SelectList(_context.Roles, "IdRol", "Nombre", model.IdRol);
                return View("~/Views/Admin/Usuarios/EditarUsuario.cshtml", model);
            }

            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.IdPersonasNavigation)
                    .FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario);

                if (usuario == null)
                {
                    return NotFound();
                }

                // Actualizar la información del usuario
                usuario.CorreoUsuario = model.CorreoUsuario;

                // Manejo de contraseña si se ha especificado una
                if (!string.IsNullOrEmpty(model.ClaveUsuario))
                {
                    usuario.ClaveUsuario = model.ClaveUsuario; // No hashear la contraseña
                }

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

                // Actualizar el rol
                usuario.IdRol = model.IdRol;

                _context.Entry(usuario).State = EntityState.Modified;
                _context.Entry(usuario.IdPersonasNavigation).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return RedirectToAction("Usuarios");
            }
            catch (Exception ex)
            {
                // Mostrar el error en consola y enviar mensaje a la vista
                Console.WriteLine($"Error al guardar el usuario: {ex.Message}");
                ModelState.AddModelError("", "No se pudo guardar el usuario. Inténtalo de nuevo.");
                ViewBag.Roles = new SelectList(_context.Roles, "IdRol", "Nombre", model.IdRol);
                return View("~/Views/Admin/Usuarios/EditarUsuario.cshtml", model);
            }
        }



        public IActionResult EliminarUsuario(int id)
        {
            var usuario = _context.Usuarios
                .Include(u => u.IdPersonasNavigation)  // Incluir la relación con Personas
                .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario != null)
            {
                // Primero eliminar la persona asociada
                if (usuario.IdPersonasNavigation != null)
                {
                    _context.Personas.Remove(usuario.IdPersonasNavigation);
                }

                // Luego eliminar el usuario
                _context.Usuarios.Remove(usuario);

                // Guardar cambios
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Usuarios));
        }








        public IActionResult CrearUsuario()
        {
            // Cargar datos para los select
            ViewBag.Roles = new SelectList(_context.Roles, "IdRol", "Nombre");

            return View("~/Views/Admin/Usuarios/CrearUsuario.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Registrar los errores del modelo para debug
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }

                // Cargar las listas nuevamente si la validación falla
                ViewBag.Roles = new SelectList(_context.Roles, "IdRol", "Nombre", model.IdRol);
                return View("~/Views/Admin/Usuarios/CrearUsuario.cshtml", model);
            }

            try
            {
                var usuario = new Usuario
                {
                    CorreoUsuario = model.CorreoUsuario,
                    ClaveUsuario = model.ClaveUsuario,
                    IdRol = model.IdRol,
                    IdPersonasNavigation = new Persona
                    {
                        Nombres = model.NombrePersona,
                        Apellidos = model.ApellidosPersona,
                        TipoDocumento = model.TipoDocumentoPersona,
                        NroDocumento = model.NroDocumentoPersona,
                        Edad = model.EdadPersona,
                        Celular = model.CelularPersona,
                        FechaNacimiento = model.FechaNacimientoPersona
                    }
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                return RedirectToAction("Usuarios");
            }
            catch (Exception ex)
            {
                // Mostrar el error en consola y enviar mensaje a la vista
                Console.WriteLine($"Error al guardar el usuario: {ex.Message}");
                ModelState.AddModelError("", "No se pudo guardar el usuario. Inténtalo de nuevo.");
                ViewBag.Roles = new SelectList(_context.Roles, "IdRol", "Nombre", model.IdRol);
                return View("~/Views/Admin/Usuarios/CrearUsuario.cshtml", model);
            }
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
