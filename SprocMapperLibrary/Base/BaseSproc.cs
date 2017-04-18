using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
using SprocMapperLibrary.Base;

// ReSharper disable once CheckNamespace
namespace SprocMapperLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseSproc : BaseInitialiser
    {
        /// <summary>
        /// 
        /// </summary>
        internal readonly List<ISprocObjectMap> SprocObjectMapList;
        /// <summary>
        /// 
        /// </summary>
        protected readonly Dictionary<Type, Dictionary<string, string>> CustomColumnMappings;
        /// <summary>
        /// 
        /// </summary>
        protected const bool ValidateSelectColumnsDefault = true;

        /// <summary>
        /// 
        /// </summary>
        protected readonly AbstractCacheProvider CacheProvider;

        private const char PartitionSplitOnChar = '|';


        /// <summary>
        /// 
        /// </summary>
        protected BaseSproc(AbstractCacheProvider cacheProvider) : base()
        {
            SprocObjectMapList = new List<ISprocObjectMap>();
            CustomColumnMappings = new Dictionary<Type, Dictionary<string, string>>();
            CacheProvider = cacheProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public abstract int ExecuteNonQuery(string storedProcedure, int? commandTimeout = null, DbConnection conn = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public abstract Task<int> ExecuteNonQueryAsync(string storedProcedure, int? commandTimeout = null, DbConnection conn = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract T ExecuteScalar<T>(string storedProcedure, int? commandTimeout = null, DbConnection conn = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract Task<T> ExecuteScalarAsync<T>(string storedProcedure, int? commandTimeout = null, DbConnection conn = null);

        /// <summary>
        /// 
        /// </summary>
        protected void ValidateCacheKey()
        {
            
        }

        /// <summary>
        /// If a property name does not match the corresponding column in select statement, 
        /// you can create a custom mapping. This is helpful in situations where you want to use 
        /// column aliasing in your stored procedure without losing mapping abilities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public BaseSproc CustomColumnMapping<T>(Expression<Func<T, object>> source, string destination) where T : class
        {
            var propertyName = SprocMapper.GetPropertyName(source);

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            Dictionary<string, string> customColumnDic;
            if (!CustomColumnMappings.TryGetValue(typeof(T), out customColumnDic))
            {
                Dictionary<string, string> newDic = new Dictionary<string, string>();
                CustomColumnMappings.Add(typeof(T), newDic);

                customColumnDic = newDic;
            }

            var typeAccessor = TypeAccessor.Create(typeof(T));

            //Get all properties
            MemberSet members = typeAccessor.GetMembers();

            foreach (var member in members)
            {
                if (member.Name.Equals(destination, StringComparison.OrdinalIgnoreCase))
                    throw new SprocMapperException($"Custom column mapping must map to a unique " +
                                                   $"property. A property with the name '{destination}' already exists.");
            }

            customColumnDic.Add(propertyName, destination);

            return this;
        }

        /// <summary>
        /// Performs synchronous version of stored procedure.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="getObjectDel"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="commandTimeout"></param>
        /// <param name="partitionOnArr"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        protected abstract IEnumerable<TResult> ExecuteReaderImpl<TResult>(
            Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr,
            bool validateSelectColumns, DbConnection conn, string cacheKey);


        /// <summary>
        /// Performs asynchronous version of stored procedure.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="getObjectDel"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="partitionOnArr"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        protected abstract Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(
            Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr,
            bool validateSelectColumns, DbConnection conn, string cacheKey);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlParameter"></param>
        /// <returns></returns>
        public BaseSproc AddSqlParameter(SqlParameter sqlParameter)
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
        public BaseSproc AddSqlParameter(string parameterName, object value)
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
        public BaseSproc AddSqlParameter(string parameterName, SqlDbType dbType, object value)
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
        public BaseSproc AddSqlParameterCollection(IEnumerable<SqlParameter> sqlParameterCollection)
        {
            if (sqlParameterCollection == null)
                throw new NullReferenceException(nameof(sqlParameterCollection));

            ParamList.AddRange(sqlParameterCollection);
            return this;
        }

        /// <summary>
        /// Perform a select statement against a single type.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult>(string storedProcedure, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
        {
            MapObject<TResult, INullType, INullType, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                res.Add(obj1);

            }, storedProcedure, commandTimeout, null, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement against a single type.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult>(string storedProcedure, Action<TResult> callBack, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
        {
            MapObject<TResult, INullType, INullType, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                callBack.Invoke(obj1);
                res.Add(obj1);

            }, storedProcedure, commandTimeout, null, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1>(string storedProcedure, Action<TResult, TJoin1> callBack,
            string partitionOn, string cacheKey = null, bool validateSelectColumns = ValidateSelectColumnsDefault, 
            int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
        {
            MapObject<TResult, TJoin1, INullType, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2>(string storedProcedure, Action<TResult, TJoin1, TJoin2> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3>(string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4>(string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4> callBack, 
            string partitionOn, string cacheKey = null, bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <typeparam name="TJoin5"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>(string storedProcedure, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = SprocMapper.GetObject<TJoin5>(SprocObjectMapList[5], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <typeparam name="TJoin5"></typeparam>
        /// <typeparam name="TJoin6"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>(string storedProcedure, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return ExecuteReaderImpl<TResult>((reader, result) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = SprocMapper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = SprocMapper.GetObject<TJoin6>(SprocObjectMapList[6], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

                result.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <typeparam name="TJoin5"></typeparam>
        /// <typeparam name="TJoin6"></typeparam>
        /// <typeparam name="TJoin7"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>(string storedProcedure, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
            where TJoin7 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return ExecuteReaderImpl<TResult>((reader, result) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = SprocMapper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = SprocMapper.GetObject<TJoin6>(SprocObjectMapList[6], reader);
                TJoin7 obj8 = SprocMapper.GetObject<TJoin7>(SprocObjectMapList[7], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

                result.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <typeparam name="TJoin5"></typeparam>
        /// <typeparam name="TJoin6"></typeparam>
        /// <typeparam name="TJoin7"></typeparam>
        /// <typeparam name="TJoin8"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>(string storedProcedure,
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
            where TJoin7 : class, new()
            where TJoin8 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return ExecuteReaderImpl<TResult>((reader, result) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = SprocMapper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = SprocMapper.GetObject<TJoin6>(SprocObjectMapList[6], reader);
                TJoin7 obj8 = SprocMapper.GetObject<TJoin7>(SprocObjectMapList[7], reader);
                TJoin8 obj9 = SprocMapper.GetObject<TJoin8>(SprocObjectMapList[8], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9);

                result.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement against a single type.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult>(string storedProcedure, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection conn = null)
             where TResult : class, new()
        {
            MapObject<TResult, INullType, INullType, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                res.Add(obj1);

            }, storedProcedure, commandTimeout, null, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement against a single type. 
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="callBack"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult>(string storedProcedure, Action<TResult> callBack, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection conn = null)
             where TResult : class, new()
        {
            MapObject<TResult, INullType, INullType, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                callBack.Invoke(obj1);
                res.Add(obj1);

            }, storedProcedure, commandTimeout, null, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1>(string storedProcedure, Action<TResult, TJoin1> callBack,
            string partitionOn, string cacheKey = null, bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
        {

            MapObject<TResult, TJoin1, INullType, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2>(string storedProcedure,
            Action<TResult, TJoin1, TJoin2> callBack, string partitionOn, string cacheKey = null, bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3>(string storedProcedure, 
            Action<TResult, TJoin1, TJoin2, TJoin3> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4>(string storedProcedure, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <typeparam name="TJoin5"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>(string storedProcedure, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = SprocMapper.GetObject<TJoin5>(SprocObjectMapList[5], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <typeparam name="TJoin5"></typeparam>
        /// <typeparam name="TJoin6"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>(string storedProcedure, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = SprocMapper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = SprocMapper.GetObject<TJoin6>(SprocObjectMapList[6], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <typeparam name="TJoin5"></typeparam>
        /// <typeparam name="TJoin6"></typeparam>
        /// <typeparam name="TJoin7"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>(string storedProcedure, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
            where TJoin7 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, INullType>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = SprocMapper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = SprocMapper.GetObject<TJoin6>(SprocObjectMapList[6], reader);
                TJoin7 obj8 = SprocMapper.GetObject<TJoin7>(SprocObjectMapList[7], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <typeparam name="TJoin5"></typeparam>
        /// <typeparam name="TJoin6"></typeparam>
        /// <typeparam name="TJoin7"></typeparam>
        /// <typeparam name="TJoin8"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="conn"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync
            <TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>(string storedProcedure,
                Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8> callBack,
                string partitionOn, string cacheKey = null,
                bool validateSelectColumns = ValidateSelectColumnsDefault,
                int? commandTimeout = null, DbConnection conn = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
            where TJoin7 : class, new()
            where TJoin8 : class, new()
        {
            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>(SprocObjectMapList, CustomColumnMappings);

            SprocMapper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = SprocMapper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = SprocMapper.GetObject<TJoin6>(SprocObjectMapList[6], reader);
                TJoin7 obj8 = SprocMapper.GetObject<TJoin7>(SprocObjectMapList[7], reader);
                TJoin8 obj9 = SprocMapper.GetObject<TJoin8>(SprocObjectMapList[7], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOnArr, validateSelectColumns, conn, cacheKey);
        }

        private void MapObject<T, T1, T2, T3, T4, T5, T6, T7, T8>(List<ISprocObjectMap> sprocObjectMapList, Dictionary<Type, Dictionary<string, string>> customColumnMappings)
        {
            HashSet<Type> tempHashSet = new HashSet<Type>();

            if (typeof(T) != typeof(INullType))
            {
                ValidateType(tempHashSet, typeof(T));
                SprocMapper.MapObject<T>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T1) != typeof(INullType))
            {
                ValidateType(tempHashSet, typeof(T1));
                SprocMapper.MapObject<T1>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T2) != typeof(INullType))
            {
                ValidateType(tempHashSet, typeof(T2));
                SprocMapper.MapObject<T2>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T3) != typeof(INullType))
            {
                ValidateType(tempHashSet, typeof(T3));
                SprocMapper.MapObject<T3>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T4) != typeof(INullType))
            {
                ValidateType(tempHashSet, typeof(T4));
                SprocMapper.MapObject<T4>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T5) != typeof(INullType))
            {
                ValidateType(tempHashSet, typeof(T5));
                SprocMapper.MapObject<T5>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T6) != typeof(INullType))
            {
                ValidateType(tempHashSet, typeof(T6));
                SprocMapper.MapObject<T6>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T7) != typeof(INullType))
            {
                ValidateType(tempHashSet, typeof(T7));
                SprocMapper.MapObject<T7>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T8) != typeof(INullType))
            {
                ValidateType(tempHashSet, typeof(T8));
                SprocMapper.MapObject<T8>(sprocObjectMapList, customColumnMappings);
            }
        }

        private void ValidateType(HashSet<Type> typeHashSet, Type targetType)
        {
            if (typeHashSet.Contains(targetType))
            {
                throw new SprocMapperException($"Each type in join set must be unique. The type '{targetType.FullName}' was seen more than once.");
            }

            typeHashSet.Add(targetType);
        }
    }
}
