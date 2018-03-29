using Code.SqlDb.Extensions;
using System.Data.SqlClient;

namespace Belgrade.SqlClient
{
    public static partial class IQueryPipeExtensions
    {
        /// <summary>
        /// Add a paramater to the pipe with a value and inferred type.
        /// </summary>
        /// <param name="pipe">The pipe object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The pipe object with the added parameter.</returns>
        public static IQueryPipe Param(this IQueryPipe pipe, string name, object value)
        {
            Util.AddParameterWithValue(pipe, name, value);
            return pipe;
        }
        /// <summary>
        /// Set the query text on the query pipe.
        /// </summary>
        /// <returns>Query Pipe.</returns>
        public static IQueryPipe Sql(this IQueryPipe pipe, string query)
        {
            var cmd = new SqlCommand(query);
            return pipe.Sql(cmd);
        }
    }
}
