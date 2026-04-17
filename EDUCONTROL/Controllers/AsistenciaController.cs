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
        public IActionResult Index() => View();
        public async Task<IActionResult> Registrar(string? grado, string? seccion)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            var gradoSesion = HttpContext.Session.GetString("GradoAsignado");

            // Si es Profesor, no puede elegir grado, se le asigna el suyo automáticamente
            if (rol == "Profesor") grado = gradoSesion;

            var q = _db.Alumnos.Where(a => a.Estado == "Activo");

            // Aplicamos filtros de grado y sección si vienen en la URL
            if (!string.IsNullOrEmpty(grado)) q = q.Where(a => a.Grado == grado);
            if (!string.IsNullOrEmpty(seccion)) q = q.Where(a => a.Seccion == seccion);

            ViewBag.Alumnos = await q.OrderBy(a => a.NombreCompleto).ToListAsync();
            ViewBag.Fecha = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.Grado = grado ?? "Todos los grados";
            ViewBag.Seccion = seccion;

            // Esta línea le dirá a la vista si debe mostrar los filtros o no
            ViewBag.EsAdmin = (rol == "Director" || rol == "Secretaria");

            return View();
        }
        // POST: /Asistencia/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(List<int> alumnoIds, IFormCollection form, string fecha)
        {
            var quien = HttpContext.Session.GetString("UsuarioNombre") ?? "sistema";

            // Validación de fecha por si acaso
            if (!DateTime.TryParse(fecha, out DateTime fechaDate))
            {
                fechaDate = DateTime.Today;
            }

            foreach (var id in alumnoIds)
            {
                // 1. Buscamos el valor que viene de la vista (ej: "estado_12")
                string nombreCampo = $"estado_{id}";
                string? estadoSeleccionado = form[nombreCampo];

                // 2. EL SALVAVIDAS: 
                // Si no marcaste nada en la fila, 'estadoSeleccionado' será nulo o vacío.
                // Aquí decidimos: o le ponemos "Presente" por defecto, o saltamos al siguiente.
                if (string.IsNullOrEmpty(estadoSeleccionado))
                {
                    // Opción A: Saltarlo si no se marcó (no guarda asistencia para ese alumno)
                    // continue; 

                    // Opción B: Ponerle "Presente" automáticamente si se te olvidó
                    estadoSeleccionado = "Presente";
                }

                // 3. Verificamos si ya existe registro para ese alumno en esa fecha
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
        public async Task<IActionResult> Consultar(string? fecha, string? grado)
        {
            // --- BLOQUEO DE SEGURIDAD PARA PROFESORES ---
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol == "Profesor")
            {
                // Si es profesor, lo redirigimos al Dashboard con un aviso
                TempData["Error"] = "No tienes permisos para consultar el historial de asistencia.";
                return RedirectToAction("Index", "Dashboard");
            }

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

            return View(await q
            .OrderByDescending(a => a.Fecha) // Cambiado a Descending para ver lo más reciente arriba
            .ThenBy(a => a.Alumno!.NombreCompleto)
            .ToListAsync());
        }
    }
}