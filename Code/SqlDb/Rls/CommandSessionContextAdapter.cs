//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.SqlDb.Rls
{
    [Obsolete("Use method: .AddRls(key, value) instead of this wrapper.")]
    /// <summary>
    /// Adapter for Command object that wraps SQL text with SESSION_CONTEXT variable.
    /// </summary>
    public class CommandSessionContextAdapter : RlsBaseAdapter, ICommand
    {
        /// <summary>
        /// Command object that will be wrapped with SESSION_CONTEXT variable.
        /// </summary>
        SqlDb.Command SqlCommand = null;

        /// <summary>
        /// Creates CommandSessionContextAdapter object.
        /// </summary>
        /// <param name="command">Command object that will be adapted.</param>
        /// <param name="key">The name of the key in Sql Database SESSION_CONTEXT collection that is used in Row-level security predicates.</param>
        /// <param name="value">The function that will evaluate a value that will be entered in SESSION_CONTEXT.</param>
        public CommandSessionContextAdapter(SqlDb.Command command, string key, Func<string> value) : base(key, value)
        {
            this.SqlCommand = command;
            SqlCommand.SetCommandModifier(base.commandModifier);
        }

        /// <summary>
        /// Executes Sql command.
        /// </summary>
        /// <param name="command">SqlCommand that will be executed.</param>
        /// <returns>Generic task.</returns>
        public Task ExecuteNonQuery(DbCommand command)
        {
            return SqlCommand.ExecuteNonQuery(command);
        }

        public Task ExecuteNonQuery(string sql)
        {
            return SqlCommand.ExecuteNonQuery(sql);
        }

        public Task ExecuteReader(DbCommand command, Action<DbDataReader> callback)
        {
            return SqlCommand.ExecuteReader(command, callback);
        }

        public Task ExecuteReader(DbCommand command, Func<DbDataReader, Task> callback)
        {
            return SqlCommand.ExecuteReader(command, callback);
        }

        public Task ExecuteReader(string sql, Action<DbDataReader> callback)
        {
            return this.SqlCommand.ExecuteReader(sql, callback);
        }

        public Task ExecuteReader(string sql, Func<DbDataReader, Task> callback)
        {
            return SqlCommand.ExecuteReader(sql, callback);
        }

        public new ICommand OnError(Action<Exception> handler)
        {
            return SqlCommand.OnError(handler) as ICommand;
        }

        public Task Stream<D>(DbCommand command, Stream output, D defaultOutput)
        {
            return SqlCommand.Stream<D>(command, output, defaultOutput);
        }

        public Task Stream<D>(string sql, Stream output, D defaultOutput)
        {
            return SqlCommand.Stream<D>(sql, output, defaultOutput);
        }
    }
}