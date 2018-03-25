using Belgrade.SqlClient.Common;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Belgrade.SqlClient.SqlDb.Rls
{
    public static class RlsExtension
    {
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

        public static IQueryMapper AddContextVariable(this IQueryMapper mapper, string key, Func<string> value, bool isReadOnly = true)
        {
            var stmt = mapper as BaseStatement;
            stmt.SetCommandModifier(CreateSessionContextCommandModifier(key, value, isReadOnly));
            return mapper;
        }

        public static ICommand AddContextVariable(this ICommand cmd, string key, Func<string> value, bool isReadOnly = true)
        {
            var stmt = cmd as BaseStatement;
            stmt.SetCommandModifier(CreateSessionContextCommandModifier(key, value, isReadOnly));
            return cmd;
        }

        private static Func<DbCommand, DbCommand> CreateSessionContextCommandModifier(string key, Func<string> value, bool isReadOnly = true)
        {
            return command =>
            {
                AddContextVariables(key, value, command, isReadOnly);
                return command;
            };
        }

        private static Func<DbCommand, DbCommand> CreateRlsCommandModifier(string key, Func<string> value)
        {
            return command =>
            {
                if (command.Parameters.Contains(key))
                {
                    command.Parameters[key].Value = value;
                }
                else
                {
                    AddContextVariables(key, value, command, isReadOnly: true);
                }

                return command;
            };
        }

        internal static DbCommand AddContextVariables(string key, Func<string> value, DbCommand command, bool isReadOnly = true)
        {
            var guid = Guid.NewGuid();
            /// Name of the sql parameter that will contain key name.
            string SESSION_KEY_NAME = guid.ToString().Replace('-', 'k');

            /// <summary>
            /// Name of the sql parameter that will contain key value.
            /// </summary>
            string SESSION_VALUE_NAME = guid.ToString().Replace('-', 'v');

            var SessionKey = new SqlParameter(SESSION_KEY_NAME, System.Data.SqlDbType.NVarChar, 128);
            SessionKey.Value = key;
            var SessionValue = new SqlParameter(SESSION_VALUE_NAME, System.Data.SqlDbType.Variant);
            SessionValue.Value = value();
            if(SessionValue.Value == null)
            {
                SessionValue.Value = System.DBNull.Value;
            }

            command.Parameters.Add(SessionKey);
            command.Parameters.Add(SessionValue);
            command.CommandText = string.Format(
                "EXEC sp_set_session_context @{0}, @{1}, @read_only = {2};"
                + command.CommandText
                + (isReadOnly ? "":";EXEC sp_set_session_context @{0}, NULL;"),
                SESSION_KEY_NAME, SESSION_VALUE_NAME, isReadOnly? '1':'0');

            return command;
        }
    }
}
