using DTO;
using LibraryNET.Filters;
using LibraryNET.Implementation;
using LibraryNET.Implementation.AdminManagers;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace LibraryNET.Controllers
{
    [Session]
    public class BooksController : Controller
    {
       
        #region Propierties

        BooksManager _booksManager = new BooksManager();
        BuySellManager _buySellManager = new BuySellManager();

        #endregion

        #region BooksMethods

        // GET: Books
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Obtiene el registro de un libro específico.
        /// </summary>
        /// <param name="bookId">Id del libro</param>
        /// <returns>Registro del libro específico</returns>
        public ActionResult BookDetail(int bookId)
        {
            var oBookDetail = _booksManager.Read(bookId);
            var buyCount = _buySellManager.GetAllBuys().Where(b => b.buyDetail.BuyProductId == bookId).Sum(b => b.buyDetail.BuyBookQ);
            var sellCount =_buySellManager.GetAllSells().Where(s => s.sellDetail.SellProductId == bookId).Sum(s => s.sellDetail.SellBookQ);

            ViewBag.BuySellCount = buyCount + sellCount;

            return View("BookDetail",oBookDetail);
        }

        // GET: Books/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Genera el registro de un nuevo libro.
        /// </summary>
        /// <param name="books">Objeto de clase Libro con los campos traidos del formulario</param>
        /// <returns>String de confirmación de registro</returns>
        [HttpPost]
        public string Create(Books book)
        {
            var id = _booksManager.Create(book);

            if (id > 0)
            {

                return "Ok";
            }
            else 
            {
                return null;
            }
        }

        // GET: Books/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Books/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Books/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Books/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        /// <summary>
        /// Obtiene la lista completa de los registros de libros.
        /// </summary>
        /// <param name="bookEditId">Id del género del libro a buscar</param>
        /// <param name="bookGenreId">Id del género del libro a buscar</param>
        /// <param name="busqueda">Título del libro a buscar</param>
        /// <param name="page">Número de página actual</param>
        /// <param name="PageSize">Cuenta total de paginación</param>
        /// <returns>Listado de Libros</returns>
        public ActionResult BooksList(int bookEditId = 0, int bookGenreId = 0, string busqueda = "", int page = 1, int PageSize = 9)
        {
            int totalCount;
            IEnumerable<Books> books;

            if (bookEditId <= 0 && bookGenreId <= 0 && string.IsNullOrEmpty(busqueda))
            {
                totalCount = _booksManager.ItemsCount();
                books = _booksManager.List(page, PageSize);
            }
            else
            {
                totalCount = _booksManager.ItemsCountFiltered(bookEditId, bookGenreId, busqueda);
                books = _booksManager.ListFiltered(bookEditId, bookGenreId, busqueda, page, PageSize);
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(books);
        }

        /// <summary>
        /// Cambiar o cargar una imagen a un libro.
        /// </summary>
        /// <param name="bookId">Id del libro a actualizar</param>
        /// <param name="image">Archivo de imágen a cargar o cambiar</param>
        /// <returns>Resultado del proceso en formato JSON</returns>
        [HttpPost]
        public ActionResult UploadImage(int bookId, HttpPostedFileBase image)
        {
            if (image != null && image.ContentLength > 0)
            {
                try
                {
                    // Leer los datos de la imagen y convertirlos en bytes
                    byte[] imageData = null;
                    using (var binaryReader = new BinaryReader(image.InputStream))
                    {
                        imageData = binaryReader.ReadBytes(image.ContentLength);
                    }

                    // Guardar los datos de la imagen en la base de datos (aquí asumimos que tienes un método en tu clase de manejo de datos para hacer esto)
                    _booksManager.SaveBookImage(bookId, imageData);

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }
            else
            {
                return Json(new { success = false, error = "No se ha seleccionado ninguna imagen." });
            }
        }

        /// <summary>
        /// Obtiene la lista de libros recomendados.
        /// </summary>
        /// <returns>Listado de Libros recomendados</returns>
        public JsonResult Recomendations()
        {
            try
            {
                var oRecList = _booksManager.getRecomendations();

                // Transformacion de los datos
                var RecList = oRecList.Select(o => new
                {
                    BuyCount = o.BuyCount,
                    BookId = o.books.Id,
                    BookTitle = o.books.Title,
                    BookAuthor = o.books.Author,
                    BookImage = Convert.ToBase64String(o.books.BookImage)
                });

                var jsonResult = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = 500000000,
                    Data = new { Recomendations = RecList }
                };

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
            
        }

        #endregion
    }
}
