using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SprocMapperLibrary
{
    public abstract class AbstractQuery
    {
        protected List<SqlParameter> ParamList;       

        protected AbstractQuery()
        {
            ParamList = new List<SqlParameter>();
        }

        protected void OpenConn(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }

        protected async Task OpenConnAsync(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
        }

        protected void SetCommandProps(SqlCommand command, int commandTimeout)
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = commandTimeout;

            if (ParamList != null && ParamList.Any())
                command.Parameters.AddRange(ParamList.ToArray());
        }
    }
}
