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
            bool isExceptionReported = false;

            try
            {
                await command.Connection.OpenAsync().ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    try
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
                        }
                    } catch (Exception ex)
                    {
                        if (callback is Action<DbDataReader, Exception>)
                            (callback as Action<DbDataReader, Exception>)(reader, ex);
                        else if (callback is Func<DbDataReader, Exception, Task>)
                            await (callback as Func<DbDataReader, Exception, Task>)(reader, ex);
                        isExceptionReported = true;
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!isExceptionReported)
                {
                    // Do not call the callback that expects exception second time 
                    // if it is already called before the exception was re-thrown.
                    if (callback is Action<DbDataReader, Exception>)
                        (callback as Action<DbDataReader, Exception>)(null, ex);
                    else if (callback is Func<DbDataReader, Exception, Task>)
                        await (callback as Func<DbDataReader, Exception, Task>)(null, ex);
                }
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

        public IQueryMapper Param(string name, DbType type, object value, int size = 0)
        {
            return base.AddParameter(name, type, value, size) as IQueryMapper;
        }

        public IQueryMapper Sql(DbCommand cmd)
        {
            return base.SetCommand(cmd) as IQueryMapper;
        }
    }
}