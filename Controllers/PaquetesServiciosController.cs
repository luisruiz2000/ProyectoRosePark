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
    public class PaquetesServiciosController : Controller
    {
        private readonly RoseParkDbContext _context;

        public PaquetesServiciosController(RoseParkDbContext context)
        {
            _context = context;
        }

        // GET: PaquetesServicios
        public async Task<IActionResult> Index()
        {
            var roseParkDbContext = _context.PaquetesServicios.Include(p => p.IdPaqueteNavigation).Include(p => p.IdServicioNavigation);
            return View(await roseParkDbContext.ToListAsync());
        }

        // GET: PaquetesServicios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paquetesServicio = await _context.PaquetesServicios
                .Include(p => p.IdPaqueteNavigation)
                .Include(p => p.IdServicioNavigation)
                .FirstOrDefaultAsync(m => m.IdPaquetesServicios == id);
            if (paquetesServicio == null)
            {
                return NotFound();
            }

            return View(paquetesServicio);
        }

        // GET: PaquetesServicios/Create
        public IActionResult Create()
        {
            ViewData["IdPaquete"] = new SelectList(_context.Paquetes, "IdPaquete", "IdPaquete");
            ViewData["IdServicio"] = new SelectList(_context.Servicios, "IdServicio", "IdServicio");
            return View();
        }

        // POST: PaquetesServicios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPaquetesServicios,IdServicio,IdPaquete")] PaquetesServicio paquetesServicio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paquetesServicio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdPaquete"] = new SelectList(_context.Paquetes, "IdPaquete", "IdPaquete", paquetesServicio.IdPaquete);
            ViewData["IdServicio"] = new SelectList(_context.Servicios, "IdServicio", "IdServicio", paquetesServicio.IdServicio);
            return View(paquetesServicio);
        }

        // GET: PaquetesServicios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paquetesServicio = await _context.PaquetesServicios.FindAsync(id);
            if (paquetesServicio == null)
            {
                return NotFound();
            }
            ViewData["IdPaquete"] = new SelectList(_context.Paquetes, "IdPaquete", "IdPaquete", paquetesServicio.IdPaquete);
            ViewData["IdServicio"] = new SelectList(_context.Servicios, "IdServicio", "IdServicio", paquetesServicio.IdServicio);
            return View(paquetesServicio);
        }

        // POST: PaquetesServicios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPaquetesServicios,IdServicio,IdPaquete")] PaquetesServicio paquetesServicio)
        {
            if (id != paquetesServicio.IdPaquetesServicios)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paquetesServicio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaquetesServicioExists(paquetesServicio.IdPaquetesServicios))
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
            ViewData["IdPaquete"] = new SelectList(_context.Paquetes, "IdPaquete", "IdPaquete", paquetesServicio.IdPaquete);
            ViewData["IdServicio"] = new SelectList(_context.Servicios, "IdServicio", "IdServicio", paquetesServicio.IdServicio);
            return View(paquetesServicio);
        }

        // GET: PaquetesServicios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paquetesServicio = await _context.PaquetesServicios
                .Include(p => p.IdPaqueteNavigation)
                .Include(p => p.IdServicioNavigation)
                .FirstOrDefaultAsync(m => m.IdPaquetesServicios == id);
            if (paquetesServicio == null)
            {
                return NotFound();
            }

            return View(paquetesServicio);
        }

        // POST: PaquetesServicios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paquetesServicio = await _context.PaquetesServicios.FindAsync(id);
            if (paquetesServicio != null)
            {
                _context.PaquetesServicios.Remove(paquetesServicio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaquetesServicioExists(int id)
        {
            return _context.PaquetesServicios.Any(e => e.IdPaquetesServicios == id);
        }
    }
}
