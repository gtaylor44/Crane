using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
namespace SprocMapperLibrary
{
    public class Select<T>
    {
        private List<SqlParameter> _paramList;
        private List<ISprocObjectMap> _sprocObjectMapList;

        public Select(List<ISprocObjectMap> sprocObjectMapList)
        {
            _paramList = new List<SqlParameter>();
            _sprocObjectMapList = sprocObjectMapList;
        }

        public Select<T> AddSqlParameterList(List<SqlParameter> paramList)
        {
            if (paramList == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("AddSqlParameterList does not accept null value");

            _paramList = paramList;
            return this;
        }

        public List<T> ExecuteReader(SqlConnection conn, string cmdText, int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();

            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T obj = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    result.Add(obj);
                }

            }
            return result;

        }

        public List<T> ExecuteReader<T1, T2>(SqlConnection conn, string cmdText, Func<T1, T2, T> customMethod,
            int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[1].Columns,
                        _sprocObjectMapList[1].CustomColumnMappings, reader);

                    T obj = customMethod.Invoke(obj1, obj2);

                    result.Add(obj);
                }

            }
            return result;
        }

        public List<T> ExecuteReader<T1, T2, T3>(SqlConnection conn, string cmdText, Func<T1, T2, T3, T> customMethod,
            int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[1].Columns,
                        _sprocObjectMapList[1].CustomColumnMappings, reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[2].Columns,
                        _sprocObjectMapList[2].CustomColumnMappings, reader);

                    T obj = customMethod.Invoke(obj1, obj2, obj3);

                    result.Add(obj);
                }

            }
            return result;
        }

        public List<T> ExecuteReader<T1, T2, T3, T4>(SqlConnection conn, string cmdText, Func<T1, T2, T3, T4, T> customMethod,
             int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[1].Columns,
                        _sprocObjectMapList[1].CustomColumnMappings, reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[2].Columns,
                        _sprocObjectMapList[2].CustomColumnMappings, reader);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[3].Columns,
                        _sprocObjectMapList[3].CustomColumnMappings, reader);

                    T obj = customMethod.Invoke(obj1, obj2, obj3, obj4);

                    result.Add(obj);
                }

            }
            return result;
        }

        public List<T> ExecuteReader<T1, T2, T3, T4, T5>(SqlConnection conn, string cmdText, Func<T1, T2, T3, T4, T5, T> customMethod,
            int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[1].Columns,
                        _sprocObjectMapList[1].CustomColumnMappings, reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[2].Columns,
                        _sprocObjectMapList[2].CustomColumnMappings, reader);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[3].Columns,
                        _sprocObjectMapList[3].CustomColumnMappings, reader);
                    T5 obj5 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[4].Columns,
                        _sprocObjectMapList[4].CustomColumnMappings, reader);

                    T obj = customMethod.Invoke(obj1, obj2, obj3, obj4, obj5);

                    result.Add(obj);
                }

            }
            return result;
        }

        public List<T> ExecuteReader<T1, T2, T3, T4, T5, T6>(SqlConnection conn, string cmdText, Func<T1, T2, T3, T4, T5, T6, T> customMethod,
              int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[1].Columns,
                        _sprocObjectMapList[1].CustomColumnMappings, reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[2].Columns,
                        _sprocObjectMapList[2].CustomColumnMappings, reader);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[3].Columns,
                        _sprocObjectMapList[3].CustomColumnMappings, reader);
                    T5 obj5 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[4].Columns,
                        _sprocObjectMapList[4].CustomColumnMappings, reader);
                    T6 obj6 = SprocMapperHelper.GetObject<T6>(_sprocObjectMapList[5].Columns,
                         _sprocObjectMapList[5].CustomColumnMappings, reader);

                    T obj = customMethod.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

                    result.Add(obj);
                }

            }
            return result;
        }

        public List<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string cmdText, Func<T1, T2, T3, T4, T5, T6, T7, T> customMethod,
            int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[1].Columns,
                        _sprocObjectMapList[1].CustomColumnMappings, reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[2].Columns,
                        _sprocObjectMapList[2].CustomColumnMappings, reader);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[3].Columns,
                        _sprocObjectMapList[3].CustomColumnMappings, reader);
                    T5 obj5 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[4].Columns,
                        _sprocObjectMapList[4].CustomColumnMappings, reader);
                    T6 obj6 = SprocMapperHelper.GetObject<T6>(_sprocObjectMapList[5].Columns,
                         _sprocObjectMapList[5].CustomColumnMappings, reader);
                    T7 obj7 = SprocMapperHelper.GetObject<T7>(_sprocObjectMapList[6].Columns,
                        _sprocObjectMapList[6].CustomColumnMappings, reader);

                    T obj = customMethod.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

                    result.Add(obj);
                }

            }
            return result;
        }

        public List<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7, T8>(SqlConnection conn, string cmdText, Func<T1, T2, T3, T4, T5, T6, T7, T8, T> customMethod,
                int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[1].Columns,
                        _sprocObjectMapList[1].CustomColumnMappings, reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[2].Columns,
                        _sprocObjectMapList[2].CustomColumnMappings, reader);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[3].Columns,
                        _sprocObjectMapList[3].CustomColumnMappings, reader);
                    T5 obj5 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[4].Columns,
                        _sprocObjectMapList[4].CustomColumnMappings, reader);
                    T6 obj6 = SprocMapperHelper.GetObject<T6>(_sprocObjectMapList[5].Columns,
                         _sprocObjectMapList[5].CustomColumnMappings, reader);
                    T7 obj7 = SprocMapperHelper.GetObject<T7>(_sprocObjectMapList[6].Columns,
                        _sprocObjectMapList[6].CustomColumnMappings, reader);
                    T8 obj8 = SprocMapperHelper.GetObject<T8>(_sprocObjectMapList[7].Columns,
                        _sprocObjectMapList[7].CustomColumnMappings, reader);

                    T obj = customMethod.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

                    result.Add(obj);
                }

            }
            return result;
        }

        public List<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(SqlConnection conn, string cmdText, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> customMethod,
                int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[1].Columns,
                        _sprocObjectMapList[1].CustomColumnMappings, reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[2].Columns,
                        _sprocObjectMapList[2].CustomColumnMappings, reader);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[3].Columns,
                        _sprocObjectMapList[3].CustomColumnMappings, reader);
                    T5 obj5 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[4].Columns,
                        _sprocObjectMapList[4].CustomColumnMappings, reader);
                    T6 obj6 = SprocMapperHelper.GetObject<T6>(_sprocObjectMapList[5].Columns,
                         _sprocObjectMapList[5].CustomColumnMappings, reader);
                    T7 obj7 = SprocMapperHelper.GetObject<T7>(_sprocObjectMapList[6].Columns,
                        _sprocObjectMapList[6].CustomColumnMappings, reader);
                    T8 obj8 = SprocMapperHelper.GetObject<T8>(_sprocObjectMapList[7].Columns,
                        _sprocObjectMapList[7].CustomColumnMappings, reader);
                    T9 obj9 = SprocMapperHelper.GetObject<T9>(_sprocObjectMapList[8].Columns,
                        _sprocObjectMapList[8].CustomColumnMappings, reader);

                    T obj = customMethod.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9);

                    result.Add(obj);
                }

            }
            return result;
        }

        public List<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(SqlConnection conn, string cmdText, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> customMethod,
        int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[1].Columns,
                        _sprocObjectMapList[1].CustomColumnMappings, reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[2].Columns,
                        _sprocObjectMapList[2].CustomColumnMappings, reader);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[3].Columns,
                        _sprocObjectMapList[3].CustomColumnMappings, reader);
                    T5 obj5 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[4].Columns,
                        _sprocObjectMapList[4].CustomColumnMappings, reader);
                    T6 obj6 = SprocMapperHelper.GetObject<T6>(_sprocObjectMapList[5].Columns,
                         _sprocObjectMapList[5].CustomColumnMappings, reader);
                    T7 obj7 = SprocMapperHelper.GetObject<T7>(_sprocObjectMapList[6].Columns,
                        _sprocObjectMapList[6].CustomColumnMappings, reader);
                    T8 obj8 = SprocMapperHelper.GetObject<T8>(_sprocObjectMapList[7].Columns,
                        _sprocObjectMapList[7].CustomColumnMappings, reader);
                    T9 obj9 = SprocMapperHelper.GetObject<T9>(_sprocObjectMapList[8].Columns,
                        _sprocObjectMapList[8].CustomColumnMappings, reader);
                    T10 obj10 = SprocMapperHelper.GetObject<T10>(_sprocObjectMapList[9].Columns,
                        _sprocObjectMapList[9].CustomColumnMappings, reader);

                    T obj = customMethod.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10);

                    result.Add(obj);
                }

            }
            return result;
        }

        public List<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(SqlConnection conn, string cmdText, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T> customMethod,
        int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[1].Columns,
                        _sprocObjectMapList[1].CustomColumnMappings, reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[2].Columns,
                        _sprocObjectMapList[2].CustomColumnMappings, reader);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[3].Columns,
                        _sprocObjectMapList[3].CustomColumnMappings, reader);
                    T5 obj5 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[4].Columns,
                        _sprocObjectMapList[4].CustomColumnMappings, reader);
                    T6 obj6 = SprocMapperHelper.GetObject<T6>(_sprocObjectMapList[5].Columns,
                         _sprocObjectMapList[5].CustomColumnMappings, reader);
                    T7 obj7 = SprocMapperHelper.GetObject<T7>(_sprocObjectMapList[6].Columns,
                        _sprocObjectMapList[6].CustomColumnMappings, reader);
                    T8 obj8 = SprocMapperHelper.GetObject<T8>(_sprocObjectMapList[7].Columns,
                        _sprocObjectMapList[7].CustomColumnMappings, reader);
                    T9 obj9 = SprocMapperHelper.GetObject<T9>(_sprocObjectMapList[8].Columns,
                        _sprocObjectMapList[8].CustomColumnMappings, reader);
                    T10 obj10 = SprocMapperHelper.GetObject<T10>(_sprocObjectMapList[9].Columns,
                        _sprocObjectMapList[9].CustomColumnMappings, reader);
                    T11 obj11 = SprocMapperHelper.GetObject<T11>(_sprocObjectMapList[10].Columns,
                        _sprocObjectMapList[10].CustomColumnMappings, reader);

                    T obj = customMethod.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10, obj11);

                    result.Add(obj);
                }

            }
            return result;
        }

        public List<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(SqlConnection conn, string cmdText, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T> customMethod,
        int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(cmdText, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[0].Columns,
                        _sprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[1].Columns,
                        _sprocObjectMapList[1].CustomColumnMappings, reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[2].Columns,
                        _sprocObjectMapList[2].CustomColumnMappings, reader);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[3].Columns,
                        _sprocObjectMapList[3].CustomColumnMappings, reader);
                    T5 obj5 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[4].Columns,
                        _sprocObjectMapList[4].CustomColumnMappings, reader);
                    T6 obj6 = SprocMapperHelper.GetObject<T6>(_sprocObjectMapList[5].Columns,
                         _sprocObjectMapList[5].CustomColumnMappings, reader);
                    T7 obj7 = SprocMapperHelper.GetObject<T7>(_sprocObjectMapList[6].Columns,
                        _sprocObjectMapList[6].CustomColumnMappings, reader);
                    T8 obj8 = SprocMapperHelper.GetObject<T8>(_sprocObjectMapList[7].Columns,
                        _sprocObjectMapList[7].CustomColumnMappings, reader);
                    T9 obj9 = SprocMapperHelper.GetObject<T9>(_sprocObjectMapList[8].Columns,
                        _sprocObjectMapList[8].CustomColumnMappings, reader);
                    T10 obj10 = SprocMapperHelper.GetObject<T10>(_sprocObjectMapList[9].Columns,
                        _sprocObjectMapList[9].CustomColumnMappings, reader);
                    T11 obj11 = SprocMapperHelper.GetObject<T11>(_sprocObjectMapList[10].Columns,
                        _sprocObjectMapList[10].CustomColumnMappings, reader);
                    T12 obj12 = SprocMapperHelper.GetObject<T12>(_sprocObjectMapList[11].Columns,
                        _sprocObjectMapList[11].CustomColumnMappings, reader);

                    T obj = customMethod.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10, obj11, obj12);

                    result.Add(obj);
                }

            }
            return result;
        }

        private void ValidateProperties()
        {
            if (!SprocMapperHelper.ValidateProperies(_sprocObjectMapList))
            {
                throw new SprocMapperException($"Duplicate column not allowed. Ensure that all columns in stored procedure are unique." +
                                               "Try setting an alias for your column in your stored procedure " +
                                               "and set up a custom column mapping for this property.");
            }
        }

        private void OpenConn(SqlConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        private void SetCommandProps(SqlCommand command, int commandTimeout)
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = commandTimeout;

            if (_paramList != null && _paramList.Any())
                command.Parameters.AddRange(_paramList.ToArray());
        }

    }

}

