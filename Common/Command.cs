using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.Common
{
    /// <summary>
    /// Sql Command that will be executed.
    /// </summary>
    public class Command <T> : ICommand
        where T : DbCommand, new()
    {
        /// <summary>
        /// Connection to Sql Database.
        /// </summary>
        private DbConnection Connection;

        /// <summary>
        /// Delegate that is called when some error happens.
        /// </summary>
        Action<Exception> ErrorHandler = null;

        /// <summary>
        /// Creates command object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        /// <param name="errorHandler">Function that will be called if some exception is thrown.</param>
        public Command(DbConnection connection, Action<Exception> errorHandler = null)
        {
            this.Connection = connection;
            this.ErrorHandler = errorHandler;
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
                command.Connection = this.Connection;
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
            if (command.Connection == null)
                command.Connection = this.Connection;
            try
            {
                await command.Connection.OpenAsync().ConfigureAwait(false);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (this.ErrorHandler != null)
                    this.ErrorHandler(ex);
            }
            finally
            {
                command.Connection.Close();
            }
        }
    }
}