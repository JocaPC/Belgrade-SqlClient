//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE. See the license files for details.
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    /// <summary>
    /// Executes SQL query and provides DataReader to callback function.
    /// </summary>
    public interface IQueryMapper
    {
        /// <summary>
        /// Set the query that should be executed.
        /// </summary>
        /// <param name="query">Query that will be executed.</param>
        /// <returns>Command.</returns>
        IQueryMapper Sql(DbCommand cmd);

        /// <summary>
        /// Executes sql command and provides each row to the callback function.
        /// </summary>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        Task Map(Action<DbDataReader> callback);

        /// <summary>
        /// Executes sql command and provides each row to the async callback function.
        /// </summary>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        Task Map(Func<DbDataReader, Task> callback);

        /// <summary>
        /// Assigns a parameter with value to the query.
        /// </summary>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="type">Type of the parameter.</param>
        /// <param name="value">Parameter value.</param>
        /// <returns>Command.</returns>
        IQueryMapper Param(string name, System.Data.DbType type, object value, int size = 0);

    }
}