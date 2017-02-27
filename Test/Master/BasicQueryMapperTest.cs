using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Xml;
using Util;

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
        public async Task ReturnsExpectedNumberOfRows()
        {
            int count = new Random().Next();
            using (MemoryStream ms = new MemoryStream())
            {
                count = count % 10000;
                int i = 0;
                await sut.ExecuteReader(String.Format("select top {0} 'a' from sys.all_objects, sys.all_parameters", count), 
                    _=> i++);
                Assert.Equal(count, i);
            }
        }

        [Fact]
        public async Task WilNotExecuteCallbackOnNoResults()
        {
            int count = new Random().Next();
            using (MemoryStream ms = new MemoryStream())
            {
                count = count % 10000;
                int i = 0;
                await sut.ExecuteReader("select * from sys.all_objects where 1 = 0",
                    _ => i++);
                Assert.Equal(0, i);
            }
        }
    }
}