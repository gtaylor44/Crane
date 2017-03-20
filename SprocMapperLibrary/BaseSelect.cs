using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
using SprocMapperLibrary.Interface;

namespace SprocMapperLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseSelect : AbstractQuery
    {
        protected readonly List<ISprocObjectMap> SprocObjectMapList;
        protected readonly Dictionary<Type, Dictionary<string, string>> CustomColumnMappings;
        protected const bool ValidateSelectColumnsDefault = true;
        /// <summary>
        /// 
        /// </summary>
        protected BaseSelect() : base()
        {
            SprocObjectMapList = new List<ISprocObjectMap>();
            CustomColumnMappings = new Dictionary<Type, Dictionary<string, string>>();
        }

        /// <summary>
        /// Add an SqlParameter to be passed into stored procedure.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public BaseSelect AddSqlParameter(SqlParameter item)
        {
            ParamList.Add(item);
            return this;
        }

        /// <summary>
        /// Add an SqlParameter to be passed into stored procedure.
        /// </summary>
        /// <returns></returns>
        public BaseSelect AddSqlParameter(string parameterName, object value)
        {
            if (parameterName == null)
                throw new NullReferenceException(nameof(parameterName));

            ParamList.Add(new SqlParameter() { ParameterName = parameterName, Value = value });
            return this;
        }

        /// <summary>
        /// Add an SqlParameter to be passed into stored procedure.
        /// </summary>
        /// <returns></returns>
        public BaseSelect AddSqlParameter(string parameterName, SqlDbType dbType, object value)
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
        public BaseSelect AddSqlParameterCollection(IEnumerable<SqlParameter> sqlParameterCollection)
        {
            if (sqlParameterCollection == null)
                throw new NullReferenceException(nameof(sqlParameterCollection));

            ParamList.AddRange(sqlParameterCollection);
            return this;
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
        public BaseSelect CustomColumnMapping<T>(Expression<Func<T, object>> source, string destination) where T : class
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
        /// <param name="partitionOn"></param>
        /// <param name="validatePartitionOn"></param>
        /// <param name="validateSelectColumns"></param>
        /// <returns></returns>
        protected abstract IEnumerable<TResult> ExecuteReaderImpl<TResult>(
            Action<SqlDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string partitionOn,
            bool validatePartitionOn,
            bool validateSelectColumns);


        /// <summary>
        /// Performs asynchronous version of stored procedure.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="getObjectDel"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="partitionOn"></param>
        /// <param name="validatePartitionOn"></param>
        /// <param name="validateSelectColumns"></param>
        /// <returns></returns>
        protected abstract Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(
            Action<SqlDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string partitionOn,
            bool validatePartitionOn, bool validateSelectColumns);

        /// <summary>
        /// Perform a select statement returning a single entity.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult>(string storedProcedure, 
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null) 
            where TResult : class, new()
        {
            SprocMapper.MapObject<TResult, INullType, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                res.Add(obj1);

            }, storedProcedure, commandTimeout, null, false, validateSelectColumns);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1>(SqlConnection conn, string storedProcedure, Action<TResult, TJoin1> callBack,
            string partitionOn, bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null) 
            where TResult : class, new() 
            where TJoin1 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2>(SqlConnection conn, 
            string storedProcedure, Action<TResult, TJoin1, TJoin2> callBack, string partitionOn, 
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3>(SqlConnection conn, 
            string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3> callBack, string partitionOn, 
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, TJoin3, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4>(SqlConnection conn, 
            string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4> callBack, string partitionOn, 
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return ExecuteReaderImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
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
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>(SqlConnection conn, 
            string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5> callBack, string partitionOn, 
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

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

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
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
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>(SqlConnection conn, 
            string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6> callBack, string partitionOn, 
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, INullType>(SprocObjectMapList, CustomColumnMappings);

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

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
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
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public IEnumerable<TResult> ExecuteReader<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>(SqlConnection conn, 
            string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7> callBack, string partitionOn, 
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
            where TJoin7 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>(SprocObjectMapList, CustomColumnMappings);

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

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult>(SqlConnection conn, string storedProcedure, 
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null)
             where TResult : class, new()
        {
            SprocMapper.MapObject<TResult, INullType, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                res.Add(obj1);

            }, storedProcedure, commandTimeout, null, false, validateSelectColumns);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1>(SqlConnection conn, string storedProcedure, Action<TResult, TJoin1> callBack, 
            string partitionOn, bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
        {

            SprocMapper.MapObject<TResult, TJoin1, INullType, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2>(SqlConnection conn, string storedProcedure, 
            Action<TResult, TJoin1, TJoin2> callBack, string partitionOn, bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, INullType, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3>(SqlConnection conn, 
            string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3> callBack, string partitionOn, 
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, TJoin3, INullType, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
        }

        /// <summary>
        /// Perform a select statement returning more than one entity. Please see documentation for more information if you need help. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TJoin1"></typeparam>
        /// <typeparam name="TJoin2"></typeparam>
        /// <typeparam name="TJoin3"></typeparam>
        /// <typeparam name="TJoin4"></typeparam>
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4>(SqlConnection conn, 
            string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4> callBack, string partitionOn, 
            bool validateSelectColumns = ValidateSelectColumnsDefault,
            int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, INullType, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

            return await ExecuteReaderAsyncImpl<TResult>((reader, res) =>
            {
                TResult obj1 = SprocMapper.GetObject<TResult>(SprocObjectMapList[0], reader);
                TJoin1 obj2 = SprocMapper.GetObject<TJoin1>(SprocObjectMapList[1], reader);
                TJoin2 obj3 = SprocMapper.GetObject<TJoin2>(SprocObjectMapList[2], reader);
                TJoin3 obj4 = SprocMapper.GetObject<TJoin3>(SprocObjectMapList[3], reader);
                TJoin4 obj5 = SprocMapper.GetObject<TJoin4>(SprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
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
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5>(SqlConnection conn, 
            string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5> callBack, string partitionOn, 
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, INullType, INullType>(SprocObjectMapList, CustomColumnMappings);

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

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
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
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6>(SqlConnection conn, 
            string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6> callBack, string partitionOn,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, INullType>(SprocObjectMapList, CustomColumnMappings);

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

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
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
        /// <param name="conn"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="callBack">A delegate that is invoked for every row that is processed.</param>
        /// <param name="partitionOn">"A pipe delimited list that separates the table according to the start of each entity e.g. "Id|Id|Id|Id|Id|Id|Id|Id"</param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> ExecuteReaderAsync<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>(SqlConnection conn, 
            string storedProcedure, Action<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7> callBack, string partitionOn,
            bool validateSelectColumns = ValidateSelectColumnsDefault, int? commandTimeout = null)
            where TResult : class, new()
            where TJoin1 : class, new()
            where TJoin2 : class, new()
            where TJoin3 : class, new()
            where TJoin4 : class, new()
            where TJoin5 : class, new()
            where TJoin6 : class, new()
            where TJoin7 : class, new()
        {
            SprocMapper.MapObject<TResult, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7>(SprocObjectMapList, CustomColumnMappings);

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

            }, storedProcedure, commandTimeout, partitionOn, true, validateSelectColumns);
        }
    }
}