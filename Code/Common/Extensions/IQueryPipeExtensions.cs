using System;
using System.IO;
using System.Threading.Tasks;
using Belgrade.SqlClient.Common;
using System.Data.Common;
using Code.SqlDb.Extensions;

namespace Belgrade.SqlClient
{
    public static partial class IQueryPipeExtensions
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
 
        /// <summary>
        /// Executes SQL query and puts results into stream.
        /// </summary>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        public static Task Stream(this IQueryPipe pipe, Stream stream, string defaultOutput = "[]")
        {
            return pipe.Stream(stream, new Options() { DefaultOutput = defaultOutput });
        }

        /// <summary>
        /// Executes SQL query and puts results into stream.
        /// </summary>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        public static Task Stream(this IQueryPipe pipe, TextWriter writer, string defaultOutput = "[]")
        {
            return pipe.Stream(writer, new Options() { DefaultOutput = defaultOutput });
        }

        public static IQueryPipe AddContextVariable(this IQueryPipe pipe, string key, Func<string> value, bool isReadOnly = true)
        {
            var stmt = pipe as BaseStatement;
            stmt.SetCommandModifier(c => SqlDb.Rls.RlsExtension.AddContextVariables(key, value, c, isReadOnly));
            return pipe;
        }
    }
}
