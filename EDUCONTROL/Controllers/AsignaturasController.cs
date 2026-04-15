using EDUCONTROL.Data;
using EDUCONTROL.Filters;
using EDUCONTROL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDUCONTROL.Controllers
{
    [Sesion("Director", "Secretaria")]
    public class AsignaturasController : Controller
    {
        private readonly AppDbContext _db;
        public AsignaturasController(AppDbContext db) { _db = db; }

        // GET: /Asignaturas
        public async Task<IActionResult> Index()
            => View(await _db.Asignaturas.OrderBy(a => a.Nombre).ToListAsync());

        // GET: /Asignaturas/Create
        public IActionResult Create() => View();

        // POST: /Asignaturas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Asignatura a)
        {
            if (!ModelState.IsValid) return View(a);
            _db.Add(a);
            await _db.SaveChangesAsync();
            TempData["OK"] = $"Asignatura {a.Nombre} creada.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Asignaturas/ToggleActiva/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActiva(int id)
        {
            var a = await _db.Asignaturas.FindAsync(id);
            if (a != null) { a.Activa = !a.Activa; await _db.SaveChangesAsync(); }
            TempData["OK"] = a!.Activa ? "Asignatura activada." : "Asignatura desactivada.";
            return RedirectToAction(nameof(Index));
        }
    }
}