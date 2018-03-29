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
            await sut.Sql(SQLCmd).Exec();

            // Assert
            Assert.Equal("Hello World", p.Value);
        }

        [Theory]
        [InlineData("sys", "columns")]
        [InlineData("sys", "objects")]
        public async Task ExecuteProc(string schema, string view)
        {
            // Arrange
            string rSchema = "", rView = "";

            // Action
            await sut.Proc("sp_help")
                        .Param("objname", schema+"."+view)
                        .OnError(ex => Assert.False(true, "Unexpected exception: " + ex))
                        .Map(r => { rView = r["Name"].ToString();
                                    rSchema = r["Owner"].ToString();
                        });

            // Assert
            Assert.Equal(view, rView);
            Assert.Equal(schema, rSchema);
        }

        [Fact]
        public void SelectNullAsParameter()
        {
            // Arrange
            string sql = "SELECT TOP 1 a = 'OK' FROM sys.columns WHERE @p IS NULL";
            string result = "DEFAULT";

            // Action
            sut
                .Sql(sql)
                .Param("@p", DbType.String, null)
                .Map(r => result = r.GetString(0))
                .Wait();

            // Assert
            Assert.Equal("OK", result);
        }
    }
}