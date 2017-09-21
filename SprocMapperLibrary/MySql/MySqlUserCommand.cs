﻿using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SprocMapperLibrary.MySql
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
        public override int ExecuteNonQuery(string storedProcedure, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                int affectedRecords;

                // Try open connection if not already open.
                if (unmanagedConn == null)                
                    _mySqlConn = new MySqlConnection(_connectionString);
                                   
                else                
                    _mySqlConn = unmanagedConn as MySqlConnection;
                
                OpenConn(_mySqlConn);

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
                    affectedRecords = command.ExecuteNonQuery();
                }

                return affectedRecords;
            }
            finally
            {
                if (unmanagedConn == null)
                    _mySqlConn.Dispose();
            }

        }

        /// <inheritdoc />
        public override async Task<int> ExecuteNonQueryAsync(string storedProcedure, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                int affectedRecords;

                // Try open connection if not already open.
                if (unmanagedConn == null)             
                    _mySqlConn = new MySqlConnection(_connectionString);
                                                       
                else                
                    _mySqlConn = unmanagedConn as MySqlConnection;
                
                await OpenConnAsync(_mySqlConn);

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
                    affectedRecords = await command.ExecuteNonQueryAsync();
                }

                return affectedRecords;
            }
            finally
            {
                if (unmanagedConn == null)
                     _mySqlConn.Dispose();
            }

        }

        /// <inheritdoc />
        public override T ExecuteScalar<T>(string storedProcedure, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                T obj;

                // Try open connection if not already open.
                if (unmanagedConn == null)                
                    _mySqlConn = new MySqlConnection(_connectionString);
                                    
                else                
                    _mySqlConn = unmanagedConn as MySqlConnection;
                
                OpenConn(_mySqlConn);

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
                    obj = (T) command.ExecuteScalar();
                }

                return obj;
            }
            finally
            {
                if (unmanagedConn == null)
                    _mySqlConn.Dispose();
            }

        }

        /// <inheritdoc />
        public override async Task<T> ExecuteScalarAsync<T>(string storedProcedure, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                T obj;

                // Try open connection if not already open.
                if (unmanagedConn == null)               
                    _mySqlConn = new MySqlConnection(_connectionString);
                                                 
                else                
                    _mySqlConn = unmanagedConn as MySqlConnection;
                
                await OpenConnAsync(_mySqlConn);

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
                    obj = (T) await command.ExecuteScalarAsync();
                }

                return obj;
            }

            finally
            {
                if (unmanagedConn == null)
                    _mySqlConn.Dispose();
            }

        }
    }
}