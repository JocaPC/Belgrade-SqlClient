using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Basic
{
    public class Mapper
    {
        IQueryMapper sut;
        public Mapper()
        {
            sut = new QueryMapper(Util.Settings.ConnectionString);
        }

        [Fact]
        public async Task ReturnConstantSync()
        {
            // Arrange
            int constant = new Random().Next();
            constant = constant % 10000;
            int result = 0;

            // Action
            var sql = String.Format("select {0} 'a'", constant);
            await sut.ExecuteReader(sql, reader => result = reader.GetInt32(0));

            // Assert
            Assert.Equal(constant, result);
        }


        [Fact]
        public async Task ReturnConstantAsync()
        {
            // Arrange
            int constant = new Random().Next();
            constant = constant % 10000;
            int result = 0;

            // Action
            var sql = String.Format("select {0} 'a'", constant);
            await sut.ExecuteReader(sql, (reader) => { result = reader.GetInt32(0); });

            // Assert
            Assert.Equal(constant, result);
        }

        [Fact]
        public async Task ReturnsExpectedNumberOfRows()
        {
            // Arrange
            int count = new Random().Next();
            int i = 0;

            // Action
            using (MemoryStream ms = new MemoryStream())
            {
                count = count % 10000;
                await sut.ExecuteReader(String.Format("select top {0} 'a' from sys.all_objects, sys.all_parameters", count), 
                    _=> i++);
            }

            // Assert
            Assert.Equal(count, i);
        }

        [Fact]
        public async Task WilNotExecuteCallbackOnNoResults()
        {
            // Arrange
            bool callbackExecuted = false;

            // Action
            using (MemoryStream ms = new MemoryStream())
            {
                await sut.ExecuteReader("select * from sys.all_objects where 1 = 0",
                    _ => callbackExecuted = true);
            }

            // Assert
            Assert.Equal(false, callbackExecuted);
        }

        [Fact]
        public async Task ReturnsEmptyResult()
        {
            // Action
            var response = await sut.GetStringAsync("select * from sys.all_objects where 1 = 0");

            // Assert
            Assert.Equal("", response);
        }
       
    }
}