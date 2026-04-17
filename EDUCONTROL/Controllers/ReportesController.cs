using EDUCONTROL.Data;
using EDUCONTROL.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduControl.Controllers
{
    [Sesion("Director", "Secretaria")]
    public class ReportesController : Controller
    {
        private readonly AppDbContext _db;
        public ReportesController(AppDbContext db) { _db = db; }

        // Método auxiliar para validar la sesión
        private bool TieneAcceso()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));
        }

        public async Task<IActionResult> Asistencia(string? grado, string? seccion, string? fecha)
        {
            if (!TieneAcceso()) return RedirectToAction("Login", "Account");

            // Configuración de ViewBags para los filtros y encabezado
            ViewBag.GradoFiltro = grado;
            ViewBag.SeccionFiltro = seccion;
            ViewBag.FechaFiltro = fecha;
            ViewBag.FechaReporte = DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.HoraReporte = DateTime.Now.ToString("HH:mm");
            ViewBag.Docente = HttpContext.Session.GetString("UsuarioNombre");

            // Si no hay grado seleccionado, devolvemos vista con contadores en cero
            if (string.IsNullOrEmpty(grado))
            {
                ViewBag.Detalle = new List<dynamic>();
                ViewBag.TotalEstudiantes = 0;
                ViewBag.TotalPresentes = 0;
                ViewBag.TotalAusentes = 0;
                ViewBag.TotalTardanzas = 0;
                ViewBag.FiltroAplicado = false;
                return View();
            }

            ViewBag.FiltroAplicado = true;

            // 1. Obtener Alumnos filtrados
            var qAl = _db.Alumnos.Where(a => a.Estado == "Activo" && a.Grado == grado);
            if (!string.IsNullOrEmpty(seccion))
                qAl = qAl.Where(a => a.Seccion == seccion);

            var alumnos = await qAl.OrderBy(a => a.NombreCompleto).ToListAsync();

            // 2. Obtener Asistencias filtradas
            var qAs = _db.Asistencias.Include(a => a.Alumno).Where(a => a.Alumno!.Grado == grado);
            if (!string.IsNullOrEmpty(seccion))
                qAs = qAs.Where(a => a.Alumno!.Seccion == seccion);

            if (!string.IsNullOrEmpty(fecha) && DateTime.TryParse(fecha, out DateTime fechaDate))
                qAs = qAs.Where(a => a.Fecha.Date == fechaDate.Date);

            var asist = await qAs.ToListAsync();

            // 3. Cruzar datos para el detalle por alumno
            ViewBag.Detalle = alumnos.Select(al => new {
                al.NombreCompleto,
                Asistencias = asist.Count(a => a.AlumnoId == al.Id && a.Estado == "Presente"),
                Ausencias = asist.Count(a => a.AlumnoId == al.Id && a.Estado == "Ausente"),
                Tardias = asist.Count(a => a.AlumnoId == al.Id && a.Estado == "Tardanza"),
            }).ToList();

            // 4. Totales generales del reporte
            ViewBag.TotalEstudiantes = alumnos.Count;
            ViewBag.TotalPresentes = asist.Count(a => a.Estado == "Presente");
            ViewBag.TotalAusentes = asist.Count(a => a.Estado == "Ausente");
            ViewBag.TotalTardanzas = asist.Count(a => a.Estado == "Tardanza");

            return View();
        }

        public async Task<IActionResult> Notas(string? grado, string? seccion, int? asignaturaId)
        {
            if (!TieneAcceso()) return RedirectToAction("Login", "Account");

            ViewBag.GradoFiltro = grado;
            ViewBag.SeccionFiltro = seccion;
            ViewBag.AsigFiltro = asignaturaId;
            ViewBag.Asignaturas = await _db.Asignaturas.Where(a => a.Activa).ToListAsync();
            ViewBag.FechaReporte = DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.HoraReporte = DateTime.Now.ToString("HH:mm");
            ViewBag.Docente = HttpContext.Session.GetString("UsuarioNombre");

            if (string.IsNullOrEmpty(grado))
            {
                ViewBag.Detalle = new List<dynamic>();
                ViewBag.TotalEstudiantes = 0;
                ViewBag.PromedioGeneral = "—";
                ViewBag.Aprobados = 0;
                ViewBag.Reprobados = 0;
                ViewBag.FiltroAplicado = false;
                return View();
            }

            ViewBag.FiltroAplicado = true;

            var qAl = _db.Alumnos.Where(a => a.Estado == "Activo" && a.Grado == grado);
            if (!string.IsNullOrEmpty(seccion))
                qAl = qAl.Where(a => a.Seccion == seccion);
            var alumnos = await qAl.OrderBy(a => a.NombreCompleto).ToListAsync();

            var qN = _db.Notas.Include(n => n.Alumno).Include(n => n.Asignatura)
                .Where(n => n.Alumno!.Grado == grado).AsQueryable();

            if (!string.IsNullOrEmpty(seccion))
                qN = qN.Where(n => n.Alumno!.Seccion == seccion);
            if (asignaturaId.HasValue)
                qN = qN.Where(n => n.AsignaturaId == asignaturaId);

            var notas = await qN.ToListAsync();

            ViewBag.Detalle = alumnos.Select(al => {
                var nota = notas.FirstOrDefault(n => n.AlumnoId == al.Id);
                return new
                {
                    al.NombreCompleto,
                    Periodo1 = nota?.Periodo1?.ToString("0.0") ?? "—",
                    Periodo2 = nota?.Periodo2?.ToString("0.0") ?? "—",
                    Periodo3 = nota?.Periodo3?.ToString("0.0") ?? "—",
                    Promedio = nota?.Promedio.ToString("0.00") ?? "—",
                    Estado = nota?.Estado ?? "—",
                };
            }).ToList();

            ViewBag.TotalEstudiantes = alumnos.Count;
            ViewBag.PromedioGeneral = notas.Any()
                ? notas.Average(n => n.Promedio).ToString("0.00") : "—";
            ViewBag.Aprobados = notas.Count(n => n.Estado == "Aprobado");
            ViewBag.Reprobados = notas.Count(n => n.Estado == "Reprobado");

            return View();
        }
    }
}