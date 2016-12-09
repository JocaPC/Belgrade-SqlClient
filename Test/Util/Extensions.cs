using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BSCT
{
    public static class AssertEx
    {
        public static void IsValidJson(string json)
        {
            Assert.NotNull(json);
            Assert.NotEmpty(json);
            Assert.True((json.StartsWith("{") && json.EndsWith("}")) || //For object
                (json.StartsWith("[") && json.EndsWith("]")));

            try
            {
                var obj = JToken.Parse(json);
            }
            catch (JsonReaderException jex)
            {
                Assert.True(false, "Invalid JSON:" + jex.Message);
            }
            catch (Exception ex) //some other exception
            {
                Assert.True(false, "Error during check JSON:" + ex.Message);
            }
        }

        public static void IsValidXml(string xml)
        {
            Assert.NotNull(xml);
            Assert.NotEmpty(xml);
            
            Assert.True((xml.StartsWith("{") && xml.EndsWith("}")) || //For object
                (xml.StartsWith("[") && xml.EndsWith("]")));

            try
            {
                var obj = JToken.Parse(xml);
            }
            catch (JsonReaderException jex)
            {
                Assert.True(false, "Invalid JSON:" + jex.Message);
            }
            catch (Exception ex) //some other exception
            {
                Assert.True(false, "Error during check JSON:" + ex.Message);
            }
        }

    }
}
