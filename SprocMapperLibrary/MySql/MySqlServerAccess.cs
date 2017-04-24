using System;
using SprocMapperLibrary.Base;

namespace SprocMapperLibrary.MySql
{
    /// <summary>
    /// 
    /// </summary>
    public class MySqlServerAccess : BaseAccess
    {
        private readonly string _connectionString;
        private const string InvalidConnMsg = "Please ensure that valid MySQL credentials have been passed in.";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public MySqlServerAccess(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentException(InvalidConnMsg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public MySqlSproc Sproc()
        {
            return new MySqlSproc(_connectionString, CacheProvider);
        }
    }
}
