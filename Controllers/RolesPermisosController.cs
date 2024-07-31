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
    public class RolesPermisosController : Controller
    {
        private readonly RoseParkDbContext _context;

        public RolesPermisosController(RoseParkDbContext context)
        {
            _context = context;
        }

        // GET: RolesPermisos
        public async Task<IActionResult> Index()
        {
            var roseParkDbContext = _context.RolesPermisos.Include(r => r.IdPermisosNavigation).Include(r => r.IdRolNavigation);
            return View(await roseParkDbContext.ToListAsync());
        }

        // GET: RolesPermisos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rolesPermiso = await _context.RolesPermisos
                .Include(r => r.IdPermisosNavigation)
                .Include(r => r.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdRolesPermisos == id);
            if (rolesPermiso == null)
            {
                return NotFound();
            }

            return View(rolesPermiso);
        }

        // GET: RolesPermisos/Create
        public IActionResult Create()
        {
            ViewData["IdPermisos"] = new SelectList(_context.Permisos, "IdPermisos", "IdPermisos");
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "IdRol");
            return View();
        }

        // POST: RolesPermisos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRolesPermisos,Estado,IdRol,IdPermisos")] RolesPermiso rolesPermiso)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rolesPermiso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdPermisos"] = new SelectList(_context.Permisos, "IdPermisos", "IdPermisos", rolesPermiso.IdPermisos);
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "IdRol", rolesPermiso.IdRol);
            return View(rolesPermiso);
        }

        // GET: RolesPermisos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rolesPermiso = await _context.RolesPermisos.FindAsync(id);
            if (rolesPermiso == null)
            {
                return NotFound();
            }
            ViewData["IdPermisos"] = new SelectList(_context.Permisos, "IdPermisos", "IdPermisos", rolesPermiso.IdPermisos);
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "IdRol", rolesPermiso.IdRol);
            return View(rolesPermiso);
        }

        // POST: RolesPermisos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRolesPermisos,Estado,IdRol,IdPermisos")] RolesPermiso rolesPermiso)
        {
            if (id != rolesPermiso.IdRolesPermisos)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rolesPermiso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RolesPermisoExists(rolesPermiso.IdRolesPermisos))
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
            ViewData["IdPermisos"] = new SelectList(_context.Permisos, "IdPermisos", "IdPermisos", rolesPermiso.IdPermisos);
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "IdRol", rolesPermiso.IdRol);
            return View(rolesPermiso);
        }

        // GET: RolesPermisos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rolesPermiso = await _context.RolesPermisos
                .Include(r => r.IdPermisosNavigation)
                .Include(r => r.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdRolesPermisos == id);
            if (rolesPermiso == null)
            {
                return NotFound();
            }

            return View(rolesPermiso);
        }

        // POST: RolesPermisos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rolesPermiso = await _context.RolesPermisos.FindAsync(id);
            if (rolesPermiso != null)
            {
                _context.RolesPermisos.Remove(rolesPermiso);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RolesPermisoExists(int id)
        {
            return _context.RolesPermisos.Any(e => e.IdRolesPermisos == id);
        }
    }
}
