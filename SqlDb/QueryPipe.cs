using Belgrade.SqlClient.Common;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Belgrade.SqlClient.SqlDb
{
    /// <summary>
    /// Component that streams results of SQL query into an output stream.
    /// </summary>
    public class QueryPipe: QueryPipe<SqlCommand>
    {
        /// <summary>
        /// Creates QueryPipe object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        /// <param name="errorHandler">Function that will be called if some exception is thrown.</param>
        public QueryPipe(DbConnection connection, Action<Exception> errorHandler = null) : base(connection, errorHandler) { }
    }
}
