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

        public NotasController(AppDbContext db) { _db = db; }

        private string? GradoActivo() => HttpContext.Session.GetString("GradoAsignado");
        private string? SeccionActiva() => HttpContext.Session.GetString("SeccionAsignada");
        private string? RolUsuario() => HttpContext.Session.GetString("UsuarioRol");

        public IActionResult Index() => RedirectToAction(nameof(Consultar));

        // GET: /Notas/Registrar
        public async Task<IActionResult> Registrar(int? alumnoId, int? asignaturaId)
        {
            var modelo = new Nota();
            ViewBag.EsEdicion = false;

            if (alumnoId.HasValue && asignaturaId.HasValue)
            {
                var notaExistente = await _db.Notas
                    .FirstOrDefaultAsync(n => n.AlumnoId == alumnoId && n.AsignaturaId == asignaturaId);

                if (notaExistente != null)
                {
                    modelo = notaExistente;
                    ViewBag.EsEdicion = (notaExistente.Periodo1 != null ||
                                         notaExistente.Periodo2 != null ||
                                         notaExistente.Periodo3 != null);
                }
                else
                {
                    modelo.AlumnoId = alumnoId.Value;
                    modelo.AsignaturaId = asignaturaId.Value;
                }
            }

            // Carga el nombre del alumno si viene desde Consultar
            if (alumnoId.HasValue)
            {
                var alumno = await _db.Alumnos.FindAsync(alumnoId.Value);
                ViewBag.AlumnoNombre = alumno != null
                    ? $"{alumno.NombreCompleto} ({alumno.Grado})"
                    : null;
            }

            // ✅ CORRECCIÓN: filtrar alumnos por grado/sección del profesor
            var rol = RolUsuario();
            IQueryable<Alumno> queryAlumnos = _db.Alumnos.Where(a => a.Estado == "Activo");

            if (rol == "Profesor")
            {
                var grado = GradoActivo();
                var seccion = SeccionActiva();
                if (!string.IsNullOrEmpty(grado)) queryAlumnos = queryAlumnos.Where(a => a.Grado == grado);
                if (!string.IsNullOrEmpty(seccion)) queryAlumnos = queryAlumnos.Where(a => a.Seccion == seccion);
            }

            ViewBag.Alumnos = await queryAlumnos.OrderBy(a => a.NombreCompleto).ToListAsync();
            ViewBag.Asignaturas = await _db.Asignaturas.Where(a => a.Activa).ToListAsync();

            return View(modelo);
        }

        // POST: /Notas/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Nota n)
        {
            var notaDb = await _db.Notas
                .FirstOrDefaultAsync(x => x.AlumnoId == n.AlumnoId && x.AsignaturaId == n.AsignaturaId);

            if (notaDb != null)
            {
                if (n.Periodo1.HasValue) notaDb.Periodo1 = n.Periodo1;
                if (n.Periodo2.HasValue) notaDb.Periodo2 = n.Periodo2;
                if (n.Periodo3.HasValue) notaDb.Periodo3 = n.Periodo3;

                notaDb.RegistradoPor = HttpContext.Session.GetString("UsuarioNombre") ?? "sistema";
                notaDb.FechaRegistro = DateTime.Now;

                _db.Update(notaDb);
                TempData["OK"] = "Cambios guardados con éxito.";
            }
            else
            {
                n.RegistradoPor = HttpContext.Session.GetString("UsuarioNombre") ?? "sistema";
                n.FechaRegistro = DateTime.Now;
                _db.Add(n);
                TempData["OK"] = "Nota registrada correctamente.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Consultar), new { asignaturaId = n.AsignaturaId });
        }

        // GET: /Notas/Consultar
        public async Task<IActionResult> Consultar(string? grado, string? seccion, int? asignaturaId)
        {
            var rol = RolUsuario();
            if (rol == "Profesor")
            {
                grado = GradoActivo();
                seccion = SeccionActiva();
            }

            var qAlumnos = _db.Alumnos
                .Include(a => a.Notas)
                .Where(a => a.Estado == "Activo");

            if (!string.IsNullOrEmpty(grado)) qAlumnos = qAlumnos.Where(a => a.Grado == grado);
            if (!string.IsNullOrEmpty(seccion)) qAlumnos = qAlumnos.Where(a => a.Seccion == seccion);

            ViewBag.GradoFiltro = grado;
            ViewBag.SeccionFiltro = seccion;
            ViewBag.AsigFiltro = asignaturaId;
            ViewBag.Asignaturas = await _db.Asignaturas.Where(a => a.Activa).ToListAsync();
            ViewBag.EsAdmin = (rol != "Profesor");

            return View(await qAlumnos.OrderBy(a => a.NombreCompleto).ToListAsync());
        }
    }
}