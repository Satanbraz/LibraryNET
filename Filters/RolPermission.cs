using System.Web.Mvc;
using System.Web.Routing;

namespace LibraryNET.Filters
{
    public class RolPermission : ActionFilterAttribute
    {
        /// <summary>
        /// Verifica si el usuario tiene rol de Administrador.
        /// </summary>
        /// <param name="filterContext">Parametro con el valor del contexto a ejecutar</param>
        /// <returns>Acceso de Administrador a metodos de controlador</returns>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Verificar si el usuario tiene el rol adecuado
            if ((int)filterContext.HttpContext.Session["UserRole"] != 0 && (int)filterContext.HttpContext.Session["UserRole"] == 1)
            {
                // El usuario es administrador, permitir el acceso
                base.OnActionExecuting(filterContext);
            }
            else
            {
                // El usuario no tiene el rol adecuado, redirigir a una página de error o realizar otra acción
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "AccesoNoAutorizado" }));
            }
        }


    }
}