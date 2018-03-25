using Code.SqlDb.Extensions;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static partial class IQueryMapperExtensions
    {
        public static IQueryMapper AddWithValue(this IQueryMapper mapper, string name, object value)
        {
            Util.AddParameterWithValue(mapper, name, value);
            return mapper;
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
