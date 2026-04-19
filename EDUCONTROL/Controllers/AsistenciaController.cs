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

        // --- HELPERS DE SESIÓN ---
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

            // REGLA: Si es Profesor, forzamos su Grado y Sección de la sesión
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

            // Permite ver filtros solo a Director o Secretaria
            ViewBag.EsAdmin = (rol == "Director" || rol == "Secretaria");

            return View();
        }

        // POST: /Asistencia/Registrar (Se mantiene igual, ya que usa los IDs de alumnos filtrados)
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
                    a => a.AlumnoId == id && a.Fecha.Date == fechaDate.Date);

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

            // 1. Manejo de la fecha: Si el usuario no elige una, usamos HOY.
            // Pero si elige una, debemos mantener esa.
            if (string.IsNullOrEmpty(fecha))
            {
                fecha = DateTime.Today.ToString("yyyy-MM-dd");
            }

            // --- SEGURIDAD PARA PROFESORES ---
            if (rol == "Profesor")
            {
                grado = gradoForzado;
                seccion = seccionForzada;
            }

            var q = _db.Asistencias.Include(a => a.Alumno).AsQueryable();

            // 2. Aplicar filtro de fecha (Convertir string a DateTime para comparar solo la fecha)
            if (DateTime.TryParse(fecha, out DateTime f))
            {
                q = q.Where(a => a.Fecha.Date == f.Date);
            }

            if (!string.IsNullOrEmpty(grado))
                q = q.Where(a => a.Alumno!.Grado == grado);

            if (!string.IsNullOrEmpty(seccion))
                q = q.Where(a => a.Alumno!.Seccion == seccion);

            // 3. ENVIAR DATOS A LA VISTA (Asegúrate de que los nombres coincidan con tu .cshtml)
            ViewBag.FechaFiltro = fecha; // Esto asegura que el input mantenga la fecha elegida
            ViewBag.GradoFiltro = grado;
            ViewBag.SeccionFiltro = seccion;

            // Aquí usamos el nombre que pusiste en tu vista: esDirector = ViewBag.EsAdminOSecretaria
            ViewBag.EsAdminOSecretaria = (rol == "Director" || rol == "Secretaria");

            return View(await q
                .OrderByDescending(a => a.Fecha)
                .ThenBy(a => a.Alumno!.NombreCompleto)
                .ToListAsync());
        }
    }
}