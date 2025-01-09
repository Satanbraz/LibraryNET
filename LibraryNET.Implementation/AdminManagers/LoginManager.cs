using DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace LibraryNET.Implementation
{
    public class LoginManager
    {
        DbConnectionProvider dbc = new DbConnectionProvider();

        #region Propierties

        protected string TableName = "Users";

        #endregion

        #region Public Methods

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura con parámetros, retornando la data del usuario.
        /// </summary>
        /// <param name="User">Parámetro que contiene el nombre del usuario</param>
        /// <param name="Pass">Parámetro que contiene la contraseña del usuario</param>
        /// <returns>Data del usuario registrado en BD</returns>
        public Users Read(string User, string Pass)
        {
            string query = string.Format(@"SELECT 
                                        A.Id, 
                                        UserName, 
                                        UserLName, 
                                        UserEmail,
                                        RolId,
                                        B.RolName as RolName, 
                                        UserPass, 
                                        UserPhone, 
                                        UserDir, 
                                        UserRut, 
                                        UserDate 
                                        FROM {0} AS A 
                                        LEFT JOIN Rol AS B on B.Id = A.RolId 
                                        WHERE (UserPhone = '{1}' OR UserEmail = '{1}') AND UserPass = '{2}';",
                                        TableName,
                                        User,
                                        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Pass)));

            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            //Convertir y retornar la data
            Users usersList = new Users();

            if ((data != null) && (data.Rows.Count > 0))
            {
                return usersList = ConvertDtRowToDTO(data.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Convierte un DataTable en un objeto de clase Users.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase Users llenado con valores del DataTable</returns>
        protected Users ConvertDtRowToDTO(DataRow data)
        {
            Users user = new Users
            {
                Id = (int)data["Id"],
                UserRolId = (int)data["RolId"],
                UserRolName = data["RolName"].ToString(),
                UserName = data["UserName"].ToString(),
                UserLastName = data["UserLName"].ToString(),
                UserEmail = data["UserEmail"].ToString(),
                UserPassword = data["UserPass"].ToString(),
                UserPhone = data["UserPhone"].ToString(),
                UserDir = data["UserDir"].ToString(),
                UserRut = data["UserRut"].ToString(),
                UserDate = DateTime.Parse(data["UserDate"].ToString())
            };

            return user;
        }

        #endregion


    }
}