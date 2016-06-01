using Belgrade.SqlClient.Common;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Belgrade.SqlClient.SqlDb
{
    /// <summary>
    /// Sql Command that will be executed.
    /// </summary>
    public class Command: Command<SqlCommand>
    {
        /// <summary>
        /// Creates command object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        /// <param name="errorHandler">Function that will be called if some exception is thrown.</param>
        public Command(DbConnection connection, Action<Exception> errorHandler = null) : base(connection, errorHandler) { }
    }
}
