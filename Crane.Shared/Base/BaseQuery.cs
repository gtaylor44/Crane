using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Crane.CacheProvider;
using Crane.Model;

// ReSharper disable once CheckNamespace
namespace Crane
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public abstract class BaseQuery : BaseInitialiser
    {
        /// <summary>
        /// 
        /// </summary>
        internal readonly List<ICraneObjectMap> SprocObjectMapList;
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
        protected readonly AbstractCraneCacheProvider CacheProvider;

        private const char PartitionSplitOnChar = '|';


        /// <inheritdoc />
        /// <summary>
        /// Interface for executing a query.
        /// </summary>
        protected BaseQuery(AbstractCraneCacheProvider cacheProvider) : base()
        {
            SprocObjectMapList = new List<ICraneObjectMap>();
            CustomColumnMappings = new Dictionary<Type, Dictionary<string, string>>();
            CacheProvider = cacheProvider;
        }

        /// <summary>
        /// Validates that cache provider is instantiated.
        /// </summary>
        protected void ValidateCacheKey(string cacheKey)
        {
            if (CacheProvider == null && cacheKey != null)
            {
                throw new CraneException("A cache key has been provided without a cache provider. " +
                                                    "Use the method 'RegisterCacheProvider' to register a cache provider.");
            }
        }

        /// <summary>
        /// If a property name does not match the corresponding column in select statement 
        /// (due to aliasing or unmatching columns between table and model representation), 
        /// create a custom mapping. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">
        /// Lambda style property selector (e.g. x => x.PropertyName)
        /// </param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public BaseQuery CustomColumnMapping<T>(Expression<Func<T, object>> source, string destination) where T : class
        {
            var propertyName = CraneHelper.GetPropertyName(source);

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            Dictionary<string, string> customColumnDic;
            if (!CustomColumnMappings.TryGetValue(typeof(T), out customColumnDic))
            {
                Dictionary<string, string> newDic = new Dictionary<string, string>();
                CustomColumnMappings.Add(typeof(T), newDic);

                customColumnDic = newDic;
            }

            var typeAccessor = (T)Activator.CreateInstance(typeof(T));

            //Get all properties
            var members = typeof(T).GetRuntimeProperties();

            foreach (var member in members)
            {
                if (member.Name.Equals(destination, StringComparison.OrdinalIgnoreCase))
                    throw new CraneException($"Custom column mapping must map to a unique " +
                                                   $"property. A property with the name '{destination}' already exists.");
            }

            customColumnDic.Add(propertyName, destination);

            return this;
        }

        /// <summary>
        /// Perform a query and map to model.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="getObjectDel"></param>
        /// <param name="command">The command can either be plain SQL or reference to a stored procedure.</param>
        /// <param name="commandTimeout"></param>
        /// <param name="partitionOnArr"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="saveCacheDel"></param>
        /// <param name="valueOrStringType"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        protected abstract IEnumerable<TResult> ExecuteReaderImpl<TResult>(
            Action<DbDataReader, List<TResult>> getObjectDel,
            string command, int? commandTimeout, string[] partitionOnArr,
            bool validateSelectColumns, DbConnection dbConnection, DbTransaction transaction, 
            string cacheKey, Action saveCacheDel, bool valueOrStringType = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getObjectDel"></param>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <param name="cacheKey"></param>
        /// <param name="saveCacheDe"></param>
        /// <param name="transaction"></param>
        protected abstract IEnumerable<dynamic> ExecuteDynamicReaderImpl(Action<dynamic, List<dynamic>> getObjectDel,
            string command, int? commandTimeout, DbConnection userConn,
            DbTransaction transaction, string cacheKey, Action saveCacheDe);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getObjectDel"></param>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <param name="cacheKey"></param>
        /// <param name="saveCacheDe"></param>
        /// <param name="transaction"></param>
        protected abstract Task<IEnumerable<dynamic>> ExecuteDynamicReaderImplAsync(Action<dynamic, List<dynamic>> getObjectDel,
            string command, int? commandTimeout, DbConnection userConn,
            DbTransaction transaction, string cacheKey, Action saveCacheDe);

        /// <summary>
        /// Performs asynchronous version of stored procedure.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="getObjectDel"></param>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="partitionOnArr"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="saveCacheDel"></param>
        /// <param name="valueOrStringType"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        protected abstract Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(
            Action<DbDataReader, List<TResult>> getObjectDel,
            string command, int? commandTimeout, string[] partitionOnArr,
            bool validateSelectColumns, DbConnection dbConnection, DbTransaction transaction, 
            string cacheKey, Action saveCacheDel, bool valueOrStringType = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="transaction"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract T ExecuteScalar<T>(string command, int? commandTimeout = null,
            DbConnection dbConnection = null, DbTransaction transaction = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="transaction"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract Task<T> ExecuteScalarAsync<T>(string command, int? commandTimeout = null, 
            DbConnection dbConnection = null, DbTransaction transaction = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlParameter"></param>
        /// <returns></returns>
        public BaseQuery AddSqlParameter(SqlParameter sqlParameter)
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
        public BaseQuery AddSqlParameter(string parameterName, object value)
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
        public BaseQuery AddSqlParameter(string parameterName, SqlDbType dbType, object value)
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
        public BaseQuery AddSqlParameterCollection(IEnumerable<SqlParameter> sqlParameterCollection)
        {
            if (sqlParameterCollection == null)
                throw new NullReferenceException(nameof(sqlParameterCollection));

            ParamList.AddRange(sqlParameterCollection);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cacheKey"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="transaction"></param>
        public IEnumerable<dynamic> ExecuteReader(string command, string cacheKey = null,
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<dynamic> cachedResult;

            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                return cachedResult;
            }

            List<dynamic> cacheList = new List<dynamic>();

            return ExecuteDynamicReaderImpl((row, list) =>
            {
                list.Add(row);

                if (cacheKey != null)
                {
                    cacheList.Add(row);
                }

            }, command, commandTimeout, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="callBack"></param>
        /// <param name="cacheKey"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="transaction"></param>
        public IEnumerable<dynamic> ExecuteReader(string command, Action<dynamic> callBack, string cacheKey = null,
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<dynamic> cachedResult;

            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var enumerable = cachedResult.ToList();
                foreach (var item in enumerable)
                {
                    callBack.Invoke(item);
                }

                return enumerable;
            }

            List<dynamic> cacheList = new List<dynamic>();

            return ExecuteDynamicReaderImpl((row, list) =>
            {
                callBack.Invoke(row);
                list.Add(row);

                if (cacheKey != null)
                {
                    cacheList.Add(row);
                }

            }, command, commandTimeout, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement against a single type.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult>(string command, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, 
            DbConnection dbConnection = null, DbTransaction transaction = null)
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<TResult> cachedResult;

            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                return cachedResult;
            }

            List<TResult> cacheList = new List<TResult>();

            var type = typeof(TResult);

            if (type == typeof(object))
            {
                return ExecuteDynamicReaderImpl((row, list) =>
                {
                    list.Add(row);

                    if (cacheKey != null)
                    {
                        cacheList.Add((TResult)row);
                    }

                }, command, commandTimeout, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList)) as dynamic;
            }

            if (type.GetTypeInfo().IsValueType || type == typeof(string))
            {
                return ExecuteReaderImpl<TResult>((reader, res) =>
                {
                    TResult obj1 = (TResult)reader[0];
                    res.Add(obj1);

                    if (cacheKey != null)
                    {
                        cacheList.Add(obj1);
                    }

                }, command, commandTimeout, null, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList), valueOrStringType: true);
            }

            MapObject<TResult, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(obj1);
                }

            }, command, commandTimeout, null, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement against a single type.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult>(string command, Action<TResult> callBack, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, 
            DbConnection dbConnection = null, DbTransaction transaction = null)
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<TResult> cachedResult;

            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var enumerable = cachedResult.ToList();
                foreach (var item in enumerable)
                {
                    callBack.Invoke(item);
                }

                return enumerable;
            }

            List<TResult> cacheList = new List<TResult>();

            var type = typeof(TResult);

            if (type == typeof(object))
            {
                return ExecuteDynamicReaderImpl((row, list) =>
                {
                    callBack.Invoke((TResult)row);
                    list.Add(row);

                    if (cacheKey != null)
                    {
                        cacheList.Add((TResult)row);
                    }

                }, command, commandTimeout, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList)) as dynamic;
            }

            if (type.GetTypeInfo().IsValueType || type == typeof(string))
            {
                return ExecuteReaderImpl<TResult>((reader, res) =>
                {
                    TResult obj1 = (TResult)reader[0];
                    res.Add(obj1);

                    if (cacheKey != null)
                    {
                        cacheList.Add(obj1);
                    }

                    callBack.Invoke(obj1);

                }, command, commandTimeout, null, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList), valueOrStringType: true);
            }

            MapObject<TResult, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(obj1);
                }

                callBack.Invoke(obj1);

            }, command, commandTimeout, null, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1>(string command, Action<TResult, TJoin1> callBack,
            string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, 
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<OneJoin<TResult, TJoin1>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var oneJoins = cachedResult.ToList();
                foreach (var item in oneJoins)
                {
                    callBack.Invoke(item.Result, item.Join1);
                }

                return oneJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<OneJoin<TResult, TJoin1>> cacheList = new List<OneJoin<TResult, TJoin1>>();
            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new OneJoin<TResult, TJoin1>
                    {
                        Result = obj1,
                        Join1 = obj2             
                    });
                }

                callBack.Invoke(obj1, obj2);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2>(string command, Action<TResult, TJoin1, TJoin2> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<TwoJoin<TResult, TJoin1, TJoin2>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var twoJoins = cachedResult.ToList();
                foreach (var item in twoJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2);
                }

                return twoJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<TwoJoin<TResult, TJoin1, TJoin2>> cacheList = new List<TwoJoin<TResult, TJoin1, TJoin2>>();

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new TwoJoin<TResult, TJoin1, TJoin2>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3
                    });
                }

                callBack.Invoke(obj1, obj2, obj3);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3>(string command, Action<TResult, TJoin1, TJoin2, TJoin3> callBack, 
            string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<ThreeJoin<TResult, TJoin1, TJoin2, TJoin3>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var threeJoins = cachedResult.ToList();
                foreach (var item in threeJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3);
                }

                return threeJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<ThreeJoin<TResult, TJoin1, TJoin2, TJoin3>> cacheList = new List<ThreeJoin<TResult, TJoin1, TJoin2, TJoin3>>();
            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new ThreeJoin<TResult, TJoin1, TJoin2, TJoin3>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4>(string command, Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4> callBack, 
            string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<FourJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var fourJoins = cachedResult.ToList();
                foreach (var item in fourJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3, item.Join4);
                }

                return fourJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);


            List<FourJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4>> cacheList = new List<FourJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4>>();
            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = CraneHelper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                res.Add(obj1);
                
                if (cacheKey != null)
                {
                    cacheList.Add(new FourJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4, 
                        Join4 = obj5
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
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
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>(string command, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<FiveJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var fiveJoins = cachedResult.ToList();
                foreach (var item in fiveJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3, item.Join4, item.Join5);
                }

                return fiveJoins.Select(x => x.Result);

            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<FiveJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>> cacheList = new List<FiveJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>>();
            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = CraneHelper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = CraneHelper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new FiveJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4,
                        Join4 = obj5,
                        Join5 = obj6
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
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
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>(string command, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<SixJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var sixJoins = cachedResult.ToList();
                foreach (var item in sixJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3, item.Join4, item.Join5, item.Join6);
                }

                return sixJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<SixJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>> cacheList = new List<SixJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>>();
            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = CraneHelper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = CraneHelper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = CraneHelper.GetObject<TJoin6>(SprocObjectMapList[6], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new SixJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4,
                        Join4 = obj5,
                        Join5 = obj6,
                        Join6 = obj7
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
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
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>(string command, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
            where TJoin7 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<SevenJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var sevenJoins = cachedResult.ToList();
                foreach (var item in sevenJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3, item.Join4, item.Join5, item.Join6, item.Join7);
                }

                return sevenJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<SevenJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>> cacheList
                = new List<SevenJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>>();

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = CraneHelper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = CraneHelper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = CraneHelper.GetObject<TJoin6>(SprocObjectMapList[6], reader);
                TJoin7 obj8 = CraneHelper.GetObject<TJoin7>(SprocObjectMapList[7], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new SevenJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4,
                        Join4 = obj5,
                        Join5 = obj6,
                        Join6 = obj7,
                        Join7 = obj8
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
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
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>(string command,
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
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
            ValidateCacheKey(cacheKey);
            IEnumerable<EightJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var eightJoins = cachedResult.ToList();
                foreach (var item in eightJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3, item.Join4, item.Join5, item.Join6, item.Join7, item.Join8);
                }

                return eightJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<EightJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>> cacheList
                = new List<EightJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>>();

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = CraneHelper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = CraneHelper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = CraneHelper.GetObject<TJoin6>(SprocObjectMapList[6], reader);
                TJoin7 obj8 = CraneHelper.GetObject<TJoin7>(SprocObjectMapList[7], reader);
                TJoin8 obj9 = CraneHelper.GetObject<TJoin8>(SprocObjectMapList[8], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new EightJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4,
                        Join4 = obj5,
                        Join5 = obj6,
                        Join6 = obj7,
                        Join7 = obj8,
                        Join8 = obj9
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cacheKey"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="transaction"></param>
        public async Task<IEnumerable<dynamic>> ExecuteReaderAsync(string command, string cacheKey = null,
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<dynamic> cachedResult;

            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                return cachedResult;
            }

            List<dynamic> cacheList = new List<dynamic>();

            return await ExecuteDynamicReaderImplAsync((row, list) =>
            {
                list.Add(row);

                if (cacheKey != null)
                {
                    cacheList.Add(row);
                }

            }, command, commandTimeout, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="callBack"></param>
        /// <param name="cacheKey"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="transaction"></param>
        public async Task<IEnumerable<dynamic>> ExecuteReaderAsync(string command, Action<dynamic> callBack, string cacheKey = null,
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<dynamic> cachedResult;

            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var enumerable = cachedResult.ToList();
                foreach (var item in enumerable)
                {
                    callBack.Invoke(item);
                }

                return enumerable;
            }

            List<dynamic> cacheList = new List<dynamic>();

            return await ExecuteDynamicReaderImplAsync((row, list) =>
            {
                callBack.Invoke(row);
                list.Add(row);

                if (cacheKey != null)
                {
                    cacheList.Add(row);
                }

            }, command, commandTimeout, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement against a single type.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult>(string command, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, 
            DbConnection dbConnection = null, DbTransaction transaction = null)
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<TResult> cachedResult;

            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                return cachedResult;
            }

            List<TResult> cacheList = new List<TResult>();

            if (typeof(TResult).GetTypeInfo().IsValueType || typeof(TResult) == typeof(string))
            {
                return ExecuteReaderImpl<TResult>((reader, res) =>
                {
                    TResult obj1 = (TResult)reader[0];
                    res.Add(obj1);

                    if (cacheKey != null)
                    {
                        cacheList.Add(obj1);
                    }

                }, command, commandTimeout, null, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList), valueOrStringType: true);
            }

            MapObject<TResult, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(obj1);
                }

            }, command, commandTimeout, null, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement against a single type. 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="callBack"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult>(string command, Action<TResult> callBack, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<TResult> cachedResult;

            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var enumerable = cachedResult.ToList(); ;
                foreach (var item in enumerable)
                {
                    callBack.Invoke(item);
                }

                return enumerable;
            }

            List<TResult> cacheList = new List<TResult>();

            if (typeof(TResult).GetTypeInfo().IsValueType || typeof(TResult) == typeof(string))
            {
                return ExecuteReaderImpl<TResult>((reader, res) =>
                {
                    TResult obj1 = (TResult)reader[0];
                    res.Add(obj1);

                    if (cacheKey != null)
                    {
                        cacheList.Add(obj1);
                    }

                    callBack.Invoke(obj1);

                }, command, commandTimeout, null, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList), valueOrStringType: true);
            }

            MapObject<TResult, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings); 

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(obj1);
                }

                callBack.Invoke(obj1);

            }, command, commandTimeout, null, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1>(string command, Action<TResult, TJoin1> callBack,
            string partitionOn, string cacheKey = null, bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<OneJoin<TResult, TJoin1>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var oneJoins = cachedResult.ToList();
                foreach (var item in oneJoins)
                {
                    callBack.Invoke(item.Result, item.Join1);
                }

                return oneJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<OneJoin<TResult, TJoin1>> cacheList = new List<OneJoin<TResult, TJoin1>>();

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new OneJoin<TResult, TJoin1>
                    {
                        Result = obj1,
                        Join1 = obj2
                    });
                }

                callBack.Invoke(obj1, obj2);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2>(string command,
            Action<TResult, TJoin1, TJoin2> callBack, string partitionOn, string cacheKey = null, 
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<TwoJoin<TResult, TJoin1, TJoin2>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var twoJoins = cachedResult.ToList();
                foreach (var item in twoJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2);
                }

                return twoJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<TwoJoin<TResult, TJoin1, TJoin2>> cacheList = new List<TwoJoin<TResult, TJoin1, TJoin2>>();

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new TwoJoin<TResult, TJoin1, TJoin2>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3
                    });
                }

                callBack.Invoke(obj1, obj2, obj3);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3>(string command, 
            Action<TResult, TJoin1, TJoin2, TJoin3> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<ThreeJoin<TResult, TJoin1, TJoin2, TJoin3>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var threeJoins = cachedResult.ToList();
                foreach (var item in threeJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3);
                }

                return threeJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<ThreeJoin<TResult, TJoin1, TJoin2, TJoin3>> cacheList = new List<ThreeJoin<TResult, TJoin1, TJoin2, TJoin3>>();

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new ThreeJoin<TResult, TJoin1, TJoin2, TJoin3>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4>(string command, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<FourJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var fourJoins = cachedResult.ToList();
                foreach (var item in fourJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3, item.Join4);
                }

                return fourJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, ICraneNullType, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);


            List<FourJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4>> cacheList = new List<FourJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4>>();

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = CraneHelper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new FourJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4,
                        Join4 = obj5
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
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
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>(string command, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<FiveJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var fiveJoins = cachedResult.ToList();
                foreach (var item in fiveJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3, item.Join4, item.Join5);
                }

                return fiveJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, ICraneNullType, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<FiveJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>> cacheList = new List<FiveJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>>();

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = CraneHelper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = CraneHelper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new FiveJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4,
                        Join4 = obj5,
                        Join5 = obj6
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
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
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>(string command, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<SixJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var sixJoins = cachedResult.ToList();
                foreach (var item in sixJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3, item.Join4, item.Join5, item.Join6);
                }

                return sixJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, ICraneNullType, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<SixJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>> cacheList = new List<SixJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>>();

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = CraneHelper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = CraneHelper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = CraneHelper.GetObject<TJoin6>(SprocObjectMapList[6], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new SixJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4,
                        Join4 = obj5,
                        Join5 = obj6,
                        Join6 = obj7
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
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
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>(string command, 
            Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7> callBack, string partitionOn, string cacheKey = null,
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
            where TJoin7 : class, new()
        {
            ValidateCacheKey(cacheKey);
            IEnumerable<SevenJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var sevenJoins = cachedResult.ToList();
                foreach (var item in sevenJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3, item.Join4, item.Join5, item.Join6, item.Join7);
                }

                return sevenJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, ICraneNullType>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<SevenJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>> cacheList
                = new List<SevenJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>>();

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = CraneHelper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = CraneHelper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = CraneHelper.GetObject<TJoin6>(SprocObjectMapList[6], reader);
                TJoin7 obj8 = CraneHelper.GetObject<TJoin7>(SprocObjectMapList[7], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new SevenJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4,
                        Join4 = obj5,
                        Join5 = obj6,
                        Join6 = obj7,
                        Join7 = obj8
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
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
        /// <param name="command">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="dbConnection"></param>
        /// <param name="cacheKey"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync
            <TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>(string command,
                Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8> callBack,
                string partitionOn, string cacheKey = null,
                bool validateSelectColumns = ValidateSelectColumnsDefault,
                int? commandTimeout = null, DbConnection dbConnection = null, DbTransaction transaction = null)
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
            ValidateCacheKey(cacheKey);
            IEnumerable<EightJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>> cachedResult;
            if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
            {
                if (cachedResult == null)
                    return null;

                var eightJoins = cachedResult.ToList();
                foreach (var item in eightJoins)
                {
                    callBack.Invoke(item.Result, item.Join1, item.Join2, item.Join3, item.Join4, item.Join5, item.Join6, item.Join7, item.Join8);
                }

                return eightJoins.Select(x => x.Result);
            }

            MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>(SprocObjectMapList, CustomColumnMappings);

            CraneHelper.ValidatePartitionOn(partitionOn);
            var partitionOnArr = partitionOn.Split(PartitionSplitOnChar);

            List<EightJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>> cacheList
                = new List<EightJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>>();

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = CraneHelper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = CraneHelper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = CraneHelper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = CraneHelper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = CraneHelper.GetObject<TJoin4>(SprocObjectMapList[4], reader);
                TJoin5 obj6 = CraneHelper.GetObject<TJoin5>(SprocObjectMapList[5], reader);
                TJoin6 obj7 = CraneHelper.GetObject<TJoin6>(SprocObjectMapList[6], reader);
                TJoin7 obj8 = CraneHelper.GetObject<TJoin7>(SprocObjectMapList[7], reader);
                TJoin8 obj9 = CraneHelper.GetObject<TJoin8>(SprocObjectMapList[7], reader);
                res.Add(obj1);

                if (cacheKey != null)
                {
                    cacheList.Add(new EightJoin<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8>
                    {
                        Result = obj1,
                        Join1 = obj2,
                        Join2 = obj3,
                        Join3 = obj4,
                        Join4 = obj5,
                        Join5 = obj6,
                        Join6 = obj7,
                        Join7 = obj8,
                        Join8 = obj9
                    });
                }

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9);

            }, command, commandTimeout, partitionOnArr, validateSelectColumns, dbConnection, transaction, cacheKey, () => CacheProvider.Add(cacheKey, cacheList));
        }

        private void MapObject<T, T1, T2, T3, T4, T5, T6, T7, T8>(List<ICraneObjectMap> sprocObjectMapList, Dictionary<Type, Dictionary<string, string>> customColumnMappings)
        {
            HashSet<Type> tempHashSet = new HashSet<Type>();

            if (typeof(T) != typeof(ICraneNullType))
            {
                ValidateType(tempHashSet, typeof(T));
                CraneHelper.MapObject<T>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T1) != typeof(ICraneNullType))
            {
                ValidateType(tempHashSet, typeof(T1));
                CraneHelper.MapObject<T1>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T2) != typeof(ICraneNullType))
            {
                ValidateType(tempHashSet, typeof(T2));
                CraneHelper.MapObject<T2>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T3) != typeof(ICraneNullType))
            {
                ValidateType(tempHashSet, typeof(T3));
                CraneHelper.MapObject<T3>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T4) != typeof(ICraneNullType))
            {
                ValidateType(tempHashSet, typeof(T4));
                CraneHelper.MapObject<T4>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T5) != typeof(ICraneNullType))
            {
                ValidateType(tempHashSet, typeof(T5));
                CraneHelper.MapObject<T5>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T6) != typeof(ICraneNullType))
            {
                ValidateType(tempHashSet, typeof(T6));
                CraneHelper.MapObject<T6>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T7) != typeof(ICraneNullType))
            {
                ValidateType(tempHashSet, typeof(T7));
                CraneHelper.MapObject<T7>(sprocObjectMapList, customColumnMappings);
            }

            if (typeof(T8) != typeof(ICraneNullType))
            {
                ValidateType(tempHashSet, typeof(T8));
                CraneHelper.MapObject<T8>(sprocObjectMapList, customColumnMappings);
            }
        }

        private void ValidateType(HashSet<Type> typeHashSet, Type targetType)
        {
            if (typeHashSet.Contains(targetType))
            {
                throw new CraneException($"Each type in join set must be unique. The type '{targetType.FullName}' was seen more than once.");
            }

            typeHashSet.Add(targetType);
        }
    }
}
