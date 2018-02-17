//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    /// <summary>
    /// Query component that streams results of SQL query into an output stream.
    /// </summary>
    public interface IQueryPipe
    {
        /// <summary>
        /// Set the query text that should be executed.
        /// </summary>
        /// <param name="query">Query that will be executed.</param>
        /// <returns>Query pipe.</returns>
        IQueryPipe Sql(DbCommand cmd);

        /// <summary>
        /// Executes SQL command and put results into stream.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        Task Stream(Stream stream, Options options = null);
        
        /// <summary>
        /// Executes SQL command and puts results into TextWriter.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="writer">TextWriter where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into TextWriter if there are no results.</param>
        /// <returns>Task</returns>
        Task Stream(TextWriter writer, Options options = null);

        IQueryPipe Param(string name, DbType type, object value, int size = 0);
    }
}