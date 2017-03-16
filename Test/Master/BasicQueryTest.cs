using Belgrade.SqlClient;
using BSCT;
using System;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
            int count = new Random().Next();
            using (MemoryStream ms = new MemoryStream())
            {
                count = count % 10000;
                await sut.Stream(String.Format("select top {0} 'a' from sys.all_objects, sys.all_parameters", count), ms);
                Assert.Equal(count, ms.Length);
            }
        }


        [Fact]
        public async Task ReturnsDefaultTextValue()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await sut.Stream("select * from sys.all_objects where 1 = 0", ms, "DEFAULT");
                ms.Position = 0;
                var text = new StreamReader(ms).ReadToEnd();
                Assert.Equal("DEFAULT", text);
            }
        }

        [Fact]
        public async Task ReturnsDefaultBinaryValue()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await sut.Stream("select * from sys.all_objects where 1 = 0", ms, System.Text.UTF8Encoding.Default.GetBytes("DEFAULT"));
                ms.Position = 0;
                var text = new StreamReader(ms).ReadToEnd();
                Assert.Equal("DEFAULT", text);
            }
        }


        [Fact]
        public async Task ReturnsBinaryValue()
        {
            using (var ms = new MemoryStream())
            {
                await sut.Stream("select cast('TEST' as varbinary)", ms, System.Text.UTF8Encoding.Default.GetBytes("DEFAULT"));
                ms.Position = 0;
                var text = new StreamReader(ms).ReadToEnd();
                Assert.Equal("TEST", text);
            }
        }

        [Fact]
        public async Task ReturnsErrorOnIntValue()
        {
            var isErrorThrown = false;
            using (var ms = new MemoryStream())
            {
                await sut
                    .OnError(ex => {
                        isErrorThrown = true;
                        Assert.Equal("DataReader returned unexpected type Int32", ex.Message);
                        Assert.Equal("reader", (ex as ArgumentException).ParamName);
                    })
                    .Stream("select 1", ms, System.Text.UTF8Encoding.Default.GetBytes("DEFAULT"));
                var text = new StreamReader(ms).ReadToEnd();
                Assert.Empty(text);
                Assert.Equal(true, isErrorThrown);
            }
        }


        [Fact]
        public async Task ReturnsCompressedValue()
        {
            using (var ms = new MemoryStream())
            {
                await sut.Stream("select COMPRESS(N'test')", ms, System.Text.UTF8Encoding.Default.GetBytes("DEFAULT"));
                ms.Position = 0;
                using (var gz = new GZipStream(ms, CompressionMode.Decompress))
                {
                    var text = new StreamReader(gz,System.Text.UnicodeEncoding.Unicode).ReadToEnd();
                    Assert.Equal("test", text);
                }
            }
        }


        [Fact]
        public async Task ReturnsCompressedLargeValue()
        {
            using (var ms = new MemoryStream())
            {
                await sut.Stream("select COMPRESS( (select * from sys.all_columns for json path) )", ms, System.Text.UTF8Encoding.Default.GetBytes("DEFAULT"));
                ms.Position = 0;
                using (var gz = new GZipStream(ms, CompressionMode.Decompress))
                {
                    var text = new StreamReader(gz, System.Text.UnicodeEncoding.Unicode).ReadToEnd();
                    AssertEx.IsValidJson(text);
                }
            }
        }


    }
}
