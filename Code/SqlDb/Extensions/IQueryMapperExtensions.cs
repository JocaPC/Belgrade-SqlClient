using Belgrade.SqlClient.Common;
using Code.SqlDb.Extensions;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static class IQueryMapperExtensions
    {
        public static IQueryMapper Sql(this IQueryMapper mapper, DbCommand cmd)
        {
            if (mapper is BaseStatement)
                (mapper as BaseStatement).SetCommand(cmd);
            return mapper;
        }

        public static IQueryMapper Param(this IQueryMapper mapper, string name, System.Data.DbType type, object value, int size = 0)
        {
            if (mapper is BaseStatement)
                (mapper as BaseStatement).AddParameter(name, type, value, size);
            return mapper;
        }

        public static IQueryMapper AddWithValue(this IQueryMapper mapper, string name, object value)
        {
            Util.AddParameterWithValue(mapper, name, value);
            return mapper;
        }

        [Obsolete("Use mapper.Sql(...).Map(callback) instead.")]
        public static Task Map(this IQueryMapper mapper, DbCommand cmd, Action<DbDataReader> callback)
        {
            return mapper.Sql(cmd).Map(callback);
        }

        #region "Text command extensions"

        /// <summary>
        /// Set the query text on the mapper.
        /// </summary>
        /// <returns>Query Mapper.</returns>
        public static IQueryMapper Sql(this IQueryMapper mapper, string query)
        {
            var cmd = new SqlCommand(query);
            return mapper.Sql(cmd);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        [Obsolete("Use mapper.Sql(...).Map(callback) instead.")]
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
        [Obsolete("Use mapper.Sql(...).Map(callback) instead.")]
        public static Task Map(this IQueryMapper mapper, string sql, Func<DbDataReader, Task> callback)
        {
            var cmd = new SqlCommand(sql);
            return mapper.Sql(cmd).Map(callback);
        }

        #endregion

        #region "Backward compatiliblity methods"
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        [Obsolete("Use mapper.Sql(...).Map(callback) instead.")]
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
        [Obsolete("Use mapper.Sql(...).Map(callback) instead.")]
        public static Task ExecuteReader(this IQueryMapper mapper, SqlCommand cmd, Action<DbDataReader> callback)
        {
            return mapper.Map(cmd, callback);
        }

        [Obsolete("Use mapper.Sql(...).Map(callback) instead.")]
        public static Task ExecuteReader(this IQueryMapper mapper, SqlCommand cmd, Func<DbDataReader, Task> callback)
        {
            return mapper.Sql(cmd).Map(callback);
        }

        [Obsolete("Use mapper.Sql(...).Map(callback) instead.")]
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
            await mapper.Sql(cmd).Map(reader => sb.Append(reader[0]));
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
