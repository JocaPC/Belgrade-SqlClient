using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static class ICommandExtensions
    {
        #region "Text command extensions"
        /// <summary>
        /// Executes SQL command text.
        /// </summary>
        /// <param name="sql">Sql text that will be executed.</param>
        /// <returns>Generic task.</returns>
        public static Task Exec(this ICommand command, string sql)
        {
            var cmd = new SqlCommand(sql);
            return command.Exec(cmd);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task Map(this ICommand command, string sql, Func<DbDataReader, Task> callback)
        {
            var cmd = new SqlCommand(sql);
            return command.Map(cmd, callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task Map(this ICommand command, string sql, Action<DbDataReader> callback)
        {
            var cmd = new SqlCommand(sql);
            return command.Map(cmd, callback);
        }

        public static Task Stream(this ICommand command, string sql, Stream output, Options options)
        {
            var cmd = new SqlCommand(sql);
            return command.Stream(cmd, output, options);
        }

        #endregion

        #region "Utilities for default output"
        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        public static Task Stream(this ICommand command, string sql, Stream output, string defaultOutput = "[]")
        {
            var cmd = new SqlCommand(sql);
            return command.Stream(cmd, output, new Options() { DefaultOutput = defaultOutput });
        }

        public static Task Stream(this ICommand command, SqlCommand cmd, Stream output, string defaultOutput = "[]")
        {
            return command.Stream(cmd, output, new Options() { DefaultOutput = defaultOutput });
        }

        public static Task Stream(this ICommand command, string sql, Stream output, byte[] defaultOutput)
        {
            var cmd = new SqlCommand(sql);
            return command.Stream(cmd, output, new Options() { DefaultOutput = defaultOutput });
        }

        public static Task Stream(this ICommand command, SqlCommand cmd, Stream output, byte[] defaultOutput)
        {
            return command.Stream(cmd, output, new Options() { DefaultOutput = defaultOutput });
        }

        #endregion

        #region "Backward compatiliblity methods"
        [Obsolete("Use Exec() method instead of this one.")]
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteNonQuery(this ICommand command, string sql)
        {
            return command.Exec(sql);
        }

        [Obsolete("Use Exec() method instead of this one.")]
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteNonQuery(this ICommand command, SqlCommand cmd)
        {
            return command.Exec(cmd);
        }

        [Obsolete("Use Map() method instead of this one.")]
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this ICommand command, string sql, Action<DbDataReader> callback)
        {
            return command.Map(sql, callback);
        }

        [Obsolete("Use Map() method instead of this one.")]
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this ICommand command, SqlCommand cmd, Action<DbDataReader> callback)
        {
            return command.Map(cmd, callback);
        }

        [Obsolete("Use Map() method instead of this one.")]
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this ICommand command, string sql, Func<DbDataReader, Task> callback)
        {
            return command.Map(sql, callback);
        }

        [Obsolete("Use Map() method instead of this one.")]
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this ICommand command, SqlCommand cmd, Func<DbDataReader, Task> callback)
        {
            return command.Map(cmd, callback);
        }
        #endregion

    }
}