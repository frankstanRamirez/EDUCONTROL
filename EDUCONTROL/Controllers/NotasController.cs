using EDUCONTROL.Data;
using EDUCONTROL.Filters;
using EDUCONTROL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDUCONTROL.Controllers
{
    [Sesion]
    public class NotasController : Controller
    {
        private readonly AppDbContext _db;

        public NotasController(AppDbContext db)
        {
            _db = db;
        }

        private string? GradoActivo() =>
            HttpContext.Session.GetString("UsuarioRol") == "Profesor"
            ? HttpContext.Session.GetString("GradoAsignado")
            : null;

        // --- 1. MÉTODO INDEX AGREGADO ---
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Notas/Registrar
        public async Task<IActionResult> Registrar()
        {
            var grado = GradoActivo();
            var qA = _db.Alumnos.Where(a => a.Estado == "Activo");

            if (grado != null) qA = qA.Where(a => a.Grado == grado);

            ViewBag.Alumnos = await qA.OrderBy(a => a.NombreCompleto).ToListAsync();
            ViewBag.Asignaturas = await _db.Asignaturas
                .Where(a => a.Activa).OrderBy(a => a.Nombre).ToListAsync();
            ViewBag.Grado = grado ?? "Todos";

            return View();
        }

        // POST: /Notas/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Nota n)
        {
            // Evitar duplicado alumno + asignatura
            if (await _db.Notas.AnyAsync(x => x.AlumnoId == n.AlumnoId && x.AsignaturaId == n.AsignaturaId))
            {
                ModelState.AddModelError("", "Ya existe una nota para este alumno en esa asignatura.");
            }

            if (!ModelState.IsValid)
            {
                var grado = GradoActivo();
                var qA = _db.Alumnos.Where(a => a.Estado == "Activo");
                if (grado != null) qA = qA.Where(a => a.Grado == grado);

                ViewBag.Alumnos = await qA.OrderBy(a => a.NombreCompleto).ToListAsync();
                ViewBag.Asignaturas = await _db.Asignaturas.Where(a => a.Activa).OrderBy(a => a.Nombre).ToListAsync();
                return View(n);
            }

            n.RegistradoPor = HttpContext.Session.GetString("UsuarioNombre") ?? "sistema";
            n.FechaRegistro = DateTime.Now;

            _db.Add(n);
            await _db.SaveChangesAsync();

            TempData["OK"] = "Nota registrada correctamente.";
            return RedirectToAction(nameof(Consultar));
        }

        /// GET: /Notas/Consultar
        public async Task<IActionResult> Consultar(string? grado, int? asignaturaId)
        {
            // --- BLOQUEO DE SEGURIDAD PARA PROFESORES ---
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol == "Profesor")
            {
                // Si intenta entrar a consultar, lo mandamos al Dashboard con un mensaje
                TempData["Error"] = "No tienes permisos para acceder a la consulta general de notas.";
                return RedirectToAction("Index", "Dashboard");
            }
            // --------------------------------------------

            var gradoForzado = GradoActivo();
            if (gradoForzado != null) grado = gradoForzado;

            var q = _db.Notas.Include(n => n.Alumno).Include(n => n.Asignatura).AsQueryable();

            if (!string.IsNullOrEmpty(grado))
                q = q.Where(n => n.Alumno!.Grado == grado);

            if (asignaturaId.HasValue)
                q = q.Where(n => n.AsignaturaId == asignaturaId);

            ViewBag.GradoFiltro = grado;
            ViewBag.AsigFiltro = asignaturaId;
            ViewBag.Asignaturas = await _db.Asignaturas.Where(a => a.Activa).ToListAsync();
            ViewBag.EsAdmin = gradoForzado == null;

            return View(await q.OrderBy(n => n.Alumno!.NombreCompleto).ToListAsync());
        }
    }
} // Se cerró correctamente el namespace