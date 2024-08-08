using DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.Security.Cryptography;

namespace LibraryNET.Implementation.AdminManagers
{
    public class BuySellManager
    {
        //Agregar conexion a BD
        DbConnectionProvider dbc = new DbConnectionProvider();

        #region Propierties

        protected string TableName = "Buys";
        protected string Fields = "Id, IdUser, ProductsQ, PhoneUser, DirUser, IdTransaction, BuySubtotal, BuyIVA, BuyTotalAmount, BuyDate";

        #endregion

        #region Public Methods

            #region BuyMethods

            /// <summary>
            /// Ejecuta una instrucción SQL de inserción con parámetros, registrando una nueva compra.
            /// </summary>
            /// <param name="buy">Objeto de clase Buy, contiene el registro de la venta única traido desde el controlador</param>
            /// <param name="buyDetail">Objeto de clase BuyDetail, contiene el detalle de la venta traido desde el controlador</param>
            /// <returns>Entero de confirmación de procesamiento exitoso</returns>
            public int InsertBuy(Buy buy, DataTable buyDetail)
            {
                int result = 0;

                using (SqlConnection con = dbc.OpenConnection())
                {
                    SqlTransaction tran = con.BeginTransaction();

                    try
                    {
                        // Definir la query para insertar en Compra
                        string query = string.Format("INSERT INTO {0} ({1}) {2}",
                                        TableName,
                                        "IdUser, ProductsQ, PhoneUser, DirUser, IdTransaction, BuySubtotal, BuyIVA, BuyTotalAmount, BuyDate",
                                        "VALUES(@IdUser, @ProductsQ, @PhoneUser, @DirUser, @IdTransaction, @BuySubtotal, @BuyIVA, @BuyTotalAmount, @BuyDate)");

                        // Definir los paramatros de enviar
                        List<SqlParameter> colParameters = new List<SqlParameter>();
                        colParameters.Add(new SqlParameter("IdUser", buy.IdUser));
                        colParameters.Add(new SqlParameter("ProductsQ", buy.ProductsQ));
                        colParameters.Add(new SqlParameter("PhoneUser", buy.PhoneUser));
                        colParameters.Add(new SqlParameter("DirUser", buy.DirUser));
                        colParameters.Add(new SqlParameter("IdTransaction", buy.IdTransaction));
                        colParameters.Add(new SqlParameter("BuySubtotal", buy.TotalBruto));
                        colParameters.Add(new SqlParameter("BuyIVA", buy.IVA));
                        colParameters.Add(new SqlParameter("BuyTotalAmount", buy.TotalAmount));
                        colParameters.Add(new SqlParameter("BuyDate", DateTime.Now));


                        //Ejecutar el comando y obtener el resultado
                        dbc = new DbConnectionProvider();
                        int id = Convert.ToInt32(dbc.ExecuteReturningId(query, "Id", colParameters, TableName));

                        if (id > 0)
                        {
                            try
                            {
                                // Crear lista de BuyDetail
                                List<BuyDetail> buyDetailList = new List<BuyDetail>();

                                // Rellenar lista con registros
                                if ((buyDetail != null) && (buyDetail.Rows.Count > 0))
                                {
                                    try
                                    {
                                        for (int i = 0; i < buyDetail.Rows.Count; i++)
                                        {
                                            // Transformar las filas del DT en fila de BuyDetail
                                            BuyDetail buyDetailObj = ConvertDtRowToDTOBuy(buyDetail.Rows[i]);
                                            buyDetailObj.BuyId = id;
                                            buyDetailList.Add(buyDetailObj);

                                            // Valida la cantidad comprada de cada libro
                                            if (buyDetailObj.BuyBookQ > 1)
                                            {
                                                try
                                                {
                                                    result = updateBookStock(buyDetailObj.BuyBookQ, buyDetailObj.BuyProductId);
                                                }
                                                catch (Exception)
                                                {

                                                    throw;
                                                }
                                            }
                                            // Genera un registro en el Detalle de Compra
                                            result = InsertBuyDetail(buyDetailList[i]);
                                        }

                                        if (result == 1)
                                        {
                                            try
                                            {
                                                // Elimina el carrito generado al usuario
                                                result = delCart(buy.IdUser);
                                            }
                                            catch (Exception)
                                            {
                                                return result;
                                                throw;
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        return result;
                                        throw;
                                    }
                                }
                                else
                                {
                                    return result;
                                }
                            }
                            catch (Exception)
                            {
                                return result;
                                throw;
                            }
                        }
                        else
                        {
                            return result;
                        }
                    }
                    catch (Exception)
                    {
                        // Regresa todo en caso de error
                        tran.Rollback();
                        return result;
                        throw;
                    }

                }

                return result;
            }

            /// <summary>
            /// Ejecuta una instrucción SQL de inserción con parámetros, retornando la data tipo lista de los registro de compras del usuario.
            /// </summary>
            /// <param name="idUser">Parámetro que contiene el ID del usuario</param>
            /// <returns>Data tipo lista de todas las compras del usuario</returns>
            public IEnumerable<Buy> GetBuys(int idUser)
            {
                string query = string.Format(@"SELECT  
                                            A.Id,
		                                    A.IdTransaction,
		                                    a.BuySubtotal,
		                                    A.BuyIVA,
		                                    A.BuyTotalAmount,
                                            A.BuyDate,
		
		                                    B.Id as BDetId,
                                            B.BuyId,
		                                    B.ProductId,
		                                    B.QProduct,
		                                    B.ProductAmount,
		
		                                    C.BookTitle,
		                                    C.BookAuthor,
		                                    C.BookImage
		                                    FROM {0} as A 
		                                    LEFT JOIN BuyDetail as B on A.Id = B.BuyId
		                                    LEFT JOIN Books as C on C.Id = B.ProductId 
                                            WHERE A.IdUser = {1}",
                                            TableName,
                                            idUser);

                // Crear conexion y ejecutar la query
                dbc = new DbConnectionProvider();
                DataTable data = dbc.GetDataTable(query);

                // Crear lista de registros
                List<Buy> buyDetailList = new List<Buy>();

                // Rellenar lista con registros
                if ((data != null) && (data.Rows.Count > 0))
                {
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        buyDetailList.Add(ConvertDtRowToDTOBuyGet(data.Rows[i]));
                    }
                }

                return buyDetailList;
            }

            /// <summary>
            /// Ejecuta una instrucción SQL de lectura, retornando la data tipo lista de todas las compras.
            /// </summary>
            /// <returns>Data tipo lista de todas las compras registradas en BD</returns>
            public IEnumerable<Buy> GetAllBuys()
            {
                string query = string.Format(@"SELECT  
                                            A.Id,
		                                    A.IdTransaction,
		                                    a.BuySubtotal,
		                                    A.BuyIVA,
		                                    A.BuyTotalAmount,
                                            A.BuyDate,
		
		                                    B.Id as BDetId,
                                            B.BuyId,
		                                    B.ProductId,
		                                    B.QProduct,
		                                    B.ProductAmount,
		
		                                    C.BookTitle,
		                                    C.BookAuthor,
		                                    C.BookImage
		                                    FROM Buys as A 
		                                    LEFT JOIN BuyDetail as B on A.Id = B.BuyId
		                                    LEFT JOIN Books as C on C.Id = B.ProductId
                                            ORDER BY BuyDate DESC");

                // Crear conexion y ejecutar la query
                dbc = new DbConnectionProvider();
                DataTable data = dbc.GetDataTable(query);

                // Crear lista de registros
                List<Buy> buyDetailList = new List<Buy>();

                // Rellenar lista con registros
                if ((data != null) && (data.Rows.Count > 0))
                {
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        buyDetailList.Add(ConvertDtRowToDTOBuyGet(data.Rows[i]));
                    }
                }

                return buyDetailList;
            }

            #endregion

            #region SellMethods

            /// <summary>
            /// Ejecuta una instrucción SQL de inserción con parámetros, registrando una nueva venta.
            /// </summary>
            /// <param name="sell">Objeto de clase Sell, contiene el registro de la venta única traido desde el controlador</param>
            /// <param name="sellDetail">Objeto de clase SellDetail, contiene el detalle de la venta traido desde el controlador</param>
            /// <returns>Entero de confirmación de procesamiento exitoso</returns>
            public int InsertSell(Sell sell, DataTable sellDetail)
            {
                int result = 0;

                using (SqlConnection con = dbc.OpenConnection())
                {
                    SqlTransaction tran = con.BeginTransaction();

                    try
                    {
                        // Definir la query para insertar en Venta
                        string query = string.Format("INSERT INTO Sells ({0}) {1}",
                                        "IdUser, IdClient, ProductsQ, PhoneUser, DirUser, IdTransaction, SellSubtotal, SellIVA, SellTotalAmount, SellDate, RutUser",
                                        "VALUES(@IdUser, @IdClient, @ProductsQ, @PhoneUser, @DirUser, @IdTransaction, @SellSubtotal, @SellIVA, @SellTotalAmount, @SellDate, @RutUser)");

                        // Definir los paramatros de enviar
                        List<SqlParameter> colParameters = new List<SqlParameter>();
                        colParameters.Add(new SqlParameter("IdUser", sell.IdUser));
                        colParameters.Add(new SqlParameter("IdClient", sell.IdClient));
                        colParameters.Add(new SqlParameter("ProductsQ", sell.ProductsQ));
                        colParameters.Add(new SqlParameter("PhoneUser", sell.PhoneUser));
                        colParameters.Add(new SqlParameter("DirUser", sell.DirUser));
                        colParameters.Add(new SqlParameter("IdTransaction", sell.IdTransaction));
                        colParameters.Add(new SqlParameter("SellSubtotal", sell.TotalBruto));
                        colParameters.Add(new SqlParameter("SellIVA", sell.IVA));
                        colParameters.Add(new SqlParameter("SellTotalAmount", sell.TotalAmount));
                        colParameters.Add(new SqlParameter("SellDate", DateTime.Now));
                        colParameters.Add(new SqlParameter("RutUser", sell.RutClient));


                        //Ejecutar el comando y obtener el resultado
                        dbc = new DbConnectionProvider();
                        int id = Convert.ToInt32(dbc.ExecuteReturningId(query, "Id", colParameters, "Sells"));

                        if (id > 0)
                        {
                            try
                            {
                                // Crear lista de SellDetail
                                List<SellDetail> sellDetailList = new List<SellDetail>();

                                // Rellenar lista con registros
                                if ((sellDetail != null) && (sellDetail.Rows.Count > 0))
                                {
                                    try
                                    {
                                        for (int i = 0; i < sellDetail.Rows.Count; i++)
                                        {
                                            // Transformar las filas del DT en fila de SellDetail
                                            SellDetail sellDetailObj = ConvertDtRowToDTOSell(sellDetail.Rows[i]);
                                            sellDetailObj.SellId = id;
                                            sellDetailList.Add(sellDetailObj);

                                            // Valida la cantidad comprada de cada libro
                                            if (sellDetailObj.SellBookQ >= 1)
                                            {
                                                try
                                                {
                                                    result = updateBookStock(sellDetailObj.SellBookQ, sellDetailObj.SellProductId);


                                                    if (result == 1)
                                                    {
                                                        // Genera un registro en el Detalle de Venta
                                                        result = InsertSellDetail(sellDetailList[i]);
                                                    }
                                                }
                                                catch (Exception)
                                                {

                                                    throw;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        return result;
                                        throw;
                                    }
                                }
                                else
                                {
                                    return result;
                                }
                            }
                            catch (Exception)
                            {
                                return result;
                                throw;
                            }
                        }
                        else
                        {
                            return result;
                        }
                    }
                    catch (Exception)
                    {
                        // Regresa todo en caso de error
                        tran.Rollback();
                        return result;
                        throw;
                    }
                }
                return result;
            }

            /// <summary>
            /// Obtiene los registro de ventas del usuario.
            /// </summary>
            /// <param name="idUser">Parámetro que contiene el ID del usuario</param>
            /// <returns>Data tipo lista de todas las ventas del usuario</returns>
            public IEnumerable<Sell> GetSells(int idUser)
            {
                string query = string.Format(@"SELECT  
                                            A.Id,
		                                    A.IdTransaction,
		                                    a.SellSubtotal,
		                                    A.SellIVA,
		                                    A.SellTotalAmount,
                                            A.SellDate,
		
		                                    B.Id as SDetId,
                                            B.SellId,
		                                    B.ProductId,
		                                    B.QProduct,
		                                    B.ProductAmount,
		
		                                    C.BookTitle,
		                                    C.BookAuthor,
		                                    C.BookImage
		                                    FROM Sells as A 
		                                    LEFT JOIN SellDetail as B on A.Id = B.SellId
		                                    LEFT JOIN Books as C on C.Id = B.ProductId 
                                            WHERE A.IdUser = {0}",
                                            idUser);

                // Crear conexion y ejecutar la query
                dbc = new DbConnectionProvider();
                DataTable data = dbc.GetDataTable(query);

                // Crear lista de registros
                List<Sell> sellDetailList = new List<Sell>();

                // Rellenar lista con registros
                if ((data != null) && (data.Rows.Count > 0))
                {
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        sellDetailList.Add(ConvertDtRowToDTOSellGet(data.Rows[i]));
                    }
                }

                return sellDetailList;
            }

            /// <summary>
            /// Ejecuta una instrucción SQL de lectura, retornando la data tipo lista de todas las ventas.
            /// </summary>
            /// <returns>Data tipo lista de todas las ventas registradas en BD</returns>
            public IEnumerable<Sell> GetAllSells()
            {
                string query = string.Format(@"SELECT  
                                            A.Id,
		                                    A.IdTransaction,
		                                    a.SellSubtotal,
		                                    A.SellIVA,
		                                    A.SellTotalAmount,
                                            A.SellDate,
		
		                                    B.Id as SDetId,
                                            B.SellId,
		                                    B.ProductId,
		                                    B.QProduct,
		                                    B.ProductAmount,
		
		                                    C.BookTitle,
		                                    C.BookAuthor,
		                                    C.BookImage
		                                    FROM Sells as A 
		                                    LEFT JOIN SellDetail as B on A.Id = B.SellId
		                                    LEFT JOIN Books as C on C.Id = B.ProductId
                                            ORDER BY SellDate DESC");

                // Crear conexion y ejecutar la query
                dbc = new DbConnectionProvider();
                DataTable data = dbc.GetDataTable(query);

                // Crear lista de registros
                List<Sell> sellDetailList = new List<Sell>();

                // Rellenar lista con registros
                if ((data != null) && (data.Rows.Count > 0))
                {
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        sellDetailList.Add(ConvertDtRowToDTOSellGet(data.Rows[i]));
                    }
                }

                return sellDetailList;
            }

            #endregion

        #endregion

        #region Protected Methods

        /// <summary>
        /// Convierte un DataTable en un objeto de clase BuyDetail.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase BuyDetail llenado con valores del DataTable</returns>
        protected BuyDetail ConvertDtRowToDTOBuy(DataRow data)
        {
            BuyDetail buyDetail = new BuyDetail
            {
                BuyProductId = (int)data["IdProduct"],
                BuyBookQ = (int)data["QProducto"],
                BookPrice = (int)data["Total"],
                BuyTotal = (int)data["Total"] * (int)data["QProducto"]
            };

            return buyDetail;
        }

        /// <summary>
        /// Convierte un DataTable en un objeto de clase SellDetail.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase SellDetail llenado con valores del DataTable</returns>
        protected SellDetail ConvertDtRowToDTOSell(DataRow data)
        {
            SellDetail sellDetail = new SellDetail
            {
                SellProductId = (int)data["IdProduct"],
                SellBookQ = (int)data["QProducto"],
                BookPrice = (int)data["Total"],
                SellTotal = (int)data["Total"] * (int)data["QProducto"]
            };

            return sellDetail;
        }

        /// <summary>
        /// Convierte un DataTable en un objeto de clase Buy.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase Buy llenado con valores del DataTable</returns>
        protected Buy ConvertDtRowToDTOBuyGet(DataRow data)
        {
            Buy buyDetail = new Buy
            {
                Id = (int)data["Id"],
                IdTransaction = data["IdTransaction"].ToString(),
                TotalBruto = (int)data["BuySubtotal"],
                IVA = (int)data["BuyIVA"],
                TotalAmount = (int)data["BuyTotalAmount"],
                BuyDate = DateTime.Parse(data["BuyDate"].ToString()),
                    buyDetail = new BuyDetail()
                    {
                        Id = (int)data["BDetId"],
                        BuyId = (int)data["BuyId"],
                        BuyProductId = (int)data["ProductId"],
                        BuyBookQ = (int)data["QProduct"],
                        BookPrice = (int)data["ProductAmount"] / (int)data["QProduct"]
                    },
                    books = new Books()
                    {
                        Title = data["BookTitle"].ToString(),
                        Author = data["BookAuthor"].ToString(),
                        BookImage = Convert.FromBase64String(data["BookImage"].ToString())
                    }
            };

            return buyDetail;
        }

        /// <summary>
        /// Convierte un DataTable en un objeto de clase Sell.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase Sell llenado con valores del DataTable</returns>
        protected Sell ConvertDtRowToDTOSellGet(DataRow data)
        {
            Sell sellDetail = new Sell
            {
                Id = (int)data["Id"],
                IdTransaction = data["IdTransaction"].ToString(),
                TotalBruto = (int)data["SellSubtotal"],
                IVA = (int)data["SellIVA"],
                TotalAmount = (int)data["SellTotalAmount"],
                SellDate = DateTime.Parse(data["SellDate"].ToString()),
                sellDetail = new SellDetail()
                {
                    Id = (int)data["SDetId"],
                    SellId = (int)data["SellId"],
                    SellProductId = (int)data["ProductId"],
                    SellBookQ = (int)data["QProduct"],
                    BookPrice = (int)data["ProductAmount"] / (int)data["QProduct"]
                },
                books = new Books()
                {
                    Title = data["BookTitle"].ToString(),
                    Author = data["BookAuthor"].ToString(),
                    BookImage = Convert.FromBase64String(data["BookImage"].ToString())
                }
            };

            return sellDetail;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de inserción con parámetros, registrando un nuevo detalle de compra.
        /// </summary>
        /// <param name="buyDetail">Objeto de clase BuyDetail, contiene el detalle del préstamo traido desde el controlador</param>
        protected int InsertBuyDetail(BuyDetail buyDetail)
        {
            int result = 0;

            try
            {
                // Definir la query
                string query = string.Format("INSERT INTO BuyDetail ({0}) {1}",
                                "BuyId, ProductId, QProduct, ProductAmount",
                                "VALUES(@BuyId, @ProductId, @QProduct, @ProductAmount)");

                // Definir los paramatros de enviar
                List<SqlParameter> colParameters = new List<SqlParameter>();
                colParameters.Add(new SqlParameter("BuyId", buyDetail.BuyId));
                colParameters.Add(new SqlParameter("ProductId", buyDetail.BuyProductId));
                colParameters.Add(new SqlParameter("QProduct", buyDetail.BuyBookQ));
                colParameters.Add(new SqlParameter("ProductAmount", buyDetail.BookPrice));


                //Ejecutar el comando y obtener el resultado
                dbc = new DbConnectionProvider();

                int id = Convert.ToInt32(dbc.ExecuteReturningId(query, "Id", colParameters, "BuyDetail"));

                if (id >= 1)
                {
                    result = 1;
                }
            }
            catch (Exception)
            {
                return result;
                throw;
            }

            return result;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de inserción con parámetros, registrando un nuevo detalle de venta.
        /// </summary>
        /// <param name="sellDetail">Objeto de clase SellDetail, contiene el detalle del préstamo traido desde el controlador</param>
        protected int InsertSellDetail(SellDetail sellDetail)
        {
            int result = 0;

            try
            {
                // Definir la query
                string query = string.Format("INSERT INTO SellDetail ({0}) {1}",
                                "SellId, ProductId, QProduct, ProductAmount",
                                "VALUES(@SellId, @ProductId, @QProduct, @ProductAmount)");

                // Definir los paramatros de enviar

                List<SqlParameter> colParameters = new List<SqlParameter>();
                colParameters.Add(new SqlParameter("SellId", sellDetail.SellId));
                colParameters.Add(new SqlParameter("ProductId", sellDetail.SellProductId));
                colParameters.Add(new SqlParameter("QProduct", sellDetail.SellBookQ));
                colParameters.Add(new SqlParameter("ProductAmount", sellDetail.BookPrice));


                //Ejecutar el comando y obtener el resultado
                dbc = new DbConnectionProvider();
                
                int id = Convert.ToInt32(dbc.ExecuteReturningId(query, "Id", colParameters, "SellDetail"));

                if (id >= 1)
                {
                    result = 1;
                }
            }
            catch (Exception)
            {
                return result;
                throw;
            }

            return result;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de eliminación con parámetros, borrando el carrito de compras del usuario.
        /// </summary>
        /// <param name="idUser">Parámetro que contiene el ID del usuario</param>
        /// <returns>Entero de confirmación de procesamiento exitoso</returns>
        protected int delCart(int idUser)
        {
            int result = 0;
            try
            {
                //Definir el comando
                string command = string.Format("DELETE FROM Cart WHERE IdUser = {0}",
                                               "@IdUser");

                //Definir los parámetros
                List<SqlParameter> colParameters = new List<SqlParameter>();
                colParameters.Add(new SqlParameter("IdUser", idUser));

                //Ejecutar el comando
                DbConnectionProvider tdBase = new DbConnectionProvider();
                tdBase.Execute(command, colParameters);
                result = 1;
            }
            catch (Exception ex)
            {
                return result;
                throw;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de actualización con parámetros, actualizando el stock del libro en BD.
        /// </summary>
        /// <param name="bookQ">Parámetro que contiene la cantidad total del libro en el carrito</param>
        /// <param name="bookId">Parámetro que contiene ID del libro seleccionado</param>
        // Metodo para actualizar el stock del libro comprado
        protected int updateBookStock(int bookQ, int bookId)
        {
            int result = 0;

            try
            {
                //Definir el comando
                string command = string.Format("UPDATE Books SET {0} WHERE Id = {1}",
                                               "BookCant = BookCant - @BookCant",
                                               "@Id");

                //Definir los parámetros
                List<SqlParameter> colParameters = new List<SqlParameter>();
                colParameters.Add(new SqlParameter("BookCant", bookQ));
                colParameters.Add(new SqlParameter("Id", bookId));

                //Ejecutar el comando
                DbConnectionProvider tdBase = new DbConnectionProvider();
                tdBase.Execute(command, colParameters);

                result = 1;
            }
            catch (Exception)
            {
                return result;
                throw;
            }

            return result;
        }

        #endregion

    }
}