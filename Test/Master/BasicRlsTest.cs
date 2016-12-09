using Belgrade.SqlClient.SqlDb;
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
            string key = Guid.NewGuid().ToString();
            string value = Guid.NewGuid().ToString().Replace('-', '_');

            var sut =
                new QueryMapperSessionContextAdapter(
                    new QueryMapper("Server=.;Database=master;Integrated Security=true"),
                    key,
                    () => value);

            await sut.ExecuteReader("select cast(SESSION_CONTEXT(N'"+key+"') as varchar(50))", 
                reader => Assert.Equal(value, reader.GetString(0)));   
        }

        [Fact]
        public async Task PipeReturnsSessionContext()
        {
            string key = Guid.NewGuid().ToString();
            string value = Guid.NewGuid().ToString().Replace('-', '_');

            var sut =
                new QueryPipeSessionContextAdapter(
                    new QueryPipe("Server=.;Database=master;Integrated Security=true"),
                    key,
                    () => value);

            using (MemoryStream ms = new MemoryStream())
            {
                await sut.Stream("select cast(SESSION_CONTEXT(N'" + key + "') as varchar(50)) as ctx for json path", ms);
                ms.Position = 0;
                var text = new StreamReader(ms).ReadToEnd();
                Assert.Equal("[{\"ctx\":\""+ value + "\"}]", text);
            }
        }

        [Fact]
        public void CommandUsesSessionContext()
        {
            string key = Guid.NewGuid().ToString();
            string value = Guid.NewGuid().ToString().Replace('-', '_');

            var sut =
                new CommandSessionContextAdapter(
                    new Command("Server=.;Database=master;Integrated Security=true"),
                    key,
                    () => value);

            string json = null;
            sut.ExecuteReader(
string.Format(@"select cast(SESSION_CONTEXT(N'{0}') as varchar(50)) as sc
into #temp;
select sc from #temp
for json path, include_null_values, without_array_wrapper", key), 
reader => json = reader.GetString(0)
).Wait();

            Assert.Equal("{\"sc\":\"" + value + "\"}", json);

        }
    }
}
