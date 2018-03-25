using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Belgrade.SqlClient.SqlDb;
using Belgrade.SqlClient.Common;
using System.Data.Common;
using Code.SqlDb.Extensions;

namespace Belgrade.SqlClient
{
    public static partial class IQueryPipeExtensions
    {
        public static IQueryPipe AddWithValue(this IQueryPipe pipe, string name, object value)
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
