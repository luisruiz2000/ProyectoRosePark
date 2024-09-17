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
using Newtonsoft.Json;

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




        public IActionResult Servicios()
        {
            // Obtener los servicios que no son gratuitos
            var serviciosNoGratuitos = _context.Servicios
                .Where(s => s.PrecioServicio > 0)
                .ToList();

            return View(serviciosNoGratuitos);
        }





        public IActionResult Paquetes()
        {
            // Cargar los paquetes con sus relaciones
            var paquetes = _context.Paquetes
                .Include(p => p.IdHabitacionNavigation) // Incluye la relación con Habitaciones
                .Include(p => p.PaquetesServicios) // Incluye la relación con PaquetesServicios
                .ThenInclude(ps => ps.IdServicioNavigation) // Incluye los servicios relacionados
                .ToList();

            return View("Paquetes", paquetes); // Devuelve la vista Paquetes.cshtml con la lista de paquetes
        }




        [HttpPost]
        public async Task<IActionResult> Search(DateTime checkinDate, DateTime checkoutDate, int numeroPersonas)
        {
            // Validar que la fecha de check-in sea menor que la de check-out
            if (checkinDate >= checkoutDate)
            {
                ModelState.AddModelError("", "La fecha de check-out debe ser posterior a la de check-in.");
                return View("Search", new List<Paquete>());
            }

            // Validar que las fechas sean válidas según el rango permitido por SQL
            if (checkinDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue.Value ||
                checkoutDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue.Value)
            {
                ModelState.AddModelError("", "Las fechas seleccionadas son inválidas.");
                return View("Search", new List<Paquete>());
            }

            // Guardar las fechas y número de personas en la sesión
            HttpContext.Session.SetString("CheckinDate", checkinDate.ToString("yyyy-MM-dd"));
            HttpContext.Session.SetString("CheckoutDate", checkoutDate.ToString("yyyy-MM-dd"));
            HttpContext.Session.SetInt32("NumeroPersonas", numeroPersonas);

            // Pasar las fechas a la vista
            ViewData["CheckinDate"] = checkinDate.ToString("yyyy-MM-dd");
            ViewData["CheckoutDate"] = checkoutDate.ToString("yyyy-MM-dd");
            ViewData["NumeroPersonas"] = numeroPersonas;

            // Consultar los paquetes disponibles que no están reservados en el rango de fechas
            var availablePackages = await _context.Paquetes
                .Include(p => p.IdHabitacionNavigation)
                .Include(p => p.PaquetesServicios)
                .ThenInclude(ps => ps.IdServicioNavigation)
                .Where(p => !_context.Reservas.Any(r =>
                    r.IdPaquete == p.IdPaquete &&
                    (
                        (r.FechaInicio < checkoutDate && r.FechaFin > checkinDate) // Verificar si el rango de fechas se solapa
                    )
                ))
                .ToListAsync();

            // Aplicar incremento del 10% en el precio de los paquetes
            foreach (var paquete in availablePackages)
            {
                if (paquete.IdHabitacionNavigation != null)
                {
                    paquete.IdHabitacionNavigation.PrecioHabitacion *= 1.10m; // Incremento del 10%
                }

                foreach (var paquetesServicio in paquete.PaquetesServicios)
                {
                    var servicio = paquetesServicio.IdServicioNavigation;
                    if (servicio != null && servicio.PrecioServicio > 0)
                    {
                        servicio.PrecioServicio *= 1.10m; // Incremento del 10%
                    }
                }
            }

            return View("Search", availablePackages);
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }





        public IActionResult ResumenReserva(int id)
        {
            // Obtener el paquete y las relaciones necesarias (Habitación y Servicios)
            var paquete = _context.Paquetes
                                  .Include(p => p.IdHabitacionNavigation)
                                  .Include(p => p.PaquetesServicios)
                                  .ThenInclude(ps => ps.IdServicioNavigation)
                                  .FirstOrDefault(p => p.IdPaquete == id);

            // Verificar si el paquete existe
            if (paquete == null)
            {
                return NotFound();
            }

            // Obtener fechas y número de personas desde la sesión
            var checkinDate = HttpContext.Session.GetString("CheckinDate");
            var checkoutDate = HttpContext.Session.GetString("CheckoutDate");
            var numeroPersonas = HttpContext.Session.GetInt32("NumeroPersonas") ?? 1;

            // Calcular el precio total dinámicamente
            decimal precioHabitacion = paquete.IdHabitacionNavigation.PrecioHabitacion;

            // Sumar los precios de los servicios asociados
            decimal precioServicios = paquete.PaquetesServicios
                                             .Sum(ps => ps.IdServicioNavigation.PrecioServicio);

            // Aplicar un porcentaje adicional si lo deseas (ejemplo: 10% adicional)
            decimal porcentajeAdicional = 0.10m; // 10% de recargo
            decimal precioTotal = (precioHabitacion + precioServicios) * (1 + porcentajeAdicional);

            // Crear el modelo de resumen de reserva
            var modeloResumen = new ResumenReservaViewModel
            {
                IdPaquete = paquete.IdPaquete,
                NombrePaquete = paquete.NombrePaquete,
                Descripcion = paquete.Descripcion,
                NorHabitacion = paquete.IdHabitacionNavigation?.NorHabitacion,
                PrecioTotal = precioTotal, // Ahora calculado dinámicamente
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
            // Verificar si el usuario está autenticado
            if (!User.Identity.IsAuthenticated)
            {
                // Almacenar los datos de la reserva en TempData
                TempData["IdPaquete"] = IdPaquete;
                TempData["ServicioAdicional"] = JsonConvert.SerializeObject(servicioAdicional);
                TempData["MetodoPago"] = metodoPago;

                // Redirigir a la página de inicio de sesión
                return RedirectToAction("Login", "Account");
            }

            // Continuar con la lógica de confirmación de la reserva...
            var checkinDate = HttpContext.Session.GetString("CheckinDate");
            var checkoutDate = HttpContext.Session.GetString("CheckoutDate");
            var numeroPersonas = HttpContext.Session.GetInt32("NumeroPersonas") ?? 1;

            // Obtener el paquete, la habitación asociada y los servicios del paquete
            var paquete = _context.Paquetes
                .Include(p => p.PaquetesServicios)
                .ThenInclude(ps => ps.IdServicioNavigation)
                .Include(p => p.IdHabitacionNavigation) // Incluir la habitación para obtener el precio
                .FirstOrDefault(p => p.IdPaquete == IdPaquete);

            if (paquete == null)
            {
                return NotFound();
            }

            // Calcular el precio total dinámicamente
            decimal precioTotal = paquete.IdHabitacionNavigation.PrecioHabitacion; // Precio de la habitación

            var serviciosAdicionales = new List<Servicio>();

            // Sumar los precios de los servicios adicionales seleccionados
            foreach (var servicioId in servicioAdicional)
            {
                var servicio = _context.Servicios.Find(int.Parse(servicioId));
                if (servicio != null)
                {
                    precioTotal += servicio.PrecioServicio; // Sumamos el costo del servicio adicional
                    serviciosAdicionales.Add(servicio);
                }
            }

            // Obtener el ID del usuario autenticado
            var userIdClaim = User.FindFirst("IdUsuario");
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int idUsuario = int.Parse(userIdClaim.Value);

            // Crear la nueva reserva
            var nuevaReserva = new Reserva
            {
                IdPaquete = IdPaquete,
                FechaReserva = DateTime.Now,
                FechaInicio = DateTime.Parse(checkinDate),
                FechaFin = DateTime.Parse(checkoutDate),
                NroPersonas = numeroPersonas,
                MontoTotal = precioTotal, // Usar el precio total calculado
                Abono = 0,
                EstadoReserva = "Pendiente",
                IdUsuario = idUsuario
            };

            _context.Reservas.Add(nuevaReserva);
            _context.SaveChanges();

            // Asignar los servicios adicionales a la reserva
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
                .ThenInclude(p => p.IdHabitacionNavigation) // Incluir la habitación para obtener el precio
                .Include(r => r.ReservasServicios)
                .ThenInclude(rs => rs.IdServicioNavigation)
                .FirstOrDefault(r => r.IdReserva == id);

            if (reserva == null)
            {
                return NotFound();
            }

            // Obtener los servicios adicionales seleccionados
            var serviciosAdicionales = reserva.ReservasServicios
                .Select(rs => rs.IdServicioNavigation)
                .ToList();

            // Calcular el precio total
            decimal precioTotal = reserva.IdPaqueteNavigation.IdHabitacionNavigation.PrecioHabitacion; // Precio base de la habitación

            // Sumar los precios de los servicios adicionales
            foreach (var servicio in serviciosAdicionales)
            {
                precioTotal += servicio.PrecioServicio; // Sumamos el precio de cada servicio adicional
            }

            // Crear el modelo para la vista de confirmación
            var modeloConfirmacion = new ConfirmacionReservaViewModel
            {
                IdReserva = reserva.IdReserva,
                Paquete = reserva.IdPaqueteNavigation,
                ServiciosAdicionales = serviciosAdicionales,
                PrecioTotal = precioTotal, // Usamos el precio calculado dinámicamente
                FechaInicio = reserva.FechaInicio,
                FechaFin = reserva.FechaFin
            };

            return View(modeloConfirmacion);
        }




    }
}
