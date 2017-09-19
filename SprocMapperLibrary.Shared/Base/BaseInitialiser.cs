using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace SprocMapperLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseInitialiser
    {
        /// <summary>
        /// 
        /// </summary>
        protected List<SqlParameter> ParamList;

        /// <summary>
        /// 
        /// </summary>
        protected BaseInitialiser()
        {
            ParamList = new List<SqlParameter>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        protected void OpenConn(DbConnection conn)
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
        protected async Task OpenConnAsync(DbConnection conn)
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
        /// <param name="commandType"></param>
        protected void SetCommandProps(DbCommand command, int? commandTimeout, CommandType? commandType)
        {
            command.CommandType = commandType ?? CommandType.Text;

            if (commandTimeout.HasValue)
                command.CommandTimeout = commandTimeout.Value;

            if (ParamList != null && ParamList.Any())
                command.Parameters.AddRange(ParamList.ToArray());
        }
    }
}
