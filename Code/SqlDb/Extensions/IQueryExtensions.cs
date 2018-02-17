using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static class IQueryExtensions
    {
        public static IQuery Sql(this IQuery query, string sql)
        {
            var cmd = new SqlCommand(sql);
            return query.Sql(cmd);
        }
    }
}