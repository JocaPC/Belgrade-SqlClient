using Code.SqlDb.Extensions;
using System.Data.SqlClient;

namespace Belgrade.SqlClient
{
    public static partial class ICommandExtensions
    {
        /// <summary>
        /// Add a paramater to the command with a value and inferred type.
        /// </summary>
        /// <param name="command">The command object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The command object with added prameter.</returns>
        public static ICommand Param(this ICommand command, string name, object value)
        {
            Util.AddParameterWithValue(command, name, value);
            return command;
        }

        #region "Text command extensions"

        /// <summary>
        /// Set the query text on the command.
        /// </summary>
        /// <returns>Command.</returns>
        public static ICommand Sql(this ICommand command, string query)
        {
            var cmd = new SqlCommand(query);
            return command.Sql(cmd);
        }

        /// <summary>
        /// Initializes a stored procedure (no need for explicit EXEC in the command text.)
        /// </summary>
        /// <param name="command">Sql command initialized as stored procedure.</param>
        /// <param name="procedure">Name of the stored procedure.</param>
        /// <returns>Command with initialized stored procedure.</returns>
        public static ICommand Proc(this ICommand command, string procedure)
        {
            var cmd = new SqlCommand(procedure);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            return command.Sql(cmd);
        }

        #endregion
    }
}