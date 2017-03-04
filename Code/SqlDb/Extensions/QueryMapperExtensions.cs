//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Text;

namespace Belgrade.SqlClient.SqlDb
{
    public static class QueryMapperExtensions
    {
        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this QueryMapper mapper, string sql, Action<DbDataReader> callback)
        {
            var cmd = new SqlCommand(sql);
            return mapper.ExecuteReader(cmd, callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static Task ExecuteReader(this QueryMapper mapper, string sql, Func<DbDataReader, Task> callback)
        {
            var cmd = new SqlCommand(sql);
            return mapper.ExecuteReader(cmd, callback);
        }

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public static async Task<string> GetStringAsync(this QueryMapper mapper, SqlCommand cmd)
        {
            var sb = new StringBuilder();
            await mapper.ExecuteReader(cmd, reader => sb.Append(reader[0]));
            return sb.ToString();
        }

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <returns>Task</returns>
        public static Task<string> GetStringAsync(this QueryMapper mapper, string sql)
        {
            var cmd = new SqlCommand(sql);
            return QueryMapperExtensions.GetStringAsync(mapper, cmd);
        }
    }
}
