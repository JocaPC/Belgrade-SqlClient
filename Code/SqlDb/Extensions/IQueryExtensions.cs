using Belgrade.SqlClient.Common;
using Code.SqlDb.Extensions;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static partial class IQueryExtensions
    {
        public static IQuery Sql(this IQuery query, DbCommand cmd)
        {
            if (query is BaseStatement)
                (query as BaseStatement).SetCommand(cmd);
            return query;
        }

        /// <summary>
        /// Add a parameter with specified value to the mapper.
        /// </summary>
        /// <param name="query">Mapper where the parameter will be added.</param>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <returns>Query object.</returns>
        public static IQuery Param(this IQuery query, string name, object value)
        {
            Util.AddParameterWithValue(query, name, value);
            return query;
        }

        #region "Text command extensions"

        /// <summary>
        /// Set the query text on the mapper.
        /// </summary>
        /// <returns>Query Mapper.</returns>
        public static IQuery Sql(this IQuery query, string queryText)
        {
            var cmd = new SqlCommand(queryText);
            return query.Sql(cmd);
        }

        #endregion
        
        /// <summary>
        /// Executes sql statement and returns concatenated result as string.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <returns>Task</returns>
        public static async Task<string> GetString(this IQuery query, SqlCommand cmd)
        {
            var sb = new StringBuilder();
            await query.Sql(cmd).Map(reader => sb.Append(reader[0]));
            return sb.ToString();
        }

        /// <summary>
        /// Executes sql statement and returns concatenated result as string.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <returns>Task</returns>
        public static async Task<string> GetString(this IQuery query, string sql)
        {
            var cmd = new SqlCommand(sql);
            return await query.GetString(cmd);
        }

#if NET46
        /// <summary>
        /// Add action that will be executed once the underlying data source is changed.
        /// Make sure that you call SqlDependency.Start(connString) once application starts, and SqlDependency.Stop(connString); when application stops.
        /// </summary>
        /// <param name="query">The object that will execute the query.</param>
        /// <param name="action">Action that will be executed once the query results change.</param>
        /// <returns>Mapper.</returns>
        public static IQuery OnChange(this IQuery query, Action<SqlNotificationEventArgs> action)
        {
            var cmd = ((query as Common.BaseStatement).Command as SqlCommand);
            new SqlDependency(cmd)
                .OnChange += (sender, e) => { if (e.Type == SqlNotificationType.Change) action(e); };
            return query;
        }

        /// <summary>
        /// Add action that will be executed once the underlying data source is changed.
        /// Make sure that you call SqlDependency.Start(connString) once application starts, and SqlDependency.Stop(connString); when application stops.
        /// </summary>
        /// <param name="query">Mapper object that will execute the query.</param>
        /// <param name="action">Action that will be executed once the query results change.</param>
        /// <returns>Mapper.</returns>
        public static IQuery OnChange(this IQuery query, OnChangeEventHandler action)
        {
            var cmd = ((query as Common.BaseStatement).Command as SqlCommand);
            new SqlDependency(cmd).OnChange += action;
            return query;
        }
#endif
    }
}
