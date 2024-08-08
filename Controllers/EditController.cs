using DTO;
using LibraryNET.Filters;
using LibraryNET.Implementation;
using LibraryNET.Implementation.AdminManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibraryNET.Controllers
{
    [Session]
    public class EditController : Controller
    {
        #region Propierties

        EditManager _editManager = new EditManager();

        #endregion

        #region EditMethods

        // GET: Edit
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Obtiene el registro de un editorial específico.
        /// </summary>
        /// <param name="edit">Objeto de clase Editorial</param>
        /// <returns>Registro del editorial específico</returns>
        [RolPermission]
        public ActionResult Details(Edit edit)
        {
            var oDataEdit = _editManager.Read(edit.Id);

            return Json(oDataEdit, JsonRequestBehavior.AllowGet);
        }

        // GET: Edit/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Genera el registro de un nuevo editorial.
        /// </summary>
        /// <param name="edit">Objeto de clase Editorial con los campos traidos del formulario</param>
        /// <returns>String de confirmación de registro</returns>
        [RolPermission]
        [HttpPost]
        public string Create(Edit edit)
        {
            var result = _editManager.Create(edit);

            if (result == 1)
            {
                return "Exists";
            }
            else if (result == 2)
            {
                return "Ok";
            }
            else
            {
                return null;
            }
        }

        // GET: Edit/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        /// <summary>
        /// Actualiza el registro de un editorial específico.
        /// </summary>
        /// <param name="genre">Objeto de clase Editorial con los campos traidos del formulario</param>
        /// <returns>String de confirmación de actualización</returns>
        [RolPermission]
        [HttpPost]
        public string Edit(Edit edit)
        {
            var id = _editManager.Update(edit);

            if (id == 1)
            {

                return "Ok";
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Elimina el registro de un editorial específico.
        /// </summary>
        /// <param name="genreId">Id del editorial a eliminar</param>
        /// <returns>String de confirmación de eliminación</returns>
        [RolPermission]
        public string Delete(int editId)
        {
            var id = _editManager.Delete(editId);

            if (id == 1)
            {

                return "Ok";
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene la lista completa de los registros de editoriales.
        /// </summary>
        /// <returns>Listado de editoriales</returns>
        [RolPermission]
        public ActionResult EditList()
        {
            var oListEdits = _editManager.List();

            ViewBag.EditList = oListEdits;

            return View(oListEdits);
        }

        /// <summary>
        /// Obtiene la lista completa de los registros de editoriales.
        /// </summary>
        /// <returns>Listado de editoriales en formato JSON</returns>
        public ActionResult EditListJson()
        {
            var oListEdits = _editManager.List();

            return Json(oListEdits, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
