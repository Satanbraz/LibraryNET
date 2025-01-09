using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.EnterpriseServices;
using System.Web.UI.WebControls;
using System.Windows.Input;

namespace LibraryNET.Implementation
{
    public class DbConnectionProvider
    {
        #region Variables

        private static string _connectionString;

        #endregion

        #region Constructor

        /// <summary>
        /// Conexion a BD y metodos de ejecucion de instrucciones a SQLServer.
        /// </summary>
        public DbConnectionProvider()
        {
            //Obtener la conexión a la base de datos
            if (string.IsNullOrEmpty(_connectionString))
            {
                GetConnection();
            }

        }

        #endregion

        #region Public Methods

        // Método para abrir la conexión a la base de datos
        public SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            return connection;
        }

        #endregion

        #region ExecutionMethods

        /// <summary>
        /// Ejecuta una instrucción SQL con parámetros, retornando el Id de la ejecución.
        /// </summary>
        /// <param name="sqlInstruction">Instrucción SQL con parámetros a ejecutar</param>
        /// <param name="idFieldName">Nombre de la columna que contiene el id a retornar</param>
        /// <param name="parameters">Parámetros de la instrucción SQL</param>
        /// <param name="tableName">Indica la tabla objetivo donde se ejecutará la query</param>
        /// <param name="trx">Transacción de la que puede ser parte la instrucción SQL (si es nula no se toma en cuenta)</param>
        /// <returns>El id resultado de la ejecución de la instrucción SQL con parámetros</returns>
        public object ExecuteReturningId(string sqlInstruction, string idFieldName, List<SqlParameter> parameters, string tableName, SqlTransaction trx = null)
        {
            //Crear el comando y asociarle la instrucción SQL
            SqlConnection cnx = new SqlConnection(_connectionString);
            SqlCommand objCommand = cnx.CreateCommand();
            objCommand.CommandText = sqlInstruction;
            if (((trx != null)))
            {
                objCommand.Transaction = trx;
            }

            //Setear los parámetros del comando
            SetCommandParameters(objCommand, parameters);

            try
            {
                //Ejecutar la instrucción SQL
                objCommand.Connection.Open();
                objCommand.ExecuteNonQuery();
                object id = null;

                //Obtener el Id
                objCommand.CommandText = String.Format("SELECT IDENT_CURRENT('{0}')",tableName);
                id = objCommand.ExecuteScalar();

                return id;
            }
            catch (Exception ex)
            {
                //Loguear el error y pasarlo
                throw;
            }
            finally
            {
                //Liberar objetos
                if (objCommand.Connection.State == ConnectionState.Open)
                {
                    objCommand.Connection.Close();
                }
                objCommand.Connection.Dispose();
                objCommand.Dispose();
            }
        }

