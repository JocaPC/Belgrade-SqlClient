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
    /// Extension of SqlParameterCollection class with methods for adding array values that will be serialized as JSON.
    /// </summary>
    public static class JsonSqlParametersExtension
    {
        /// <summary>
        /// Adds a parameter to collection with array value.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the array that will be serialized as JSON.</typeparam>
        /// <param name="paramCollection">Parameter collection where new parameter with a value will be added.</param>
        /// <param name="parameterName">The name of new parameter.</param>
        /// <param name="values">Array of values that will be assigned to parameter</param>
        /// <returns>SqlParameterCollection with added parameter.</returns>
        public static SqlParameterCollection AddWithValues<T>(this SqlParameterCollection paramCollection, string parameterName, T[] values)
            where T: struct
        {
            paramCollection.AddWithValue(parameterName, "[" + string.Join(",", values) + "]");
            return paramCollection;
        }

        /// <summary>
        /// Adds a parameter to collection with array value.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the array that will be serialized as JSON.</typeparam>
        /// <param name="paramCollection">Parameter collection where new parameter with a value will be added.</param>
        /// <param name="parameterName">The name of new parameter.</param>
        /// <param name="values">Array of values that will be assigned to parameter</param>
        /// <param name="serializer">Function that serializes array of values as string.</param>
        /// <returns>SqlParameterCollection with added parameter.</returns>
        public static SqlParameterCollection AddWithValues<T>(this SqlParameterCollection paramCollection, 
                                                                string parameterName, T[] values, Func<T, string> serializer)
        {
            paramCollection.AddWithValue(parameterName, "[" + string.Join(",", values.Select(serializer)) + "]");
            return paramCollection;
        }
    }
}
