using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Belgrade.SqlClient.SqlDb;
using System.Text;

namespace Belgrade.SqlClient
{
    public static class IQueryMapperExtensions
    {
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this IQueryMapper mapper, string sql, Action<DbDataReader> callback)
        {
            if (!(mapper is QueryMapper))
                throw new ArgumentException("Argument mapper must be derived from QueryMapper", "mapper");
            return QueryMapperExtensions.ExecuteReader((mapper as QueryMapper), sql, callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this IQueryMapper mapper, string sql, Func<DbDataReader, Task> callback)
        {
            if (!(mapper is QueryMapper))
                throw new ArgumentException("Argument mapper must be derived from QueryMapper", "mapper");
            return QueryMapperExtensions.ExecuteReader((mapper as QueryMapper), sql, callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static async Task<string> GetStringAsync(this IQueryMapper mapper, SqlCommand cmd)
        {
            if (!(mapper is QueryMapper))
                throw new ArgumentException("Argument mapper must be derived from QueryMapper", "mapper");
            return await QueryMapperExtensions.GetStringAsync((mapper as QueryMapper), cmd);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <returns>Task</returns>
        public static async Task<string> GetStringAsync(this IQueryMapper mapper, string sql)
        {
            if (!(mapper is QueryMapper))
                throw new ArgumentException("Argument mapper must be derived from QueryMapper", "mapper");
            return await QueryMapperExtensions.GetStringAsync((mapper as QueryMapper), sql);
        }
    }
}
