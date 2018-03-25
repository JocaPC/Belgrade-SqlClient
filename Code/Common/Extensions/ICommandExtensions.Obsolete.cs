using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static partial class ICommandExtensions
    {
        /// <summary>
        /// Executes SQL command text.
        /// </summary>
        /// <returns>Generic task.</returns>
        [Obsolete("Use command.Sql(...).Exec() instead.")]
        public static Task Exec(this ICommand command, DbCommand cmd)
        {
            return command.Sql(cmd).Exec();
        }

        /// <summary>
        /// Maps results of SQL command to callback.
        /// </summary>
        /// <returns>Generic task.</returns>
        [Obsolete("Use command.Sql(...).Map(callback) instead.")]
        public static Task Map(this ICommand command, DbCommand cmd, Action<DbDataReader> callback)
        {
            return command.Sql(cmd).Map(callback);
        }

        /// <summary>
        /// Maps results of SQL command to callback.
        /// </summary>
        /// <returns>Generic task.</returns>
        [Obsolete("Use command.Sql(...).Map(callback) instead.")]
        public static Task Map(this ICommand command, DbCommand cmd, Func<DbDataReader, Task> callback)
        {
            return command.Sql(cmd).Map(callback);
        }

        /// <summary>
        /// Maps results of SQL command to callback.
        /// </summary>
        /// <returns>Generic task.</returns>
        [Obsolete("Use command.Sql(...).Stream(output, options) instead.")]
        public static Task Stream(this ICommand command, DbCommand cmd, Stream output, Options options = null)
        {
            return command.Sql(cmd).Stream(output, options);
        }

        #region "Utilities for default output"

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
        public static Task ExecuteReader(this ICommand command, string sql, Func<DbDataReader, Task> callback)
        {
            return command.Map(sql, callback);
        }
        #endregion

    }
}