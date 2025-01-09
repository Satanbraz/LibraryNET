using DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Text;
using System.Drawing.Printing;
using System.Web.UI;

namespace LibraryNET.Implementation.AdminManagers
{
    public class BooksManager
    {
        //Agregar conexion a BD
        DbConnectionProvider dbc = new DbConnectionProvider();

        #region Propierties

        protected string TableName = "Books";
        protected string Fields = "Id, BookTitle, BookAuthor, BookEditId, BookYear, BookGenreId, BookStateId, BookBC, BookCant";

        #endregion

        #region Public Methods

        /// <summary>
        /// Ejecuta una instrucción SQL de inserción con parámetros, para registrar un nuevo libro.
        /// </summary>
        /// <param name="book">Objeto de clase Books con los valores enviados desde el controlador</param>
        /// <returns>Id del nuevo libro registrado en BD</returns>
        public int Create(Books book)
        {
            // Definir la query
            string query = string.Format("INSERT INTO {0} ({1}) {2}",
                            TableName,
                            "BookTitle, BookAuthor, BookEditId, BookYear, BookGenreId, BookStateId, BookBC, BookCant, BookPrice",
                            "VALUES(@BookTitle, @BookAuthor, @BookEditId, @BookYear, @BookGenreId, @BookStateId, @BookBC, @BookCant, @BookPrice)");

            // Definir los paramatros de enviar

            List<SqlParameter> colParameters = new List<SqlParameter>();
            colParameters.Add(new SqlParameter("BookTitle", book.Title));
            colParameters.Add(new SqlParameter("BookAuthor", book.Author));
            colParameters.Add(new SqlParameter("BookYear", book.BookYear));
            colParameters.Add(new SqlParameter("BookGenreId", book.BookGenreId));
            colParameters.Add(new SqlParameter("BookStateId", book.BookStateId));
            colParameters.Add(new SqlParameter("BookEditId", book.BookEditId));
            colParameters.Add(new SqlParameter("BookBC", book.BookBC));
            colParameters.Add(new SqlParameter("BookCant", book.Stock));
            colParameters.Add(new SqlParameter("BookPrice", book.BookPrice));

            //Ejecutar el comando y obtener el resultado
            dbc = new DbConnectionProvider();

            int id = Convert.ToInt32(dbc.ExecuteReturningId(query, "Id", colParameters,TableName));

            return id;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura con parámetros, retornando la data del libro.
        /// </summary>
        /// <param name="bookId">Parámetro que contiene el Id del libro</param>
        /// <returns>Data del libro registrado en BD</returns>
        public Books Read(int bookId)
        {
            string query = string.Format(@"SELECT 
                                        A.Id, 
                                        BookTitle, 
                                        BookAuthor, 
                                        BookYear, 
                                        B.GenreName as BookGenre, 
                                        C.StateName as BookState, 
                                        D.EditName as BookEdit, 
                                        BookBC, 
                                        BookCant, 
                                        BookPrice, 
                                        BookImage 
                                        FROM {0} as A 
                                        LEFT JOIN Genres as B on B.Id = A.BookGenreId 
                                        LEFT JOIN BookStates as C on C.Id = A.BookStateId 
                                        LEFT JOIN EditBook as D on D.Id = A.BookEditId 
                                        WHERE a.Id = {1}",
                                        TableName,
                                        bookId);

            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            //Convertir y retornar la data
            Books booksList = new Books();

            if ((data != null) && (data.Rows.Count > 0))
            {
                return booksList = ConvertDtRowToDTO(data.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int Update(Books book)
        {
            throw new NotImplementedException();
        }

        public void Delete(int bookId)
        {
            string query = string.Format(@"DELETE FROM {0}
                                        WHERE Id = {1}",
                                        TableName,
                                        bookId);
            // SqlCommand cmd = new SqlCommand(query, dbc.GetConnection());
            // cmd.ExecuteReader();

        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura, retornando la data tipo lista de todos los libros.
        /// </summary>
        /// <returns>Data tipo lista de todos los libros registrados en BD</returns>
        public IEnumerable<Books> TotalList()
        {
            string query = string.Format(@"SELECT 
                                        A.Id, 
                                        BookTitle, 
                                        BookAuthor, 
                                        BookYear, 
                                        B.GenreName as BookGenre, 
                                        C.StateName as BookState, 
                                        D.EditName as BookEdit, 
                                        BookBC, 
                                        BookCant, 
                                        BookPrice,
                                        NULL as BookImage 
                                        FROM {0} as A 
                                        LEFT JOIN Genres as B on B.Id = A.BookGenreId 
                                        LEFT JOIN BookStates as C on C.Id = A.BookStateId 
                                        LEFT JOIN EditBook as D on D.Id = A.BookEditId ",
                                        TableName);

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            // Crear lista de registros
            List<Books> booksList = new List<Books>();

            // Rellenar lista con registros
            if ((data != null) && (data.Rows.Count > 0))
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    booksList.Add(ConvertDtRowToDTO(data.Rows[i]));
                }
            }

            return booksList;

        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura con parámetros, retornando una data tipo lista paginada de todos los libros.
        /// </summary>
        /// <param name="page">Parámetro que contiene el numero de la página actual</param>
        /// <param name="pageSize">Parámetro que contiene el tamaño de la paginación</param>
        /// <returns>Data tipo lista paginada de todos los libros registrados en BD</returns>
        public IEnumerable<Books> List(int page, int pageSize)
        {
            // Calcular el índice de inicio
            int startIndex = (page - 1) * pageSize;

            string query = string.Format(@"SELECT 
                                        A.Id, 
                                        BookTitle, 
                                        BookAuthor, 
                                        BookYear, 
                                        B.GenreName as BookGenre, 
                                        C.StateName as BookState, 
                                        D.EditName as BookEdit, 
                                        BookBC, 
                                        BookCant, 
                                        BookPrice, 
                                        BookImage 
                                        FROM {0} as A 
                                        LEFT JOIN Genres as B on B.Id = A.BookGenreId 
                                        LEFT JOIN BookStates as C on C.Id = A.BookStateId 
                                        LEFT JOIN EditBook as D on D.Id = A.BookEditId 
                                        ORDER BY A.Id OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY",
                                        TableName,
                                        startIndex,
                                        pageSize);

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            // Crear lista de registros
            List<Books> booksList = new List<Books>();

            // Rellenar lista con registros
            if ((data != null) && (data.Rows.Count > 0))
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    booksList.Add(ConvertDtRowToDTO(data.Rows[i]));
                }
            }

            return booksList;

        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura con parámetros, retornando una data tipo lista paginada de libros filtrados.
        /// </summary>
        /// <param name="bookEditId">Parámetro que contiene el Id seleccionado del género del libro</param>
        /// <param name="bookGenreId">Parámetro que contiene el Id seleccionado del género del libro</param>
        /// <param name="busqueda">Parámetro que contiene el titulo del libro buscado</param>
        /// <param name="page">Parámetro que contiene el numero de la página actual</param>
        /// <param name="pageSize">Parámetro que contiene el tamaño de la paginación</param>
        /// <returns>Data tipo lista paginada y filtrada de todos los libros registrados en BD</returns>
        public IList<Books> ListFiltered(int bookEditId, int bookGenreId, string busqueda, int page, int pageSize) {

            int startIndex = (page - 1) * pageSize;

            string queryWhere = string.Empty;

            if (bookEditId > 0 && bookGenreId <= 0 && string.IsNullOrEmpty(busqueda))
            {
                queryWhere = " WHERE BookEditId = " + bookEditId;
            }else if (bookEditId <= 0 && bookGenreId > 0 && string.IsNullOrEmpty(busqueda))
            {
                queryWhere = " WHERE BookGenreId = " + bookGenreId;
            }else if (bookEditId > 0 && bookGenreId > 0 && string.IsNullOrEmpty(busqueda))
            {
                queryWhere = " WHERE BookEditId = "+ bookEditId +"AND BookGenreId = " + bookGenreId;
            }else
            {
                queryWhere = " WHERE BookTitle LIKE '%" + busqueda + "%'";
            }

            string query = string.Format(@"SELECT 
                                        A.Id, 
                                        BookTitle, 
                                        BookAuthor, 
                                        BookYear, 
                                        B.GenreName as BookGenre, 
                                        C.StateName as BookState, 
                                        D.EditName as BookEdit, 
                                        BookBC, 
                                        BookCant, 
                                        BookPrice, 
                                        BookImage 
                                        FROM {0} as A 
                                        LEFT JOIN Genres as B on B.Id = A.BookGenreId 
                                        LEFT JOIN BookStates as C on C.Id = A.BookStateId 
                                        LEFT JOIN EditBook as D on D.Id = A.BookEditId 
                                        {1} 
                                        ORDER BY A.Id OFFSET {2} ROWS FETCH NEXT {3} ROWS ONLY",
                                        TableName,
                                        queryWhere,
                                        startIndex,
                                        pageSize);

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            // Crear lista de registros
            List<Books> booksList = new List<Books>();

            // Rellenar lista con registros
            if ((data != null) && (data.Rows.Count > 0))
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    booksList.Add(ConvertDtRowToDTO(data.Rows[i]));
                }
            }

            return booksList;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura, retornando cantidad total de todos los libros.
        /// </summary>
        /// <returns>Cuenta de todos los libros registrados en BD</returns>
        public int ItemsCount()
        {
            string query = $"SELECT COUNT(*) FROM {TableName}";
            // Crear conexion y ejecutar la query

            dbc = new DbConnectionProvider();
            var data = dbc.GetValue(query);

            return Convert.ToInt32(data);
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura con parámetros, retornando la cantidad de registros de libros encontrados segun filtros.
        /// </summary>
        /// <param name="bookEditId">Parámetro que contiene el Id seleccionado del género del libro</param>
        /// <param name="bookGenreId">Parámetro que contiene el Id seleccionado del género del libro</param>
        /// <param name="busqueda">Parámetro que contiene el titulo del libro buscado</param>
        /// <returns>Entero con la cantidad de registros encontrados en BD</returns>
        public int ItemsCountFiltered(int bookEditId, int bookGenreId, string busqueda)
        {
            var parameters = new List<SqlParameter>();

            string queryWhere = string.Empty;

            if (bookEditId > 0 && bookGenreId <= 0 && string.IsNullOrEmpty(busqueda))
            {
                queryWhere = " WHERE BookEditId = " + bookEditId;
            }
            else if (bookEditId <= 0 && bookGenreId > 0 && string.IsNullOrEmpty(busqueda))
            {
                queryWhere = " WHERE BookGenreId = " + bookGenreId;
            }
            else if (bookEditId > 0 && bookGenreId > 0 && string.IsNullOrEmpty(busqueda))
            {
                queryWhere = " WHERE BookEditId = " + bookEditId + "AND BookGenreId = " + bookGenreId;
            }
            else
            {
                queryWhere = " WHERE BookTitle LIKE '%" + busqueda + "%'";
            }

            string query = String.Format(@"SELECT COUNT(*) 
                                            FROM {0} as A 
                                            LEFT JOIN Genres as B on B.Id = A.BookGenreId 
                                            LEFT JOIN BookStates as C on C.Id = A.BookStateId 
                                            LEFT JOIN EditBook as D on D.Id = A.BookEditId 
                                            {1}",
                                            TableName,
                                            queryWhere);

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            return Convert.ToInt32(data.Rows.Count);
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de actualización con parámetros, actualizando el campo de imagen en BD.
        /// </summary>
        /// <param name="bookId">Parámetro que contiene el Id del libro seleccionado</param>
        /// <param name="imageData">Parámetro que contiene la imagen en formato bytes</param>
        public void SaveBookImage(int bookId, byte[] imageData)
        {
            // Convertir la imagen a una cadena Base64
            string base64Image = Convert.ToBase64String(imageData);

            try
            {
                // Consulta SQL con parámetros
                string command = @"UPDATE " + TableName + @" 
                            SET BookImage = @Base64Image
                            WHERE Id = @bookId";


                //Definir los parámetros
                List<SqlParameter> colParameters = new List<SqlParameter>();
                colParameters.Add(new SqlParameter("bookId", bookId));
                colParameters.Add(new SqlParameter("Base64Image", base64Image));

                //Ejecutar el comando
                DbConnectionProvider tdBase = new DbConnectionProvider();
                tdBase.Execute(command, colParameters);
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura, retornando cantidad los libros mas vendidos.
        /// </summary>
        /// <returns>Data tipo lista de los libros registrados en BD mas vendidos</returns>
        public IEnumerable<Buy> getRecomendations()
        {
            string query = String.Format(@"
                                        SELECT TOP 6 BuyQ, bookID, BookTitle, BookAuthor, BookImage
                                        FROM (
                                            SELECT 
		                                        SUM(b.QProduct) AS BuyQ,
                                                b.productId  AS bookID, 
                                                c.BookTitle,
                                                c.BookAuthor,
                                                c.BookImage
	                                        FROM buys AS a
	                                        LEFT JOIN BuyDetail AS B ON A.Id = B.BuyId
	                                        LEFT JOIN Books AS C ON C.Id = B.ProductId
                                            GROUP BY b.productId, c.BookTitle, c.BookAuthor, c.BookImage
                                        ) AS subquery
                                        ORDER BY BuyQ DESC,BookTitle ASC;");

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            // Crear lista de registros
            List<Buy> recomendationsList = new List<Buy>();

            // Rellenar lista con registros
            if ((data != null) && (data.Rows.Count > 0))
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    recomendationsList.Add(ConvertDtRowRecomToDTO(data.Rows[i]));
                }
            }

            return recomendationsList;
        }


        #endregion

        #region Protected Methods

        /// <summary>
        /// Convierte un DataTable en un objeto de clase Books.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase Books llenado con valores del DataTable</returns>
        protected Books ConvertDtRowToDTO(DataRow data)
        {
            Books book = new Books
            {
                Id = (int)data["Id"],
                Title = data["BookTitle"].ToString(),
                Author = data["BookAuthor"].ToString(),
                BookEdit = data["BookEdit"].ToString(),
                BookYear = Convert.IsDBNull(data["BookYear"]) ? 0 : (int)data["BookYear"],
                BookGenre = data["BookGenre"].ToString(),
                BookState = data["BookState"].ToString(),
                BookBC = data["BookBC"].ToString(),
                Stock = (int)data["BookCant"],
                BookPrice = Convert.IsDBNull(data["BookPrice"]) ? 0 : (int)data["BookPrice"],
                BookImage = Convert.FromBase64String(data["BookImage"].ToString()),
            };

            return book;
        }

        /// <summary>
        /// Convierte un DataTable en un objeto de clase Books.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase Books llenado con valores del DataTable de libros Recomendados</returns>
        protected Buy ConvertDtRowRecomToDTO(DataRow data) {

            Buy buyDetail = new Buy
            {
                BuyCount = (int)data["BuyQ"],
                books = new Books()
                {
                    Id = (int)data["bookID"],
                    Title = data["BookTitle"].ToString(),
                    Author = data["BookAuthor"].ToString(),
                    BookImage = Convert.FromBase64String(data["BookImage"].ToString())
                }
            };

            return buyDetail;

        }
        #endregion
    }
}