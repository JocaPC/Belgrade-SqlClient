using Belgrade.SqlClient.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Code.SqlDb.Extensions
{
    static class Util
    {
        internal static void AddParameterWithValue(object command, string name, object value)
        {
            if (value == null)
                value = DBNull.Value;
            if (command is BaseStatement &&
                            (command as BaseStatement).Command is SqlCommand)
            {
                var p = ((command as BaseStatement).Command as SqlCommand).Parameters.AddWithValue(name, value);
                if (p.SqlDbType == System.Data.SqlDbType.NVarChar
                    || p.SqlDbType == System.Data.SqlDbType.VarChar)
                {
                    p.Size = 100 * (value.ToString().Length / 100 + 1);
                }
            }
        }

    }
}
