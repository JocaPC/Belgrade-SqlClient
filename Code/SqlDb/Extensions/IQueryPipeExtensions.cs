using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Belgrade.SqlClient.SqlDb;
using Belgrade.SqlClient.Common;

namespace Belgrade.SqlClient
{
    public static class IQueryPipeExtensions
    {
        public static Task Stream(this IQueryPipe pipe, string sql, TextWriter writer, string defaultOutput)
        {
            if (!(pipe is QueryPipe))
                throw new ArgumentException("Argument pipe must be derived from QueryPipe", "pipe");
            SqlCommand cmd = new SqlCommand(sql);
            return pipe.Stream(cmd, writer, new Options() { DefaultOutput = defaultOutput });
        }
        
        public static Task Stream(this IQueryPipe pipe, SqlCommand sql, TextWriter writer, string defaultOutput)
        {
            if (!(pipe is QueryPipe))
                throw new ArgumentException("Argument pipe must be derived from QueryPipe", "pipe");
            return pipe.Stream(sql, writer, new Options() { DefaultOutput = defaultOutput });
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
            SqlCommand cmd = new SqlCommand(sql);
            return pipe.Stream(cmd, stream);
        }

        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into stream if there are no results.</param>
        /// <returns>Task</returns>
        public static Task Stream(this IQueryPipe pipe, string sql, Stream stream, string defaultOutput)
        {
            if (!(pipe is QueryPipe))
                throw new ArgumentException("Argument pipe must be derived from QueryPipe", "pipe");
            SqlCommand cmd = new SqlCommand(sql);
            return (pipe as QueryPipe).Stream(cmd, stream, new Options() { DefaultOutput = defaultOutput });
        }

        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into stream if there are no results.</param>
        /// <returns>Task</returns>
        public static Task Stream(this IQueryPipe pipe, SqlCommand sql, Stream stream, string defaultOutput)
        {
            if (!(pipe is QueryPipe))
                throw new ArgumentException("Argument pipe must be derived from QueryPipe", "pipe");
            return pipe.Stream(sql, stream, new Options() { DefaultOutput = defaultOutput });
        }

        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into stream if there are no results.</param>
        /// <returns>Task</returns>
        public static Task Stream(this IQueryPipe pipe, string sql, Stream stream, byte[] defaultOutput)
        {
            if (!(pipe is QueryPipe))
                throw new ArgumentException("Argument pipe must be derived from QueryPipe", "pipe");
            SqlCommand cmd = new SqlCommand(sql);
            return (pipe as QueryPipe).Stream(cmd, stream, new Options() { DefaultOutput = defaultOutput });
        }

        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into stream if there are no results.</param>
        /// <returns>Task</returns>
        public static Task Stream(this IQueryPipe pipe, SqlCommand sql, Stream stream, byte[] defaultOutput)
        {
            if (!(pipe is QueryPipe))
                throw new ArgumentException("Argument pipe must be derived from QueryPipe", "pipe");
            return (pipe as QueryPipe).Stream(sql, stream, new Options() { DefaultOutput = defaultOutput });
        }

        public static Task Stream(this IQueryPipe pipe, string sql, Stream stream, Options options)
        {
            if (!(pipe is QueryPipe))
                throw new ArgumentException("Argument pipe must be derived from QueryPipe", "pipe");
            SqlCommand cmd = new SqlCommand(sql);
            return (pipe as QueryPipe).Stream(cmd, stream, options);
        }

        public static Task Stream(this IQueryPipe pipe, string sql, TextWriter writer, Options options)
        {
            if (!(pipe is QueryPipe))
                throw new ArgumentException("Argument pipe must be derived from QueryPipe", "pipe");
            SqlCommand cmd = new SqlCommand(sql);
            return (pipe as QueryPipe).Stream(cmd, writer, options);
        }

        public static IQueryPipe AddContextVariable(this IQueryPipe pipe, string key, Func<string> value)
        {
            var stmt = pipe as BaseStatement;
            stmt.SetCommandModifier(c => SqlDb.Rls.RlsExtension.AddContextVariables(key, value, c));
            return pipe;
        }
    }
}
