using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RosePark.Models;

namespace RosePark.Controllers
{
    public class ReservasServiciosController : Controller
    {
        private readonly RoseParkDbContext _context;

        public ReservasServiciosController(RoseParkDbContext context)
        {
            _context = context;
        }

        // GET: ReservasServicios
        public async Task<IActionResult> Index()
        {
            var roseParkDbContext = _context.ReservasServicios.Include(r => r.IdReservaNavigation).Include(r => r.IdServicioNavigation);
            return View(await roseParkDbContext.ToListAsync());
        }

        // GET: ReservasServicios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservasServicio = await _context.ReservasServicios
                .Include(r => r.IdReservaNavigation)
                .Include(r => r.IdServicioNavigation)
                .FirstOrDefaultAsync(m => m.IdReservasServicios == id);
            if (reservasServicio == null)
            {
                return NotFound();
            }

            return View(reservasServicio);
        }

        // GET: ReservasServicios/Create
        public IActionResult Create()
        {
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva");
            ViewData["IdServicio"] = new SelectList(_context.Servicios, "IdServicio", "IdServicio");
            return View();
        }

        // POST: ReservasServicios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdReservasServicios,IdServicio,IdReserva")] ReservasServicio reservasServicio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reservasServicio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", reservasServicio.IdReserva);
            ViewData["IdServicio"] = new SelectList(_context.Servicios, "IdServicio", "IdServicio", reservasServicio.IdServicio);
            return View(reservasServicio);
        }

        // GET: ReservasServicios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservasServicio = await _context.ReservasServicios.FindAsync(id);
            if (reservasServicio == null)
            {
                return NotFound();
            }
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", reservasServicio.IdReserva);
            ViewData["IdServicio"] = new SelectList(_context.Servicios, "IdServicio", "IdServicio", reservasServicio.IdServicio);
            return View(reservasServicio);
        }

        // POST: ReservasServicios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdReservasServicios,IdServicio,IdReserva")] ReservasServicio reservasServicio)
        {
            if (id != reservasServicio.IdReservasServicios)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservasServicio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservasServicioExists(reservasServicio.IdReservasServicios))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", reservasServicio.IdReserva);
            ViewData["IdServicio"] = new SelectList(_context.Servicios, "IdServicio", "IdServicio", reservasServicio.IdServicio);
            return View(reservasServicio);
        }

        // GET: ReservasServicios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservasServicio = await _context.ReservasServicios
                .Include(r => r.IdReservaNavigation)
                .Include(r => r.IdServicioNavigation)
                .FirstOrDefaultAsync(m => m.IdReservasServicios == id);
            if (reservasServicio == null)
            {
                return NotFound();
            }

            return View(reservasServicio);
        }

        // POST: ReservasServicios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservasServicio = await _context.ReservasServicios.FindAsync(id);
            if (reservasServicio != null)
            {
                _context.ReservasServicios.Remove(reservasServicio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservasServicioExists(int id)
        {
            return _context.ReservasServicios.Any(e => e.IdReservasServicios == id);
        }
    }
}
