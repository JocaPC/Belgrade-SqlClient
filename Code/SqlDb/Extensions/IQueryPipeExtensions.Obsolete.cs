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
        [Obsolete("Use pipe.Sql(...).Stream(stream, options) instead.")]
        public static Task Stream(this IQueryPipe pipe, DbCommand cmd, Stream stream, Options options = null)
        {
            pipe.Sql(cmd);
            return pipe.Stream(stream, options);
        }

        [Obsolete("Use pipe.Sql(...).Stream(writer, options) instead.")]
        public static Task Stream(this IQueryPipe pipe, DbCommand cmd, TextWriter writer, Options options = null)
        {
            pipe.Sql(cmd);
            return pipe.Stream(writer, options);
        }

        [Obsolete("Use pipe.Sql(...).Stream(writer, defaultOptions) instead.")]
        public static Task Stream(this IQueryPipe pipe, string sql, TextWriter writer, string defaultOutput)
        {
            SqlCommand cmd = new SqlCommand(sql);
            return pipe.Sql(sql).Stream(writer, new Options() { DefaultOutput = defaultOutput });
        }

        [Obsolete("Use pipe.Sql(...).Stream(writer, defaultOptions) instead.")]
        public static Task Stream(this IQueryPipe pipe, SqlCommand sql, TextWriter writer, string defaultOutput)
        {
            return pipe.Sql(sql).Stream(writer, new Options() { DefaultOutput = defaultOutput });
        }

        /// <summary>
        /// Executes SQL query and puts results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        [Obsolete("Use pipe.Sql(...).Stream(stream) instead.")]
        public static Task Stream(this IQueryPipe pipe, string sql, Stream stream)
        {
            SqlCommand cmd = new SqlCommand(sql);
            return pipe.Sql(cmd).Stream(stream);
        }

        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into stream if there are no results.</param>
        /// <returns>Task</returns>
        [Obsolete("Use pipe.Sql(...).Stream(stream, defaultOutput) instead.")]
        public static Task Stream(this IQueryPipe pipe, string sql, Stream stream, string defaultOutput)
        {
            SqlCommand cmd = new SqlCommand(sql);
            return pipe.Sql(cmd).Stream(stream, new Options() { DefaultOutput = defaultOutput });
        }

        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into stream if there are no results.</param>
        /// <returns>Task</returns>
        [Obsolete("Use pipe.Sql(...).Stream(stream, defaultOutput) instead.")]
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
        [Obsolete("Use pipe.Sql(...).Stream(stream, defaultOutput) instead.")]
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
        [Obsolete("Use pipe.Sql(...).Stream(stream, defaultOutput) instead.")]
        public static Task Stream(this IQueryPipe pipe, SqlCommand sql, Stream stream, byte[] defaultOutput)
        {
            return pipe.Stream(sql, stream, new Options() { DefaultOutput = defaultOutput });
        }

        [Obsolete("Use pipe.Sql(...).Stream(stream, options) instead.")]
        public static Task Stream(this IQueryPipe pipe, string sql, Stream stream, Options options)
        {
            SqlCommand cmd = new SqlCommand(sql);
            return pipe.Stream(cmd, stream, options);
        }

        [Obsolete("Use pipe.Sql(...).Stream(writer, options) instead.")]
        public static Task Stream(this IQueryPipe pipe, string sql, TextWriter writer, Options options)
        {
            SqlCommand cmd = new SqlCommand(sql);
            return pipe.Sql(cmd).Stream(writer, options);
        }        
    }
}
