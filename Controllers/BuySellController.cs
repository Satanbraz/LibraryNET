using DTO;
using LibraryNET.Filters;
using LibraryNET.Implementation.AdminManagers;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZXing;

namespace LibraryNET.Controllers
{

    [Session]
    public class BuySellController : Controller
    {
        #region Propierties

        GUID _guidManager = new GUID();
        BuySellManager _buySellManager = new BuySellManager();

        private int ultimoNumeroBoleta = 0; // Variable para almacenar el último número de boleta generado

        #endregion

        #region SellMethods

        // GET: BuySell/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BuySell/Create
        public ActionResult Create()
        {
            return PartialView();
        }

        // POST: BuySell/Create
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

        // GET: BuySell/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BuySell/Edit/5
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

        // GET: BuySell/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BuySell/Delete/5
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
        /// Realiza el proceso de pago de una venta.
        /// </summary>
        /// <param name="sellModel">Objeto de clase Venta, contiene el registro de la venta unica</param>
        /// <param name="sellDetailModel">Objeto de clase Detalle Venta, contiene el detalle de la venta</param>
        /// <returns>String de confirmación de procesamiento exitoso</returns>
        public string ProcessSellPayment(Sell sellModel, List<SellDetail> sellDetailModel)
        {
            int sellSubtotal = 0;
            DataTable sellDetail = new DataTable();
            sellDetail.Columns.Add("IdProduct", typeof(int));
            sellDetail.Columns.Add("QProducto", typeof(int));
            sellDetail.Columns.Add("Total", typeof(int));

            foreach (var item in sellDetailModel)
            {
                int subtotal = item.BookPrice * item.SellBookQ;

                sellSubtotal += subtotal;

                sellDetail.Rows.Add(new Object[]
                {
                    item.SellProductId,
                    item.SellBookQ,
                    subtotal
                });
            }

            sellModel.TotalBruto = sellSubtotal;
            sellModel.IVA = (int)(sellSubtotal * 0.19);
            sellModel.TotalAmount = sellModel.TotalBruto + sellModel.IVA;
            sellModel.SellDate = DateTime.Now;
            sellModel.IdTransaction = "SID" + _guidManager.GenerateTransactionId();
            sellModel.ProductsQ = sellDetailModel.Count();

            int result = _buySellManager.InsertSell(sellModel, sellDetail);

            if (result == 1)
            {
                GenerarBoletaExcel(sellModel, sellDetailModel);
                return "Ok";
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region BuyMethods

        /// <summary>
        /// Realiza el proceso de pago de una compra.
        /// </summary>
        /// <param name="buyModel">Objeto de clase Compra, contiene el registro de la compra unica</param>
        /// <param name="buyDetailModel">Objeto de clase Detalle Compra, contiene el detalle de la compra</param>
        /// <returns>String de confirmación de procesamiento exitoso</returns>
        public string ProcessBuyPayment(Buy buyModel, List<BuyDetail> buyDetailModel)
        {
            int buySubtotal = 0;
            DataTable buyDetail = new DataTable();
            buyDetail.Columns.Add("IdProduct",typeof(int));
            buyDetail.Columns.Add("QProducto", typeof(int));
            buyDetail.Columns.Add("Total", typeof(int));

            foreach (var item in buyDetailModel)
            {
                int subtotal = item.BookPrice * item.BuyBookQ;

                buySubtotal += subtotal;

                buyDetail.Rows.Add(new Object[]
                {
                    item.BuyProductId,
                    item.BuyBookQ,
                    subtotal
                });
            }

            buyModel.TotalBruto = buySubtotal;
            buyModel.IVA = (int)(buySubtotal * 0.19);
            buyModel.TotalAmount = buyModel.TotalBruto + buyModel.IVA;
            buyModel.BuyDate = DateTime.Now;
            buyModel.IdTransaction = "BID" + _guidManager.GenerateTransactionId();
            buyModel.ProductsQ = buyDetailModel.Count();

            int result = _buySellManager.InsertBuy(buyModel, buyDetail);

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
        /// Obtiene la lista completa de los registros de Compras de usuario.
        /// </summary>
        /// <returns>Listado de compras</returns>
        public ActionResult BuyList()
        {
            var oListBuys = _buySellManager.GetBuys(Convert.ToInt32(Session["UserId"]));

            ViewBag.BuyList = oListBuys;

            return View(oListBuys);
        }

        #endregion

        /// <summary>
        /// Obtiene la lista completa de los registros de Compras y ventas.
        /// </summary>
        /// <returns>Listado de compras y ventas</returns>
        public ActionResult AdminBuySellList()
        {
            var oBuyList = _buySellManager.GetAllBuys();
            var oSellList = _buySellManager.GetAllSells();

            ViewBag.BuyList = oBuyList;
            ViewBag.SellList = oSellList;


            return View("AdminBuySellList");
        }


        #region Private Methods

        /// <summary>
        /// Genera boleta de venta.
        /// </summary>
        /// <param name="sellModel">Objeto de clase Compra, contiene el registro de la compra unica</param>
        /// <param name="sellDetailModel">Objeto de clase Detalle Compra, contiene el detalle de la compra</param>
        private void GenerarBoletaExcel(Sell sellModel, List<SellDetail> sellDetailModel)
        {
            // Generar número de boleta
            string numeroBoleta = GenerarNumeroBoleta();

            // Crear archivo Excel
            string relativePath = Path.Combine("E:", "ProjectsVS", "LibraryNET", "Content", "Boletas");
            string filePath = Path.Combine(relativePath, $"boleta_{sellModel.IdTransaction}.xlsx");

            // Establecer el contexto de licencia de EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            FileInfo file = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Boleta");

                // Configurar estilo de celda para encabezado
                worksheet.Cells["B2"].Value = "LIBRERÍA DIGITAL LibraryNET";
                worksheet.Cells["B2"].Style.Font.Bold = true;
                worksheet.Cells["B2"].Style.Font.Size = 18;
                worksheet.Cells["B2:F2"].Merge = true; // Combinar celdas para el encabezado
                worksheet.Cells["B2:F2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["C3"].Value = "-----------------------------------------------";
                worksheet.Cells["C3:E3"].Merge = true;
                worksheet.Cells[$"C3:E3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Información de la boleta
                worksheet.Cells["C4"].Value = $"Boleta N°: {numeroBoleta}";
                worksheet.Cells["C4:E4"].Merge = true;
                worksheet.Cells["C4:E4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells["C5"].Value = $"Fecha de Transacción: {sellModel.SellDate:yyyy-MM-dd}";
                worksheet.Cells["C5:E5"].Merge = true;
                worksheet.Cells["C5:E5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells["C6"].Value = $"RUT: {null}";
                worksheet.Cells["C6:E6"].Merge = true;
                worksheet.Cells["C7"].Value = "-----------------------------------------------";
                worksheet.Cells["C7:E7"].Merge = true;
                worksheet.Cells[$"C7:E7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Detalle de la transacción
                worksheet.Cells["C8"].Value = "Detalle de Venta:";
                worksheet.Cells["C8:E8"].Merge = true;
                worksheet.Cells[$"C8:E8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["C9"].Value = "N°";
                worksheet.Cells["C9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["D9"].Value = "Detalle del Producto";
                worksheet.Cells["E9"].Value = "Valor";

                int row = 10;
                int index = 1;
                for (int i = 0; i < sellDetailModel.Count; i++)
                {
                    var item = sellDetailModel[i];
                    var book = sellModel.bookList[i];

                    worksheet.Cells[$"C{row}"].Value = index;
                    worksheet.Cells[$"C{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[$"D{row}"].Value = $"{book.Title} -x{item.SellBookQ}";
                    worksheet.Cells[$"E{row}"].Value = item.BookPrice * item.SellBookQ;
                    worksheet.Cells[$"E{row}"].Style.Numberformat.Format = "$#,##0";

                    row++;
                    index++;
                }

                worksheet.Cells[$"C{row}"].Value = "-----------------------------------------------";
                worksheet.Cells[$"C{row}:E{row}"].Merge = true;
                worksheet.Cells[$"C{row}:E{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                row++;

                // Total de la transacción
                worksheet.Cells[$"D{row}"].Value = "Subtotal:";
                worksheet.Cells[$"D{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[$"E{row}"].Value = sellModel.TotalBruto;
                worksheet.Cells[$"E{row}"].Style.Numberformat.Format = "$#,##0";
                row++;
                worksheet.Cells[$"D{row}"].Value = "IVA (19%):";
                worksheet.Cells[$"D{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[$"E{row}"].Value = sellModel.IVA;
                worksheet.Cells[$"E{row}"].Style.Numberformat.Format = "$#,##0";
                row++;
                worksheet.Cells[$"D{row}"].Value = "Total:";
                worksheet.Cells[$"D{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[$"E{row}"].Value = sellModel.TotalAmount;
                worksheet.Cells[$"E{row}"].Style.Numberformat.Format = "$#,##0";
                row++;
                worksheet.Cells[$"C{row}"].Value = "-----------------------------------------------";
                worksheet.Cells[$"C{row}:E{row}"].Merge = true;
                worksheet.Cells[$"C{row}:E{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                row++;

                // Método de Pago
                worksheet.Cells[$"C{row}"].Value = $"Método de Pago: {null}";
                worksheet.Cells[$"C{row }:E{row}"].Merge = true;
                worksheet.Cells[$"C{row }:E{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[$"C{row + 1}"].Value = "-----------------------------------------------";
                worksheet.Cells[$"C{row + 1}:E{row + 1}"].Merge = true;
                worksheet.Cells[$"C{row + 1}:E{row + 1}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Agradecimiento
                worksheet.Cells[$"C{row + 2}"].Value = "¡Gracias por su compra!";
                worksheet.Cells[$"C{row + 2}:E{row + 2}"].Merge = true;
                worksheet.Cells[$"C{row + 2}:E{row + 2}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"C{row + 3}"].Value = "----------------------------------------------------------------";
                worksheet.Cells[$"C{row + 3}:E{row + 3}"].Merge = true;
                worksheet.Cells[$"C{row + 3}:E{row + 3}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // TyC
                worksheet.Cells[$"C{row + 4}"].Value = "- Política de Devolución: LibraryNET solo acepta devoluciones de libros dentro de los 30 días posteriores a la compra, siempre que presenten la boleta física, estén en su estado original y sin daños.\n" +
                    "Para iniciar el proceso de devolución, contacta a nuestro servicio de atención al cliente con tu número de pedido y motivo de la devolución.\n" +
                    "Los costos de envío de la devolución son responsabilidad del cliente, excepto en casos de error por nuestra parte.\n" +
                    "Una vez recibida y verificada la devolución, procesaremos el reembolso a través del método de pago original.";
                worksheet.Cells[$"C{row + 4}:E{row + 4}"].Merge = true;
                worksheet.Cells[$"C{row + 4}:E{row + 4}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Ajustar el alto de la fila según el contenido
                worksheet.Cells[$"C{row + 4}:E{row + 4}"].Style.WrapText = true;
                worksheet.Row(row + 4).Height = worksheet.Row(row + 4).Height * 15;

                // Warranty
                worksheet.Cells[$"C{row + 5}"].Value = "- Garantía: LibraryNET ofrece una garantía de 60 días para todos nuestros libros contra defectos de fabricación.\n" +
                    "Si descubres algún defecto en tu libro, por favor, presenta la boleta física y contacta a nuestro servicio de atención al cliente para iniciar un reclamo.\n" +
                    "Los libros defectuosos serán reemplazados sin costo adicional para el cliente.\n" +
                    "Si el reemplazo no está disponible, se ofrecerá un reembolso completo utilizando el método de pago original.";
                worksheet.Cells[$"C{row + 5}:E{row + 5}"].Merge = true;
                worksheet.Cells[$"C{row + 5}:E{row + 5}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                // Ajustar el alto de la fila según el contenido
                worksheet.Cells[$"C{row + 5}:E{row + 5}"].Style.WrapText = true;
                worksheet.Row(row + 5).Height = worksheet.Row(row + 5).Height * 14;

                // Info Boleta
                worksheet.Cells[$"C{row + 7}"].Value = "<------ Boleta valida como Ticket de Cambio ------>";
                worksheet.Cells[$"C{row + 7}:E{row + 7}"].Merge = true;
                worksheet.Cells[$"C{row + 7}:E{row + 7}"].Style.Font.Size = 9;
                worksheet.Cells[$"C{row + 7}:D{row + 7}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                row += 4;

                // Generar código de barras
                var barcodeWriter = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = 80,
                        Width = 300
                    }
                };
                var barcodeBitmap = barcodeWriter.Write(sellModel.IdTransaction);

                // Guardar el código de barras como imagen temporal
                string barcodePath = Path.Combine(relativePath, $"barcode_{sellModel.IdTransaction}.png");
                barcodeBitmap.Save(barcodePath, ImageFormat.Png);

                // Insertar el código de barras en el Excel
                ExcelPicture barcodePicture = worksheet.Drawings.AddPicture("Barcode", new FileInfo(barcodePath));
                barcodePicture.SetPosition(row + 4, 0, 2, 0); // Ajustar la posición según sea necesario

                

                // Ajustar el ancho de las columnas C, D y E
                worksheet.Column(3).AutoFit();
                worksheet.Column(4).AutoFit();
                worksheet.Column(5).AutoFit();

                // Aplicar bordes a todo el contenido de la boleta
                worksheet.Cells["B1:F1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"B1:B{row+10}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"F1:F{row+10}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"B{row+10}:F{row+10}"].Style.Border.Bottom.Style = ExcelBorderStyle.Dashed;



                // Guardar Excel
                package.Save();
            }

            // Guardar información en la base de datos
            // GuardarBoletaEnBD(numeroBoleta, sellModel, sellDetailModel);
        }

        /// <summary>
        /// Genera un numero de boleta.
        /// </summary>
        /// <returns>String con el número de boleta</returns>
        private string GenerarNumeroBoleta()
        {
            // Incrementar el contador para generar el siguiente número de boleta
            ultimoNumeroBoleta++;

            // Formato para generar el número de boleta: B20240000001, B20240000002, ...
            return $"B{DateTime.Now.Year.ToString("0000")}{ultimoNumeroBoleta.ToString("0000000")}";
        }

        private void GuardarBoletaEnBD(string numeroBoleta, Sell sellModel, List<SellDetail> sellDetailModel)
        {
            // Aquí puedes implementar el código para guardar la boleta en la base de datos
            // Simplemente inserta los datos en tu tabla de boletas
        }

        #endregion
    }
}
