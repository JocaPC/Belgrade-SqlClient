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
    public static class IQueryPipeExtensions
    {
        /// <summary>
        /// Set the query text on the query pipe.
        /// </summary>
        /// <returns>Query Pipe.</returns>
        public static IQueryPipe Sql(this IQueryPipe pipe, DbCommand cmd)
        {
            if (pipe is BaseStatement)
                (pipe as BaseStatement).SetCommand(cmd);
            return pipe;
        }

        public static IQueryPipe Param(this IQueryPipe pipe, string name, System.Data.DbType type, object value, int size = 0)
        {
            if (pipe is BaseStatement)
                (pipe as BaseStatement).AddParameter(name, type, value, size);
            return pipe;
        }

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

        public static Task Stream(this IQueryPipe pipe, DbCommand cmd, Stream stream, Options options = null)
        {
            pipe.Sql(cmd);
            return pipe.Stream(stream, options);
        }

        public static Task Stream(this IQueryPipe pipe, DbCommand cmd, TextWriter writer, Options options = null)
        {
            pipe.Sql(cmd);
            return pipe.Stream(writer, options);
        }

        public static Task Stream(this IQueryPipe pipe, string sql, TextWriter writer, string defaultOutput)
        {
            SqlCommand cmd = new SqlCommand(sql);
            return pipe.Stream(cmd, writer, new Options() { DefaultOutput = defaultOutput });
        }
        
        public static Task Stream(this IQueryPipe pipe, SqlCommand sql, TextWriter writer, string defaultOutput)
        {
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
            SqlCommand cmd = new SqlCommand(sql);
            return pipe.Stream(cmd, stream, new Options() { DefaultOutput = defaultOutput });
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
            return pipe.Stream(sql, stream, new Options() { DefaultOutput = defaultOutput });
        }

        public static Task Stream(this IQueryPipe pipe, string sql, Stream stream, Options options)
        {
            SqlCommand cmd = new SqlCommand(sql);
            return pipe.Stream(cmd, stream, options);
        }

        public static Task Stream(this IQueryPipe pipe, string sql, TextWriter writer, Options options)
        {
            SqlCommand cmd = new SqlCommand(sql);
            return pipe.Stream(cmd, writer, options);
        }

        public static IQueryPipe AddContextVariable(this IQueryPipe pipe, string key, Func<string> value)
        {
            var stmt = pipe as BaseStatement;
            stmt.SetCommandModifier(c => SqlDb.Rls.RlsExtension.AddContextVariables(key, value, c));
            return pipe;
        }
    }
}
