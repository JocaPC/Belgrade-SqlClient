//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using Belgrade.SqlClient.SqlDb;
using System;
using System.Data.Common;
using System.Data.SqlClient;
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
        /// Executes SQL command and put results into stream.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        Task Stream(DbCommand command, Stream stream);

        /// <summary>
        /// Executes SQL command and put results into stream.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into stream if there are no results.</param>
        /// <returns>Task</returns>
        Task Stream<T>(DbCommand command, Stream stream, T defaultOutput);

        /// <summary>
        /// Executes SQL command and puts results into TextWriter.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="writer">TextWriter where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into TextWriter if there are no results.</param>
        /// <returns>Task</returns>
        Task Stream(DbCommand command, TextWriter writer, string defaultOutput);
    }
}