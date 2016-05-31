using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    /// <summary>
    /// Sql Command that will be executed.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes SQL command text.
        /// </summary>
        /// <param name="sql">Sql text that will be executed.</param>
        /// <returns>Generic task.</returns>
        Task ExecuteNonQuery(string sql);

        /// <summary>
        /// Executes Sql command.
        /// </summary>
        /// <param name="command">SqlCommand that will be executed.</param>
        /// <returns>Generic task.</returns>
        Task ExecuteNonQuery(SqlCommand command);
    }
    
}