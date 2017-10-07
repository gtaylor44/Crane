using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Crane;
using Crane.Shared;

namespace Crane.MySql
{
    /// <inheritdoc />
    public class MySqlUserCommand : BaseCommand
    {
        private MySqlConnection _mySqlConn;
        private readonly string _connectionString;

        /// <inheritdoc />
        public MySqlUserCommand(string connectionString) : base()
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc />
        public override int ExecuteNonQuery(string command, int? commandTimeout = null, 
            DbConnection dbConnection = null, DbTransaction transaction = null)
        {
            try
            {
                int affectedRecords;

                command = GetCleanSqlCommand(command);

                // Try open connection if not already open.
                if (dbConnection == null)                
                    _mySqlConn = new MySqlConnection(_connectionString);
                                   
                else                
                    _mySqlConn = dbConnection as MySqlConnection;
                
                OpenConn(_mySqlConn);

                using (MySqlCommand cmd = new MySqlCommand(command, _mySqlConn))
                {
                    SetCommandProps(cmd, transaction, commandTimeout, command);
                    affectedRecords = cmd.ExecuteNonQuery();
                }

                return affectedRecords;
            }
            finally
            {
                if (dbConnection == null)
                    _mySqlConn.Dispose();
            }

        }

        /// <inheritdoc />
        public override async Task<int> ExecuteNonQueryAsync(string command, int? commandTimeout = null, 
            DbConnection dbConnection = null, DbTransaction transaction = null)
        {
            try
            {
                int affectedRecords;

                command = GetCleanSqlCommand(command);

                // Try open connection if not already open.
                if (dbConnection == null)             
                    _mySqlConn = new MySqlConnection(_connectionString);
                                                       
                else                
                    _mySqlConn = dbConnection as MySqlConnection;
                
                await OpenConnAsync(_mySqlConn);

                using (MySqlCommand cmd = new MySqlCommand(command, _mySqlConn))
                {
                    SetCommandProps(cmd, transaction, commandTimeout, command);
                    affectedRecords = await cmd.ExecuteNonQueryAsync();
                }

                return affectedRecords;
            }
            finally
            {
                if (dbConnection == null)
                     _mySqlConn.Dispose();
            }

        }
    }
}
