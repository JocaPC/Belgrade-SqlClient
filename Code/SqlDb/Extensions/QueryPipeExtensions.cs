using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Belgrade.SqlClient.SqlDb;

namespace Belgrade.SqlClient
{
    public static class QueryPipeExtensions
    {
        public static Task Stream(this IQueryPipe pipe, string sql, TextWriter writer, string defaultOutput)
        {
            if (!(pipe is QueryPipe))
                throw new ArgumentException("Argument pipe must be derived from QueryPipe", "pipe");
            var cmd = new SqlCommand(sql);
            return pipe.Stream(cmd, writer, defaultOutput);
        }

        /// <summary>
        /// Executes SQL query and puts results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        public static Task Stream(this IQueryPipe pipe, string sql, Stream stream)
        {
            if (!(pipe is QueryPipe))
                throw new ArgumentException("Argument pipe must be derived from QueryPipe", "pipe");
            var cmd = new SqlCommand(sql);
            return pipe.Stream(cmd, stream);
        }

        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into stream if there are no results.</param>
        /// <returns>Task</returns>
        public static Task Stream<T>(this IQueryPipe pipe, string sql, Stream stream, T defaultOutput)
        {
            if (!(pipe is QueryPipe))
                throw new ArgumentException("Argument pipe must be derived from QueryPipe", "pipe");
            var cmd = new SqlCommand(sql);
            return pipe.Stream<T>(cmd, stream, defaultOutput);
        }
    }
}
