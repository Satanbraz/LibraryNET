using DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Security.Cryptography;

namespace LibraryNET.Implementation.AdminManagers
{
    public class CartManager
    {
        //Agregar conexion a BD
        DbConnectionProvider dbc = new DbConnectionProvider();

        #region Propierties

        BooksManager booksManager = new BooksManager();

        #endregion

        #region Public Methods

            #region CartMethods

            /// <summary>
            /// Ejecuta una instrucción SQL de inserción con parámetros, agregando un libro al carrito de compras.
            /// </summary>
            /// <param name="cart">Objeto de clase Cart, contiene la data del libro traido desde el controlador</param>
            /// <returns>Entero de confirmación de procesamiento exitoso</returns>
            public int AddBookToCart(Cart cart)
            {
                int result = 0;

                //Comprobar que no exista el libro en el carrito.
                //Definir la query   
                string query = string.Format(@"SELECT 
                                            Id, IdUser, IdBook 
                                            FROM Cart WHERE IdBook = {0} AND IdUser = {1}",
                                            "@IdBook",
                                            "@IdUser");

                //Definir los parámetros
                List<SqlParameter> colParameters = new List<SqlParameter>();
                colParameters.Add(new SqlParameter("IdBook", cart.IdProduct));
                colParameters.Add(new SqlParameter("IdUser", cart.IdUser));

                //Ejecutar la query y obtener el resultado
                DbConnectionProvider tdBase = new DbConnectionProvider();
                DataTable data = tdBase.GetDataTable(query, colParameters);

                //Convertir y retornar la data
                if ((data == null) || (data.Rows.Count == 0))
                {
                    try
                    {
                        //Definir el comando
                        string command = string.Format("UPDATE Books SET {0} WHERE Id = {1}",
                                                       "BookCant = BookCant - 1",
                                                       "@Id");

                        //Definir los parámetros
                        colParameters = new List<SqlParameter>();
                        colParameters.Add(new SqlParameter("Id", cart.IdProduct));

                        //Ejecutar el comando
                        tdBase = new DbConnectionProvider();
                        tdBase.Execute(command, colParameters);

                        try {
                            //Comprobar que no exista el libro en el carrito.
                            //Definir la query   
                            query = string.Format("INSERT INTO Cart ({0}) {1}",
                                                         "IdBook, IdUser",
                                                         "VALUES(@IdBook, @IdUser)");

                            //Definir los parámetros
                            colParameters = new List<SqlParameter>();
                            colParameters.Add(new SqlParameter("IdBook", cart.IdProduct));
                            colParameters.Add(new SqlParameter("IdUser", cart.IdUser));

                            //Ejecutar el comando
                            tdBase = new DbConnectionProvider();
                            tdBase.Execute(query, colParameters);

                            result = 1;
                        }
                        catch {
                    
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                return result;
            }

            /// <summary>
            /// Ejecuta una instrucción SQL de eliminación con parámetros, borrando el libro del carrito.
            /// </summary>
            /// <param name="idCart">Parámetro que contiene el ID del carrito</param>
            /// <param name="bookId">Parámetro que contiene el ID del libro</param>
            /// <returns>Entero de confirmación de procesamiento exitoso</returns>
            public int RemoveBookFromCart(int idCart, int bookId)
            {
                int result = 0;
                try
                {
                    //Definir el comando
                    string query = string.Format("DELETE FROM Cart WHERE Id = {0}",
                                                   "@Id");

                    //Definir los parámetros
                    List<SqlParameter> colParameters = new List<SqlParameter>();
                    colParameters.Add(new SqlParameter("Id", idCart));

                    //Ejecutar el comando
                    DbConnectionProvider tdBase = new DbConnectionProvider();
                    tdBase.Execute(query, colParameters);

                    try
                    {
                        //Definir el comando
                        string command = string.Format("UPDATE Books SET {0} WHERE Id = {1}",
                                                        "BookCant = BookCant + 1",
                                                        "@Id");


                        //Definir los parámetros
                        colParameters = new List<SqlParameter>();
                        colParameters.Add(new SqlParameter("Id", bookId));

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
                catch (Exception ex)
                {
                    throw;
                }

                return result;
            }

            /// <summary>
            /// Ejecuta una instrucción SQL de lectura con parámetros, retornando el carrito de compras del usuario.
            /// </summary>
            /// <param name="idUser">Parámetro que contiene el ID del usuario</param>
            /// <returns>Data tipo lista del carrito de compras del usuario</returns>
            public IEnumerable<Cart> GetCartDetail(int userId)
            {
                string query = string.Format(@"SELECT 
                                            A.Id, 
                                            B.Id AS IdBook,
                                            B.BookTitle, 
                                            B.BookAuthor, 
                                            B.BookYear, 
                                            B.BookBC,
                                            B.BookCant,
                                            B.BookPrice, 
                                            B.BookImage 
                                            FROM Cart as A 
                                            LEFT JOIN Books as B on B.Id = A.IdBook
                                            WHERE A.IdUser = {0}",
                                            userId);

                // Crear conexion y ejecutar la query
                dbc = new DbConnectionProvider();
                DataTable data = dbc.GetDataTable(query);

                // Crear lista de registros
                List<Cart> cartList = new List<Cart>();

                // Rellenar lista con registros
                if ((data != null) && (data.Rows.Count > 0))
                {
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        cartList.Add(ConvertCartDtRowToDTO(data.Rows[i]));
                    }
                }

                return cartList;
            }

            /// <summary>
            /// Ejecuta una instrucción SQL de lectura con parámetros, retornando la existencia de un carrito de compras.
            /// </summary>
            /// <param name="userId">Parámetro que contiene el ID del usuario</param>
            /// <returns>Entero de confirmacion de existencia de carrito de compras</returns>
            public int VarifyCart(int userId)
            {
                int result = 0;

                string query = string.Format(@"SELECT 
                                            Id, 
                                            IdUser, 
                                            IdBook 
                                            FROM Cart 
                                            WHERE IdUser = {0}",
                                            userId);

                dbc = new DbConnectionProvider();
                DataTable data = dbc.GetDataTable(query);

                if ((data != null) && (data.Rows.Count > 0))
                {
                    return result = 1;
                }
                else
                {
                    return result;
                }
            }

            #endregion

            #region BorrowCartMethods

            /// <summary>
            /// Ejecuta una instrucción SQL de inserción con parámetros, agregando un libro a la bolsa de préstamos.
            /// </summary>
            /// <param name="borrowCart">Objeto de clase BorrowCart, contiene la data del libro traido desde el controlador</param>
            /// <returns>Entero de confirmación de procesamiento exitoso</returns>
            public int AddBookToBorrowCart(BorrowCart borrowCart)
            {
                int result = 0;

                //Comprobar que no exista el libro en el carrito.
                //Definir la query   
                string query = string.Format(@"SELECT 
                                            Id, 
                                            IdUser, 
                                            IdBook 
                                            FROM BorrowCart
                                            WHERE IdBook = {0} AND IdUser = {1}",
                                            "@IdBook",
                                            "@IdUser");

                //Definir los parámetros
                List<SqlParameter> colParameters = new List<SqlParameter>();
                colParameters.Add(new SqlParameter("IdBook", borrowCart.IdProduct));
                colParameters.Add(new SqlParameter("IdUser", borrowCart.IdUser));

                //Ejecutar la query y obtener el resultado
                DbConnectionProvider tdBase = new DbConnectionProvider();
                DataTable data = tdBase.GetDataTable(query, colParameters);

                //Convertir y retornar la data
                if ((data == null) || (data.Rows.Count == 0))
                {
                    try
                    {
                        //Definir el comando
                        string command = string.Format("UPDATE Books SET {0} WHERE Id = {1}",
                                                       "BookCant = BookCant - 1",
                                                       "@Id");

                        //Definir los parámetros
                        colParameters = new List<SqlParameter>();
                        colParameters.Add(new SqlParameter("Id", borrowCart.IdProduct));

                        //Ejecutar el comando
                        tdBase = new DbConnectionProvider();
                        tdBase.Execute(command, colParameters);

                        try
                        {
                            //Comprobar que no exista el libro en el carrito.
                            //Definir la query   
                            query = string.Format("INSERT INTO BorrowCart ({0}) {1}",
                                                         "IdBook, IdUser, ItemBorrowDate, ItemBorrowReturnDate",
                                                         "VALUES(@IdBook, @IdUser, @ItemBorrowDate, @ItemBorrowReturnDate)");

                            //Definir los parámetros
                            colParameters = new List<SqlParameter>();
                            colParameters.Add(new SqlParameter("IdBook", borrowCart.IdProduct));
                            colParameters.Add(new SqlParameter("IdUser", borrowCart.IdUser));
                            colParameters.Add(new SqlParameter("ItemBorrowDate", borrowCart.ItemBorrowDate));
                            colParameters.Add(new SqlParameter("ItemBorrowReturnDate", borrowCart.ItemBorrowReturnDate));

                            //Ejecutar el comando
                            tdBase = new DbConnectionProvider();
                            tdBase.Execute(query, colParameters);

                            result = 1;
                        }
                        catch
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                return result;
            }

            /// <summary>
            /// Ejecuta una instrucción SQL de eliminación con parámetros, borrando el libro de la bolsa de préstamos.
            /// </summary>
            /// <param name="idBorrowCart">Parámetro que contiene el ID de la bolsa de préstamos</param>
            /// <param name="bookId">Parámetro que contiene el ID del libro</param>
            /// <returns>Entero de confirmación de procesamiento exitoso</returns>
            public int RemoveBookFromBorrowCart(int idBorrowCart, int bookId)
            {
                int result = 0;
                try
                {
                    //Definir el comando
                    string query = string.Format("DELETE FROM BorrowCart WHERE Id = {0}",
                                                   "@Id");

                    //Definir los parámetros
                    List<SqlParameter> colParameters = new List<SqlParameter>();
                    colParameters.Add(new SqlParameter("Id", idBorrowCart));

                    //Ejecutar el comando
                    DbConnectionProvider tdBase = new DbConnectionProvider();
                    tdBase.Execute(query, colParameters);

                    try
                    {
                        //Definir el comando
                        string command = string.Format("UPDATE Books SET {0} WHERE Id = {1}",
                                                        "BookCant = BookCant + 1",
                                                        "@Id");


                        //Definir los parámetros
                        colParameters = new List<SqlParameter>();
                        colParameters.Add(new SqlParameter("Id", bookId));

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
                catch (Exception ex)
                {
                    throw;
                }

                return result;
            }

            /// <summary>
            /// Ejecuta una instrucción SQL de lectura con parámetros, retornando la bolsa de préstamos del usuario.
            /// </summary>
            /// <param name="userId">Parámetro que contiene el ID del usuario</param>
            /// <returns>Data tipo lista de la bolsa de préstamos del usuario</returns>
            public IEnumerable<BorrowCart> GetBorrowCartDetail(int userId)
            {
                string query = string.Format(@"SELECT 
                                                A.Id, 
                                                A.ItemBorrowDate, 
                                                A.ItemBorrowReturnDate, 
                                                B.Id AS IdBook,
                                                B.BookTitle, 
                                                B.BookAuthor, 
                                                B.BookYear, 
                                                B.BookBC,
                                                B.BookCant,
                                                B.BookPrice, 
                                                B.BookImage 
                                                FROM BorrowCart as A 
                                                LEFT JOIN Books as B on B.Id = A.IdBook
                                                WHERE A.IdUser = {0}",
                                            userId);

                // Crear conexion y ejecutar la query
                dbc = new DbConnectionProvider();
                DataTable data = dbc.GetDataTable(query);

                // Crear lista de registros
                List<BorrowCart> borrowCartList = new List<BorrowCart>();

                // Rellenar lista con registros
                if ((data != null) && (data.Rows.Count > 0))
                {
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        borrowCartList.Add(ConvertBorrowCartDtRowToDTO(data.Rows[i]));
                    }
                }

                return borrowCartList;
            }

            /// <summary>
            /// Ejecuta una instrucción SQL de lectura con parámetros, retornando la existencia de una bolsa de préstamos.
            /// </summary>
            /// <param name="userId">Parámetro que contiene el ID del usuario</param>
            /// <returns>Entero de confirmacion de existencia de una bolsa de préstamos</returns>
            public int VarifyBorrowCart(int userId)
            {
                int result = 0;

                string query = string.Format(@"SELECT Id, IdUser, IdBook 
                                                FROM BorrowCart 
                                                WHERE IdUser = {0}",
                                                userId);

                dbc = new DbConnectionProvider();
                DataTable data = dbc.GetDataTable(query);

                if ((data != null) && (data.Rows.Count > 0))
                {
                    return result = 1;
                }
                else
                {
                    return result;
                }
            }

            /// <summary>
            /// Ejecuta una instrucción SQL de actualización con parámetros, actualizando las fechas de préstamo.
            /// </summary>
            /// <param name="borrowCart">Objeto de clase BorrowCart, contiene la data del libro traido desde el controlador</param>
            /// <returns>Entero de confirmacion de existencia de una bolsa de préstamos</returns>
            public int ModifyBorrowDates(BorrowCart borrowCart)
            {
                int result = 0;

                try
                {
                    //Definir el comando
                    string command = string.Format("UPDATE BorrowCart SET {0} WHERE Id = {1}",
                                                    "ItemBorrowDate = @ItemBorrowDate, ItemBorrowReturnDate = @ItemBorrowReturnDate",
                                                    "@Id");

                    //Definir los parámetros
                    List<SqlParameter>  colParameters = new List<SqlParameter>();
                    colParameters.Add(new SqlParameter("Id", borrowCart.Id));
                    colParameters.Add(new SqlParameter("ItemBorrowDate", borrowCart.ItemBorrowDate));
                    colParameters.Add(new SqlParameter("ItemBorrowReturnDate", borrowCart.ItemBorrowReturnDate));

                    //Ejecutar el comando
                    DbConnectionProvider tdBase = new DbConnectionProvider();
                    tdBase.Execute(command, colParameters);

                        result = 1;
                }
                catch (Exception ex)
                {
                    throw;
                }

                return result;
            }

        #endregion

        #endregion

        #region Protected Methods

        /// <summary>
        /// Convierte un DataTable en un objeto de clase Cart.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase Cart llenado con valores del DataTable</returns>
        protected Cart ConvertCartDtRowToDTO(DataRow data)
        {
            Cart book = new Cart
            {
                Id = (int)data["Id"],
                Books = new Books()
                {
                    Id = (int)data["IdBook"],
                    Title = data["BookTitle"].ToString(),
                    Author = data["BookAuthor"].ToString(),
                    BookYear = Convert.IsDBNull(data["BookYear"]) ? 0 : (int)data["BookYear"],
                    BookBC = data["BookBC"].ToString(),
                    Stock = (int)data["BookCant"],
                    BookPrice = Convert.IsDBNull(data["BookPrice"]) ? 0 : (int)data["BookPrice"],
                    BookImage = Convert.FromBase64String(data["BookImage"].ToString()),
                }
            };

            return book;
        }

        /// <summary>
        /// Convierte un DataTable en un objeto de clase BorrowCart.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase BorrowCart llenado con valores del DataTable</returns>
        protected BorrowCart ConvertBorrowCartDtRowToDTO(DataRow data)
        {
            BorrowCart book = new BorrowCart
            {
                Id = (int)data["Id"],
                ItemBorrowDate = DateTime.Parse(data["ItemBorrowDate"].ToString()),
                ItemBorrowReturnDate = DateTime.Parse(data["ItemBorrowReturnDate"].ToString()),
                Books = new Books()
                {
                    Id = (int)data["IdBook"],
                    Title = data["BookTitle"].ToString(),
                    Author = data["BookAuthor"].ToString(),
                    BookYear = Convert.IsDBNull(data["BookYear"]) ? 0 : (int)data["BookYear"],
                    BookBC = data["BookBC"].ToString(),
                    Stock = (int)data["BookCant"],
                    BookPrice = Convert.IsDBNull(data["BookPrice"]) ? 0 : (int)data["BookPrice"],
                    BookImage = Convert.FromBase64String(data["BookImage"].ToString()),
                }
            };

            return book;
        }

        #endregion
    }
}