using Belgrade.SqlClient;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Errors
{
    public class Pipe
    {
        IQueryPipe sut;
        IQueryMapper mapper;
        ICommand command;
        public Pipe()
        {
            sut = new Belgrade.SqlClient.SqlDb.QueryPipe(Util.Settings.ConnectionString);
            mapper = new Belgrade.SqlClient.SqlDb.QueryMapper(Util.Settings.ConnectionString);
            command = new Belgrade.SqlClient.SqlDb.Command(Util.Settings.ConnectionString);
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
                    .Stream("SELECT hrkljush", ms,
                                new Options() { Prefix = "<start>", Suffix = "<end>" });
                Assert.True(exceptionThrown);
            }
        }
        
        //[Theory, CombinatorialData]
        [Theory, PairwiseData]
        public async Task HandlesCompileErrors(
        [CombinatorialValues(
            "select * from NonExistentTable FOR JSON PATH/Invalid object name 'NonExistentTable'.",
            "select UnknownColumn from sys.objects FOR JSON PATH/Invalid column name 'UnknownColumn'.",
            "select g= geometry::STGeomFromText('LINESTRING (100 100, 20 180, 180 180)', 0) from sys.objects FOR JSON PATH/FOR JSON cannot serialize CLR objects. Cast CLR types explicitly into one of the supported types in FOR JSON queries.")]
            string query_error,
        [CombinatorialValues(false, true)] bool async,
        [CombinatorialValues("query", "mapper", "command")] string client,
        [CombinatorialValues(true, false)] bool useCommandAsPipe,
        [CombinatorialValues("?", "N/A", null, "")] string defaultValue,
        [CombinatorialValues("<s>", "{", "<!--", null, "")] string prefix,
        [CombinatorialValues(null, "</s>", "}", "", "-->")] string suffix)
        {
            var pair = query_error.Split('/');
            var query = pair[0];
            var error = pair[1];
            bool exceptionThrown = false;

            using (MemoryStream ms = new MemoryStream())
            {
                Task t = null;
                switch(client)
                {
                    case "query":
                        t = sut
                            .OnError(ex =>
                            {
                                Assert.True(ex.GetType().Name == "SqlException");
                                Assert.Equal(error, ex.Message);
                                exceptionThrown = true;
                            })
                            .Stream(query, ms,
                                new Options() { Prefix = prefix, DefaultOutput = defaultValue, Suffix = suffix });
                        break;

                    case "mapper":
                        t = mapper
                            .OnError(ex =>
                            {
                                Assert.True(ex.GetType().Name == "SqlException");
                                Assert.Equal(error, ex.Message);
                                exceptionThrown = true;
                            })
                            .ExecuteReader(query, r => { throw new Exception("Should not execute callback!"); });
                        break;

                    case "command":
                        if (useCommandAsPipe)
                        {
                            t = command
                                .OnError(ex =>
                                {
                                    Assert.True(ex.GetType().Name == "SqlException");
                                    Assert.Equal(error, ex.Message);
                                    exceptionThrown = true;
                                })
                                .Stream(query, ms,
                                    new Options() { Prefix = prefix, DefaultOutput = defaultValue, Suffix = suffix });
                        } else
                        {
                            t = command.OnError(ex =>
                            {
                                Assert.True(ex.GetType().Name == "SqlException");
                                Assert.Equal("Could not find stored procedure 'NON_EXISTING_PROCEDURE'.", ex.Message);
                                exceptionThrown = true;
                            }).ExecuteNonQuery("EXEC NON_EXISTING_PROCEDURE");
                        }
                        break;
                }
                if (async)
                    await t;
                else
                    t.Wait();

                Assert.True(exceptionThrown);
                if (client == "query" || client == "command" && useCommandAsPipe)
                {
                    ms.Position = 0;
                    var text = new StreamReader(ms).ReadToEnd();
                    Assert.Equal("", text);
                }
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
