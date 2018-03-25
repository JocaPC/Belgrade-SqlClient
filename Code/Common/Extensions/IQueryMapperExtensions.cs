using Belgrade.SqlClient.Common;
using System;
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
    }
}
