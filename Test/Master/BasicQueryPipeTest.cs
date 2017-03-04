using Belgrade.SqlClient;
using BSCT;
using System;
using System.Data.SqlClient;
using System.IO;
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

        [Theory, PairwiseData]
        public async Task ReturnsJson( [CombinatorialValues("stream", "writer")] string client,
                                       [CombinatorialValues(1, 5, 500, 1000)] string top, 
                                       [CombinatorialValues("auto","path")] string mode1,
                                       [CombinatorialValues(",include_null_values", ",root('test')", ",root")] string mode2)
        {
            var sql = "select top " + top + " o.object_id tid, o.name t_name, o.schema_id, o.type t, o.type_desc, o.create_date cd, o.modify_date md, c.* from sys.objects o, sys.columns c for json " + mode1 + mode2;
            string json;
            if (client == "stream")
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await sut.Stream(sql, ms);
                    ms.Position = 0;
                    json = new StreamReader(ms).ReadToEnd();
                }
            } else 
            {
                using (var sw = new StringWriter())
                {
                    await sut.Stream(sql, sw, "[]");
                    json = sw.ToString();
                }
            }
            AssertEx.IsValidJson(json);
        }

        [Theory, PairwiseData]

        public async Task ReturnsXml(  [CombinatorialValues("stream", "writer")] string client, 
                                       [CombinatorialValues(1, 5, 500, 1000)] string top,
                                       [CombinatorialValues("auto", "path", "raw")] string mode,
                                       [CombinatorialValues("", "test")] string rootmode)
        {
            string sql = "select top " + top + " 'CONST_ID' as const, o.object_id tid, o.name t_name, o.schema_id, o.type t, o.type_desc, o.create_date cd, o.modify_date md from sys.objects o for xml " + mode + ", root" + (rootmode!=""?"('"+rootmode+"')":"");
            var xml = new XmlDocument();
            if (client == "stream")
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await sut.Stream(sql, ms);
                    ms.Position = 0;
                    xml.Load(ms);   
                }
            } else
            {
                using (var sw = new StringWriter())
                {
                    await sut.Stream(new SqlCommand(sql), sw, "<root/>");
                    xml.LoadXml(sw.ToString());
                }
            }

//-- auto, root             root / o / @const
//-- auto, root('test')     test / o / @const
//-- raw, root('test')      test / row / @const
//-- raw, root              root / row / @const
//-- path, root             root / row /const
//-- path, root('test')     test / row /const

            string prefix = (rootmode=="")?"root":"test";
            if (mode == "auto")
                prefix += "/o/";
            else
                prefix += "/row/";

            if (mode == "raw" || mode == "auto")
                prefix += "@";

            Assert.True(xml.ChildNodes[0].ChildNodes.Count > 0);
            Assert.Equal("CONST_ID", xml.SelectSingleNode("//" + prefix + "const").InnerText);
            Assert.NotEmpty(xml.SelectSingleNode("//" + prefix + "tid").InnerText);
        }
    }
}
