using Belgrade.SqlClient.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static partial class IQueryMapperExtensions
    {
        public static IQueryMapper Sql(this IQueryMapper mapper, DbCommand cmd)
        {
            if (mapper is BaseStatement)
                (mapper as BaseStatement).SetCommand(cmd);
            return mapper;
        }


        public static IQueryMapper Param(this IQueryMapper mapper, string name, System.Data.DbType type, object value, int size = 0)
        {
            if (mapper is BaseStatement)
                (mapper as BaseStatement).AddParameter(name, type, value, size);
            return mapper;
        }

        /// <summary>
        /// Returns first row mapped as object T, or default value of T if no results are returned.
        /// </summary>
        /// <typeparam name="T">Generic type of the objects that wil be returned.</typeparam>
        /// <param name="query">The IQueryMapper object.</param>
        /// <param name="mapper">Mapper function that will convert DbDatareader to T</param>
        /// <returns>First object in the result set of default(T).</returns>
        public async static Task<T> FirstOrDefault<T>(this IQueryMapper query, Func<DbDataReader, T> mapper) where T : new()
        {
            var res = new T();
            bool isNull = true;
            await query.Map(reader => { res = mapper(reader); isNull = false; });
            if (isNull)
                return default(T);
            else
                return res;
        }

        public async static Task<List<T>> Map<T>(this IQueryMapper query, Func<DbDataReader, T> mapper)
        {
            var res = new List<T>();
            await query.Map(reader => { res.Add(mapper(reader)); });
            return res;
        }
    }
}
