//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.SqlDb
{
    public static class QueryPipeExtensions
    {
        public static Task Stream(this QueryPipe pipe, string sql, TextWriter writer, string defaultOutput)
        { 
            var cmd = new SqlCommand(sql);
            return pipe.Stream(cmd, writer, defaultOutput);
        }

        /// <summary>
        /// Executes SQL query and puts results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        public static Task Stream(this QueryPipe pipe, string sql, Stream stream)
        {
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
        public static Task Stream<T>(this QueryPipe pipe, string sql, Stream stream, T defaultOutput)
        {
            var cmd = new SqlCommand(sql);
            return pipe.Stream<T>(cmd, stream, defaultOutput);
        }
    }
}
