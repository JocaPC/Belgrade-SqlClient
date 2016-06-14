//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using Belgrade.SqlClient.Common;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Belgrade.SqlClient.SqlDb
{
    /// <summary>
    /// Component that streams results of SQL query into an output stream.
    /// </summary>
    public class QueryPipe: GenericQueryPipe<SqlCommand>
    {
        /// <summary>
        /// Creates QueryPipe object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        /// <param name="errorHandler">Function that will be called if some exception is thrown.</param>
        public QueryPipe(DbConnection connection, Action<Exception> errorHandler = null) : base(connection, errorHandler) { }

        /// <summary>
        /// Creates QueryPipe object.
        /// </summary>
        /// <param name="connectionString">Connection string to Sql Database.</param>
        /// <param name="errorHandler">Function that will be called if some exception is thrown.</param>
        public QueryPipe(string connectionString, Action<Exception> errorHandler = null) : base(new SqlConnection(connectionString), errorHandler) { }
    }
}
