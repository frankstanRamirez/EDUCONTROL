using EDUCONTROL.Data;
using EDUCONTROL.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EDUCONTROL.Controllers
{
    [Sesion] // Cualquier rol con sesion activa
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) { _db = db; }
        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            var grado = HttpContext.Session.GetString("GradoAsignado");
            // Tarjetas del dashboard
            if (rol == "Profesor")
            {
                // Profesor: solo datos de su grado
                ViewBag.TotalAlumnos = await _db.Alumnos
                .CountAsync(a => a.Grado == grado && a.Estado == "Activo");
                ViewBag.AsistenciasHoy = await _db.Asistencias
                .CountAsync(a => a.Alumno!.Grado == grado
                && a.Fecha.Date == DateTime.Today
               && a.Estado == "Presente");
                ViewBag.TotalNotas = await _db.Notas
                .CountAsync(n => n.Alumno!.Grado == grado);
                ViewBag.Grado = grado;
            }
            else
            {
                // Director / Secretaria: todos los grados
                ViewBag.TotalAlumnos = await _db.Alumnos.CountAsync(a => a.Estado ==
               "Activo");
                ViewBag.TotalUsuarios = await _db.Usuarios.CountAsync(u => u.Activo);
                ViewBag.AsistenciasHoy = await _db.Asistencias
                .CountAsync(a => a.Fecha.Date == DateTime.Today
                && a.Estado == "Presente");
                ViewBag.TotalNotas = await _db.Notas.CountAsync();
            }
            return View();
        }
    }
}