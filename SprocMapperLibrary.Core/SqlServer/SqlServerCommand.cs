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
        public override int ExecuteNonQuery(string command, CommandType? commandType = null, int? commandTimeout = null, 
            DbConnection userConn = null, DbTransaction transaction = null)
        {
            try
            {
                command = GetCleanSqlCommand(command);

                int affectedRecords;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                using (SqlCommand cmd = new SqlCommand(command, _conn))
                {
                    SetCommandProps(cmd, transaction, commandTimeout, command);
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
        public override async Task<int> ExecuteNonQueryAsync(string command, CommandType? commandType = null, int? commandTimeout = null, 
            DbConnection userConn = null, DbTransaction transaction = null)
        {
            try
            {
                command = GetCleanSqlCommand(command);

                int affectedRecords;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                await OpenConnAsync(_conn);

                using (SqlCommand cmd = new SqlCommand(command, _conn))
                {
                    SetCommandProps(cmd, transaction, commandTimeout, command);
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
    }
}
