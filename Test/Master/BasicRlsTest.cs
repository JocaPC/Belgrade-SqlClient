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
            var sut = new Belgrade.SqlClient.SqlDb.QueryMapper(Util.Settings.ConnectionString).AddRls(key,() => value);

            // Action
            await sut.ExecuteReader("select cast(SESSION_CONTEXT(N'"+key+"') as varchar(50))", 
                reader => 
                // Assert
                Assert.Equal(value, reader.GetString(0)));   
        }

        [Fact]
        public async Task PipeReturnsSessionContext()
        {
            // Arrange
            string key = Guid.NewGuid().ToString();
            string value = Guid.NewGuid().ToString().Replace('-', '_');
            IQueryPipe sut = new Belgrade.SqlClient.SqlDb.QueryPipe(Util.Settings.ConnectionString).AddRls(key,() => value);
            string result = "";

            // Action
            using (MemoryStream ms = new MemoryStream())
            {
                await sut.Stream("select cast(SESSION_CONTEXT(N'" + key + "') as varchar(50)) as ctx for json path", ms);
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
            var sut = new Belgrade.SqlClient.SqlDb.Command(Util.Settings.ConnectionString).AddRls(key,() => value);
            var sql = string.Format(
@"select cast(SESSION_CONTEXT(N'{0}') as varchar(50)) as sc
into #temp;
select sc from #temp
for json path, include_null_values, without_array_wrapper", key);
            string result = null;
            
            // Action
            sut.Map( sql, reader => result = reader.GetString(0) ).Wait();

            // Assert
            Assert.Equal("{\"sc\":\"" + value + "\"}", result);

        }
    }
}