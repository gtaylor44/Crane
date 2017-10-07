using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;

namespace Crane.Shared
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public abstract class BaseCommand : BaseInitialiser
    {
        /// <summary>
        /// Interface for executing a command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public abstract int ExecuteNonQuery(string command, int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null);

        /// <summary>
        /// Executes a command asynchronously.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public abstract Task<int> ExecuteNonQueryAsync(string command, int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null);

        /// <summary>
        /// Add an sql parameter to the command.
        /// </summary>
        /// <param name="sqlParameter"></param>
        /// <returns></returns>
        public BaseCommand AddSqlParameter(SqlParameter sqlParameter)
        {
            if (sqlParameter == null)
                throw new NullReferenceException(nameof(sqlParameter));

            ParamList.Add(sqlParameter);
            return this;
        }

        /// <summary>
        /// Add an sql parameter to the command.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public BaseCommand AddSqlParameter(string parameterName, object value)
        {
            if (parameterName == null)
                throw new NullReferenceException(nameof(parameterName));

            ParamList.Add(new SqlParameter() { Value = value ?? DBNull.Value, ParameterName = parameterName });
            return this;
        }

        /// <summary>
        /// Add an sql parameter to the command.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public BaseCommand AddSqlParameter(string parameterName, SqlDbType dbType, object value)
        {
            if (parameterName == null)
                throw new NullReferenceException(nameof(parameterName));

            ParamList.Add(new SqlParameter() { Value = value ?? DBNull.Value, ParameterName = parameterName, SqlDbType = dbType });
            return this;
        }

        /// <summary>
        /// Add a list of parameters to the command.
        /// </summary>
        /// <returns></returns>
        public BaseCommand AddSqlParameterCollection(IEnumerable<SqlParameter> sqlParameterCollection)
        {
            if (sqlParameterCollection == null)
                throw new NullReferenceException(nameof(sqlParameterCollection));

            ParamList.AddRange(sqlParameterCollection);
            return this;
        }
    }
}
