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

        // --- MÉTODOS DE AYUDA PARA SESIÓN ---
        private string? GradoActivo() => HttpContext.Session.GetString("GradoAsignado");
        private string? SeccionActiva() => HttpContext.Session.GetString("SeccionAsignada");
        private string? RolUsuario() => HttpContext.Session.GetString("UsuarioRol");

        public IActionResult Index()
        {
            return View();
        }

        // GET: /Notas/Registrar
        public async Task<IActionResult> Registrar()
        {
            var grado = GradoActivo();
            var seccion = SeccionActiva();
            var rol = RolUsuario();

            var qA = _db.Alumnos.Where(a => a.Estado == "Activo");

            // Si es profesor, filtramos por SU grado y SU sección
            if (rol == "Profesor")
            {
                if (!string.IsNullOrEmpty(grado)) qA = qA.Where(a => a.Grado == grado);
                if (!string.IsNullOrEmpty(seccion)) qA = qA.Where(a => a.Seccion == seccion);
            }

            ViewBag.Alumnos = await qA.OrderBy(a => a.NombreCompleto).ToListAsync();
            ViewBag.Asignaturas = await _db.Asignaturas.Where(a => a.Activa).OrderBy(a => a.Nombre).ToListAsync();

            ViewBag.Grado = grado ?? "Todos";
            ViewBag.Seccion = seccion ?? "Todas";

            return View();
        }

        // GET: /Notas/Consultar
        public async Task<IActionResult> Consultar(string? grado, string? seccion, int? asignaturaId)
        {
            var rol = RolUsuario();
            var gradoForzado = GradoActivo();
            var seccionForzada = SeccionActiva();

            // REGLA: Si es Profe, le clavamos su grado y sección, no puede ver otros
            if (rol == "Profesor")
            {
                grado = gradoForzado;
                seccion = seccionForzada;
            }

            var q = _db.Notas.Include(n => n.Alumno).Include(n => n.Asignatura).AsQueryable();

            if (!string.IsNullOrEmpty(grado))
                q = q.Where(n => n.Alumno!.Grado == grado);

            if (!string.IsNullOrEmpty(seccion))
                q = q.Where(n => n.Alumno!.Seccion == seccion);

            if (asignaturaId.HasValue)
                q = q.Where(n => n.AsignaturaId == asignaturaId);

            ViewBag.GradoFiltro = grado;
            ViewBag.SeccionFiltro = seccion;
            ViewBag.AsigFiltro = asignaturaId;
            ViewBag.Asignaturas = await _db.Asignaturas.Where(a => a.Activa).ToListAsync();

            // Si no tiene grado forzado es porque es Admin/Director
            ViewBag.EsAdmin = (rol != "Profesor");

            return View(await q.OrderBy(n => n.Alumno!.NombreCompleto).ToListAsync());
        }

        // POST: /Notas/Registrar (Ajustado para validación)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Nota n)
        {
            if (await _db.Notas.AnyAsync(x => x.AlumnoId == n.AlumnoId && x.AsignaturaId == n.AsignaturaId))
            {
                ModelState.AddModelError("", "Ya existe una nota para este alumno en esa asignatura.");
            }

            if (!ModelState.IsValid)
            {
                var rol = RolUsuario();
                var qA = _db.Alumnos.Where(a => a.Estado == "Activo");

                if (rol == "Profesor")
                {
                    var g = GradoActivo();
                    var s = SeccionActiva();
                    if (!string.IsNullOrEmpty(g)) qA = qA.Where(a => a.Grado == g);
                    if (!string.IsNullOrEmpty(s)) qA = qA.Where(a => a.Seccion == s);
                }

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
    }
}