using LibraryNET.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LibraryNET.Controllers
{
    public class AccountController : Controller
    {
        #region Propierties

        LoginManager _loginManager = new LoginManager();

        #endregion

        #region LoginMethods

        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Genera el inicio de sesion de usuario.
        /// </summary>
        /// <param name="User">Nombre de usuario</param>
        /// <param name="Password">Contraseña de usuario</param>
        /// <returns>Resultado de inicio correcto</returns>
        [HttpPost]
        public ActionResult Login(string User, string Password)
        {
            var userLogin = _loginManager.Read(User, Password);

            if (userLogin != null)
            {
                // Configurar la sesión con los datos del usuario
                Session["UserId"] = userLogin.Id;
                Session["UserName"] = userLogin.UserName;
                Session["UserLName"] = userLogin.UserLastName;
                Session["UserRole"] = userLogin.UserRolId;
                Session["UserPass"] = userLogin.UserPassword;
                Session["UserPhone"] = userLogin.UserPhone;
                Session["UserDir"] = userLogin.UserDir;
                Session["UserRut"] = userLogin.UserRut;
                Session["LoginTime"] = DateTime.Now;

                return Json(new { redirectUrl = Url.Action("Index", "Home") });
                
            }

            // Si el login falla, devolver un mensaje de error al cliente
            return Json(new { error = "Usuario o contraseña incorrectos" });
        }

        /// <summary>
        /// Genera el cierre de sesion de usuario.
        /// </summary>
        /// <param name="User">Nombre de usuario</param>
        public void Logout()
        {
            // Limpiar la sesión
            Session.Clear();
        }

        /// <summary>
        /// Calcula el tiempo restante de sesion activa.
        /// </summary>
        /// <returns>Tiempo restante en formato JSON</returns>
        public ActionResult RemainingTime()
        {

            if (Session["UserId"] != null && Session["LoginTime"] != null)
            {
                int sessionDurationMinutes = Session.Timeout;
                DateTime loginTime = (DateTime)Session["LoginTime"];
                TimeSpan elapsedTime = DateTime.Now - loginTime;
                TimeSpan remainingTime = TimeSpan.FromMinutes(sessionDurationMinutes) - elapsedTime;

                return Json(remainingTime.ToString("mm\\:ss"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("SessionExpired", JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string newPassword)
        {
            // Aquí procesarías el nuevo password, actualizando la base de datos con el nuevo valor
            // y asociando el token con el usuario correcto

            // Si todo sale bien, redirigirías a la vista de inicio de sesión o una página de éxito.
            return RedirectToAction("Login");
        }

        #endregion
    }
}
