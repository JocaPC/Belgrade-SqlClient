using System;
using System.Data.SqlClient;
using System.Linq;

namespace Belgrade.SqlClient.JSON
{
    /// <summary>
    /// Extension of JsonSqlReaderExtension class with a method for reading array serialized as JSON.
    /// </summary>
    public static class JsonSqlReaderExtension
    {
        public static T[] GetArray<T>(this SqlDataReader reader, int i)
            where T: struct
        {
            return (reader.GetString(i).Trim("[]".ToCharArray()).Split(',').Select(str=>(T)Convert.ChangeType(str, typeof(T)))).ToArray<T>();
        }
    }
}
