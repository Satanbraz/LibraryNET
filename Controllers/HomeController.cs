using DTO;
using LibraryNET.Filters;
using LibraryNET.Implementation;
using LibraryNET.Implementation.AdminManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace LibraryNET.Controllers
{
    [Session]
    public class HomeController : Controller
    {
        #region Propierties

        GenreManager _genreManager = new GenreManager();
        UserManager _userManager = new UserManager();
        BooksManager _booksManager = new BooksManager();
        EditManager _editManager = new EditManager();
        BuySellManager _buySellManager = new BuySellManager();
        BorrowManager _borrowManager = new BorrowManager();
        HomeManager _homeManager = new HomeManager();

        #endregion


        #region HomeMethods
        public ActionResult Index()
        {
            if (Session["UserId"] != null) {

                // Contadores
                int GenresCount = _genreManager.List().Count();
                int UsersCount = _userManager.List().Count();
                int BooksCount = _booksManager.ItemsCount();
                int EditsCount = _editManager.List().Count();
                int UserBuysCount = _buySellManager.GetBuys(Convert.ToInt32(Session["UserId"])).GroupBy(m => m.IdTransaction).Count();
                int UserBorrowsCount = _borrowManager.GetBorrows(Convert.ToInt32(Session["UserId"])).GroupBy(m => m.IdTransaction).Count();
                int TotalUsersBuysCount = _buySellManager.GetAllBuys().GroupBy(a => a.IdTransaction).Count();
                int TotalUsersBorrowsCount = _borrowManager.GetAllBorrows().GroupBy(b => b.IdTransaction).Count();
                int SellsCount = _buySellManager.GetAllSells().GroupBy(c => c.IdTransaction).Count();
                
                // Data
                var BookList = _booksManager.TotalList();

                ViewBag.GenreCount = GenresCount;
                ViewBag.UsersCount = UsersCount;
                ViewBag.BooksCount = BooksCount;
                ViewBag.EditsCount = EditsCount;
                ViewBag.BuysCount = UserBuysCount;
                ViewBag.BorrowsCount = UserBorrowsCount;
                ViewBag.TotalBuysCount = TotalUsersBuysCount;
                ViewBag.TotalBorrowsCount = TotalUsersBorrowsCount;
                ViewBag.TotalSellsCount = SellsCount;
                ViewBag.AllBooks = BookList;
                ViewBag.IsHomePage = true;

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Obtiene las últimas transacciones realizadas por usuarios en la vista de inicio de Administrador.
        /// </summary>
        /// <param name="page">Numero de pagina actual</param>
        /// <returns>Registro de las transacciones en formato JSON</returns>
        public JsonResult LastTransactionsData(int page = 1)
        {
            int PageSize = 5;

            try
            {
                // Obtener datos paginados aquí

                int buyCount = _buySellManager.GetAllBuys().GroupBy(a => a.IdTransaction).Count();
                int borrowCount = _borrowManager.GetAllBorrows().GroupBy(b => b.IdTransaction).Count();

                int totalCount = buyCount + borrowCount;
                var TrancData = _homeManager.GetRegisterData(page, PageSize);
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                // Transformacion de los datos
                var orders = TrancData.Select(o => new {
                    TrancId = o.TrancId,
                    TrancUserName = o.TrancUserName,
                    TrancType = o.TrancType,
                    TrancDate = o.TrancDate.ToString("MM-dd-yyyy HH:mm:ss"), // Formatear la fecha aquí
                    TrancAmount = o.TrancAmount
                });

                return Json(new { Orders = orders, CurrentPage = page, TotalPages = totalPages }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Manejar excepciones aquí, loguearlas, etc.
                return Json(new { Error = ex.Message });
            }

        }

        #endregion
    }
}