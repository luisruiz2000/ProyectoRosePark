using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RosePark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace RosePark.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RoseParkDbContext _context;

        public HomeController(ILogger<HomeController> logger, RoseParkDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var paquetes = _context.Paquetes.ToList();
            var servicios = _context.Servicios.ToList();
            var habitaciones = _context.Habitaciones.ToList();

            ViewData["Paquetes"] = paquetes;
            ViewData["Servicios"] = servicios;
            ViewData["Habitaciones"] = habitaciones;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(DateTime checkinDate, DateTime checkoutDate, int numeroPersonas)
        {
            if (checkinDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue.Value || checkoutDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue.Value)
            {
                ModelState.AddModelError("", "Las fechas seleccionadas son inválidas.");
                return View("Search", new List<Paquete>());
            }

            HttpContext.Session.SetString("CheckinDate", checkinDate.ToString("yyyy-MM-dd"));
            HttpContext.Session.SetString("CheckoutDate", checkoutDate.ToString("yyyy-MM-dd"));
            HttpContext.Session.SetInt32("NumeroPersonas", numeroPersonas);

            ViewData["CheckinDate"] = checkinDate.ToString("yyyy-MM-dd");
            ViewData["CheckoutDate"] = checkoutDate.ToString("yyyy-MM-dd");
            ViewData["NumeroPersonas"] = numeroPersonas;

            var availablePackages = await _context.Paquetes
                .Include(p => p.IdHabitacionNavigation)
                .Include(p => p.PaquetesServicios)
                .ThenInclude(ps => ps.IdServicioNavigation)
                .Where(p => !_context.Reservas.Any(r =>
                        r.IdPaquete == p.IdPaquete &&
                        (
                            (r.FechaInicio <= checkinDate && r.FechaFin >= checkinDate) ||
                            (r.FechaInicio <= checkoutDate && r.FechaFin >= checkoutDate) ||
                            (r.FechaInicio >= checkinDate && r.FechaFin <= checkoutDate)
                        )
                    )
                )
                .ToListAsync();

            return View("Search", availablePackages);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ResumenReserva(int id)
        {
            var paquete = _context.Paquetes
                                  .Include(p => p.IdHabitacionNavigation)
                                  .Include(p => p.PaquetesServicios)
                                  .ThenInclude(ps => ps.IdServicioNavigation)
                                  .FirstOrDefault(p => p.IdPaquete == id);

            if (paquete == null)
            {
                return NotFound();
            }

            var checkinDate = HttpContext.Session.GetString("CheckinDate");
            var checkoutDate = HttpContext.Session.GetString("CheckoutDate");
            var numeroPersonas = HttpContext.Session.GetInt32("NumeroPersonas") ?? 1;

            var modeloResumen = new ResumenReservaViewModel
            {
                IdPaquete = paquete.IdPaquete,
                NombrePaquete = paquete.NombrePaquete,
                Descripcion = paquete.Descripcion,
                NorHabitacion = paquete.IdHabitacionNavigation?.NorHabitacion,
                PrecioTotal = paquete.PrecioTotal,
                FechaInicio = DateTime.Parse(checkinDate),
                FechaFin = DateTime.Parse(checkoutDate),
                NumeroPersonas = numeroPersonas,
                ServiciosAdicionales = paquete.PaquetesServicios
                                              .Select(ps => ps.IdServicioNavigation)
                                              .ToList(),
                ServiciosDisponibles = _context.Servicios.ToList()
            };

            return View(modeloResumen);
        }

        [HttpPost]
        public IActionResult ConfirmarReserva(int IdPaquete, string[] servicioAdicional, string metodoPago)
        {
            var checkinDate = HttpContext.Session.GetString("CheckinDate");
            var checkoutDate = HttpContext.Session.GetString("CheckoutDate");
            var numeroPersonas = HttpContext.Session.GetInt32("NumeroPersonas") ?? 1;

            var paquete = _context.Paquetes
                .Include(p => p.PaquetesServicios)
                .ThenInclude(ps => ps.IdServicioNavigation)
                .FirstOrDefault(p => p.IdPaquete == IdPaquete);

            if (paquete == null)
            {
                return NotFound();
            }

            decimal precioTotal = paquete.PrecioTotal;
            var serviciosAdicionales = new List<Servicio>();

            foreach (var servicioId in servicioAdicional)
            {
                var servicio = _context.Servicios.Find(int.Parse(servicioId));
                if (servicio != null)
                {
                    precioTotal += servicio.PrecioServicio;
                    serviciosAdicionales.Add(servicio);
                }
            }

            var nuevaReserva = new Reserva
            {
                IdPaquete = IdPaquete,
                FechaReserva = DateTime.Now,
                FechaInicio = DateTime.Parse(checkinDate),
                FechaFin = DateTime.Parse(checkoutDate),
                NroPersonas = numeroPersonas,
                MontoTotal = precioTotal,
                Abono = 0,
                EstadoReserva = "Pendiente"
            };

            _context.Reservas.Add(nuevaReserva);
            _context.SaveChanges();

            foreach (var servicio in serviciosAdicionales)
            {
                var reservaServicio = new ReservasServicio
                {
                    IdReserva = nuevaReserva.IdReserva,
                    IdServicio = servicio.IdServicio
                };
                _context.ReservasServicios.Add(reservaServicio);
            }

            _context.SaveChanges();

            return RedirectToAction("ConfirmacionReserva", new { id = nuevaReserva.IdReserva });
        }

        public IActionResult ConfirmacionReserva(int id)
        {
            var reserva = _context.Reservas
                .Include(r => r.IdPaqueteNavigation)
                .ThenInclude(p => p.IdHabitacionNavigation)
                .Include(r => r.ReservasServicios)
                .ThenInclude(rs => rs.IdServicioNavigation)
                .FirstOrDefault(r => r.IdReserva == id);

            if (reserva == null)
            {
                return NotFound();
            }

            var serviciosAdicionales = reserva.ReservasServicios
                .Select(rs => rs.IdServicioNavigation)
                .ToList();

            decimal precioTotal = reserva.IdPaqueteNavigation.PrecioTotal;

            foreach (var servicio in serviciosAdicionales)
            {
                precioTotal += servicio.PrecioServicio;
            }

            var modeloConfirmacion = new ConfirmacionReservaViewModel
            {
                IdReserva = reserva.IdReserva,
                Paquete = reserva.IdPaqueteNavigation,
                ServiciosAdicionales = serviciosAdicionales,
                PrecioTotal = precioTotal,
                FechaInicio = reserva.FechaInicio,
                FechaFin = reserva.FechaFin
            };

            return View(modeloConfirmacion);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View("~/Views/Account/Login.cshtml", new LoginViewModel());
        }

[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View("~/Views/Account/Login.cshtml", model);
    }

    var usuario = _context.Usuarios
        .FirstOrDefault(u => u.CorreoUsuario == model.CorreoUsuario && u.ClaveUsuario == model.ClaveUsuario);

    if (usuario != null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario.CorreoUsuario),
            new Claim("IdUsuario", usuario.IdUsuario.ToString()),
            new Claim("IdRol", usuario.IdRol.ToString()) // Añadir el rol en los claims
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        // Redirigir basado en el rol del usuario
        if (usuario.IdRol == 1)
        {
            return RedirectToAction("Index", "Home"); // Redirigir a la vista para clientes
        }
        else
        {
            return RedirectToAction("Dashboard", "Admin"); // Redirigir al dashboard administrativo
        }
    }

    // Si el usuario no es válido, agrega un error al modelo de vista
    ModelState.AddModelError("", "Correo electrónico o contraseña incorrectos.");
    return View("~/Views/Account/Login.cshtml", model);
}


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View("~/Views/Account/Register.cshtml", new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/Register.cshtml", model);
            }

            var existingUser = _context.Usuarios.FirstOrDefault(u => u.CorreoUsuario == model.CorreoUsuario);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "El correo electrónico ya está registrado.");
                return View("~/Views/Account/Register.cshtml", model);
            }

            var persona = new Persona
            {
                Nombres = model.Nombres,
                Apellidos = model.Apellidos,
                TipoDocumento = model.TipoDocumento,
                NroDocumento = model.NroDocumento,
                Edad = model.Edad,
                Celular = model.Celular,
                FechaNacimiento = model.FechaNacimiento
            };

            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();

            var usuario = new Usuario
            {
                CorreoUsuario = model.CorreoUsuario,
                ClaveUsuario = model.ClaveUsuario,
                IdPersonas = persona.IdPersonas,
                IdRol = model.IdRol
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.CorreoUsuario),
                new Claim("IdUsuario", usuario.IdUsuario.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return RedirectToAction("Index", "Home");
        }
    }
}
