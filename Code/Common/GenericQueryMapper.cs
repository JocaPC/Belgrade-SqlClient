//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.Common
{
    /// <summary>
    /// Executes SQL query and provides DataReader to callback function.
    /// </summary>
    public class GenericQueryMapper<T> : BaseStatement, IQueryMapper
        where T : DbCommand, new()
    {
        /// <summary>
        /// Creates Mapper object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        public GenericQueryMapper(DbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("Connection is not defined.");

            if (string.IsNullOrWhiteSpace(connection.ConnectionString))
                throw new ArgumentNullException("Connection string is not set.");

            this.Connection = connection;
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(string sql, Func<DbDataReader, Task> callback)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException("Command SQL text is not set.");

            using (DbCommand command = new T())
            {
                command.CommandText = sql;                
                await this.ExecuteReader(command, callback);
            }
        }

        /// <summary>
        /// Executes sql command and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(DbCommand command, Action<DbDataReader> callback)
        {
            if (command == null)
                throw new ArgumentNullException("Command is not defined.");

            if (string.IsNullOrWhiteSpace(command.CommandText))
                throw new ArgumentNullException("Command SQL text is not set.");

            command = this.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;
            try
            {
                await command.Connection.OpenAsync().ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        callback(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorHandler = base.GetErrorHandlerBuilder().SetCommand(command).CreateErrorHandler();
                if (errorHandler == null)
                    throw;
                else
                    errorHandler(ex);
            }
            finally
            {
                command.Connection.Close();
            }
        }

        //protected virtual DbCommand ModifyCommand(DbCommand command)
        //{
        //    return command;
        //}

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(string sql, Action<DbDataReader> callback)
        {
            using (DbCommand command = new T())
            {
                command.CommandText = sql;
                await this.ExecuteReader(command, callback);
            }
        }

        /// <summary>
        /// Executes sql command and provides each row to the async callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(DbCommand command, Func<DbDataReader, Task> callback)
        { 
            command = base.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;
            try
            {
                await command.Connection.OpenAsync().ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        await callback(reader).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorHandler = base.GetErrorHandlerBuilder().SetCommand(command).CreateErrorHandler();
                if (errorHandler == null)
                    throw;
                else
                    errorHandler(ex);
            }
            finally
            {
                command.Connection.Close();
            }
        }
    }
}