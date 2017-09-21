using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SprocMapperLibrary.SqlServer
{
    /// <inheritdoc />
    public class SqlServerCommand : BaseCommand
    {
        private SqlConnection _conn;

        private readonly string _connectionString;

        /// <inheritdoc />
        public SqlServerCommand(string connectionString) : base()
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc />
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

                using (var cmd = new SqlCommand(command, _conn))
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

                using (var cmd = new SqlCommand(command, _conn))
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

                using (var cmd = new SqlCommand(command, _conn))
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

                using (var cmd = new SqlCommand(command, _conn))
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
