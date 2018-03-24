//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license.
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using Belgrade.SqlClient.Common;
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
        public QueryPipe(DbConnection connection) : base(connection) { }

        /// <summary>
        /// Creates QueryPipe object.
        /// </summary>
        /// <param name="connectionString">Connection string to Sql Database.</param>
        public QueryPipe(string connectionString) : base(new SqlConnection(connectionString)) { }
    }
}
