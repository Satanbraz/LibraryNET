using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibraryNET.Filters
{
    public class SessionAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Verifica si hay una sesion iniciada antes de ejecutar un metodo de Controller
        /// </summary>
        /// <param name="filterContext">Parametro con el valor del contexto a ejecutar</param>
        /// <returns>Permiso de acceso al metodo de controlador</returns>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Verificar si hay una sesión iniciada
            if (filterContext.HttpContext.Session["UserId"] == null)
            {
                // No hay una sesión iniciada, redirigir a la página de inicio de sesión
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            // Hay una sesión iniciada, permitir el acceso a la acción
            base.OnActionExecuting(filterContext);
        }
    }
}