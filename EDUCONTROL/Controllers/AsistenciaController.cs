using EDUCONTROL.Data;
using EDUCONTROL.Filters;
using EDUCONTROL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduControl.Controllers
{
    [Sesion]
    public class AsistenciaController : Controller
    {
        private readonly AppDbContext _db;
        public AsistenciaController(AppDbContext db) { _db = db; }

        private string? GradoActivo() => HttpContext.Session.GetString("GradoAsignado");
        private string? SeccionActiva() => HttpContext.Session.GetString("SeccionAsignada");
        private string? RolUsuario() => HttpContext.Session.GetString("UsuarioRol");

        public IActionResult Index() => View();

        // GET: /Asistencia/Registrar
        public async Task<IActionResult> Registrar(string? grado, string? seccion)
        {
            var rol = RolUsuario();
            var gradoSesion = GradoActivo();
            var seccionSesion = SeccionActiva();

            if (rol == "Profesor")
            {
                grado = gradoSesion;
                seccion = seccionSesion;
            }

            var q = _db.Alumnos.Where(a => a.Estado == "Activo");

            if (!string.IsNullOrEmpty(grado)) q = q.Where(a => a.Grado == grado);
            if (!string.IsNullOrEmpty(seccion)) q = q.Where(a => a.Seccion == seccion);

            ViewBag.Alumnos = await q.OrderBy(a => a.NombreCompleto).ToListAsync();
            ViewBag.Fecha = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.Grado = grado ?? "Todos los grados";
            ViewBag.Seccion = seccion ?? "Todas";
            ViewBag.EsAdmin = (rol == "Director" || rol == "Secretaria");

            return View();
        }

        // POST: /Asistencia/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(List<int> alumnoIds, IFormCollection form, string fecha)
        {
            var quien = HttpContext.Session.GetString("UsuarioNombre") ?? "sistema";
            if (!DateTime.TryParse(fecha, out DateTime fechaDate)) fechaDate = DateTime.Today;

            foreach (var id in alumnoIds)
            {
                string nombreCampo = $"estado_{id}";
                string? estadoSeleccionado = form[nombreCampo];

                if (string.IsNullOrEmpty(estadoSeleccionado)) estadoSeleccionado = "Presente";

                var existe = await _db.Asistencias.AnyAsync(
                    a => a.AlumnoId == id && a.Fecha >= fechaDate.Date && a.Fecha < fechaDate.Date.AddDays(1));

                if (!existe)
                {
                    _db.Asistencias.Add(new Asistencia
                    {
                        AlumnoId = id,
                        Fecha = fechaDate,
                        Estado = estadoSeleccionado,
                        RegistradoPor = quien
                    });
                }
            }

            await _db.SaveChangesAsync();
            TempData["OK"] = "Asistencia procesada correctamente.";
            return RedirectToAction(nameof(Consultar));
        }

        // GET: /Asistencia/Consultar
        public async Task<IActionResult> Consultar(string? fecha, string? grado, string? seccion)
        {
            var rol = RolUsuario();
            var gradoForzado = GradoActivo();
            var seccionForzada = SeccionActiva();

            if (string.IsNullOrEmpty(fecha))
            {
                fecha = DateTime.Today.ToString("yyyy-MM-dd");
            }

            if (rol == "Profesor")
            {
                grado = gradoForzado;
                seccion = seccionForzada;
            }

            var q = _db.Asistencias.Include(a => a.Alumno).AsQueryable();

            // ✅ FIX: Comparación de fecha compatible con EF Core / SQLite
            if (DateTime.TryParse(fecha, out DateTime f))
            {
                q = q.Where(a => a.Fecha >= f.Date && a.Fecha < f.Date.AddDays(1));
            }

            if (!string.IsNullOrEmpty(grado))
                q = q.Where(a => a.Alumno!.Grado == grado);

            if (!string.IsNullOrEmpty(seccion))
                q = q.Where(a => a.Alumno!.Seccion == seccion);

            ViewBag.FechaFiltro = fecha;
            ViewBag.GradoFiltro = grado;
            ViewBag.SeccionFiltro = seccion;
            ViewBag.EsAdminOSecretaria = (rol == "Director" || rol == "Secretaria");

            return View(await q
                .OrderByDescending(a => a.Fecha)
                .ThenBy(a => a.Alumno!.NombreCompleto)
                .ToListAsync());
        }
    }
}