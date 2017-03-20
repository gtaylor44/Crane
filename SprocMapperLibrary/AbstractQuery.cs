using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SprocMapperLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractQuery
    {
        /// <summary>
        /// 
        /// </summary>
        protected List<SqlParameter> ParamList;       

        /// <summary>
        /// 
        /// </summary>
        protected AbstractQuery()
        {
            ParamList = new List<SqlParameter>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        protected void OpenConn(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        protected void OpenConn(MySqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        protected async Task OpenConnAsync(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        protected async Task OpenConnAsync(MySqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        protected void SetCommandProps(SqlCommand command, int? commandTimeout)
        {
            command.CommandType = CommandType.StoredProcedure;

            if (commandTimeout.HasValue)
                command.CommandTimeout = commandTimeout.Value;

            if (ParamList != null && ParamList.Any())
                command.Parameters.AddRange(ParamList.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        protected void SetCommandProps(MySqlCommand command, int? commandTimeout)
        {
            command.CommandType = CommandType.StoredProcedure;

            if (commandTimeout.HasValue)
                command.CommandTimeout = commandTimeout.Value;

            if (ParamList != null && ParamList.Any())
                command.Parameters.AddRange(ParamList.ToArray());
        }
    }
}
