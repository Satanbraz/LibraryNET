using DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;

namespace LibraryNET.Implementation
{
    public class GenreManager
    {
        //Agregar conexion a BD
        DbConnectionProvider dbc = new DbConnectionProvider();

        #region Propierties

        protected string TableName = "Genres";
        protected string Fields = "Id, GenreName";

        #endregion

        #region Public Methods

        /// <summary>
        /// Ejecuta una instrucción SQL de inserción con parámetros, para registrar un nuevo genero literario.
        /// </summary>
        /// <param name="genre">Objeto de clase Genre con los valores enviados desde el controlador</param>
        /// <returns>Id del nuevo genero literario registrado en BD</returns>
        public int Create(Genre genre)
        {
            int result = 0;
            //Comprobar que no existan Editoriales con el mismo nombre.
            //Definir la query   
            string query = string.Format("SELECT {0} FROM {1} WHERE GenreName = {2}",
                                         Fields,
                                         TableName,
                                         "@GenreName");

            //Definir los parámetros
            List<SqlParameter> colParameters = new List<SqlParameter>();
            colParameters.Add(new SqlParameter("GenreName", genre.GenreName));

            //Ejecutar la query y obtener el resultado
            DbConnectionProvider tdBase = new DbConnectionProvider();
            DataTable data = tdBase.GetDataTable(query, colParameters);

            //Convertir y retornar la data
            if ((data == null) || (data.Rows.Count == 0))
            {
                // Definir la query
                string command = string.Format("INSERT INTO {0} ({1}) {2}",
                            TableName,
                            "GenreName",
                            "VALUES(@GenreName)");

                // Definir los paramatros de enviar

                colParameters = new List<SqlParameter>();
                colParameters.Add(new SqlParameter("GenreName", genre.GenreName));

                //Ejecutar el comando y obtener el resultado
                dbc = new DbConnectionProvider();
                int id = Convert.ToInt32(dbc.ExecuteReturningId(command, "Id", colParameters, TableName));

                result = 2;
            }
            else
            {
                // Genero existente
                result = 1;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura con parámetros, retornando la data del genero literario.
        /// </summary>
        /// <param name="genreId">Parámetro que contiene el Id del genero literario</param>
        /// <returns>Data del genero literario registrado en BD</returns>
        public Genre Read(int genreId)
        {

            string query = string.Format("SELECT {0} FROM {1} WHERE ID = {2}",
                                        Fields,
                                        TableName,
                                        genreId);

            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            //Convertir y retornar la data
            Genre genresList = new Genre();

            if ((data != null) && (data.Rows.Count > 0))
            {
                return genresList = ConvertDtRowToDTO(data.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de actualización con parámetros, actualizando el registro del genero literario.
        /// </summary>
        /// <param name="genre">Objeto de clase Genre con los valores enviados desde el controlador</param>
        /// <returns>Entero de confirmación de procesamiento exitoso</returns>
        public int Update(Genre genre)
        {
            int result = 0;

            //Comprobar que no existan Editoriales con el mismo nombre.
            //Definir la query   
            string query = string.Format("SELECT {0} FROM {1} WHERE GenreName = {2}",
                                         Fields,
                                         TableName,
                                         "@GenreName");

            //Definir los parámetros
            List<SqlParameter> colParameters = new List<SqlParameter>();
            colParameters.Add(new SqlParameter("GenreName", genre.GenreName));

            //Ejecutar la query y obtener el resultado
            DbConnectionProvider tdBase = new DbConnectionProvider();
            DataTable data = tdBase.GetDataTable(query, colParameters);

            //Convertir y retornar la data
            if ((data == null) || (data.Rows.Count == 0))
            {
                try
                {
                    //Definir el comando
                    string command = string.Format("UPDATE {0} SET {1} WHERE Id = {2}",
                                                   TableName,
                                                   "GenreName = @GenreName",
                                                   "@Id");


                    //Definir los parámetros
                    colParameters = new List<SqlParameter>();
                    colParameters.Add(new SqlParameter("GenreName", genre.GenreName));
                    colParameters.Add(new SqlParameter("Id", genre.Id));

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
        /// Ejecuta una instrucción SQL de eliminación con parámetros, borrando el genero literario de BD.
        /// </summary>
        /// <param name="genreId">Parámetro que contiene el ID del genero literario</param>
        /// <returns>Entero de confirmación de procesamiento exitoso</returns>
        public int Delete(int genreId)
        {
            int result = 0;
            try
            {
                //Definir el comando
                string command = string.Format("DELETE FROM {0} WHERE ID = {1}",
                                               TableName,
                                               "@Id");

                //Definir los parámetros
                List<SqlParameter> colParameters = new List<SqlParameter>();
                colParameters.Add(new SqlParameter("Id", genreId));

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

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura, retornando la data tipo lista de todos los generos literarios.
        /// </summary>
        /// <returns>Data tipo lista de todos los generos literarios registrados en BD</returns>
        public IEnumerable<Genre> List()
        {
            string query = string.Format(@"SELECT 
                                        Id, 
                                        GenreName   
                                        FROM {0} 
                                        ORDER BY GenreName",
                                        TableName);

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            // Crear lista de registros
            List<Genre> genresList = new List<Genre>();

            // Rellenar lista con registros
            if ((data != null) && (data.Rows.Count > 0))
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    genresList.Add(ConvertDtRowToDTO(data.Rows[i]));
                }
            }

            return genresList;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Convierte un DataTable en un objeto de clase Genre.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase Genre llenado con valores del DataTable</returns>
        protected Genre ConvertDtRowToDTO(DataRow data)
        {
            Genre genre = new Genre
            {
                Id = (int)data["Id"],
                GenreName = data["GenreName"].ToString()
            };

            return genre;
        }

        #endregion
    }
}