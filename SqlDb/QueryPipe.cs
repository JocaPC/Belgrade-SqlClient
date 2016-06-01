using Belgrade.SqlClient.Common;
using System.Data.Common;
using System.Data.SqlClient;

namespace Belgrade.SqlClient.SqlDb
{
    public class QueryPipe: QueryPipe<SqlCommand>
    {
        public QueryPipe(DbConnection connection) : base(connection) { }
    }
}
