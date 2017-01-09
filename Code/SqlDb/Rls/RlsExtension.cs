using Belgrade.SqlClient;
using Belgrade.SqlClient.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.SqlDb.Rls
{
    public static class RlsExtension
    {
        /// <summary>
        /// Name of the sql parameter that will contain key name.
        /// </summary>
        private static readonly string SESSION_KEY_NAME = Guid.NewGuid().ToString().Replace('-', 'k');

        /// <summary>
        /// Name of the sql parameter that will contain key value.
        /// </summary>
        private static readonly string SESSION_VALUE_NAME = Guid.NewGuid().ToString().Replace('-', 'v');

        public static IQueryPipe AddRls(this IQueryPipe pipe, string key, Func<string> value)
        {
            var stmt = pipe as BaseStatement;
            stmt.SetCommandModifier(CreateRlsCommandModifier(key, value));
            return pipe;
        }

        public static IQueryMapper AddRls(this IQueryMapper mapper, string key, Func<string> value)
        {
            var stmt = mapper as BaseStatement;
            stmt.SetCommandModifier(CreateRlsCommandModifier(key, value));
            return mapper;
        }

        public static ICommand AddRls(this ICommand cmd, string key, Func<string> value)
        {
            var stmt = cmd as BaseStatement;
            stmt.SetCommandModifier(CreateRlsCommandModifier(key, value));
            return cmd;
        }

        private static Func<DbCommand, DbCommand> CreateRlsCommandModifier(string key, Func<string> value)
        {
            return command =>
            {
                if (command.CommandType != System.Data.CommandType.Text)
                    throw new InvalidOperationException("Row-level security adapters can work only with CommandType Text.");
                var SessionKey = new SqlParameter(SESSION_KEY_NAME, System.Data.SqlDbType.NVarChar, 128);
                SessionKey.Value = key;
                var SessionValue = new SqlParameter(SESSION_VALUE_NAME, System.Data.SqlDbType.Variant);
                SessionValue.Value = value();

                command.Parameters.Add(SessionKey);
                command.Parameters.Add(SessionValue);
                command.CommandText = string.Format(
                    "EXEC sp_set_session_context @{0}, @{1};"
                    + command.CommandText
                    + ";EXEC sp_set_session_context @{0}, NULL;",
                    SESSION_KEY_NAME, SESSION_VALUE_NAME);

                return command;
            };
        }
    }
}
