//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    /// <summary>
    /// Sql Command that will be executed. Command does not return any result.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Set the query text that should be executed.
        /// </summary>
        /// <param name="query">Query that will be executed.</param>
        /// <returns>Command.</returns>
        ICommand Sql(DbCommand cmd);

        /// <summary>
        /// Executes Sql command.
        /// </summary>
        /// <param name="command">SqlCommand that will be executed.</param>
        /// <returns>Generic task.</returns>
        Task Exec();

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        Task Map(DbCommand command, Func<DbDataReader, Task> callback);

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        Task Map(DbCommand command, Action<DbDataReader> callback);

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        Task Stream(DbCommand command, Stream output, Options options);

        /// <summary>
        /// Assigns a parameter with value to the query.
        /// </summary>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="type">Type of the parameter.</param>
        /// <param name="value">Parameter value.</param>
        /// <returns>Command.</returns>
        ICommand Param(string name, System.Data.DbType type, object value, int size = 0);

    }

}