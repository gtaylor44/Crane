using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SprocMapperLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseProcedure : BaseQuery
    {
        public BaseProcedure() : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public abstract int ExecuteNonQuery(string storedProcedure, int? commandTimeout = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public abstract Task<int> ExecuteNonQueryAsync(string storedProcedure, int? commandTimeout = null);

        public abstract T ExecuteScalar<T>(string storedProcedure, int? commandTimeout = null);

        public abstract Task<T> ExecuteScalarAsync<T>(string storedProcedure, int? commandTimeout = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public BaseProcedure AddSqlParameter(SqlParameter item)
        {
            ParamList.Add(item);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public BaseProcedure AddSqlParameter(string parameterName, object value)
        {
            if (parameterName == null)
                throw new NullReferenceException(nameof(parameterName));

            ParamList.Add(new SqlParameter() { Value = value, ParameterName = parameterName });
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public BaseProcedure AddSqlParameter(string parameterName, SqlDbType dbType, object value)
        {
            if (parameterName == null)
                throw new NullReferenceException(nameof(parameterName));

            ParamList.Add(new SqlParameter() { Value = value, ParameterName = parameterName, SqlDbType = dbType });
            return this;
        }

        /// <summary>
        /// Adds a list of SqlParameters to be passed into stored procedure.
        /// </summary>
        /// <returns></returns>
        public BaseProcedure AddSqlParameterCollection(IEnumerable<SqlParameter> sqlParameterCollection)
        {
            if (sqlParameterCollection == null)
                throw new NullReferenceException(nameof(sqlParameterCollection));

            ParamList.AddRange(sqlParameterCollection);
            return this;
        }
    }
}
