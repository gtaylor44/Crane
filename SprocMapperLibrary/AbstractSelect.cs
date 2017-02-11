using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace SprocMapperLibrary
{
    public abstract class AbstractSelect
    {
        protected List<SqlParameter> ParamList;
        protected List<ISprocObjectMap> SprocObjectMapList;

        protected AbstractSelect(List<ISprocObjectMap> sprocObjectMapList)
        {
            ParamList = new List<SqlParameter>();
            SprocObjectMapList = sprocObjectMapList;
        }

        protected void AddSqlParameterList(List<SqlParameter> paramList)
        {
            if (paramList == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("AddSqlParameterList does not accept null value");

            ParamList = paramList;
        }

        protected void ValidateProperties()
        {
            if (!SprocMapperHelper.ValidateProperies(SprocObjectMapList))
            {
                throw new SprocMapperException($"Duplicate column not allowed. Ensure that all columns in stored procedure are unique." +
                                               "Try setting an alias for your column in your stored procedure " +
                                               "and set up a custom column mapping.");
            }
        }

        protected void OpenConn(SqlConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
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
