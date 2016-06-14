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
