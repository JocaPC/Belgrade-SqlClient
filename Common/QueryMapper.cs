using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.Common
{
    /// <summary>
    /// Executes SQL query and provides DataReader to callback function.
    /// </summary>
    public class QueryMapper<T> : IQueryMapper
        where T : DbCommand, new()
    {
        /// <summary>
        /// Connection to Sql Database.
        /// </summary>
        private DbConnection Connection;

        /// <summary>
        /// Creates Query object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        public QueryMapper(DbConnection connection)
        {
            this.Connection = connection;
        }

        /// <summary>
        /// Executes sql statement and provides each row to the callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(string sql, Func<DbDataReader, Task> callback)
        {
            using (DbCommand command = new T())
            {
                command.CommandText = sql;
                command.Connection = this.Connection;
                await this.ExecuteReader(command, callback);
            }
        }

        /// <summary>
        /// Executes sql command and provides each row to the callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(DbCommand command, Action<DbDataReader> callback)
        {
            if (command.Connection == null)
                command.Connection = this.Connection;
            try
            {
                await command.Connection.OpenAsync().ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        callback(reader);
                    }
                }
            }
            finally
            {
                command.Connection.Close();
            }
        }

        /// <summary>
        /// Executes sql statement and provides each row to the async callback function.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(string sql, Action<DbDataReader> callback)
        {
            using (DbCommand command = new T())
            {
                command.CommandText = sql;
                command.Connection = this.Connection;
                await this.ExecuteReader(command, callback);
            }
        }

        /// <summary>
        /// Executes sql command and provides each row to the async callback function.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="callback">Async callback function that will be called for each row.</param>
        /// <returns>Task</returns>
        public async Task ExecuteReader(DbCommand command, Func<DbDataReader, Task> callback)
        {
            if(command.Connection == null)
                command.Connection = this.Connection;
            try
            {
                await command.Connection.OpenAsync().ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        await callback(reader).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                command.Connection.Close();
            }
        }
    }
}