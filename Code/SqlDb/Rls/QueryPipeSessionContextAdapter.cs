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
    /// QueryPipeSessionContextAdapter that adapts QueryPipe with required RlS variables.
    /// </summary>
    public class QueryPipeSessionContextAdapter: RlsBaseAdapter, IQueryPipe
    {
        /// <summary>
        /// Sql Pipe object that will be adapted for Rls.
        /// </summary>
        SqlDb.QueryPipe Pipe = null;

        /// <summary>
        /// Creates QueryPipeSessionContextAdapter object.
        /// </summary>
        /// <param name="pipe">Sql Pipe object that will be adapted for Rls.</param>
        /// <param name="key">The name of the key in Sql Database SESSION_CONTEXT collection that is used in Row-level security predicates.</param>
        /// <param name="value">The function that will evaluate a value that will be entered in SESSION_CONTEXT.</param>
        public QueryPipeSessionContextAdapter(SqlDb.QueryPipe pipe, string key, Func<string> value): base(key, value)
        {
            this.Pipe = pipe;
            this.Pipe.SetCommandModifier(base.commandModifier);            
        }

        public new IQueryPipe OnError(Action<Exception> handler)
        {
            return Pipe.OnError(handler) as IQueryPipe;
        }

        public Task Stream(DbCommand command, Stream stream)
        {
            return Pipe.Stream(command, stream);
        }

        public Task Stream(string sql, Stream stream)
        {
            return Pipe.Stream(sql, stream);
        }

        public Task Stream<T>(DbCommand command, Stream stream, T defaultOutput)
        {
            return ((IQueryPipe)Pipe).Stream<T>(command, stream, defaultOutput);
        }

        public Task Stream<T>(string sql, Stream stream, T defaultOutput)
        {
            return Pipe.Stream<T>(sql, stream, defaultOutput);
        }
    }
}