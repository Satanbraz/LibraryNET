using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DTO;
using LibraryNET.Filters;
using LibraryNET.Implementation;

namespace LibraryNET.Controllers
{
    public class UsersController : Controller
    {
        #region Propierties

        UserManager _userManager = new UserManager();

        #endregion

        #region UserMethods

        // GET: Users
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Vista de registro de usuario.
        /// </summary>
        public ActionResult UserRegister()
        {
            return View();
        }

        /// <summary>
        /// Genera el registro de un nuevo usuario.
        /// </summary>
        /// <param name="user">Objeto de clase Usuario con los campos traidos del formulario</param>
        /// <returns>String de confirmación de registro</returns>
        [HttpPost]
        public string UserRegister(Users user)
        {
            var id = _userManager.Create(user);

            if (id > 0)
            {

                return "Ok";
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene el registro de un usuario específico.
        /// </summary>
        /// <param name="user">Objeto de clase Usuario</param>
        /// <returns>Registro de datos del usuario específico</returns>
        [RolPermission]
        [Session]
        public ActionResult Details(Users user)
        {
            var oDataUser = _userManager.Read(user.Id);

            return Json(oDataUser, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Obtiene el registro de un usuario específico filtrado por Rut.
        /// </summary>
        /// <param name="userRut">Objeto de clase Usuario</param>
        /// <returns>Registro de datos del usuario específico</returns>
        public ActionResult SearchUserByRut (Users user) { 

            var oListUser = _userManager.ReadUserByrut(user.UserRut);

            return Json(oListUser, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Obtiene el registro del usuario en sesion.
        /// </summary>
        /// <returns>Registro de datos del Usuario conectado</returns>
        [Session]
        public ActionResult UserData()
        {
            var oDataUser = _userManager.Read(Convert.ToInt32(Session["UserId"]));

            return View("UserData", oDataUser);
        }

        /// <summary>
        /// Genera el registro de un nuevo usuario desde panel Admin.
        /// </summary>
        /// <param name="user">Objeto de clase Usuario con los campos traidos del formulario</param>
        /// <returns>String de confirmación de registro</returns>
        [RolPermission]
        [Session]
        public string Create(Users user)
        {
            var id = _userManager.Create(user);

            if (id > 0)
            {

                return "Ok";
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Actualiza el registro de un usuario específico.
        /// </summary>
        /// <param name="user">Objeto de clase Usuario con los campos traidos del formulario</param>
        /// <returns>String de confirmación de actualización</returns>
        [Session]
        public string Edit(Users user)
        {
            var result = _userManager.Update(user);

            if (result == 1)
            {

                return "Ok";
            }
            else
            {
                return null;
            };
        }

        /// <summary>
        /// Elimina el registro de un usuario específico.
        /// </summary>
        /// <param name="userId">Id del usuario a eliminar</param>
        /// <returns>String de confirmación de elininación</returns>
        [Session]
        public string Delete(int userId)
        {
            var result = _userManager.Delete(userId);

            if (result == 1)
            {

                return "Ok";
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene la lista completa de los registro de usuarios.
        /// </summary>
        /// <returns>Listado de usuarios</returns>
        [RolPermission]
        [Session]
        public ActionResult UserList()
        {
            var oListUsers = _userManager.List();

            ViewBag.UserList = oListUsers;
            
            return View(oListUsers);
        }

        /// <summary>
        /// Obtiene la lista completa de los registro de usuarios.
        /// </summary>
        /// <returns>Listado de usuario en formato JSON</returns>
        [RolPermission]
        [Session]
        public ActionResult RolListJson()
        {
            var oListUserRol = _userManager.RolList();

            return Json(oListUserRol, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}