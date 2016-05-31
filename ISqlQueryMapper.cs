using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    /// <summary>
    /// Executes SQL query and provides DataReader to callback function.
    /// </summary>
    public interface IQueryMapper
    {

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        Task ExecuteReader(string sql, Action<SqlDataReader> callback);

        /// <summary>
        /// Executes sql command and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        Task ExecuteReader(SqlCommand command, Action<SqlDataReader> callback);

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        Task ExecuteReader(string sql, Func<SqlDataReader, Task> callback);

        /// <summary>
        /// Executes sql command and provides each row to the async callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        Task ExecuteReader(SqlCommand command, Func<SqlDataReader, Task> callback);

    }
}