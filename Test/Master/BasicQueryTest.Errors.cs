using Belgrade.SqlClient;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Errors
{
    public class Pipe
    {
        IQueryPipe sut;
        public Pipe()
        {
            sut = new Belgrade.SqlClient.SqlDb.QueryPipe(Util.Settings.ConnectionString);
        }


        [Fact]
        public async Task ErrorInSql()
        {
            bool exceptionThrown = false;
            using (MemoryStream ms = new MemoryStream())
            {
                await sut.OnError(ex => { exceptionThrown = true; Assert.True(ex.GetType().Name == "SqlException"); })
                    .Stream("select 1 as a, 1/0 as b for json path", ms);
                Assert.True(exceptionThrown);
            }
        }

        [Fact]
        public async Task InvalidSql()
        {
            bool exceptionThrown = false;
            using (MemoryStream ms = new MemoryStream())
            {
                await sut
                    .OnError(ex=> { exceptionThrown = true;
                                    Assert.True(ex.GetType().Name == "SqlException"); })
                    .Stream("SELECT hrkljush", ms);
                Assert.True(exceptionThrown);
            }
        }

        [Fact]
        public async Task NonExistingTableSql()
        {
            bool exceptionThrown = false;
            using (MemoryStream ms = new MemoryStream())
            {
                await sut
                    .OnError(ex => {
                        Assert.True(ex.GetType().Name == "SqlException");
                        Assert.Equal("Invalid object name 'NonExistentTable'.", ex.Message);
                        exceptionThrown = true;
                    })
                    .Stream("select * from NonExistentTable FOR JSON PATH", ms);
                Assert.True(exceptionThrown);
            }
        }

        [Fact]
        public void ClosedStream()
        {
            bool exceptionThrown = false;
            using (var ms = new MemoryStream())
            {
                ms.Close();
                var result = Assert.ThrowsAsync<ArgumentException>(async () =>
                   {
                       await sut
                           .OnError(ex =>
                           {
                               exceptionThrown = true;
                               Assert.True(false, "Exception should not be catched here.");
                           })
                           .Stream("SELECT 1 as a for json path", ms);
                       Assert.False(exceptionThrown, "Belgrade Sql client should fast fail on this exception.");
                   });
            }
        }
        
        [Theory]
        [InlineData("Server=.\\SQLEXPRESS", "Server=.\\NONEXISTINGSERVER")]
        [InlineData("Database=master;", "Database=INVALIDDTABASE;")]
        [InlineData("Integrated Security=true", "User Id=sa;Password=invalidpassword")]
        public async Task InvalidConnection(string ConnStringToken, string NewValue)
        {
            bool exceptionThrown = false;
            var connString = Util.Settings.ConnectionString.Replace(ConnStringToken, NewValue);
            var sut = new Belgrade.SqlClient.SqlDb.QueryPipe(connString);
            using (MemoryStream ms = new MemoryStream())
            {
                await sut
                    .OnError(ex => {
                        exceptionThrown = true;
                        Assert.True(ex.GetType().Name == "SqlException");
                        Assert.Equal("A network-related or instance-specific error occurred while establishing a connection to SQL Server.The server was not found or was not accessible.Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: SQL Network Interfaces, error: 26 - Error Locating Server/ Instance Specified)", ex.Message);
                    })
                    .Stream("select 1 as a for json path", ms);
                Assert.True(exceptionThrown, "Exception should be thrown when using connection string: " + connString);
            }
        }
    }
}
