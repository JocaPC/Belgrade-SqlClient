using Code.SqlDb.Extensions;
using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static partial class IQueryMapperExtensions
    {
        /// <summary>
        /// Add a parameter with specified value to the mapper.
        /// </summary>
        /// <param name="mapper">Mapper where the parameter will be added.</param>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <returns>Mapper object.</returns>
        public static IQueryMapper Param(this IQueryMapper mapper, string name, object value)
        {
            Util.AddParameterWithValue(mapper, name, value);
            return mapper;
        }

        #region "Text command extensions"

        /// <summary>
        /// Set the query text on the mapper.
        /// </summary>
        /// <returns>Query Mapper.</returns>
        public static IQueryMapper Sql(this IQueryMapper mapper, string query)
        {
            var cmd = new SqlCommand(query);
            return mapper.Sql(cmd);
        }

        #endregion
        
        /// <summary>
        /// Executes sql statement and returns concatenated result as string.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <returns>Task</returns>
        public static async Task<string> GetString(this IQueryMapper mapper, SqlCommand cmd)
        {
            var sb = new StringBuilder();
            await mapper.Sql(cmd).Map(reader => sb.Append(reader[0]));
            return sb.ToString();
        }

        /// <summary>
        /// Executes sql statement and returns concatenated result as string.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <returns>Task</returns>
        public static async Task<string> GetString(this IQueryMapper mapper, string sql)
        {
            var cmd = new SqlCommand(sql);
            return await mapper.GetString(cmd);
        }

#if NET46
        /// <summary>
        /// Add action that will be executed once the underlying data source is changed.
        /// Make sure that you call SqlDependency.Start(connString) once application starts, and SqlDependency.Stop(connString); when application stops.
        /// </summary>
        /// <param name="mapper">Mapper object that will execute the query.</param>
        /// <param name="action">Action that will be executed once the query results change.</param>
        /// <returns>Mapper.</returns>
        public static IQueryMapper OnChange(this IQueryMapper mapper, Action<SqlNotificationEventArgs> action)
        {
            var cmd = ((mapper as Common.BaseStatement).Command as SqlCommand);
            new SqlDependency(cmd)
                .OnChange += (sender, e) => { if (e.Type == SqlNotificationType.Change) action(e); };
            return mapper;
        }

        /// <summary>
        /// Add action that will be executed once the underlying data source is changed.
        /// Make sure that you call SqlDependency.Start(connString) once application starts, and SqlDependency.Stop(connString); when application stops.
        /// </summary>
        /// <param name="mapper">Mapper object that will execute the query.</param>
        /// <param name="action">Action that will be executed once the query results change.</param>
        /// <returns>Mapper.</returns>
        public static IQueryMapper OnChange(this IQueryMapper mapper, OnChangeEventHandler action)
        {
            var cmd = ((mapper as Common.BaseStatement).Command as SqlCommand);
            new SqlDependency(cmd).OnChange += action;
            return mapper;
        }
#endif
    }
}
