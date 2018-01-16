using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.SqlDb
{
    public static class CommandExtensions
    {
        /// <summary>
        /// Executes SQL command text.
        /// </summary>
        /// <param name="sql">Sql text that will be executed.</param>
        /// <returns>Generic task.</returns>
        public static Task ExecuteNonQuery(this Command command, string sql)
        {
            var cmd = new SqlCommand(sql);
            return command.ExecuteNonQuery(cmd);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this Command command, string sql, Func<DbDataReader, Task> callback)
        {
            var cmd = new SqlCommand(sql);
            return command.ExecuteReader(cmd, callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this Command command, string sql, Action<DbDataReader> callback)
        {
            var cmd = new SqlCommand(sql);
            return command.ExecuteReader(cmd, callback);
        }

        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        public static Task Stream(this Command command, string sql, Stream output, string defaultOutput)
        {
            var cmd = new SqlCommand(sql);
            return command.Stream(cmd, output, new Options() { DefaultOutput = defaultOutput });
        }
    }
}