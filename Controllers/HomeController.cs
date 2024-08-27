using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RosePark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
            // Recuperar los datos de la base de datos
            var paquetes = _context.Paquetes.ToList();
            var servicios = _context.Servicios.ToList();
            var habitaciones = _context.Habitaciones.ToList();

            // Pasa los datos a la vista utilizando ViewData
            ViewData["Paquetes"] = paquetes;
            ViewData["Servicios"] = servicios;
            ViewData["Habitaciones"] = habitaciones;

            return View();
        }


       [HttpPost]
public async Task<IActionResult> Search(DateTime checkinDate, DateTime checkoutDate, int numeroPersonas)
{
    // Validación de fechas
    if (checkinDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue.Value || checkoutDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue.Value)
    {
        ModelState.AddModelError("", "Las fechas seleccionadas son inválidas.");
        return View("Search", new List<Paquete>()); // Devuelve la vista sin resultados si las fechas son inválidas
    }

    // Actualizar la sesión con los nuevos valores
    HttpContext.Session.SetString("CheckinDate", checkinDate.ToString("yyyy-MM-dd"));
    HttpContext.Session.SetString("CheckoutDate", checkoutDate.ToString("yyyy-MM-dd"));
    HttpContext.Session.SetInt32("NumeroPersonas", numeroPersonas);

    // Actualizar ViewData para mostrar los datos actualizados en la vista
    ViewData["CheckinDate"] = checkinDate.ToString("yyyy-MM-dd");
    ViewData["CheckoutDate"] = checkoutDate.ToString("yyyy-MM-dd");
    ViewData["NumeroPersonas"] = numeroPersonas;

    // Realizar la búsqueda con los datos actualizados
    var availablePackages = await _context.Paquetes
        .Include(p => p.IdHabitacionNavigation) // Incluir la relación con Habitaciones
        .Include(p => p.PaquetesServicios) // Incluir la relación con PaquetesServicios
        .ThenInclude(ps => ps.IdServicioNavigation) // Incluir la relación con Servicios
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

    // Devuelve la vista "Search" con los paquetes disponibles
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




    }
}