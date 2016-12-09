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
    /// Sql Command that will be executed.
    /// </summary>
    public class Command: GenericCommand<SqlCommand>
    {
        /// <summary>
        /// Creates command object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        public Command(DbConnection connection) : base(connection) { }
        
        /// <summary>
        /// Creates command object.
        /// </summary>
        /// <param name="connectionString">Connection string to Sql Database.</param>
        public Command(string connectionString) : base(new SqlConnection(connectionString)) { }
    }
}
