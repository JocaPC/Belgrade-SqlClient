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
            sut = new Belgrade.SqlClient.SqlDb.QueryPipe(Util.Settings.MasterConnectionString);
            mapper = new Belgrade.SqlClient.SqlDb.QueryMapper(Util.Settings.MasterConnectionString);
            command = new Belgrade.SqlClient.SqlDb.Command(Util.Settings.MasterConnectionString);
        }


        [Fact]
        public async Task ErrorInSql()
        {
            bool exceptionThrown = false;
            using (MemoryStream ms = new MemoryStream())
            {
                await sut.Sql("select 1 as a, 1/0 as b for json path")
                    .OnError(ex => { exceptionThrown = true; Assert.True(ex.GetType().Name == "SqlException"); })
                    .Stream(ms);
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
                    .Sql("SELECT hrkljush").Stream(ms,
                                new Options() { Prefix = "<start>", Suffix = "<end>" });
                Assert.True(exceptionThrown);
            }
        }

        [Fact]
        public async Task HandlesCompileErrorsRepro()
        {
            await HandlesCompileErrors(query_error: "select * from UnknownTable FOR JSON PATH/Invalid object name 'UnknownTable'.",
                async: false, client: "command", useCommandAsPipe: true, defaultValue: "", prefix: "<s>", suffix: "</s>", executeCallbackOnError: true);
        }

#if EXTENSIVE_TEST
        [Theory, CombinatorialData]
#else
        [Theory, PairwiseData]
#endif
        public async Task HandlesCompileErrors(
        [CombinatorialValues(
            "select * from UnknownTable FOR JSON PATH/Invalid object name 'UnknownTable'.",
            "select UnknownColumn from sys.objects FOR JSON PATH/Invalid column name 'UnknownColumn'.",
            "select g= geometry::STGeomFromText('LINESTRING (100 100, 20 180, 180 180)', 0) from sys.objects FOR JSON PATH/FOR JSON cannot serialize CLR objects. Cast CLR types explicitly into one of the supported types in FOR JSON queries.")]
            string query_error,
        [CombinatorialValues(false, true)] bool async,
        [CombinatorialValues("query", "mapper", "command")] string client,
        [CombinatorialValues(true, false)] bool useCommandAsPipe,
        [CombinatorialValues("?", "N/A", null, "")] string defaultValue,
        [CombinatorialValues("<s>", "{", "<!--", null, "")] string prefix,
        [CombinatorialValues(null, "</s>", "}", "", "-->")] string suffix,
        [CombinatorialValues(true, false)] bool executeCallbackOnError)
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
                            .Sql(query)
                            .Stream(ms,
                                new Options() { Prefix = prefix, DefaultOutput = defaultValue, Suffix = suffix });
                        break;

                    case "mapper":
                        var m = mapper;
                        if(executeCallbackOnError)
                            t = m.Sql(query)
                                .OnError(ex =>
                                {
                                    Assert.True(false, "OnError should not be executed if the callback with exception is provided.");
                                })
                                .Map((r,ex) => {
                                    Assert.Null(r);
                                    Assert.NotNull(ex);
                                    Assert.True(ex.GetType().Name == "SqlException");
                                    Assert.Equal(error, ex.Message);
                                    exceptionThrown = true;
                                });
                        else
                            t = m.Sql(query)
                                .OnError(ex =>
                                {
                                    Assert.True(ex.GetType().Name == "SqlException");
                                    Assert.Equal(error, ex.Message);
                                    exceptionThrown = true;
                                })
                                .Map(r => { throw new Exception("Should not execute callback!"); });
                        break;

                    case "command":
                        if (useCommandAsPipe)
                        {
                            t = command
                                .Sql(query)
                                .OnError(ex =>
                                {
                                    Assert.True(ex.GetType().Name == "SqlException");
                                    Assert.Equal(error, ex.Message);
                                    exceptionThrown = true;
                                })
                                .Stream(ms,
                                    new Options() { Prefix = prefix, DefaultOutput = defaultValue, Suffix = suffix });
                        } else
                        {
                            t = command
                                .Sql("EXEC NON_EXISTING_PROCEDURE")
                                .OnError(ex =>
                            {
                                Assert.True(ex.GetType().Name == "SqlException");
                                Assert.Equal("Could not find stored procedure 'NON_EXISTING_PROCEDURE'.", ex.Message);
                                exceptionThrown = true;
                            }).Exec();
                        }
                        break;
                }
                if (async)
                    await t;
                else
                    t.Wait();

                Assert.True(exceptionThrown, "Exception handler is not invoked!");
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
                           .Sql("SELECT 1 as a for json path").Stream(ms);
                       Assert.False(exceptionThrown, "Belgrade Sql client should fast fail on this exception.");
                   });
            }
        }
        
        [Theory]
        [InlineData("Server=.", "Server=.\\NONEXISTINGSERVER", -1)]
        [InlineData("Database=master;", "Database=INVALIDDATABASE;", 4060)]
        [InlineData("Integrated Security=true", "User Id=sa;Password=invalidpassword", 18456)]
        public async Task InvalidConnection(string ConnStringToken, string NewValue, int ErrorCode)
        {
            bool exceptionThrown = false;
            var connString = Util.Settings.MasterConnectionString.Replace(ConnStringToken, NewValue);
            var sut = new Belgrade.SqlClient.SqlDb.QueryPipe(connString);
            using (MemoryStream ms = new MemoryStream())
            {
                await sut
                    .Sql("select 1 as a for json path")
                    .OnError(ex => {
                        exceptionThrown = true;
                        Assert.True(ex is SqlException);
                        Assert.Equal(ErrorCode, (ex as SqlException).Number);
                    })
                    .Stream(ms);
                Assert.True(exceptionThrown, "Exception should be thrown when using connection string: " + connString);
            }
        }
    }
}