        /// <summary>
        /// Ejecuta una instrucción SQL con parámetros.
        /// </summary>
        /// <param name="sqlInstruction">Instrucción SQL con parámetros a ejecutar</param>
        /// <param name="parameters">Parámetros de la instrucción SQL</param>
        /// <param name="trx">Transacción de la que puede ser parte la instrucción SQL (si es nula no se toma en cuenta)</param>
        public void Execute(string sqlInstruction, List<SqlParameter> parameters, SqlTransaction trx = null)
        {
            //Crear el comando y asociarle la instrucción SQL
            SqlConnection cnx = new SqlConnection(_connectionString);
            SqlCommand objCommand = cnx.CreateCommand();
            objCommand.CommandText = sqlInstruction;
            if (((trx != null)))
            {
                objCommand.Transaction = trx;
            }

            //Setear los parámetros del comando
            SetCommandParameters(objCommand, parameters);

            try
            {
                //Ejecutar la instrucción SQL
                objCommand.Connection.Open();
                objCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //Loguear el error y pasarlo
                throw;
            }
            finally
            {
                //Liberar objetos
                if (objCommand.Connection.State == ConnectionState.Open)
                {
                    objCommand.Connection.Close();
                }
                objCommand.Connection.Dispose();
                objCommand.Dispose();
            }
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Obtiene un valor a través de una query.
        /// </summary>
        /// <param name="query">Query con la que obtener el valor</param>
        /// <param name="trx">Transacción de la que puede ser parte la query (si es nula no se toma en cuenta)</param>
        /// <returns>El valor obtenido a través de la query</returns>
        public object GetValue(string query, SqlTransaction trx = null)
        {
            //Crear un comando, setearle el tipo, conexión, texto y transacción (si existe)
            SqlCommand objCommand = new SqlCommand();
            objCommand.CommandType = CommandType.Text;
            objCommand.Connection = new SqlConnection(_connectionString);
            objCommand.CommandTimeout = 180000;
            objCommand.CommandText = query;
            if (((trx != null)))
            {
                objCommand.Transaction = trx;
            }

            object objReturn = null;
            try
            {
                //Ejecutar la query y obtener el objeto retornado
                objCommand.Connection.Open();
                objReturn = objCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                //Loguear el error y pasarlo
                throw;
            }
            finally
            {
                //Liberar objetos
                if (objCommand.Connection.State == ConnectionState.Open)
                {
                    objCommand.Connection.Close();
                }
                objCommand.Connection.Dispose();
                objCommand.Dispose();
            }

            //Retornar el valor
            return objReturn;
        }

        /// <summary>
        /// Obtiene una DataTable a través de una query.
        /// </summary>
        /// <param name="query">Query con la que obtener la DataTable</param>
        /// <param name="trx">Transacción de la que puede ser parte la query (si es nula no se toma en cuenta)</param>
        /// <returns>La DataTable obtenida a través de la query</returns>
        public DataTable GetDataTable(string query, SqlTransaction trx = null)
        {
            //Crear un DataAdapter, asociarle la query, la conexión y la transacción (si existe)
            SqlConnection cnx = new SqlConnection(_connectionString);
            SqlDataAdapter objDataAdapter = new SqlDataAdapter(query, cnx);
            objDataAdapter.SelectCommand.CommandTimeout = 180000;

            if (trx != null)
            {
                objDataAdapter.SelectCommand.Transaction = trx;
            }

            //Llenar la DataTable con el DataAdapter
            DataTable objDataTable = new DataTable();
            try
            {
                cnx.Open();
                objDataAdapter.Fill(objDataTable);
            }
            catch (Exception ex)
            {
                //Loguear el error y pasarlo
                throw;
            }
            finally
            {
                //Liberar objetos
                if (cnx.State == ConnectionState.Open)
                {
                    cnx.Close();
                }
                cnx.Dispose();
                objDataAdapter.Dispose();
            }

            //Retornar la DataTable
            return objDataTable;
        }

        /// <summary>
        /// Obtiene una DataTable a través de una query con parámetros.
        /// </summary>
        /// <param name="query">Query con la que obtener la DataTable</param>
        /// <param name="parameters">Parámetros de la query</param>
        /// <param name="trx">Transacción de la que puede ser parte la query (si es nula no se toma en cuenta)</param>
        /// <returns>La DataTable obtenida a través de la query con párámetros</returns>
        public DataTable GetDataTable(string query, List<SqlParameter> parameters, SqlTransaction trx = null)
        {
            //Crear un comando, setearle el tipo, conexión, texto y transacción (si existe)
            SqlCommand objCommand = new SqlCommand();
            objCommand.CommandType = CommandType.Text;
            objCommand.Connection = new SqlConnection(_connectionString);
            objCommand.CommandTimeout = 180000;
            objCommand.CommandText = query;
            if (((trx != null)))
            {
                objCommand.Transaction = trx;
            }

            //Setear los parámetros del comando
            SetCommandParameters(objCommand, parameters);

            //Crear un DataAdapter y asociarle el comando
            SqlDataAdapter objDataAdapter = new SqlDataAdapter(objCommand);

            //Llenar la DataTable con el DataAdapter
            DataTable objDataTable = new DataTable();
            try
            {
                objCommand.Connection.Open();
                objDataAdapter.Fill(objDataTable);
            }
            catch (Exception ex)
            {
                //Loguear el error y pasarlo
                throw;
            }
            finally
            {
                //Liberar objetos
                if (objCommand.Connection.State == ConnectionState.Open)
                {
                    objCommand.Connection.Close();
                }
                objCommand.Connection.Dispose();
                objCommand.Dispose();
                objDataAdapter.Dispose();
            }

            //Retornar la DataTable
            return objDataTable;
        }


        #endregion

        #region Private Methods


        // Método para cerrar la conexión a la base de datos
        private void CloseConnection(SqlConnection connection)
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
            }
        }

        // Generar conexion a BD
        private SqlConnection GetConnection()
        {
            // Configura la cadena de conexión
            _connectionString = "Data Source=localhost;Initial Catalog=LibraryNET; integrated security=true;encrypt=false";

            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Agrega una lista de parámetros a un comando
        /// </summary>
        /// <param name="command">Comando al que agregar los parámetros</param>
        /// <param name="parameters">Lista de parámetros a agregar</param>
        private void SetCommandParameters(SqlCommand command, List<SqlParameter> parameters)
        {
            //Recorrer la lista de parámetros y setearlos al comando, manejando las peculiaridades que puedan existir
            for (int i = 0; i <= (parameters.Count - 1); i++)
            {
                //Booleanos
                bool testVar;
                if (parameters[i].Value != null && bool.TryParse(parameters[i].Value.ToString(), out testVar))
                {
                    parameters[i].Value = ConvertBoolToShort((bool)parameters[i].Value);
                }

                //Valores nulos
                if (parameters[i].Value == null)
                {
                    parameters[i].Value = DBNull.Value;
                }

                command.Parameters.Add(parameters[i]);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Convierte un valor booleano a short (1 = true, 0 = false).
        /// </summary>
        /// <param name="valueToConvert">Booleano a convertir</param>
        /// <returns>El booleano como short</returns>
        protected short ConvertBoolToShort(bool valueToConvert)
        {
            if (valueToConvert)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        #endregion
    }
}
