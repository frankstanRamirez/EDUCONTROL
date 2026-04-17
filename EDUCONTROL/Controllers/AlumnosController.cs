using EDUCONTROL.Data;
using EDUCONTROL.Filters;
using EDUCONTROL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EduControl.Controllers
{
    [Sesion("Director", "Secretaria")]
    public class AlumnosController : Controller
    {
        private readonly AppDbContext _db;
        public AlumnosController(AppDbContext db) { _db = db; }
        public async Task<IActionResult> Index(string? buscar, string? grado, string? seccion)
        {
            var q = _db.Alumnos.AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                q = q.Where(a => a.NIE.Contains(buscar) || a.NombreCompleto.Contains(buscar));

            if (!string.IsNullOrEmpty(grado))
                q = q.Where(a => a.Grado == grado);

            if (!string.IsNullOrEmpty(seccion))
                q = q.Where(a => a.Seccion == seccion);

            ViewBag.Buscar = buscar;
            ViewBag.GradoFiltro = grado;
            ViewBag.SeccionFiltro = seccion;

            return View(await q.OrderBy(a => a.NombreCompleto).ToListAsync());
        }
        public IActionResult Create() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Alumno a)
        {
            if (await _db.Alumnos.AnyAsync(x => x.NIE == a.NIE))
                ModelState.AddModelError("NIE", "NIE ya registrado.");
            if (!ModelState.IsValid) return View(a);
            a.FechaRegistro = DateTime.Now;
            _db.Add(a); await _db.SaveChangesAsync();
            TempData["OK"] = $"Alumno {a.NombreCompleto} registrado.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var a = await _db.Alumnos.FindAsync(id);
            return a == null ? NotFound() : View(a);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Alumno a)
        {
            if (id != a.Id) return NotFound();
            if (!ModelState.IsValid) return View(a);
            _db.Update(a); await _db.SaveChangesAsync();
            TempData["OK"] = "Alumno actualizado.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var a = await _db.Alumnos.FindAsync(id);
            return a == null ? NotFound() : View(a);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var a = await _db.Alumnos.FindAsync(id);
            if (a != null) { _db.Remove(a); await _db.SaveChangesAsync(); }
            TempData["OK"] = "Alumno eliminado.";
            return RedirectToAction(nameof(Index));
        }


    }
}