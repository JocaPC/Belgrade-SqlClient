using Belgrade.SqlClient.Common;
using System;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static partial class IQueryMapperExtensions
    {
        [Obsolete("Use mapper.Sql(...).Map(callback) instead.")]
        public static Task Map(this IQueryMapper mapper, DbCommand cmd, Action<DbDataReader> callback)
        {
            return mapper.Sql(cmd).Map(callback);
        }

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
    }
}
