using DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace LibraryNET.Implementation.AdminManagers
{
    public class BorrowManager
    {
        //Agregar conexion a BD
        DbConnectionProvider dbc = new DbConnectionProvider();

        #region Propierties

        protected string TableName = "Borrows";
        protected string Fields = "Id, UserId, IdTransaction, ProductsQ, UserPhone, DirUser, BorrowRegisterDate";

        #endregion

        #region Public Methods

        /// <summary>
        /// Ejecuta una instrucción SQL de inserción con parámetros, registrando una nueva solicitud de préstamo.
        /// </summary>
        /// <param name="borrow">Objeto de clase Borrow, contiene el registro del préstamo único traido desde el controlador</param>
        /// <param name="borrowDetail">Objeto de clase BorrowDetail, contiene el detalle del préstamo traido desde el controlador</param>
        /// <returns>Entero de confirmación de procesamiento exitoso</returns>
        public int InsertBorrowRequest(Borrows borrow, DataTable borrowDetail)
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
                                    "UserId, IdClient, IdTransaction, ProductsQ, UserPhone, DirUser, RutUser, BorrowRegisterDate",
                                    "VALUES(@UserId, @IdClient, @IdTransaction, @ProductsQ, @UserPhone, @DirUser, @RutUser, @BorrowRegisterDate)");

                    // Definir los paramatros de enviar
                    List<SqlParameter> colParameters = new List<SqlParameter>();
                    colParameters.Add(new SqlParameter("UserId", borrow.IdUser));
                    colParameters.Add(new SqlParameter("IdClient", borrow.IdClient));
                    colParameters.Add(new SqlParameter("IdTransaction", borrow.IdTransaction));
                    colParameters.Add(new SqlParameter("ProductsQ", borrow.ProductsQ));
                    colParameters.Add(new SqlParameter("UserPhone", borrow.PhoneUser));
                    colParameters.Add(new SqlParameter("DirUser", borrow.DirUser));
                    colParameters.Add(new SqlParameter("RutUser", borrow.UserRut));
                    colParameters.Add(new SqlParameter("BorrowRegisterDate", DateTime.Now));


                    //Ejecutar el comando y obtener el resultado
                    dbc = new DbConnectionProvider();
                    int id = Convert.ToInt32(dbc.ExecuteReturningId(query, "Id", colParameters, TableName));

                    if (id > 0)
                    {
                        try
                        {
                            // Crear lista de BorrowDetail
                            List<BorrowDetail> borrowDetailList = new List<BorrowDetail>();

                            // Rellenar lista con registros
                            if ((borrowDetail != null) && (borrowDetail.Rows.Count > 0))
                            {
                                try
                                {
                                    for (int i = 0; i < borrowDetail.Rows.Count; i++)
                                    {
                                        // Transformar las filas del DT en fila de BorrowDetail
                                        BorrowDetail borrowDetailObj = ConvertDtRowToDTO(borrowDetail.Rows[i]);
                                        borrowDetailObj.BorrowId = id;
                                        borrowDetailObj.isActive = true;
                                        borrowDetailList.Add(borrowDetailObj);

                                        // Genera un registro en el Detalle de Préstamo
                                        result = InsertBorrowDetail(borrowDetailList[i]);
                                    }

                                    if (result == 1)
                                    {
                                        try
                                        {
                                            // Elimina el carrito generado al usuario
                                            result = delBorrowCart(borrow.IdUser);
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
        /// Ejecuta una instrucción SQL de actualización con parámetros, actualizando tabla BorrowDetail y Books segun corresponda.
        /// </summary>
        /// <param name="borrowDetail">Objeto de clase BorrowDetail, contiene el detalle del elemento en préstamo</param>
        /// <param name="isReturn">Boleano que verifica si el préstamo será retornado</param>
        /// <returns>Entero de confirmación de procesamiento exitoso</returns>
        public int UpdateBorrowDetail(BorrowDetail borrowDetail, bool isReturn)
        {
            var result = 0;

            List<SqlParameter> colParameters = new List<SqlParameter>();
            DbConnectionProvider tdBase = new DbConnectionProvider();

            if (isReturn == true)
            {
                try
                {
                    //Definir el comando
                    string command = string.Format(@"UPDATE BorrowDetail SET {0} WHERE Id = {1} AND BorrowId = {2} AND ProductId = {3}",
                                                    "BorrowReturnedDate = @BorrowReturnedDate, isActive = @isActive",
                                                    "@Id",
                                                    "@BorrowId",
                                                    "@ProductId");

                    //Definir los parámetros
                    colParameters = new List<SqlParameter>();
                    colParameters.Add(new SqlParameter("BorrowReturnedDate", DateTime.Now));
                    colParameters.Add(new SqlParameter("isActive", Convert.ToBoolean(borrowDetail.isActive)));
                    colParameters.Add(new SqlParameter("Id", borrowDetail.Id));
                    colParameters.Add(new SqlParameter("BorrowId", borrowDetail.BorrowId));
                    colParameters.Add(new SqlParameter("ProductId", borrowDetail.BorrowProductId));

                    //Ejecutar el comando
                    tdBase = new DbConnectionProvider();
                    tdBase.Execute(command, colParameters);

                    try
                    {
                        string query = string.Format("UPDATE Books SET {0} WHERE Id = {1}",
                                                        "BookCant = BookCant + 1",
                                                        "@Id");

                        //Definir los parámetros
                        colParameters = new List<SqlParameter>();
                        colParameters.Add(new SqlParameter("Id", borrowDetail.BorrowProductId));

                        //Ejecutar el comando
                        tdBase = new DbConnectionProvider();
                        tdBase.Execute(query, colParameters);

                        result = 1;
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    //Definir el comando
                    string command = string.Format("UPDATE BorrowDetail SET {0} WHERE Id = {1} AND BorrowId = {2} AND ProductId = {3}",
                                                    "BorrowReturnDate = @BorrowReturnDate",
                                                    "@Id",
                                                    "@BorrowId",
                                                    "@ProductId");


                    //Definir los parámetros
                    colParameters = new List<SqlParameter>();
                    colParameters.Add(new SqlParameter("BorrowReturnDate", borrowDetail.BorrowReturnDate));
                    colParameters.Add(new SqlParameter("Id", borrowDetail.Id));
                    colParameters.Add(new SqlParameter("BorrowId", borrowDetail.BorrowId));
                    colParameters.Add(new SqlParameter("ProductId", borrowDetail.BorrowProductId));

                    //Ejecutar el comando
                    tdBase = new DbConnectionProvider();
                    tdBase.Execute(command, colParameters);

                    result = 1;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura con parámetros, retornando la lista de préstamos de un usuario.
        /// </summary>
        /// <param name="idUser">Parámetro que contiene el ID del usuario</param>
        /// <returns>Data tipo lista de los préstamos encontrados en BD para el usuario</returns>
        public IEnumerable<Borrows> GetBorrows(int idUser)
        {
            string query = string.Format(@"SELECT  
                                        A.Id,
		                                A.IdTransaction,
                                        A.BorrowRegisterDate,
		
		                                B.Id as BRDetId,
                                        B.BorrowId,
		                                B.ProductId,
                                        B.BorrowDate,
                                        B.BorrowReturnDate,
                                        B.BorrowReturnedDate,
                                        B.isActive,
		
		                                C.BookTitle,
		                                C.BookAuthor,
		                                C.BookImage
		                                FROM Borrows as A 
		                                LEFT JOIN BorrowDetail as B on A.Id = B.BorrowId
		                                LEFT JOIN Books as C on C.Id = B.ProductId 
                                        WHERE A.IdClient = {0}",
                                        idUser);

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            // Crear lista de registros
            List<Borrows> buyDetailList = new List<Borrows>();

            // Rellenar lista con registros
            if ((data != null) && (data.Rows.Count > 0))
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    buyDetailList.Add(ConvertDtRowToDTOGet(data.Rows[i]));
                }
            }

            return buyDetailList;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura, retornando todos los préstamos.
        /// </summary>
        /// <returns>Data tipo lista de los todos los préstamos registrados en BD</returns>
        public IEnumerable<Borrows> GetAllBorrows()
        {
            string query = string.Format(@"SELECT  
                                        A.Id,
		                                A.IdTransaction,
                                        A.BorrowRegisterDate,
		
		                                B.Id as BRDetId,
                                        B.BorrowId,
		                                B.ProductId,
                                        B.BorrowDate,
                                        B.BorrowReturnDate,
                                        B.BorrowReturnedDate,
                                        B.isActive,
		
		                                C.BookTitle,
		                                C.BookAuthor,
		                                C.BookImage
		                                FROM Borrows as A 
		                                LEFT JOIN BorrowDetail as B on A.Id = B.BorrowId
		                                LEFT JOIN Books as C on C.Id = B.ProductId
                                        ORDER BY BorrowRegisterDate DESC");

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            // Crear lista de registros
            List<Borrows> borrowDetailList = new List<Borrows>();

            // Rellenar lista con registros
            if ((data != null) && (data.Rows.Count > 0))
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    borrowDetailList.Add(ConvertDtRowToDTOGet(data.Rows[i]));
                }
            }

            return borrowDetailList;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Convierte un DataTable en un objeto de clase BorrowDetail.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase BorrowDetail llenado con valores del DataTable</returns>
        protected BorrowDetail ConvertDtRowToDTO(DataRow data)
        {
            BorrowDetail buyDetail = new BorrowDetail
            {
                BorrowProductId = (int)data["IdProduct"],
                BorrowDate = DateTime.Parse(data["BorrowDate"].ToString()),
                BorrowReturnDate = DateTime.Parse(data["BorrowReturnDate"].ToString())
            };

            return buyDetail;
        }

        /// <summary>
        /// Convierte un DataTable en un objeto de clase Borrows.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase Borrows llenado con valores del DataTable</returns>
        protected Borrows ConvertDtRowToDTOGet(DataRow data)
        {
            Borrows borrowDetail = new Borrows();

            DateTime? fecharetorno;

            if (Convert.IsDBNull(data["BorrowReturnedDate"]))
            {
                fecharetorno = null;
            }
            else
            {
                fecharetorno = Convert.ToDateTime(data["BorrowReturnedDate"]);
            };

            borrowDetail.Id = (int)data["Id"];
            borrowDetail.IdTransaction = data["IdTransaction"].ToString();
            borrowDetail.BorrowRegisterDate = DateTime.Parse(data["BorrowRegisterDate"].ToString());
            borrowDetail.borrowDetail = new BorrowDetail()
            {
                Id = (int)data["BRDetId"],
                BorrowId = (int)data["BorrowId"],
                BorrowProductId = (int)data["ProductId"],
                BorrowDate = DateTime.Parse(data["BorrowDate"].ToString()),
                BorrowReturnDate = DateTime.Parse(data["BorrowReturnDate"].ToString()),
                BorrowReturnedDate = fecharetorno,
                isActive = (bool)data["isActive"],
            };
            borrowDetail.books = new Books()
            {
                Title = data["BookTitle"].ToString(),
                Author = data["BookAuthor"].ToString(),
                BookImage = Convert.FromBase64String(data["BookImage"].ToString())
            };
            

            return borrowDetail;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de inserción con parámetros, registrando un nuevo detalle de préstamo.
        /// </summary>
        /// <param name="borrowDetail">Objeto de clase BorrowDetail, contiene el detalle del préstamo traido desde el controlador</param>
        /// <returns>Entero de confirmación de procesamiento exitoso</returns>
        protected int InsertBorrowDetail(BorrowDetail borrowDetail)
        {
            int result = 0;

            try
            {
                // Definir la query
                string query = string.Format("INSERT INTO BorrowDetail ({0}) {1}",
                                "BorrowId, ProductId, BorrowDate, BorrowReturnDate, isActive",
                                "VALUES(@BorrowId, @ProductId, @BorrowDate, @BorrowReturnDate, @isActive)");

                // Definir los paramatros de enviar
                List<SqlParameter> colParameters = new List<SqlParameter>();
                colParameters.Add(new SqlParameter("BorrowId", borrowDetail.BorrowId));
                colParameters.Add(new SqlParameter("ProductId", borrowDetail.BorrowProductId));
                colParameters.Add(new SqlParameter("BorrowDate", DateTime.Parse(borrowDetail.BorrowDate.ToLongDateString())));
                colParameters.Add(new SqlParameter("BorrowReturnDate", DateTime.Parse(borrowDetail.BorrowReturnDate.ToLongDateString())));
                colParameters.Add(new SqlParameter("isActive", borrowDetail.isActive));

                //Ejecutar el comando y obtener el resultado
                dbc = new DbConnectionProvider();
                int id = Convert.ToInt32(dbc.ExecuteReturningId(query, "Id", colParameters, TableName));
                result = 1;
            }
            catch (Exception)
            {
                return result;
                throw;
            }

            return result;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de eliminación con parámetros, borrando el carrito de préstamos del usuario.
        /// </summary>
        /// <param name="idUser">Parámetro que contiene el ID del usuario</param>
        /// <returns>Entero de confirmación de procesamiento exitoso</returns>
        protected int delBorrowCart(int idUser)
        {
            int result = 0;
            try
            {
                //Definir el comando
                string command = string.Format("DELETE FROM BorrowCart WHERE IdUser = {0}",
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

        #endregion

    }
}