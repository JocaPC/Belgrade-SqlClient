using Belgrade.SqlClient.Common;
using System.Data.Common;
using System.Data.SqlClient;

namespace Belgrade.SqlClient.SqlDb
{
    public class QueryMapper: QueryMapper<SqlCommand>
    {
        public QueryMapper(DbConnection connection) : base(connection) { }
    }
}
