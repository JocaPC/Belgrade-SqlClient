using Belgrade.SqlClient.Common;
using System.Data.Common;
using System.Data.SqlClient;

namespace Belgrade.SqlClient.SqlDb
{
    public class Command: Command<SqlCommand>
    {
        public Command(DbConnection connection) : base(connection) { }
    }
}
