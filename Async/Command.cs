using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.Async
{
    /// <summary>
    /// Sql Command that will be executed.
    /// </summary>
    public class Command : ICommand
    {
        /// <summary>
        /// Connection to Sql Database.
        /// </summary>
        private SqlConnection Connection;

        /// <summary>
        /// Creates command object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        public Command(SqlConnection connection)
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
            using (var command = new SqlCommand(sql, this.Connection))
            {
                await this.ExecuteNonQuery(command);
            }
        }

        /// <summary>
        /// Executes Sql command.
        /// </summary>
        /// <param name="command">SqlCommand that will be executed.</param>
        /// <returns>Generic task.</returns>
        public async Task ExecuteNonQuery(SqlCommand command)
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