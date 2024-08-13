using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RosePark.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

            // Pasa los datos a la vista utilizando ViewData
            ViewData["Paquetes"] = paquetes;
            ViewData["Servicios"] = servicios;
            ViewData["Habitaciones"] = habitaciones;

            return View();
        }

        [HttpPost]
        public IActionResult Search(DateTime checkin_date, DateTime checkout_date)
        {
            var availablePackages = _context.Paquetes
                .Where(p => !_context.Reservas.Any(r =>
                        r.IdPaquete == p.IdPaquete &&
                        (
                            (r.FechaInicio <= checkin_date && r.FechaFin >= checkin_date) ||
                            (r.FechaInicio <= checkout_date && r.FechaFin >= checkout_date) ||
                            (r.FechaInicio >= checkin_date && r.FechaFin <= checkout_date)
                        )
                    )
                )
                .ToList();

            // Pasa los datos de los paquetes disponibles a la vista
            ViewData["Paquetes"] = availablePackages;

            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}