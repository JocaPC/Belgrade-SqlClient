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


        [Theory, PairwiseData]
        public async Task ReturnsDefaultValue(
            [CombinatorialValues(0, 1, 100, 3500, 10000)] int length,
            [CombinatorialValues(false, true)] bool STRING,
            [CombinatorialValues("writer", "stream")] string client,
            [CombinatorialValues(true, false)] bool useCommand)
        {
            var sql = "select * from sys.all_objects where 1 = 0";
            string defaultValue = "", text = "INITIAL_VALUE";
            if (client == "stream")
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (length == 0)
                    {
                        if (useCommand)
                            await sut.Stream(new SqlCommand(sql), ms);
                        else
                            await sut.Stream(sql, ms);
                    }
                    else if (STRING)
                    {
                        defaultValue = GenerateChar(length);
                        if (useCommand)
                            await sut.Stream(new SqlCommand(sql), ms, defaultValue);
                        else
                            await sut.Stream(sql, ms, defaultValue);
                    }
                    else
                    {
                        defaultValue = GenerateChar(length);
                        if (useCommand)
                            await sut.Stream(new SqlCommand(sql), ms, Encoding.UTF8.GetBytes(defaultValue));
                        else
                            await sut.Stream(sql, ms, Encoding.UTF8.GetBytes(defaultValue));
                    }

                    ms.Position = 0;
                    text = new StreamReader(ms).ReadToEnd();

                }
            }
            else
            {
                using (var ms = new StringWriter())
                {
                    if (length == 0)
                    {
                        if (useCommand)
                            await sut.Stream(new SqlCommand(sql), ms, "");
                        else
                            await sut.Stream(sql, ms, "");
                    }
                    else if (STRING)
                    {
                        defaultValue = GenerateChar(length);
                        if (useCommand)
                            await sut.Stream(new SqlCommand(sql), ms, defaultValue);
                        else
                            await sut.Stream(sql, ms, defaultValue);
                    }
                    else
                    {
                        // cannot send binary default value to TextWriter.
                    }

                    text = ms.ToString();
                }
            }
            Assert.Equal(defaultValue, text);
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


           public string GenerateChar()
            {
                Random random = new Random();

                return Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65))).ToString();
            }

            public string GenerateChar(int count)
            {        
                string randomString = "";

                for (int i = 0; i < count; i++)
                {
                        randomString += GenerateChar();
                }

                return randomString;
            }
    }
}
