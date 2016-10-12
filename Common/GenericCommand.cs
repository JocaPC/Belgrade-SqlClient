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
            this.Connection = connection;
        }

        public new GenericCommand<T> AddErrorHandlerBuilder(ErrorHandlerBuilder builder)
        {
            return base.AddErrorHandlerBuilder(builder) as GenericCommand<T>;
        }

        /// <summary>
        /// Executes SQL command text.
        /// </summary>
        /// <param name="sql">Sql text that will be executed.</param>
        /// <returns>Generic task.</returns>
        public async Task ExecuteNonQuery(string sql)
        {
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
                base.GetErrorHandlerBuilder().SetCommand(command).CreateErrorHandler()(ex);
            }
            finally
            {
                command.Connection.Close();
            }
        }
    }
}