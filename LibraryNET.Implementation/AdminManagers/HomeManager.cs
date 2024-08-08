using DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace LibraryNET.Implementation.AdminManagers
{
    public class HomeManager
    {
        //Agregar conexion a BD
        DbConnectionProvider dbc = new DbConnectionProvider();

        #region Public Methods

        /// <summary>
        /// Ejecuta una instrucción SQL de lectura con parámetros, retornando una data tipo lista paginada de todos los registros de compras y préstamos.
        /// </summary>
        /// <param name="page">Parámetro que contiene el numero de la página actual</param>
        /// <param name="pageSize">Parámetro que contiene el tamaño de la paginación</param>
        /// <returns>Data tipo lista paginada de todas las compras y préstamos registrados en BD</returns>
        public IEnumerable<Registers> GetRegisterData(int page, int pageSize)
        {
            int startIndex = (page - 1) * pageSize;

            string query = string.Format(@"
                                        -- Consulta para las transacciones de compra (Buys)
                                        SELECT  
                                            a.IdTransaction AS TransactionId,
                                            a.BuyTotalAmount AS TransactionAmount,
                                            a.BuyDate AS TransactionDate,  -- Usamos BuyDate como TransactionDate
                                            a.IdUser,
	                                        CONCAT(u.UserName, ' ',u.UserLName) AS Nombres,
                                            1 AS TransactionType  -- 1 para compras (buys)
                                        FROM buys a
                                        LEFT JOIN Users u ON a.IdUser = u.Id

                                        UNION ALL

                                        -- Consulta para las transacciones de préstamo (Borrows)
                                        SELECT  
                                            b.IdTransaction AS TransactionId,
                                            NULL AS TransactionAmount,
                                            b.BorrowRegisterDate AS TransactionDate,  -- Usamos BorrowRegisterDate como TransactionDate
                                            b.IdClient AS IdUser,
                                            CONCAT(u.UserName, ' ',u.UserLName) AS Nombres,
                                            2 AS TransactionType  -- 2 para préstamos (borrows)
                                        FROM borrows b
                                        LEFT JOIN Users u ON b.IdClient = u.Id

                                        ORDER BY TransactionDate DESC OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;",
                                        startIndex,
                                        pageSize);

            // Crear conexion y ejecutar la query
            dbc = new DbConnectionProvider();
            DataTable data = dbc.GetDataTable(query);

            // Crear lista de registros
            List<Registers> registerDataList = new List<Registers>();

            // Rellenar lista con registros
            if ((data != null) && (data.Rows.Count > 0))
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    registerDataList.Add(ConvertDtRowToDTOGet(data.Rows[i]));
                }
            }

            return registerDataList;

        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Convierte un DataTable en un objeto de clase Registers.
        /// </summary>
        /// <param name="data">DataTable con los valores traidos desde la consulta SQL</param>
        /// <returns>Objeto de clase Registers llenado con valores del DataTable</returns>
        protected Registers ConvertDtRowToDTOGet(DataRow data)
        {
            Registers registerDataDetail = new Registers
            {
                TrancId = data["TransactionId"].ToString(),
                TrancUserId = (int)data["IdUser"],
                TrancUserName = data["Nombres"].ToString(),
                TrancDate = Convert.ToDateTime(data["TransactionDate"]),
                TrancType = (int)data["TransactionType"],
                TrancAmount = Convert.IsDBNull(data["TransactionAmount"]) ? 0 : Convert.ToInt32(data["TransactionAmount"])
            };

            return registerDataDetail;
        }

        #endregion

    }
}