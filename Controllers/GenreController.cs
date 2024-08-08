using DTO;
using LibraryNET.Filters;
using LibraryNET.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibraryNET.Controllers
{
    [Session]
    public class GenreController : Controller
    {
        #region Propierties

        GenreManager _genreManager = new GenreManager();

        #endregion

        #region GenreMethods

        // GET: Genre
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Obtiene el registro de un género literario específico.
        /// </summary>
        /// <param name="genre">Objeto de clase Género</param>
        /// <returns>Registro del género específico</returns>
        [RolPermission]
        public ActionResult Details(Genre genre)
        {
            var oDataGenre = _genreManager.Read(genre.Id);

            return Json(oDataGenre, JsonRequestBehavior.AllowGet);
        }

        // GET: Genre/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Genera el registro de un nuevo literario Género.
        /// </summary>
        /// <param name="genre">Objeto de clase Género con los campos traidos del formulario</param>
        /// <returns>String de confirmación de registro</returns>
        [RolPermission]
        [HttpPost]
        public string Create(Genre genre)
        {
            var result = _genreManager.Create(genre);

            if (result == 1)
            {
                return "Exists";
            }
            else if(result == 2)
            {
                return "OK";
            }
            else
            {
                return null;
            }
        }

        // GET: Genre/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        /// <summary>
        /// Actualiza el registro de un genero literario específico.
        /// </summary>
        /// <param name="genre">Objeto de clase Género con los campos traidos del formulario</param>
        /// <returns>String de confirmación de actualización</returns>
        [RolPermission]
        [HttpPost]
        public string Edit(Genre genre)
        {
            var id = _genreManager.Update(genre);

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
        /// Elimina el registro de un género literario específico.
        /// </summary>
        /// <param name="genreId">Id del genero literario a eliminar</param>
        /// <returns>String de confirmación de eliminación</returns>
        [RolPermission]
        public string Delete(int genreId)
        {
            var id = _genreManager.Delete(genreId);

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
        /// Obtiene la lista completa de los registros de géneros literarios.
        /// </summary>
        /// <returns>Listado de géneros literarios</returns>
        [RolPermission]
        public ActionResult GenreList()
        {
            var oListGenres = _genreManager.List();

            ViewBag.GenreList = oListGenres;

            return View(oListGenres);
        }

        /// <summary>
        /// Obtiene la lista completa de los registros de géneros literarios.
        /// </summary>
        /// <returns>Listado de géneros literarios en formato JSON</returns>
        public ActionResult GenreListJson()
        {
            var oListGenres = _genreManager.List();

            return Json(oListGenres, JsonRequestBehavior.AllowGet);
        }
        
        #endregion
    }
}
