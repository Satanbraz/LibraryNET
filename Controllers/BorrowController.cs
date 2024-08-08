using DTO;
using LibraryNET.Filters;
using LibraryNET.Implementation.AdminManagers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using ZXing;
using ZXing.QrCode.Internal;
using static System.Net.Mime.MediaTypeNames;

namespace LibraryNET.Controllers
{
    public class BorrowController : Controller
    {
        #region Propierties

        BorrowManager _borrowManager = new BorrowManager();
        GUID _guidManager = new GUID();

        #endregion


        // GET: Borrow
        public ActionResult Index()
        {
            return View();
        }

        // GET: Borrow/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Borrow/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Borrow/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Borrow/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        /// <summary>
        /// Modifica la fecha de retorno del préstamo o genera el retorno del préstamo.
        /// </summary>
        /// <param name="borrowDetail">Objeto de clase Detalle de Préstamo, contiene el detalle del elemento en préstamo</param>
        /// <param name="isReturn">Boleano que verifica si el préstamo será retornado</param>
        /// <returns>String de confirmación de procesamiento exitoso</returns>
        [Session]
        [RolPermission]
        [HttpPost]
        public String EditBorrowDetail(BorrowDetail borrowDetail, bool isReturn = false)
        {
            var result = _borrowManager.UpdateBorrowDetail(borrowDetail, isReturn);

            if(result == 1)
            {
                return "Ok";
            }
            else
            {
                return null;
            }
        }

        // GET: Borrow/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Borrow/Delete/5
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
        /// Obtiene los registro de préstamos del usuario.
        /// </summary>
        /// <returns>Listado de préstamos del usuario</returns>
        public ActionResult BorrowList()
        {
            var oListBorrows = _borrowManager.GetBorrows(Convert.ToInt32(Session["UserId"]));

            ViewBag.BorrowList = oListBorrows;

            return View(oListBorrows);
        }

        /// <summary>
        /// Obtiene los registro de todos los préstamos realizados.
        /// </summary>
        /// <returns>Listado de todos los préstamos</returns>
        [Session]
        [RolPermission]
        public ActionResult AdminBorrowList() 
        {
            var oBorrowList = _borrowManager.GetAllBorrows();

            return View("AdminBorrowList", oBorrowList);
        }

        /// <summary>
        /// Realiza el proceso de un préstamo.
        /// </summary>
        /// <param name="borrowModel">Objeto de clase Préstamo, contiene el registro del préstamo único</param>
        /// <param name="borrowDetailModel">Objeto de clase Detalle Préstamo, contiene el detalle del préstamo</param>
        /// <returns>String de confirmación de procesamiento exitoso</returns>
        public string ProcessBorrowRequest(Borrows borrowModel, List<BorrowDetail> borrowDetailModel)
        {
            DataTable borrowDetail = new DataTable();
            borrowDetail.Columns.Add("IdProduct", typeof(int));
            borrowDetail.Columns.Add("BorrowDate", typeof(DateTime));
            borrowDetail.Columns.Add("BorrowReturnDate", typeof(DateTime));

            foreach (var item in borrowDetailModel)
            {
                borrowDetail.Rows.Add(new Object[]
                {
                    item.BorrowProductId,
                    item.BorrowDate,
                    item.BorrowReturnDate
                });
            }

            borrowModel.IdTransaction = "BRID" +  _guidManager.GenerateTransactionId();
            borrowModel.ProductsQ = borrowDetailModel.Count();

            GenerateBarCode(borrowModel.IdTransaction);

            int result = _borrowManager.InsertBorrowRequest(borrowModel, borrowDetail);

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
        /// Genera un códgo de barra.
        /// </summary>
        /// <param name="borrowTransac">String que contiene el GUID de Préstamo</param>
        public static void GenerateBarCode(string borrowTransac) {

            //var split = borrowTransac.Split('-');
            //var borrowTrancToBarCode = split[0] + "/" + split[1];

            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Height = 200,
                    Width = 600
                }
            };

            Bitmap barcodeBitmap = barcodeWriter.Write(borrowTransac);

            // Construir la ruta relativa para la carpeta de imágenes
            string relativePath = Path.Combine("E:","ProjectsVS", "LibraryNET", "Content", "images", "BarCodes");

            // Verificar si la carpeta existe, si no, crearla
            if (!Directory.Exists(relativePath))
            {
                Directory.CreateDirectory(relativePath);
            }

            // Crear la ruta completa del archivo
            string filePath = Path.Combine(relativePath, $"{borrowTransac}.png");

            // Guardar el código de barras como imagen
            barcodeBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        }


    }
}
