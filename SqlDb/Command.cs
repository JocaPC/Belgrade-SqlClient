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
    /// Sql Command that will be executed.
    /// </summary>
    public class Command: GenericCommand<SqlCommand>
    {
        /// <summary>
        /// Creates command object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        /// <param name="errorHandler">Function that will be called if some exception is thrown.</param>
        public Command(DbConnection connection, Action<Exception> errorHandler = null) : base(connection, errorHandler) { }
        
        /// <summary>
        /// Creates command object.
        /// </summary>
        /// <param name="connectionString">Connection string to Sql Database.</param>
        /// <param name="errorHandler">Function that will be called if some exception is thrown.</param>
        public Command(string connectionString, Action<Exception> errorHandler = null) : base(new SqlConnection(connectionString), errorHandler) { }
    }
}
