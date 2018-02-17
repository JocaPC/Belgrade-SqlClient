//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Data.Common;
namespace Belgrade.SqlClient
{
    /// <summary>
    /// Sql Query that will be executed. Command does not return any result.
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Set the query text that should be executed.
        /// </summary>
        /// <param name="query">Query text that will be executed.</param>
        /// <returns>Query.</returns>
        IQuery Sql(DbCommand cmd);

        /// <summary>
        /// Assigns a parameter with value to the query.
        /// </summary>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="type">Type of the parameter.</param>
        /// <param name="value">Parameter value.</param>
        /// <returns>Query.</returns>
        IQuery Param(string name, System.Data.DbType type, object value, int size = 0);
    }
    
}