//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Data;
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
        /// Executes sql command and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        private async Task MapToCallback(object callback)
        {
            DbCommand command = base.Command;
            if (command == null)
                throw new InvalidOperationException("Command is not defined.");

            if (string.IsNullOrWhiteSpace(command.CommandText))
                throw new ArgumentNullException("Command SQL text is not set.");

            command = this.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;
            await base.ExecuteWithRetry(command, callback);
        }

        protected override async Task<bool> ExecuteCommand(DbCommand command, object callback)
        {
            bool isResultSentToCallback = false;
            await command.Connection.OpenAsync().ConfigureAwait(false);
            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    if (callback is Action<DbDataReader>)
                        (callback as Action<DbDataReader>)(reader);
                    else if (callback is Action<DbDataReader, Exception>)
                        (callback as Action<DbDataReader, Exception>)(reader, null);
                    else if (callback is Func<DbDataReader, Task>)
                        await (callback as Func<DbDataReader, Task>)(reader);
                    else if (callback is Func<DbDataReader, Exception, Task>)
                        await (callback as Func<DbDataReader, Exception, Task>)(reader, null);
                    else
                        throw new ArgumentException("Cannot use " + callback.GetType().Name + "as a callback.");
                    isResultSentToCallback = true;
                }
            }

            return isResultSentToCallback;
        }

        protected override async Task ExecuteCallbackWithException(object callback, Exception ex)
        {
            if (_logger != null)
                _logger.ErrorFormat("Error {message} occurred while trying to provide query results to mapper function. \n{exception}", ex.Message, ex);
            try
            {
                if (callback is Action<DbDataReader, Exception>)
                    (callback as Action<DbDataReader, Exception>)(null, ex);
                else if (callback is Func<DbDataReader, Exception, Task>)
                    await (callback as Func<DbDataReader, Exception, Task>)(null, ex);
                else {
                    var errorHandler = base.GetErrorHandlerBuilder().CreateErrorHandler(base._logger);
                    errorHandler(ex);
                }
            }
            catch (Exception ex2)
            {
                if (_logger != null)
                    _logger.ErrorFormat("Callback provided to Map() thrown the error {error} while trying to handle exception.\n{exception}", ex2.Message, ex2);
                throw;
            }
        }

        /// <summary>
        /// Executes sql command and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task Map(Action<DbDataReader> callback)
            => await MapToCallback(callback);

        /// <summary>
        /// Executes sql command and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task Map(Action<DbDataReader, Exception> callback)
            => await MapToCallback(callback);

        /// <summary>
        /// Executes sql command and provides each row to the async callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task Map(Func<DbDataReader, Task> callback)
            => await MapToCallback(callback);

        /// <summary>
        /// Executes sql command and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task Map(Func<DbDataReader, Exception, Task> callback)
            => await MapToCallback(callback);

        /// <summary>
        /// Adds a parameter to the mapper.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="type">Parameter type.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <param name="size">Size of the parameter.</param>
        /// <returns>Mapper with new parameter.</returns>
        public IQueryMapper Param(string name, DbType type, object value, int size = 0)
        {
            return base.AddParameter(name, type, value, size) as IQueryMapper;
        }

        /// <summary>
        /// Set T-SQL query that should be executed.
        /// </summary>
        /// <param name="cmd">DbCommand with the query text.</param>
        /// <returns>Mapper initialized with query text that will be executed.</returns>
        public IQueryMapper Sql(DbCommand cmd)
        {
            return base.SetCommand(cmd) as IQueryMapper;
        }
    }
}