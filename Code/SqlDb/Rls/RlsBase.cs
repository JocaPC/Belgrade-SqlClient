//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Belgrade.SqlClient.SqlDb.Rls
{
    [Obsolete("Use method: .AddRls(key, value) instead of this wrapper.")]
    /// <summary>
    /// Base Rls adapter that contains common code that wraps SQL command with SESSION_CONTEXT variable.
    /// </summary>
    public abstract class RlsBaseAdapter
    {
        /// <summary>
        /// Function that modifies DbCommand object and add SESSION_CONTEXT variable.
        /// </summary>
        protected Func<DbCommand, DbCommand> commandModifier;

        /// <summary>
        /// Name of the sql parameter that will contain key name.
        /// </summary>
        private static readonly string SESSION_KEY_NAME = Guid.NewGuid().ToString().Replace('-', 'k');

        /// <summary>
        /// Name of the sql parameter that will contain key value.
        /// </summary>
        private static readonly string SESSION_VALUE_NAME = Guid.NewGuid().ToString().Replace('-', 'v');

        /// <summary>
        /// Initializes values in RLS adapter.
        /// </summary>
        /// <param name="key">Key of variable in the SESSION_CONTEXT.</param>
        /// <param name="value">Function that evaluates a value that will be placed in SESSION_CONTEXT.</param>
        protected RlsBaseAdapter(string key, Func<string> value)
        {
             this.commandModifier =
                    command =>
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