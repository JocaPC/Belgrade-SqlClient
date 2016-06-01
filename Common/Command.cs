using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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
        /// Creates command object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        public Command(DbConnection connection)
        {
            this.Connection = connection;
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
            finally
            {
                command.Connection.Close();
            }
        }
    }
}