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
    /// Executes SQL query and provides DataReader to callback function.
    /// </summary>
    public class QueryMapper: GenericQueryMapper<SqlCommand>
    {
        /// <summary>
        /// Creates Mapper object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        public QueryMapper(DbConnection connection) : base(connection) { }

        /// <summary>
        /// Creates Mapper object.
        /// </summary>
        /// <param name="connectionString">Connection string to Sql Database.</param>
        public QueryMapper(string connectionString) : base(new SqlConnection(connectionString)) { }

    }
}
