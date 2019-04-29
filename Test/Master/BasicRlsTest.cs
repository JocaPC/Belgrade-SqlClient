using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb.Rls;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Basic
{
    public class Rls
    {
        public Rls()
        {
            
        }

        [Fact]
        public async Task MapperReturnsSessionContext()
        {
            // Arrange
            string key = Guid.NewGuid().ToString();
            string value = Guid.NewGuid().ToString().Replace('-', '_');
            var sut = new Belgrade.SqlClient.SqlDb.QueryMapper(Util.Settings.MasterConnectionString).AddRls(key,() => value);
            string result = null;

            // Action
            await sut
                    .Sql("select cast(SESSION_CONTEXT(N'"+key+"') as varchar(50))")
                    .OnError(ex=> Assert.True(false, "Exception should not be thrown: " + ex))
                    .Map(reader => {result = reader.GetString(0);});  
            
            // Assert
            Assert.Equal(value, result); 
        }

        [Fact]
        public async Task PipeReturnsSessionContext()
        {
            // Arrange
            string key = Guid.NewGuid().ToString();
            string value = Guid.NewGuid().ToString().Replace('-', '_');
            IQueryPipe sut = new Belgrade.SqlClient.SqlDb.QueryPipe(Util.Settings.MasterConnectionString).AddRls(key,() => value);
            string result = "";

            // Action
            using (MemoryStream ms = new MemoryStream())
            {
                await sut.Sql("select cast(SESSION_CONTEXT(N'" + key + "') as varchar(50)) as ctx for json path")
                    .OnError(ex=> Assert.True(false, "Exception should not be thrown: " + ex))
                    .Stream(ms);
                ms.Position = 0;
                result = new StreamReader(ms).ReadToEnd();
            }

            // Assert
            Assert.Equal("[{\"ctx\":\"" + value + "\"}]", result);
        }

        [Fact]
        public void CommandUsesSessionContext()
        {
            // Arrange
            string key = Guid.NewGuid().ToString();
            string value = Guid.NewGuid().ToString().Replace('-', '_');
            var sut = new Belgrade.SqlClient.SqlDb.Command(Util.Settings.MasterConnectionString).AddRls(key,() => value);
            var sql = string.Format(
@"select cast(SESSION_CONTEXT(N'{0}') as varchar(50)) as sc
into #temp;
select sc from #temp
for json path, include_null_values, without_array_wrapper", key);
            string result = null;
            
            // Action
            sut.Sql(sql).Map( reader => result = reader.GetString(0) ).Wait();

            // Assert
            Assert.Equal("{\"sc\":\"" + value + "\"}", result);

        }


        [Fact]
        public void ProcedureCannotUseRls()
        {
            // Arrange
            string key = Guid.NewGuid().ToString();
            string value = Guid.NewGuid().ToString().Replace('-', '_');
            var sut = new Belgrade.SqlClient.SqlDb.Command(Util.Settings.MasterConnectionString).AddRls(key, () => value);
            bool isExceptionThrown = false;
            // Action
            try
            {
                sut.Proc("sp_help")
                    .Param("objname", "sys.columns")
                    .OnError(ex =>
                    {
                        isExceptionThrown = true;
                        Assert.True(ex is InvalidOperationException);
                    }
                    )
                    .Map(r => { }).Wait();
            } catch(AggregateException aggex)
            {
                isExceptionThrown = true;
                Assert.True(aggex.InnerException is InvalidOperationException);
            }
            catch (Exception ex)
            {
                Assert.True(false, "Unexpected exception " + ex);
            }

            // Assert
            Assert.True(isExceptionThrown);

        }
    }
}