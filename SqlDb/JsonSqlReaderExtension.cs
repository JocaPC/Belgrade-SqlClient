//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Data.SqlClient;
using System.Linq;

namespace Belgrade.SqlClient.SqlDb
{
    /// <summary>
    /// Extension of JsonSqlReaderExtension class with a method for reading array serialized as JSON.
    /// </summary>
    public static class JsonSqlReaderExtension
    {
        /// <summary>
        /// Reads an array of objects from SqlReader.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the array.</typeparam>
        /// <param name="reader">SqlDataReader that executed the query where one of the columns is array serialized as JSON.</param>
        /// <param name="i">Position of the array.</param>
        /// <returns></returns>
        public static T[] GetArray<T>(this SqlDataReader reader, int i)
            where T: struct
        {
            return (reader.GetString(i).Trim("[]".ToCharArray()).Split(',').Select(str=>(T)Convert.ChangeType(str, typeof(T)))).ToArray<T>();
        }
    }
}
