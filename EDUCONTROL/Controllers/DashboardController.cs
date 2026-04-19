using EDUCONTROL.Data;
using EDUCONTROL.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDUCONTROL.Controllers
{
    [Sesion]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            var grado = HttpContext.Session.GetString("GradoAsignado");
            var seccion = HttpContext.Session.GetString("SeccionAsignada");

            if (rol == "Profesor")
            {
                ViewBag.TotalAlumnos = await _db.Alumnos
                    .CountAsync(a => a.Grado == grado && a.Seccion == seccion && a.Estado == "Activo");

                ViewBag.AsistenciasHoy = await _db.Asistencias
                    .CountAsync(a => a.Alumno!.Grado == grado
                        && a.Alumno.Seccion == seccion
                        && a.Fecha.Date == DateTime.Today
                        && a.Estado == "Presente");

                ViewBag.TotalNotas = await _db.Notas
                    .CountAsync(n => n.Alumno!.Grado == grado && n.Alumno.Seccion == seccion);

                ViewBag.Grado = grado;
                ViewBag.Seccion = seccion;
            }
            else
            {
                ViewBag.TotalAlumnos = await _db.Alumnos.CountAsync(a => a.Estado == "Activo");
                ViewBag.TotalUsuarios = await _db.Usuarios.CountAsync(u => u.Activo);
                ViewBag.AsistenciasHoy = await _db.Asistencias
                    .CountAsync(a => a.Fecha.Date == DateTime.Today && a.Estado == "Presente");
                ViewBag.TotalNotas = await _db.Notas.CountAsync();
            }

            // Pasamos el rol a la vista para que el @if (rol == "Profesor") funcione
            ViewBag.Rol = rol;
            return View();
        }

    }

}