using DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;

namespace LibraryNET.Implementation
{
    public class EditManager
    {
        //Agregar conexion a BD
        DbConnectionProvider dbc = new DbConnectionProvider();

        #region Propierties

        protected string TableName = "EditBook";
        protected string Fields = "Id, EditName";

        #endregion

        #region Public Methods

        /// <summary>
        /// Ejecuta una instrucción SQL de inserción con parámetros, para registrar un nuevo editorial.
        /// </summary>
        /// <param name="edit">Objeto de clase Edit con los valores enviados desde el controlador</param>
        /// <returns>Id del nuevo editorial registrado en BD</returns>
        public int Create(Edit edit)
        {
            int result = 0;

            //Comprobar que no existan Editoriales con el mismo nombre.
            //Definir la query   
            string query = string.Format("SELECT {0} FROM {1} WHERE EditName = {2}",
                                         Fields,
                                         TableName,
                                         "@EditName");

            //Definir los parámetros
            List<SqlParameter> colParameters = new List<SqlParameter>();
            colParameters.Add(new SqlParameter("EditName", edit.EditName));

            //Ejecutar la query y obtener el resultado
            DbConnectionProvider tdBase = new DbConnectionProvider();
            DataTable data = tdBase.GetDataTable(query, colParameters);

            //Convertir y retornar la data
            if ((data == null) || (data.Rows.Count == 0))
            {
                // Definir la query
                string command = string.Format("INSERT INTO {0} ({1}) {2}",
                            TableName,
                            "EditName",
                            "VALUES(@EditName)");

                // Definir los paramatros de enviar

                colParameters = new List<SqlParameter>();
                colParameters.Add(new SqlParameter("EditName", edit.EditName));

                //Ejecutar el comando y obtener el resultado
                dbc = new DbConnectionProvider();
                int id = Convert.ToInt32(dbc.ExecuteReturningId(command, "Id", colParameters, TableName));
                result = 2;
            }
            else
            {
                // Registro existente
                result = 1;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura con parámetros, retornando la data del editorial.
        /// </summary>
        /// <param name="editId">Parámetro que contiene el Id del editorial</param>
        /// <returns>Data del editorial registrado en BD</returns>
        public Edit Read(int editId)
        {

            string query = string.Format("SELECT {0} FROM {1} WHERE ID = {2}",
                                        Fields,
                                        TableName,
                                        editId);

            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            //Convertir y retornar la data
            Edit editList = new Edit();

            if ((data != null) && (data.Rows.Count > 0))
            {
                return editList = ConvertDtRowToDTO(data.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de actualización con parámetros, actualizando el registro del editorial.
        /// </summary>
        /// <param name="edit">Objeto de clase Edit con los valores enviados desde el controlador</param>
        /// <returns>Entero de confirmación de procesamiento exitoso</returns>
        public int Update(Edit edit)
        {
            int result = 0;

            //Comprobar que no existan Editoriales con el mismo nombre.
            //Definir la query   
            string query = string.Format("SELECT {0} FROM {1} WHERE EditName = {2}",
                                         Fields,
                                         TableName,
                                         "@EditName");

            //Definir los parámetros
            List<SqlParameter> colParameters = new List<SqlParameter>();
            colParameters.Add(new SqlParameter("EditName", edit.EditName));

            //Ejecutar la query y obtener el resultado
            DbConnectionProvider tdBase = new DbConnectionProvider();
            DataTable data = tdBase.GetDataTable(query,colParameters);

            //Convertir y retornar la data
            if ((data == null) || (data.Rows.Count == 0))
            {
                try
                {
                    //Definir el comando
                    string command = string.Format("UPDATE {0} SET {1} WHERE ID = {2}",
                                                   TableName,
                                                   "EditName = @EditName",
                                                   "@Id");


                    //Definir los parámetros
                    colParameters = new List<SqlParameter>();
                    colParameters.Add(new SqlParameter("EditName", edit.EditName));
                    colParameters.Add(new SqlParameter("Id", edit.Id));

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
        /// Ejecuta una instrucción SQL de eliminación con parámetros, borrando el editorial de BD.
        /// </summary>
        /// <param name="editId">Parámetro que contiene el ID del editorial</param>
        /// <returns>Entero de confirmación de procesamiento exitoso</returns>
        public int Delete(int editId)
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
                colParameters.Add(new SqlParameter("Id", editId));

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
        /// Ejecuta una instrucción SQL de lectura, retornando la data tipo lista de todos los editoriales.
        /// </summary>
        /// <returns>Data tipo lista de todos los editoriales registrados en BD</returns>
        public IEnumerable<Edit> List()
        {
            string query = string.Format(@"SELECT 
                                        Id, 
                                        EditName   
                                        FROM {0}",
                                        TableName);

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            // Crear lista de registros
            List<Edit> editsList = new List<Edit>();

            // Rellenar lista con registros
            if ((data != null) && (data.Rows.Count > 0))
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    editsList.Add(ConvertDtRowToDTO(data.Rows[i]));
                }
            }

            return editsList;

        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Convierte un DataTable en un objeto de clase Edit.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase Edit llenado con valores del DataTable</returns>
        protected Edit ConvertDtRowToDTO(DataRow data)
        {
            Edit edit = new Edit
            {
                Id = (int)data["Id"],
                EditName = data["EditName"].ToString()
            };

            return edit;
        }

        #endregion
    }
}