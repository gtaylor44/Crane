using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SprocMapperLibrary.MySql
{
    /// <summary>
    /// 
    /// </summary>
    public class Procedure : BaseProcedure
    {
        private readonly MySqlConnection _mySqlConn;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mySqlConn"></param>
        public Procedure(MySqlConnection mySqlConn) : base()
        {
            _mySqlConn = mySqlConn;
        }

        /// <summary>
        /// Execute a MySQL stored procedure synchronously.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Number of affected records.</returns>
        public override int ExecuteNonQuery(string storedProcedure, int? commandTimeout = null)
        {
            int affectedRecords;

            OpenConn(_mySqlConn);

            using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
            {
                SetCommandProps(command, commandTimeout);
                affectedRecords = command.ExecuteNonQuery();
            }

            return affectedRecords;
        }

        /// <summary>
        /// Execute a stored procedure asynchronously.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Number of affected records.</returns>
        public override async Task<int> ExecuteNonQueryAsync(string storedProcedure, int? commandTimeout = null)
        {
            int affectedRecords;

            await OpenConnAsync(_mySqlConn);
            using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
            {
                SetCommandProps(command, commandTimeout);
                affectedRecords = await command.ExecuteNonQueryAsync();
            }

            return affectedRecords;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public T ExecuteScalar<T>(string storedProcedure, int? commandTimeout = null)
        {
            T obj;

            OpenConn(_mySqlConn);
            using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
            {
                SetCommandProps(command, commandTimeout);
                obj = (T)command.ExecuteScalar();
            }

            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public async Task<T> ExecuteScalarAsync<T>(string storedProcedure, int? commandTimeout = null)
        {
            T obj;

            await OpenConnAsync(_mySqlConn);
            using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
            {
                SetCommandProps(command, commandTimeout);
                obj = (T)await command.ExecuteScalarAsync();
            }

            return obj;
        }
    }
}
