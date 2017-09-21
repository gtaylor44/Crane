﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SprocMapperLibrary.CacheProvider;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;

// ReSharper disable once CheckNamespace
namespace SprocMapperLibrary
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public abstract class BaseCommand : BaseInitialiser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <returns></returns>
        public abstract int ExecuteNonQuery(string command, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <returns></returns>
        public abstract Task<int> ExecuteNonQueryAsync(string command, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <param name="commandType"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract T ExecuteScalar<T>(string command, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract Task<T> ExecuteScalarAsync<T>(string command, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null);

        /// <summary>
        /// 
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
        /// 
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
        /// 
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
        /// Adds a list of SqlParameters to be passed into stored procedure.
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