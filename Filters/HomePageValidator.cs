using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibraryNET.Filters
{
    public static class HomePageValidator
    {
        /// <summary>
        /// Comprueba si la pagina actual es Home.
        /// </summary>
        /// <param name="htmlHelper">Helper de la página actual</param>
        /// <returns>Metodo del controller donde se ubica la vista Index en Home</returns>
        public static bool IsHomePage(this HtmlHelper htmlHelper)
        {
            var routeData = htmlHelper.ViewContext.RouteData;
            var currentAction = routeData.GetRequiredString("action");
            var currentController = routeData.GetRequiredString("controller");

            return currentController.Equals("Home", StringComparison.OrdinalIgnoreCase) &&
                   currentAction.Equals("Index", StringComparison.OrdinalIgnoreCase);
        }
    }
}