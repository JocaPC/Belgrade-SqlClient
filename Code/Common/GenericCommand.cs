//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE. See the license files for details.
using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.Common
{
    /// <summary>
    /// Sql Command that will be executed.
    /// </summary>
    public class GenericCommand <T> : BaseStatement, ICommand
        where T : DbCommand, new()
    {
        /// <summary>
        /// Creates command object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        public GenericCommand(DbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("Connection is not defined.");

            if (string.IsNullOrWhiteSpace(connection.ConnectionString))
                throw new ArgumentNullException("Connection string is not set.");

            this.Connection = connection;
            this.Pipe = new GenericQueryPipe<T>(connection);
            this.Mapper = new GenericQueryMapper<T>(connection);
        }

        /// <summary>
        /// Executes SQL command text.
        /// </summary>
        /// <param name="sql">Sql text that will be executed.</param>
        /// <returns>Generic task.</returns>
        public async Task ExecuteNonQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException("Command SQL text is not set.");

            using (DbCommand command = new T())
            {
                command.CommandText = sql;
                await this.ExecuteNonQuery(command);
            }
        }

        /// <summary>
        /// Executes Sql command.
        /// </summary>
        /// <param name="command">SqlCommand that will be executed.</param>
        /// <returns>Generic task.</returns>
        public async Task ExecuteNonQuery(DbCommand command)
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
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
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

        /// <summary>
        /// Query pipe used to stream results.
        /// </summary>
        private GenericQueryPipe<T> Pipe;

        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        public async Task Stream<D>(string sql, Stream output, D defaultOutput)
        {
            using (DbCommand command = new T())
            {
                command.CommandText = sql;
                await this.Stream<D>(command, output, defaultOutput);
            }
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task Stream<D>(DbCommand command, Stream output, D defaultOutput)
        {
            command = this.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;

            await this.Pipe.Stream<D>(command, output, defaultOutput);
        }

        /// <summary>
        /// Query mapper used to stream results.
        /// </summary>
        private GenericQueryMapper<T> Mapper;

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(string sql, Func<DbDataReader, Task> callback)
        {
            using (DbCommand command = new T())
            {
                command.CommandText = sql;
                await this.ExecuteReader(command, callback);
            }
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(DbCommand command, Func<DbDataReader, Task> callback)
        {
            command = this.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;

            await this.Mapper.ExecuteReader(command, callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(DbCommand command, Action<DbDataReader> callback)
        {
            command = this.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;

            await this.Mapper.ExecuteReader(command, callback);
        }
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(string sql, Action<DbDataReader> callback)
        {
            using (DbCommand command = new T())
            {
                command.CommandText = sql;
                await this.ExecuteReader(command, callback);
            }
        }

    }
}