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

        public async Task<IActionResult> Asistencia(string? grado, string? fecha)
        {
            var qAl = _db.Alumnos.Where(a => a.Estado == "Activo").AsQueryable();
            if (!string.IsNullOrEmpty(grado)) qAl = qAl.Where(a => a.Grado == grado);
            var alumnos = await qAl.OrderBy(a => a.NombreCompleto).ToListAsync();

            var qAs = _db.Asistencias.Include(a => a.Alumno).AsQueryable();
            if (!string.IsNullOrEmpty(grado))
                qAs = qAs.Where(a => a.Alumno!.Grado == grado);
            if (!string.IsNullOrEmpty(fecha))
                qAs = qAs.Where(a => a.Fecha.Date == DateTime.Parse(fecha).Date);

            var asist = await qAs.ToListAsync();

            ViewBag.Detalle = alumnos.Select(al => new {
                al.NombreCompleto,
                Asistencias = asist.Count(a => a.AlumnoId == al.Id && a.Estado == "Presente"),
                Ausencias = asist.Count(a => a.AlumnoId == al.Id && a.Estado == "Ausente"),
                Tardias = asist.Count(a => a.AlumnoId == al.Id && a.Estado == "Tardanza"),
            }).ToList();

            ViewBag.GradoFiltro = grado;
            ViewBag.FechaFiltro = fecha;
            ViewBag.FechaReporte = DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.HoraReporte = DateTime.Now.ToString("HH:mm");
            ViewBag.Docente = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.TotalEstudiantes = alumnos.Count;
            ViewBag.TotalPresentes = asist.Count(a => a.Estado == "Presente");
            ViewBag.TotalAusentes = asist.Count(a => a.Estado == "Ausente");
            ViewBag.TotalTardanzas = asist.Count(a => a.Estado == "Tardanza");
            return View();
        }

        public async Task<IActionResult> Notas(string? grado, string? seccion, int? asignaturaId)
        {
            var qAl = _db.Alumnos.Where(a => a.Estado == "Activo").AsQueryable();
            if (!string.IsNullOrEmpty(grado)) qAl = qAl.Where(a => a.Grado == grado);
            var alumnos = await qAl.OrderBy(a => a.NombreCompleto).ToListAsync();

            var qN = _db.Notas.Include(n => n.Alumno).Include(n => n.Asignatura).AsQueryable();
            if (!string.IsNullOrEmpty(grado))
                qN = qN.Where(n => n.Alumno!.Grado == grado);
            if (asignaturaId.HasValue)
                qN = qN.Where(n => n.AsignaturaId == asignaturaId);

            var notas = await qN.ToListAsync();

            // Filtrar también por sección si se seleccionó
            if (!string.IsNullOrEmpty(seccion))
                qAl = qAl.Where(a => a.Seccion == seccion);

            ViewBag.SeccionFiltro = seccion;

            ViewBag.Detalle = alumnos.Select(al => {
                var nota = notas.FirstOrDefault(n => n.AlumnoId == al.Id);
                return new
                {
                    al.NombreCompleto,
                    // CORRECCIÓN LÍNEAS 59, 60, 61: Uso de Value.ToString con el formato correcto
                    Periodo1 = nota?.Periodo1.HasValue == true ? nota.Periodo1.Value.ToString("0.0") : "—",
                    Periodo2 = nota?.Periodo2.HasValue == true ? nota.Periodo2.Value.ToString("0.0") : "—",
                    Periodo3 = nota?.Periodo3.HasValue == true ? nota.Periodo3.Value.ToString("0.0") : "—",
                    Promedio = nota != null ? nota.Promedio.ToString("0.00") : "—",
                    Estado = nota?.Estado ?? "—",
                };
            }).ToList();

            var promedioGeneral = notas.Any()
                ? notas.Average(n => n.Promedio).ToString("0.00")
                : "—";

            ViewBag.GradoFiltro = grado;
            ViewBag.AsigFiltro = asignaturaId;
            ViewBag.Asignaturas = await _db.Asignaturas.Where(a => a.Activa).ToListAsync();
            ViewBag.FechaReporte = DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.HoraReporte = DateTime.Now.ToString("HH:mm");
            ViewBag.Docente = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.TotalEstudiantes = alumnos.Count;
            ViewBag.PromedioGeneral = promedioGeneral;
            ViewBag.Aprobados = notas.Count(n => n.Estado == "Aprobado");
            ViewBag.Reprobados = notas.Count(n => n.Estado == "Reprobado");
            return View();
        }
    }
}