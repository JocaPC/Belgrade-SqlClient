using Belgrade.SqlClient.SqlDb;
using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static class ICommandExtensions
    {
        /// <summary>
        /// Executes SQL command text.
        /// </summary>
        /// <param name="sql">Sql text that will be executed.</param>
        /// <returns>Generic task.</returns>
        public static Task ExecuteNonQuery(this ICommand command, string sql)
        {
            if (!(command is Command))
                throw new ArgumentException("The argument command must be derived from Command class.", "command");
            return CommandExtensions.ExecuteNonQuery((command as Command), sql);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this ICommand command, string sql, Func<DbDataReader, Task> callback)
        {
            if (!(command is Command))
                throw new ArgumentException("The argument command must be derived from Command class.", "command");
            return CommandExtensions.ExecuteReader((command as Command), sql, callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this ICommand command, string sql, Action<DbDataReader> callback)
        {
            if (!(command is Command))
                throw new ArgumentException("The argument command must be derived from Command class.", "command");
            return CommandExtensions.ExecuteReader((command as Command), sql, callback);
        }

        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        public static Task Stream<D>(this ICommand command, string sql, Stream output, D defaultOutput)
        {
            if (!(command is Command))
                throw new ArgumentException("Argument command must be derived from Command", "command");
            return CommandExtensions.Stream<D>((command as Command), sql, output, defaultOutput);
        }
    }
}