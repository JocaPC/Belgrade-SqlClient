//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE. See the license files for details.
using System;
using System.Data;
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
        /// Executes Sql command.
        /// </summary>
        /// <param name="command">SqlCommand that will be executed.</param>
        /// <returns>Generic task.</returns>
        public async Task Exec()
        {
            var command = base.Command;
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
                var errorHandler = base.GetErrorHandlerBuilder().SetCommand(command).CreateErrorHandler(
#if NETCOREAPP2_0
                    base._logger
#endif
                    );
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
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task Stream(Stream output, Options options)
        {
            DbCommand command = base.Command;
            if (command == null)
                throw new InvalidOperationException("Command is not set in SqlCommand.");
            command = this.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;

            await this.Pipe.Sql(command).Stream(output, options);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task Stream(TextWriter writer, Options options)
        {
            DbCommand command = base.Command;
            if (command == null)
                throw new InvalidOperationException("Command is not set in SqlCommand.");
            command = this.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;

            await this.Pipe.Sql(command).Stream(writer, options);
        }

        /// <summary>
        /// Query mapper used to stream results.
        /// </summary>
        private GenericQueryMapper<T> Mapper;

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task Map(Func<DbDataReader, Task> callback)
        {
            DbCommand command = base.Command;
            if (command == null)
                throw new InvalidOperationException("Command is not set in SqlCommand.");
            command = this.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;

            await (this.Mapper.SetCommand(command) as IQueryMapper).Map(callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task Map(Func<DbDataReader, Exception, Task> callback)
        {
            DbCommand command = base.Command;
            if (command == null)
                throw new InvalidOperationException("Command is not set in SqlCommand.");
            command = this.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;

            await (this.Mapper.SetCommand(command) as IQueryMapper).Map(callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task Map(Action<DbDataReader> callback)
        {
            DbCommand command = base.Command;
            if (command == null)
                throw new InvalidOperationException("Command is not set in SqlCommand.");
            command = this.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;

            await this.Mapper.Sql(command).Map(callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task Map(Action<DbDataReader, Exception> callback)
        {
            DbCommand command = base.Command;
            if (command == null)
                throw new InvalidOperationException("Command is not set in SqlCommand.");
            command = this.CommandModifier(command);
            if (command.Connection == null)
                command.Connection = this.Connection;

            await this.Mapper.Sql(command).Map(callback);
        }

        internal override BaseStatement AddErrorHandler(ErrorHandlerBuilder builder)
        {
            if (this.Mapper != null) this.Mapper.AddErrorHandler(builder);
            if (this.Pipe != null) this.Pipe.AddErrorHandler(builder);

            return base.AddErrorHandler(builder);
        }

        public ICommand Sql(DbCommand cmd)
        {
            return base.SetCommand(cmd) as ICommand;
        }

        public ICommand Param(string name, DbType type, object value, int size = 0)
        {
            return base.AddParameter(name, type, value, size) as ICommand;
        }
    }
}