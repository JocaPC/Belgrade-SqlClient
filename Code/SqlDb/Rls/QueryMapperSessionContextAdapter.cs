//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.SqlDb.Rls
{
	[Obsolete("Use method: .AddRls(key, value) instead of this wrapper.")]
    /// <summary>
    /// Adapter for QueryMapper object that wraps SQL text with SESSION_CONTEXT variable.
    /// </summary>
    public class QueryMapperSessionContextAdapter: RlsBaseAdapter, IQueryMapper
    {
        /// <summary>
        /// QueryMapper object that will be wrapped with code that set/reset SESSION_VARIABLE required for RLS.
        /// </summary>
        SqlDb.QueryMapper Mapper = null;

        /// <summary>
        /// Creates QueryMapperSessionContextAdapter object.
        /// </summary>
        /// <param name="mapper">Mapper object that will be modified.</param>
        /// <param name="key">The name of the key in Sql Database SESSION_CONTEXT collection that is used in Row-level security predicates.</param>
        /// <param name="value">The function that will evaluate a value that will be entered in SESSION_CONTEXT.</param>
        public QueryMapperSessionContextAdapter(SqlDb.QueryMapper mapper, string key, Func<string> value): base(key, value)
        {
            this.Mapper = mapper;
            mapper.SetCommandModifier(base.commandModifier);            
        }

        public Task ExecuteReader(string sql, Action<DbDataReader> callback)
        {
            return Mapper.ExecuteReader(sql, callback);
        }

        public Task ExecuteReader(DbCommand command, Action<DbDataReader> callback)
        {
            return Mapper.ExecuteReader(command, callback);
        }

        public Task ExecuteReader(string sql, Func<DbDataReader, Task> callback)
        {
            return Mapper.ExecuteReader(sql, callback);
        }

        public Task ExecuteReader(DbCommand command, Func<DbDataReader, Task> callback)
        {
            return Mapper.ExecuteReader(command, callback);
        }

        public new IQueryMapper OnError(Action<Exception> handler)
        {
            return Mapper.OnError(handler) as IQueryMapper;
        }
    }
}