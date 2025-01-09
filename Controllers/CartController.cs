using DTO;
using LibraryNET.Filters;
using LibraryNET.Implementation.AdminManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibraryNET.Controllers
{
    [Session]
    public class CartController : Controller
    {
        #region Propierties

        CartManager _cartManager = new CartManager();

        #endregion

        #region CartMethods

        // GET: Cart
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Obtiene los elementos del carito de compras.
        /// </summary>
        /// <returns>Lista de los elementos del carrito de compras</returns>
        public ActionResult CartDetail()
        {
            var oCartList = _cartManager.GetCartDetail(Convert.ToInt32(Session["UserId"]));

            ViewBag.BooksList = oCartList;

            return View("CartDetail", oCartList);
        }

        /// <summary>
        /// Añade un elemento al carrito de compras.
        /// </summary>
        /// <returns>String de confirmación de adición al carrito de compras</returns>
        [HttpPost]
        public string AddToCart(Cart cart)
        {
            var id = _cartManager.AddBookToCart(cart);

            if (id > 0)
            {

                return "Ok";
            }
            else
            {
                return null;
            }
        }

        // GET: Cart/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Cart/Edit/5
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

        // GET: Cart/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        /// <summary>
        /// Elimina un elemento del carrito de compras.
        /// </summary>
        /// <param name="idCart">Id del carrito de compras</param>
        /// <param name="idBook">Id del elemento a eliminar del carrito</param>
        /// <returns>String de confirmación de elminación de elemento del carrito de compras</returns>
        [HttpPost]
        public string DelFromCart(int idCart, int idBook)
        {
            var result = _cartManager.RemoveBookFromCart(idCart, idBook);

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
        /// Comprueba si hay un carrito de compras activo para el usuario.
        /// </summary>
        /// <returns>Resultado de confirmacion de carrito encontrado</returns>
        public int FindCart()
        {
            var result = _cartManager.VarifyCart(Convert.ToInt32(Session["UserId"]));

            if (result == 0)
            {
                return 0;
            }
            else
            {
                return result;
            }

            
        }

        /// <summary>
        /// Obtiene los elementos del carrito del usuario.
        /// </summary>
        /// <returns>Listado de elementos del carrito de compras del usuario</returns>
        public ActionResult QCart()
        {
            var cantidad = _cartManager.GetCartDetail(Convert.ToInt32(Session["UserId"])).Count();

            return Json(new { cantidad = cantidad }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region BorrowCartMethods

        /// <summary>
        /// Obtiene los elementos de la bolsa de préstamos.
        /// </summary>
        /// <returns>Lista de los elementos de la bolsa de préstamos</returns>
        public ActionResult BorrowCartDetail()
        {
            var oBorrowCartList = _cartManager.GetBorrowCartDetail(Convert.ToInt32(Session["UserId"]));

            ViewBag.BooksList = oBorrowCartList;

            return View("BorrowCartDetail", oBorrowCartList);
        }

        /// <summary>
        /// Añade un elemento a la bolsa de préstamos.
        /// </summary>
        /// <returns>String de confirmación de adición a la bolsa de préstamos</returns>
        [HttpPost]
        public string AddToBorrowCart(BorrowCart borrowCart)
            {
            var id = _cartManager.AddBookToBorrowCart(borrowCart);

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
        /// Elimina un elemento de la bolsa de préstamos.
        /// </summary>
        /// <param name="idCart">Id de la bolsa de préstamos</param>
        /// <param name="idBook">Id del elemento a eliminar de la bolsa de préstamos</param>
        /// <returns>String de confirmación de elminación de elemento de la bolsa de préstamos</returns>
        [HttpPost]
        public string DelFromBorrowCart(int idCart, int idBook)
        {
            var result = _cartManager.RemoveBookFromBorrowCart(idCart, idBook);

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
        /// Comprueba si hay una bolsa de préstamos activa para el usuario.
        /// </summary>
        /// <returns>Resultado de confirmacion de bolsa de préstamos encontrado</returns>
        public int FindBorrowCart()
        {
            var result = _cartManager.VarifyBorrowCart(Convert.ToInt32(Session["UserId"]));

            if (result == 0)
            {
                return 0;
            }
            else
            {
                return result;
            }


        }

        /// <summary>
        /// Comprueba si ya existe el elemento en la bolsa de préstamos activa para el usuario.
        /// </summary>
        /// <param name="idBook">Id del elemento de la bolsa de préstamos</param>
        /// <returns>Resultado de confirmacion de existencia en bolsa de préstamos</returns>
        [HttpPost]
        public int VerifyExistenceInBorrowCart(int idBook)
        {
            int result = 0;

            var cantidad = _cartManager.GetBorrowCartDetail(Convert.ToInt32(Session["UserId"]));
            if (cantidad.Any(cant => cant.Books.Id == idBook))
            {
                result = 1;
            }

            return result;
        }

        /// <summary>
        /// ACtualiza las fechas de préstamos de los elementos en la bolsa de préstamos.
        /// </summary>
        /// <param name="borrowCart">Objeto de clase Bolsa de Préstamos con los valores traidos de la bolsa de préstamos</param>
        /// <returns>String de confirmación de actualización de fechas del elemento de la bolsa de préstamos</returns>
        [HttpPost]
        public string ModifyBorrowCartDates(BorrowCart borrowCart)
        {
            var result = _cartManager.ModifyBorrowDates(borrowCart);

            if (result == 1)
            {
                return "Ok";
            }
            else
            {
                return null;
            }
        }

        #endregion

    }
}
