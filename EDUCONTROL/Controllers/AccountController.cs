using EDUCONTROL.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDUCONTROL.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        public AccountController(AppDbContext db) { _db = db; }
        // GET: /Account/Login
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UsuarioRol") != null)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string usuario, string contrasena)
        {
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasena))
            {
                ViewBag.Error = "Completa usuario y contrasena.";
                return View();
            }

            var user = await _db.Usuarios.FirstOrDefaultAsync(
                u => u.NombreUsuario == usuario
                && u.Contrasena == contrasena
                && u.Activo);

            if (user == null)
            {
                ViewBag.Error = "Usuario o contrasena incorrectos.";
                return View();
            }

            // --- GUARDAR DATOS EN SESIÓN ---
            HttpContext.Session.SetInt32("UsuarioId", user.Id);
            HttpContext.Session.SetString("UsuarioNombre", user.NombreCompleto);
            HttpContext.Session.SetString("UsuarioRol", user.Rol);

            // Guardamos el Grado si tiene uno
            if (!string.IsNullOrEmpty(user.GradoAsignado))
                HttpContext.Session.SetString("GradoAsignado", user.GradoAsignado);

            // NUEVO: Guardamos la Sección en la sesión para usarla en los reportes
            if (!string.IsNullOrEmpty(user.SeccionAsignada))
                HttpContext.Session.SetString("SeccionAsignada", user.SeccionAsignada);

            return RedirectToAction("Index", "Dashboard");
        }
        // GET: /Account/Logout
        public IActionResult Logout()
        { HttpContext.Session.Clear(); return RedirectToAction("Login"); }
    }
}