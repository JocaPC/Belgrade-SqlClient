using Belgrade.SqlClient.Common;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Belgrade.SqlClient.SqlDb
{
    /// <summary>
    /// Executes SQL query and provides DataReader to callback function.
    /// </summary>
    public class QueryMapper: GenericQueryMapper<SqlCommand>
    {
        /// <summary>
        /// Creates Mapper object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        /// <param name="errorHandler">Function that will be called if some exception is thrown.</param>
        public QueryMapper(DbConnection connection, Action<Exception> errorHandler = null) : base(connection, errorHandler) { }

        /// <summary>
        /// Creates Mapper object.
        /// </summary>
        /// <param name="connectionString">Connection string to Sql Database.</param>
        /// <param name="errorHandler">Function that will be called if some exception is thrown.</param>
        public QueryMapper(string connectionString, Action<Exception> errorHandler = null) : base(new SqlConnection(connectionString), errorHandler) { }


    }
}
