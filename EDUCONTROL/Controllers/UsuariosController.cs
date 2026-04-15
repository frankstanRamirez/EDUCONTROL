using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EDUCONTROL.Data;
using EDUCONTROL.Models;
using EDUCONTROL.Filters;


namespace EDUCONTROL.Controllers
{
    [Sesion("Director")] // Solo Director
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _db;
        public UsuariosController(AppDbContext db) { _db = db; }
        // GET: /Usuarios
        public async Task<IActionResult> Index()
        => View(await _db.Usuarios.OrderBy(u => u.Rol).ToListAsync());
        // GET: /Usuarios/Create
        public IActionResult Create() => View();
        // POST: /Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario u)
        {
            // Validar: si es Profesor, GradoAsignado es obligatorio
            if (u.Rol == "Profesor" && string.IsNullOrEmpty(u.GradoAsignado))
                ModelState.AddModelError("GradoAsignado",
                "El grado es obligatorio para un Profesor.");
            // Si no es Profesor, GradoAsignado debe ser null
            if (u.Rol != "Profesor") u.GradoAsignado = null;
            if (await _db.Usuarios.AnyAsync(x => x.NombreUsuario == u.NombreUsuario))
                ModelState.AddModelError("NombreUsuario", "Ese usuario ya existe.");
            if (!ModelState.IsValid) return View(u);
            u.FechaCreacion = DateTime.Now;
            _db.Add(u);
            await _db.SaveChangesAsync();
            TempData["OK"] = $"Usuario {u.NombreCompleto} creado.";
            return RedirectToAction(nameof(Index));
        }
        // GET: /Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var u = await _db.Usuarios.FindAsync(id);
            return u == null ? NotFound() : View(u);
        }
        // POST: /Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Usuario u)
        {
            if (id != u.Id) return NotFound();
            if (u.Rol == "Profesor" && string.IsNullOrEmpty(u.GradoAsignado))
                ModelState.AddModelError("GradoAsignado",
                "El grado es obligatorio para un Profesor.");
            if (u.Rol != "Profesor") u.GradoAsignado = null;
            if (!ModelState.IsValid) return View(u);
            _db.Update(u);
            await _db.SaveChangesAsync();
            TempData["OK"] = "Usuario actualizado.";
            return RedirectToAction(nameof(Index));
        }
        // POST: /Usuarios/Toggleactivo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var u = await _db.Usuarios.FindAsync(id);
            if (u != null) { u.Activo = !u.Activo; await _db.SaveChangesAsync(); }
            TempData["OK"] = u!.Activo ? "Usuario activado." : "Usuario desactivado.";
            return RedirectToAction(nameof(Index));
        }
    }
}