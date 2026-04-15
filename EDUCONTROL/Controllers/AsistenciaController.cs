using EDUCONTROL.Data;
using EDUCONTROL.Filters;
using EDUCONTROL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EduControl.Controllers
{
    [Sesion] // Cualquier rol con sesion
    public class AsistenciaController : Controller
    {
        private readonly AppDbContext _db;
        public AsistenciaController(AppDbContext db) { _db = db; }
        // Devuelve el grado que corresponde segun el rol
        private string? GradoActivo() =>
        HttpContext.Session.GetString("UsuarioRol") == "Profesor"
        ? HttpContext.Session.GetString("GradoAsignado")
        : null; // null = sin restriccion de grado
                // GET: /Asistencia/Registrar
        public async Task<IActionResult> Registrar()
        {
            var grado = GradoActivo();
            var q = _db.Alumnos.Where(a => a.Estado == "Activo");
            if (grado != null) q = q.Where(a => a.Grado == grado);
            ViewBag.Alumnos = await q.OrderBy(a => a.NombreCompleto).ToListAsync();
            ViewBag.Fecha = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.Grado = grado ?? "Todos los grados";
            return View();
        }
        // POST: /Asistencia/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
        List<int> alumnoIds, List<string> estados, string fecha)
        {
            var quien = HttpContext.Session.GetString("UsuarioNombre") ??
           "sistema";
            var fechaDate = DateTime.Parse(fecha);
            for (int i = 0; i < alumnoIds.Count; i++)
            {
                var existe = await _db.Asistencias.AnyAsync(
                a => a.AlumnoId == alumnoIds[i]
                && a.Fecha.Date == fechaDate.Date);
                if (!existe)
                    _db.Asistencias.Add(new Asistencia
                    {
                        AlumnoId = alumnoIds[i],
                        Fecha = fechaDate,
                        Estado = estados[i],
                        RegistradoPor = quien
                    });
            }
            await _db.SaveChangesAsync();
            TempData["OK"] = "Asistencia guardada correctamente.";
            return RedirectToAction(nameof(Consultar));
        }
        // GET: /Asistencia/Consultar
        public async Task<IActionResult> Consultar(string? fecha, string? grado)
        {
            var gradoForzado = GradoActivo();
            // Si es Profesor: ignorar el filtro de grado del querystring
            if (gradoForzado != null) grado = gradoForzado;
            var q = _db.Asistencias.Include(a => a.Alumno).AsQueryable();
            if (!string.IsNullOrEmpty(fecha))
                q = q.Where(a => a.Fecha.Date == DateTime.Parse(fecha).Date);
            if (!string.IsNullOrEmpty(grado))
                q = q.Where(a => a.Alumno!.Grado == grado);
            ViewBag.FechaFiltro = fecha;
            ViewBag.GradoFiltro = grado;
            ViewBag.EsAdmin = gradoForzado == null; // para mostrar filtro de grado
            return View(await q
            .OrderBy(a => a.Fecha).ThenBy(a => a.Alumno!.NombreCompleto)
            .ToListAsync());
        }
    }
}