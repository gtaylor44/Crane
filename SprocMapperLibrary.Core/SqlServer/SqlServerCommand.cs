using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SprocMapperLibrary.CacheProvider;

namespace SprocMapperLibrary.SqlServer
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public class SqlServerCommand : BaseCommand
    {
        private SqlConnection _conn;

        private readonly string _connectionString;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlServerCommand(string connectionString) : base()
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc />
        /// <summary>
        /// Execute a MSSql stored procedure synchronously.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>Number of affected records.</returns>
        public override int ExecuteNonQuery(string command, CommandType? commandType = null, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                int affectedRecords;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                using (SqlCommand cmd = new SqlCommand(command, _conn))
                {
                    SetCommandProps(cmd, commandTimeout, commandType);
                    affectedRecords = cmd.ExecuteNonQuery();
                }

                return affectedRecords;
            }
            finally
            {
                if (userConn == null)
                    _conn.Dispose();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Execute a stored procedure asynchronously.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>Number of affected records.</returns>
        public override async Task<int> ExecuteNonQueryAsync(string command, CommandType? commandType = null, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                int affectedRecords;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                await OpenConnAsync(_conn);

                using (SqlCommand cmd = new SqlCommand(command, _conn))
                {
                    SetCommandProps(cmd, commandTimeout, commandType);
                    affectedRecords = await cmd.ExecuteNonQueryAsync();
                }

                return affectedRecords;
            }
            finally
            {
                if (userConn == null)
                    _conn.Dispose();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override T ExecuteScalar<T>(string command, CommandType? commandType = null, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                T obj;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                using (SqlCommand cmd = new SqlCommand(command, _conn))
                {
                    SetCommandProps(cmd, commandTimeout, commandType);
                    obj = (T)cmd.ExecuteScalar();
                }

                return obj;
            }

            finally
            {
                if (userConn == null)
                    _conn.Dispose();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override async Task<T> ExecuteScalarAsync<T>(string command, CommandType? commandType = null, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                T obj;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                await OpenConnAsync(_conn);

                using (SqlCommand cmd = new SqlCommand(command, _conn))
                {
                    SetCommandProps(cmd, commandTimeout, commandType);
                    obj = (T)await cmd.ExecuteScalarAsync();
                }

                return obj;
            }

            finally
            {
                if (userConn == null)
                    _conn.Dispose();
            }

        }
    }
}
