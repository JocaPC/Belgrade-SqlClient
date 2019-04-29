﻿using Belgrade.SqlClient.Common;
using Common.Logging;
using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static partial class ICommandExtensions
    {
        /// <summary>
        /// Set the query text on the command.
        /// </summary>
        /// <returns>Query Pipe.</returns>
        public static ICommand Sql(this ICommand command, DbCommand cmd)
        {
            if (command is BaseStatement)
                (command as BaseStatement).SetCommand(cmd);
            return command;
        }

        /// <summary>
        /// Initializes a stored procedure (no need for explicit EXEC in command text.)
        /// </summary>
        /// <param name="command">Sql command initialized as stored procedure.</param>
        /// <param name="cmd">DbCommand containing the name of stored procedure.</param>
        /// <returns>Command with initialized stored procedure.</returns>
        public static ICommand Proc(this ICommand command, DbCommand cmd)
        {
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            return command.Sql(cmd);
        }

        /// <summary>
        /// Add the parameter to the command.
        /// </summary>
        /// <param name="command">the command where the parameter will be added.</param>
        /// <param name="name">The name of parameter.</param>
        /// <param name="type">The type of parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="size">The size of the parameter.</param>
        /// <returns></returns>
        public static ICommand Param(this ICommand command, string name, System.Data.DbType type, object value, int size = 0)
        {
            if (command is BaseStatement)
            {
                if(value != null)
                    (command as BaseStatement).AddParameter(name, type, value, size);
                else
                    (command as BaseStatement).AddParameter(name, type, DBNull.Value, size);
            }
            return command;
        }
        
        #region "Utilities for default output"
        
        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="output">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Output text that will be placed into stream if there are no results from database.</param>
        /// <returns>Task</returns>
        public static Task Stream(this ICommand command, Stream output, string defaultOutput = "[]")
        {
            return command.Stream(output, new Options() { DefaultOutput = defaultOutput });
        }

        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="output">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Output content that will be placed into stream if there are no results from database.</param>
        /// <returns>Task</returns>
        public static Task Stream(this ICommand command, Stream output, byte[] defaultOutput)
        {
            return command.Stream(output, new Options() { DefaultOutput = defaultOutput });
        }
        
        /// <summary>
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="writer">Text writer where results will be written.</param>
        /// <param name="defaultOutput">Output text that will be placed into stream if there are no results from database.</param>
        /// <returns>Task</returns>
        public static Task Stream(this ICommand command, TextWriter writer, string defaultOutput = "[]")
        {
            return command.Stream(writer, new Options() { DefaultOutput = defaultOutput });
        }

        #endregion

        /// <summary>
        /// Adds a logger that will be used by SQL Command.
        /// </summary>
        /// <param name="logger">Common.Logging.ILog where log records will be written.</param>
        /// <returns>This statement.</returns>
        public static ICommand AddLogger(this ICommand command, ILog logger)
        {
            (command as BaseStatement).AddLogger(logger);
            return command;
        }

    }
}