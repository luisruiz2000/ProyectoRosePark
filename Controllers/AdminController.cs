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
        private readonly ILogger<AdminController> _logger;

        // Constructor que recibe el contexto y el logger
        public AdminController(RoseParkDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
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


        public IActionResult CrearServicio()
        {
            return View("~/Views/Admin/Servicios/CrearServicio.cshtml"); // Asegúrate de que esta ruta coincide con la ubicación de la vista
        }



        [HttpPost]
        public IActionResult CrearServicio(Servicio servicio)
        {
            if (ModelState.IsValid)
            {
                _context.Servicios.Add(servicio); // Añadir el nuevo servicio
                _context.SaveChanges();
                return RedirectToAction(nameof(Servicios)); // Redirigir a la lista de servicios
            }
            return View("~/Views/Admin/Servicios/CrearServicio.cshtml", servicio); // Volver a la vista si hay errores
        }






        public IActionResult Paquetes()
        {
            // Cargar los paquetes con sus relaciones
            var paquetes = _context.Paquetes
                .Include(p => p.IdHabitacionNavigation) // Incluye la relación con Habitaciones
                .Include(p => p.PaquetesServicios) // Incluye la relación con PaquetesServicios
                .ThenInclude(ps => ps.IdServicioNavigation) // Incluye los servicios relacionados
                .ToList();

            return View("Paquetes/Paquetes", paquetes); // Devuelve la vista Paquetes.cshtml con la lista de paquetes
        }


        [HttpGet]
        public IActionResult EditarPaquete(int id)
        {
            var paquete = _context.Paquetes
                .Include(p => p.PaquetesServicios)
                .Include(p => p.IdHabitacionNavigation)
                .FirstOrDefault(p => p.IdPaquete == id);

            if (paquete == null)
                return NotFound();

            // Cargar todos los servicios en memoria
            var todosServicios = _context.Servicios.ToList();

            var viewModel = new PaqueteViewModel
            {
                IdPaquete = paquete.IdPaquete,
                NombrePaquete = paquete.NombrePaquete,
                Descripcion = paquete.Descripcion,
                PrecioTotal = paquete.PrecioTotal,
                IdHabitacion = paquete.IdHabitacion.GetValueOrDefault(),
                Estado = paquete.Estado,
                Habitaciones = _context.Habitaciones.Select(h => new SelectListItem
                {
                    Value = h.IdHabitacion.ToString(),
                    Text = h.NorHabitacion,
                    Selected = h.IdHabitacion == paquete.IdHabitacion // Marcar la habitación seleccionada
                }).ToList(),
                Servicios = todosServicios.Select(s => new SelectListItem
                {
                    Value = s.IdServicio.ToString(),
                    Text = s.NombreServicio,
                    Selected = paquete.PaquetesServicios.Any(ps => ps.IdServicio == s.IdServicio) // Marcar servicios seleccionados
                }).ToList(),
                ServiciosSeleccionados = paquete.PaquetesServicios.Select(ps => ps.IdServicio).ToList()
            };

            return View("Paquetes/EditarPaquete", viewModel);
        }








        [HttpPost]
        public IActionResult EditarPaquete(PaqueteViewModel viewModel)
        {
            _logger.LogInformation("Iniciando el método EditarPaquete");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("El modelo no es válido");

                // Registrar los errores de validación
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        _logger.LogError("Error en el campo {Key}: {ErrorMessage}", modelStateKey, error.ErrorMessage);
                    }
                }

                // Cargar las listas para la vista en caso de error
                viewModel.Habitaciones = _context.Habitaciones.Select(h => new SelectListItem
                {
                    Value = h.IdHabitacion.ToString(),
                    Text = h.NorHabitacion
                }).ToList();

                viewModel.Servicios = _context.Servicios.Select(s => new SelectListItem
                {
                    Value = s.IdServicio.ToString(),
                    Text = s.NombreServicio
                }).ToList();

                return View("Paquetes/EditarPaquete", viewModel);
            }

            try
            {
                var paquete = _context.Paquetes
                    .Include(p => p.PaquetesServicios)
                    .FirstOrDefault(p => p.IdPaquete == viewModel.IdPaquete);

                if (paquete == null)
                {
                    _logger.LogWarning("No se encontró el paquete con Id {IdPaquete}", viewModel.IdPaquete);
                    return NotFound();
                }

                // Actualizar el paquete
                paquete.NombrePaquete = viewModel.NombrePaquete;
                paquete.Descripcion = viewModel.Descripcion;
                paquete.PrecioTotal = viewModel.PrecioTotal;
                paquete.IdHabitacion = viewModel.IdHabitacion;
                paquete.Estado = viewModel.Estado;

                // Eliminar servicios existentes asociados con el paquete
                var serviciosExistentes = _context.PaquetesServicios
                    .Where(ps => ps.IdPaquete == viewModel.IdPaquete)
                    .ToList();

                _context.PaquetesServicios.RemoveRange(serviciosExistentes);

                // Verifica si hay servicios seleccionados antes de agregar nuevos
                if (viewModel.ServiciosSeleccionados != null && viewModel.ServiciosSeleccionados.Count > 0)
                {
                    foreach (var idServicio in viewModel.ServiciosSeleccionados)
                    {
                        _context.PaquetesServicios.Add(new PaquetesServicio
                        {
                            IdPaquete = viewModel.IdPaquete,
                            IdServicio = idServicio
                        });
                    }
                }

                // Guardar cambios en la base de datos
                _context.SaveChanges();
                _logger.LogInformation("Paquete guardado correctamente con Id {IdPaquete}", viewModel.IdPaquete);
                return RedirectToAction(nameof(Paquetes));
            }
            catch (Exception ex)
            {
                _logger.LogError("Ocurrió un error al guardar el paquete: {ExceptionMessage}", ex.Message);

                // Cargar listas en caso de excepción
                viewModel.Habitaciones = _context.Habitaciones.Select(h => new SelectListItem
                {
                    Value = h.IdHabitacion.ToString(),
                    Text = h.NorHabitacion
                }).ToList();

                viewModel.Servicios = _context.Servicios.Select(s => new SelectListItem
                {
                    Value = s.IdServicio.ToString(),
                    Text = s.NombreServicio
                }).ToList();

                return View("Paquetes/EditarPaquete", viewModel);
            }
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






        // GET: TipoHabitaciones
        public IActionResult TipoHabitaciones()
        {
            var tiposHabitaciones = _context.TiposHabitaciones.ToList();
            return View("TipoHabitaciones/TipoHabitaciones", tiposHabitaciones); // Ajuste para buscar la vista en la carpeta correcta
        }

        // GET: CrearTipoHabitaciones
        public IActionResult CrearTipoHabitaciones()
        {
            return View("TipoHabitaciones/CrearTipoHabitaciones"); // Ajuste para buscar la vista en la carpeta correcta
        }

        // POST: CrearTipoHabitaciones
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearTipoHabitaciones(TiposHabitacione tipoHabitacion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoHabitacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(TipoHabitaciones));
            }
            return View("TipoHabitaciones/CrearTipoHabitaciones", tipoHabitacion); // Ajuste para buscar la vista en la carpeta correcta
        }

        // GET: EditarTipoHabitaciones/5
        public async Task<IActionResult> EditarTipoHabitaciones(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoHabitacion = await _context.TiposHabitaciones.FindAsync(id);
            if (tipoHabitacion == null)
            {
                return NotFound();
            }
            return View("TipoHabitaciones/EditarTipoHabitaciones", tipoHabitacion); // Ajuste para buscar la vista en la carpeta correcta
        }

        // POST: EditarTipoHabitaciones/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarTipoHabitaciones(int id, TiposHabitacione tipoHabitacion)
        {
            if (id != tipoHabitacion.IdTipoHabitacion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoHabitacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoHabitacionExists(tipoHabitacion.IdTipoHabitacion))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(TipoHabitaciones));
            }
            return View("TipoHabitaciones/EditarTipoHabitaciones", tipoHabitacion); // Ajuste para buscar la vista en la carpeta correcta
        }

        // POST: Eliminar (en background)
        [HttpPost]
        public async Task<IActionResult> EliminarTipoHabitacion(int id)
        {
            var tipoHabitacion = await _context.TiposHabitaciones.FindAsync(id);
            if (tipoHabitacion != null)
            {
                _context.TiposHabitaciones.Remove(tipoHabitacion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(TipoHabitaciones));
        }

        private bool TipoHabitacionExists(int id)
        {
            return _context.TiposHabitaciones.Any(e => e.IdTipoHabitacion == id);
        }












        // GET: Habitaciones
        public IActionResult Habitaciones()
        {
            var habitaciones = _context.Habitaciones.Include(h => h.IdTipoHabitacionNavigation).ToList();
            return View("Habitaciones/Habitaciones", habitaciones);
        }




        // GET: CrearHabitaciones
        public IActionResult CrearHabitaciones()
        {
            ViewBag.TiposHabitacion = new SelectList(_context.TiposHabitaciones, "IdTipoHabitacion", "Nombre");
            return View("Habitaciones/CrearHabitaciones");
        }




        // POST: CrearHabitaciones
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearHabitaciones(Habitacione habitacion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(habitacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Habitaciones));
            }
            ViewBag.TiposHabitacion = new SelectList(_context.TiposHabitaciones, "IdTipoHabitacion", "Nombre");
            return View("Habitaciones/CrearHabitaciones", habitacion);
        }




        // GET: EditarHabitaciones/5
        public async Task<IActionResult> EditarHabitaciones(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var habitacion = await _context.Habitaciones.FindAsync(id);
            if (habitacion == null)
            {
                return NotFound();
            }
            ViewBag.TiposHabitacion = new SelectList(_context.TiposHabitaciones, "IdTipoHabitacion", "Nombre", habitacion.IdTipoHabitacion);
            return View("Habitaciones/EditarHabitaciones", habitacion);
        }




        // POST: EditarHabitaciones/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarHabitaciones(int id, Habitacione habitacion)
        {
            if (id != habitacion.IdHabitacion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(habitacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Habitaciones.Any(h => h.IdHabitacion == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Habitaciones));
            }
            ViewBag.TiposHabitacion = new SelectList(_context.TiposHabitaciones, "IdTipoHabitacion", "Nombre", habitacion.IdTipoHabitacion);
            return View("Habitaciones/EditarHabitaciones", habitacion);
        }



        // POST: EliminarHabitacion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarHabitacion(int id)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);
            if (habitacion == null)
            {
                return NotFound();
            }

            _context.Habitaciones.Remove(habitacion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Habitaciones));
        }







    }
}
