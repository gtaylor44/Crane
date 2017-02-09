using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SprocMapperLibrary
{
    public class Select<T>
    {
        private ICollection<SqlParameter> _paramList;
        private SprocObjectMap _sprocObjectMap;

        public Select(SprocObjectMap sprocObjectMap)
        {
            _sprocObjectMap = sprocObjectMap;
            _paramList = new List<SqlParameter>();
        }

        public Select<T> AddSqlParameterList(ICollection<SqlParameter> paramList)
        {
            if (paramList == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("AddSqlParameterList does not accept null");

            _paramList = paramList;
            return this;
        }

        public List<T> ExecuteReader(SqlConnection sqlConnection, string cmdText)
        {
            List<T> result = new List<T>();
            using (SqlConnection conn = sqlConnection)
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                using (SqlCommand command = new SqlCommand(cmdText, conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (_paramList != null && _paramList.Any())
                        command.Parameters.AddRange(_paramList.ToArray());

                    var reader = command.ExecuteReader();

                    if (!reader.HasRows)
                        return default(List<T>);

                    while (reader.Read())
                    {
                        
                    }
                }

                return result;
            }

        }

    }
}
