using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Basic
{
    public class Command
    {
        ICommand sut;
        public Command()
        {
            sut = new Belgrade.SqlClient.SqlDb.Command(Util.Settings.ConnectionString);
        }

        [Fact]
        public async Task ReturnOutputParameter()
        {
            // Arrange
            string sql = "EXEC xp_sprintf @string OUTPUT , 'Hello %s' , 'World'";
            var SQLCmd = new SqlCommand();
            SQLCmd.CommandTimeout = 360;
            SQLCmd.CommandText = sql;
            var p = SQLCmd.Parameters.Add(new SqlParameter("@string", SqlDbType.VarChar));
            p.Direction = ParameterDirection.Output;
            p.Size = 4000;

            // Action
            await sut.ExecuteNonQuery(SQLCmd);

            // Assert
            Assert.Equal("Hello World", p.Value);
        }
    }
}