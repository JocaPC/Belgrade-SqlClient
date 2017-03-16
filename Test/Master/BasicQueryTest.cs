using Belgrade.SqlClient;
using BSCT;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Basic
{
    public class Pipe
    {
        IQueryPipe sut;
        public Pipe()
        {
            sut = new Belgrade.SqlClient.SqlDb.QueryPipe(Util.Settings.ConnectionString);
        }

        [Fact]
        public async Task ConcatenteCells()
        {
            // Arrange
            int count = new Random().Next();

            // Action
            using (MemoryStream ms = new MemoryStream())
            {
                count = count % 10000;
                await sut.Stream(String.Format("select top {0} 'a' from sys.all_objects, sys.all_parameters", count), ms);

                // Assert
                Assert.Equal(count, ms.Length);
            }
        }

        [Fact]
        public async Task ReturnsDefaultTextValue()
        {
            // Arrange
            string text = "";

            // Action
            using (MemoryStream ms = new MemoryStream())
            {
                await sut.Stream("select * from sys.all_objects where 1 = 0", ms, "DEFAULT");
                ms.Position = 0;
                text = new StreamReader(ms).ReadToEnd();
            }

            // Assert
            Assert.Equal("DEFAULT", text);
        }

        [Fact]
        public async Task ReturnsDefaultBinaryValue()
        {
            // Action
            using (MemoryStream ms = new MemoryStream())
            {
                await sut.Stream("select * from sys.all_objects where 1 = 0", ms, UTF8Encoding.Default.GetBytes("DEFAULT"));
                ms.Position = 0;
                var text = new StreamReader(ms).ReadToEnd();

                // Assert
                Assert.Equal("DEFAULT", text);
            }
        }


        [Fact]
        public async Task ReturnsBinaryValue()
        {
            // Action
            using (var ms = new MemoryStream())
            {
                await sut.Stream("select cast('TEST' as varbinary)", ms, System.Text.UTF8Encoding.Default.GetBytes("DEFAULT"));
                ms.Position = 0;
                var text = new StreamReader(ms).ReadToEnd();

                // Assert
                Assert.Equal("TEST", text);
            }
        }

        [Fact]
        public async Task ReturnsErrorOnIntValue()
        {
            // Arrange
            var isErrorThrown = false;
            var text = "WRONG VALUE";

            // Action
            using (var ms = new MemoryStream())
            {
                await sut
                    .OnError(ex => {
                        isErrorThrown = true;

                        // Assert (refactor)
                        Assert.Equal("DataReader returned unexpected type Int32", ex.Message);
                        Assert.Equal("reader", (ex as ArgumentException).ParamName);
                    })
                    .Stream("select 1", ms, System.Text.UTF8Encoding.Default.GetBytes("DEFAULT"));
                text = new StreamReader(ms).ReadToEnd();
            }

            // Assert
            Assert.Empty(text);
            Assert.Equal(true, isErrorThrown);
        }


        [Theory]
        [InlineData("N''", true,false)]
        [InlineData("N'test'",true,false)]
        [InlineData("(select * from sys.all_columns for json path)",false,true)]
        public async Task ReturnsCompressedValue(string data, bool isStatic, bool isJson)
        {
            // Arrange
            var sql = string.Format("select COMPRESS({0})", data);
            var text = "WRONG VALUE";

            // Action
            using (var ms = new MemoryStream())
            {
                await sut.Stream(sql, ms, System.Text.UTF8Encoding.Default.GetBytes("DEFAULT"));
                ms.Position = 0;
                using (var gz = new GZipStream(ms, CompressionMode.Decompress))
                {
                    text = new StreamReader(gz,System.Text.UnicodeEncoding.Unicode).ReadToEnd(); 
                }
            }

            // Assert
            if (isStatic)
            {
                Assert.Equal(data.Substring(2, data.Length - 3), text);
            }
            if (isJson)
            {
                AssertEx.IsValidJson(text);
            }
        }
    }
}
