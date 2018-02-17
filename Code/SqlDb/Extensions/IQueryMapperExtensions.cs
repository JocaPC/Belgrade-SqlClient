using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static class IQueryMapperExtensions
    {
        #region "Text command extensions"
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task Map(this IQueryMapper mapper, string sql, Action<DbDataReader> callback)
        {
            var cmd = new SqlCommand(sql);
            return mapper.Map(cmd, callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task Map(this IQueryMapper mapper, string sql, Func<DbDataReader, Task> callback)
        {
            var cmd = new SqlCommand(sql);
            return mapper.Map(cmd, callback);
        }

        #endregion

        #region "Backward compatiliblity methods"
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this IQueryMapper mapper, string sql, Action<DbDataReader> callback)
        {
            return mapper.Map(sql, callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this IQueryMapper mapper, SqlCommand cmd, Action<DbDataReader> callback)
        {
            return mapper.Map(cmd, callback);
        }

        public static Task ExecuteReader(this IQueryMapper mapper, SqlCommand cmd, Func<DbDataReader, Task> callback)
        {
            return mapper.Map(cmd, callback);
        }

        public static Task ExecuteReader(this IQueryMapper mapper, string sql, Func<DbDataReader, Task> callback)
        {
            return mapper.Map(sql, callback);
        }
        #endregion

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static async Task<string> GetString(this IQueryMapper mapper, SqlCommand cmd)
        {
            var sb = new StringBuilder();
            await mapper.Map(cmd, reader => sb.Append(reader[0]));
            return sb.ToString();
        }

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <returns>Task</returns>
        public static async Task<string> GetString(this IQueryMapper mapper, string sql)
        {
            var cmd = new SqlCommand(sql);
            return await mapper.GetString(cmd);
        }
    }
}
