using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Basic
{
    public class Mapper
    {
        IQuery mapper;
        public Mapper()
        {
            mapper = new QueryMapper(Util.Settings.MasterConnectionString);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ReturnsConstant(bool useAsync)
        {
            // Arrange
            int constant = new Random().Next();
            constant = constant % 10000;
            int result = 0;
            var sql = String.Format("select {0} 'a'", constant);

            // Action
            var t = mapper.Sql(sql).Map(reader => { result = reader.GetInt32(0); });
            if (useAsync)
                await t;
            else
                t.Wait();

            // Assert
            Assert.Equal(constant, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ReturnsExpectedNumberOfRows(bool useAsync)
        {
            // Arrange
            int count = new Random().Next();
            int i = 0;

            // Action
            using (MemoryStream ms = new MemoryStream())
            {
                count = count % 10000;
                var t = mapper.Sql(String.Format("select top {0} 'a' from sys.all_objects, sys.all_parameters", count))
                    .Map(_ => i++);

                if (useAsync)
                    await t;
                else
                    t.Wait();
            }

            // Assert
            Assert.Equal(count, i);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WilNotExecuteCallbackOnNoResults(bool useAsync)
        {
            // Arrange
            bool callbackExecuted = false;

            // Action
            using (MemoryStream ms = new MemoryStream())
            {
                var t = mapper.Sql("select * from sys.all_objects where 1 = 0")
                    .Map(_ => callbackExecuted = true);

                if (useAsync)
                    await t;
                else
                    t.Wait();
            }

            // Assert
            Assert.False(callbackExecuted);
        }

        [Fact]
        public async Task ReturnsEmptyString()
        {
            // Action
            var response = await mapper.GetString("select * from sys.all_objects where 1 = 0");

            // Assert
            Assert.Equal("", response);
        }

        [Fact]
        public async Task ReturnsValueFromBatch()
        {
            // Arrange
            string title = null;

            // Action
            var cmd = new SqlCommand(
@"create table #Todo(i int);
insert into #Todo(i) select @i;
SELECT 'a' as Title ");
            cmd.Parameters.AddWithValue("i", 1);
            await mapper
            .Sql(cmd)
            .Map(row => {
                title = row["Title"].ToString();
            });

            // Assert
            Assert.Equal("a", title);
        }
    }
}