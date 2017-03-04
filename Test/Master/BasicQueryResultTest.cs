using Belgrade.SqlClient;
using BSCT;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace Basic
{
    public class Query
    {
        IQueryPipe pipe;
        IQueryMapper mapper;
        ICommand command;
        public Query()
        {
            pipe = new Belgrade.SqlClient.SqlDb.QueryPipe(Util.Settings.ConnectionString);
            mapper = new Belgrade.SqlClient.SqlDb.QueryMapper(Util.Settings.ConnectionString);
            command = new Belgrade.SqlClient.SqlDb.Command(Util.Settings.ConnectionString);
        }
        
        [Theory, PairwiseData]
        public async Task ReturnsJson( [CombinatorialValues("stream", "writer", "mapper", "command")] string client,
                                       [CombinatorialValues(1, 5, 500, 1000)] string top, 
                                       [CombinatorialValues("auto","path")] string mode1,
                                       [CombinatorialValues(",include_null_values", ",root('test')", ",root")] string mode2,
                                       [CombinatorialValues(true, false)] bool useCommand)
        {
            var sql = "select top " + top + " o.object_id tid, o.name t_name, o.schema_id, o.type t, o.type_desc, o.create_date cd, o.modify_date md, c.* from sys.objects o, sys.columns c for json " + mode1 + mode2;
            string json;
            if (client == "stream")
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (useCommand)
                        await pipe.Stream(new SqlCommand(sql), ms);
                    else
                        await pipe.Stream(sql, ms);
                    ms.Position = 0;
                    json = new StreamReader(ms).ReadToEnd();
                }
            } else if (client == "writer")
            {
                using (var sw = new StringWriter())
                {
                    if (useCommand)
                        await pipe.Stream(new SqlCommand(sql), sw, "[]");
                    else
                        await pipe.Stream(sql, sw, "[]");
                    json = sw.ToString();
                }
            } else if (client == "command")
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (useCommand)
                        await command.Stream(new SqlCommand(sql), ms, "");
                    else
                        await command.Stream(sql, ms, "");
                    ms.Position = 0;
                    json = new StreamReader(ms).ReadToEnd();
                }
            } else
            {
                if(useCommand)
                    json = await mapper.GetStringAsync(new SqlCommand(sql));
                else
                    json = await mapper.GetStringAsync(sql);
                AssertEx.IsValidJson(json);
            }
            AssertEx.IsValidJson(json);
        }

        [Theory, PairwiseData]

        public async Task ReturnsXml(  [CombinatorialValues("stream", "writer","mapper", "command")] string client, 
                                       [CombinatorialValues(1, 5, 500, 1000)] string top,
                                       [CombinatorialValues("auto", "path", "raw")] string mode,
                                       [CombinatorialValues("", "test")] string rootmode,
                                       [CombinatorialValues(true, false)] bool useCommand)
        {
            string sql = "select top " + top + " 'CONST_ID' as const, o.object_id tid, o.name t_name, o.schema_id, o.type t, o.type_desc, o.create_date cd, o.modify_date md from sys.objects o for xml " + mode + ", root" + (rootmode!=""?"('"+rootmode+"')":"");
            var xml = new XmlDocument();
            if (client == "stream")
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (useCommand)
                        await pipe.Stream(new SqlCommand(sql), ms);
                    else
                        await pipe.Stream(sql, ms);
                    ms.Position = 0;
                    xml.Load(ms);   
                }
            } else if (client == "writer")
            {
                using (var sw = new StringWriter())
                {
                    if(useCommand)
                        await pipe.Stream(new SqlCommand(sql), sw, "<root/>");
                    else
                        await pipe.Stream(sql, sw, "<root/>");
                    xml.LoadXml(sw.ToString());
                }
            } else if (client == "command")
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (useCommand)
                        await command.Stream(new SqlCommand(sql), ms, "<root/>");
                    else
                        await command.Stream(sql, ms, "<root/>");
                    ms.Position = 0;
                    xml.LoadXml( new StreamReader(ms).ReadToEnd() );
                }
            } else
            {
                if (useCommand)
                    xml.LoadXml(await mapper.GetStringAsync(new SqlCommand(sql)));
                else
                    xml.LoadXml(await mapper.GetStringAsync(sql));
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
