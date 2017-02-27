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
using BSCT;

namespace Basic
{
    public class Pipe
    {
        IQueryPipe sut;
        public Pipe()
        {
            sut = new QueryPipe(Util.Settings.ConnectionString);
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
        public async Task ReturnsDefaultValue()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await sut.Stream("select * from sys.all_objects where 1 = 0", ms, "DEFAULT");
                ms.Position = 0;
                var text = new StreamReader(ms).ReadToEnd();
                Assert.Equal("DEFAULT", text);
            }
        }
        
        [Theory, PairwiseData]
        public async Task ReturnsJson( [CombinatorialValues(1, 5, 500, 1000)] string top, 
                                       [CombinatorialValues("auto","path")] string mode1,
                                       [CombinatorialValues(",include_null_values", ",root('test')", ",root")] string mode2)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                await sut.Stream("select top " + top + " o.object_id tid, o.name t_name, o.schema_id, o.type t, o.type_desc, o.create_date cd, o.modify_date md, c.* from sys.objects o, sys.columns c for json " + mode1 + mode2, ms);
                ms.Position = 0;
                var json = new StreamReader(ms).ReadToEnd();
                AssertEx.IsValidJson(json);
            }
        }

        [Theory, PairwiseData]

        public async Task ReturnsXml(  [CombinatorialValues(1, 5, 500, 1000)] string top,
                                       [CombinatorialValues("auto", "path", "raw")] string mode,
                                       [CombinatorialValues("", "('test')")] string rootmode)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                string sql = "select top " + top + " * from sys.objects for xml " + mode + ", root" + rootmode;
                await sut.Stream(sql, ms);
                ms.Position = 0;
                var xml = new XmlDocument();
                xml.Load(ms);
                Assert.True(xml.ChildNodes[0].ChildNodes.Count > 0);
            }
        }
    }
}
