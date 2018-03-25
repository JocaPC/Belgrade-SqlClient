using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static partial class IQueryMapperExtensions
    {

        #region "Text command extensions"
        
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
        public static Task ExecuteReader(this IQueryMapper mapper, SqlCommand cmd, Action<DbDataReader> callback)
        {
            return mapper.Map(cmd, callback);
        }

        [Obsolete("Use mapper.Sql(...).Map(callback) instead.")]
        public static Task ExecuteReader(this IQueryMapper mapper, SqlCommand cmd, Func<DbDataReader, Task> callback)
        {
            return mapper.Sql(cmd).Map(callback);
        }

        
        #endregion

        
    }
}
