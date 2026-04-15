using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EDUCONTROL.Filters
{
    public class SesionAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;
        // Sin roles = solo verifica que haya sesion activa
        public SesionAttribute(params string[] roles)
        {
            _roles = roles;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var rol = session.GetString("UsuarioRol");
            // Sin sesion: redirigir a login
            if (string.IsNullOrEmpty(rol))
            {
                context.Result = new RedirectToActionResult(
                "Login", "Account", null);
                return;
            }
            // Con roles requeridos: verificar que el rol coincida
            if (_roles.Length > 0 && !_roles.Contains(rol))
            {
                context.Result = new RedirectToActionResult(
                "Index", "Dashboard", null);
                return;
            }
        }
    }
}
