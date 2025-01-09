using DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace LibraryNET.Implementation
{
    public class UserManager
    {
        //Agregar conexion a BD
        DbConnectionProvider dbc = new DbConnectionProvider();

        #region Propierties

        protected string TableName = "Users";
        protected string Fields = "Id,UserName,UserLName,UserEmail,UserPass,UserPhone,UserDir,UserDate";

        #endregion

        #region Public Methods

        /// <summary>
        /// Ejecuta una instrucción SQL de inserción con parámetros, para registrar un nuevo usuario.
        /// </summary>
        /// <param name="user">Objeto de clase Users con los valores enviados desde el controlador</param>
        /// <returns>Id del nuevo usuario registrado en BD</returns>
        public int Create(Users user)
        {
            // Definir la query
            string query = string.Format("INSERT INTO {0} ({1}) {2}",
                            TableName,
                            "UserName, UserLName, UserEmail, UserPass, UserPhone, UserDir, UserRut, RolId, UserDate",
                            "VALUES(@UserName, @UserLName, @UserEmail, @UserPass, @UserPhone ,@UserDir, @UserRut, @RolId, @UserDate)");

            // Definir los paramatros de enviar
            List<SqlParameter> colParameters = new List<SqlParameter>();
            colParameters.Add(new SqlParameter("UserName", user.UserName));
            colParameters.Add(new SqlParameter("UserLName", user.UserLastName));
            colParameters.Add(new SqlParameter("UserEmail", user.UserEmail));
            colParameters.Add(new SqlParameter("UserPass", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(user.UserPassword))));
            colParameters.Add(new SqlParameter("UserPhone", user.UserPhone));
            colParameters.Add(new SqlParameter("UserDir", user.UserDir));
            colParameters.Add(new SqlParameter("UserRut", user.UserRut));
            colParameters.Add(new SqlParameter("RolId", user.UserRolId));
            colParameters.Add(new SqlParameter("UserDate", DateTime.Now.Date));

            //Ejecutar el comando y obtener el resultado
            dbc = new DbConnectionProvider();
            int id = Convert.ToInt32(dbc.ExecuteReturningId(query, "Id", colParameters, TableName));

            return id;
        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura con parámetros, retornando la data del usuario.
        /// </summary>
        /// <param name="UserId">Parámetro que contiene el Id del usuario</param>
        /// <returns>Data del usuario registrado en BD</returns>
        public Users Read(int UserId)
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
                                        LEFT JOIN Rol AS B ON B.Id = A.RolId 
                                        Where A.Id = {1}",
                                        TableName,
                                        UserId);

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

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura con parámetros, retornando la data del usuario.
        /// </summary>
        /// <param name="UserRut">Parámetro que contiene el Rut del usuario cliente</param>
        /// <returns>Data del usuario registrado en BD</returns>
        public Users ReadUserByrut(string UserRut)
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
                                        LEFT JOIN Rol AS B ON B.Id = A.RolId 
                                        WHERE UserRut = @UserRut",
                                        TableName);

            // Definir los paramatros de enviar
            List<SqlParameter> colParameters = new List<SqlParameter>();
            colParameters.Add(new SqlParameter("UserRut", UserRut));

            //Ejecutar el comando y obtener el resultado
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query,colParameters);

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

        /// <summary>
        /// Ejecuta una instrucción SQL de actualización con parámetros, actualizando el registro del usuario.
        /// </summary>
        /// <param name="user">Objeto de clase Users con los valores enviados desde el controlador</param>
        /// <returns>Entero de confirmación de procesamiento exitoso</returns>
        public int Update(Users user)
        {
            int result = 0;

            //Comprobar que no existan Editoriales con el mismo nombre.
            //Definir la query   
            string query = string.Format("SELECT {0} FROM {1} WHERE UserName = {2} AND UserLName = {3} AND UserEmail = {4} AND UserPhone = {5} AND UserDir = {6} AND UserRut = {7} AND RolId = {8}",
                                         Fields,
                                         TableName,
                                         "@UserName", "@UserLName", "@UserEmail", "@UserPhone", "@UserDir", "@UserRut" ,"@RolId");

            //Definir los parámetros
            List<SqlParameter> colParameters = new List<SqlParameter>();
            colParameters.Add(new SqlParameter("UserName", user.UserName));
            colParameters.Add(new SqlParameter("UserLName", user.UserLastName));
            colParameters.Add(new SqlParameter("UserEmail", user.UserEmail));
            colParameters.Add(new SqlParameter("UserPhone", user.UserPhone));
            colParameters.Add(new SqlParameter("UserDir", user.UserDir));
            colParameters.Add(new SqlParameter("UserRut", user.UserRut));

            colParameters.Add(new SqlParameter("RolId", user.UserRolId));

            //Ejecutar la query y obtener el resultado
            DbConnectionProvider tdBase = new DbConnectionProvider();
            DataTable data = tdBase.GetDataTable(query, colParameters);

            //Convertir y retornar la data
            if ((data == null) || (data.Rows.Count == 0))
            {
                try
                {
                    //Definir el comando
                    string command = string.Format("UPDATE {0} \nSET \n{1} \nWHERE Id = {2}",
                                                   TableName,
                                                   "UserName = @UserName, \n" +
                                                   "UserLName = @UserLName, \n" +
                                                   "UserEmail = @UserEmail, \n" +
                                                   "UserPhone = @UserPhone, \n" +
                                                   "UserDir = @UserDir, \n" +
                                                   "UserRut = @UserRut, \n" +
                                                   "RolId = @RolId",
                                                   "@Id");


                    //Definir los parámetros
                    colParameters = new List<SqlParameter>();
                    colParameters.Add(new SqlParameter("UserName", user.UserName));
                    colParameters.Add(new SqlParameter("UserLName", user.UserLastName));
                    colParameters.Add(new SqlParameter("UserEmail", user.UserEmail));
                    colParameters.Add(new SqlParameter("UserPhone", user.UserPhone));
                    colParameters.Add(new SqlParameter("UserDir",   user.UserDir));
                    colParameters.Add(new SqlParameter("UserRut", user.UserRut));
                    colParameters.Add(new SqlParameter("RolId", user.UserRolId));
                    colParameters.Add(new SqlParameter("Id", user.Id));

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
        /// Ejecuta una instrucción SQL de eliminación con parámetros, borrando el usuario de BD.
        /// </summary>
        /// <param name="UserId">Parámetro que contiene el ID del usuario</param>
        /// <returns>Entero de confirmación de procesamiento exitoso</returns>
        public int Delete(int UserId)
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
                colParameters.Add(new SqlParameter("Id", UserId));

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
        /// Ejecuta una instrucción SQL de lectura, retornando la data tipo lista de todos los usuarios.
        /// </summary>
        /// <returns>Data tipo lista de todos los usuarios registrados en BD</returns>
        public IEnumerable<Users> List()
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
                                        FROM {0} as A 
                                        LEFT JOIN Rol AS B ON B.Id = A.RolId",
                                        TableName);

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            // Crear lista de registros
            List<Users> usersList = new List<Users>();

            // Rellenar lista con registros
            if ((data != null) && (data.Rows.Count > 0))
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    usersList.Add(ConvertDtRowToDTO(data.Rows[i]));
                }
            }

            return usersList;

        }

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura, retornando la data tipo lista de todos los roles.
        /// </summary>
        /// <returns>Data tipo lista de todos los roles registrados en BD</returns>
        public IEnumerable<Rol> RolList()
        {
            string query = string.Format(@"SELECT 
                                        Id, 
                                        RolName   
                                        FROM Rol");

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            // Crear lista de registros
            List<Rol> userRolList = new List<Rol>();

            // Rellenar lista con registros
            if ((data != null) && (data.Rows.Count > 0))
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    userRolList.Add(ConvertDtRowURToDTO(data.Rows[i]));
                }
            }

            return userRolList;
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
                UserName = data["UserName"].ToString(),
                UserLastName = data["UserLName"].ToString(),
                UserEmail = data["UserEmail"].ToString(),
                UserRolId = (int)data["RolId"], 
                UserRolName = data["RolName"].ToString(),
                UserPassword = data["UserPass"].ToString(),
                UserPhone = data["UserPhone"].ToString(),
                UserDir = data["UserDir"].ToString(),
                UserRut = data["UserRut"].ToString(),
                UserDate = DateTime.Parse(data["UserDate"].ToString())
            };

            return user;
        }

        /// <summary>
        /// Convierte un DataTable en un objeto de clase Rol.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase Rol llenado con valores del DataTable</returns>
        protected Rol ConvertDtRowURToDTO(DataRow data)
        {
            Rol user = new Rol
            {
                Id = (int)data["Id"],
                RolName = data["RolName"].ToString(),
            };

            return user;
        }
        #endregion

    }
}
