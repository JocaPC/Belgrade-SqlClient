//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Belgrade.SqlClient.Common
{
    public abstract class BaseStatement
    {
        /// <summary>
        /// Connection to Sql Database.
        /// </summary>
        protected DbConnection Connection;

        /// <summary>
        /// Sql Command that will be executed.
        /// </summary>
        private DbCommand command;

        protected List<Func<DbCommand, DbCommand>> CommandModifierList = new List<Func<DbCommand, DbCommand>>();

        protected DbCommand CommandModifier(DbCommand command) {
            for(int i = 0; i< CommandModifierList.Count; i++)
            {
                var commandModifier = CommandModifierList[i];
                command = commandModifier(command);
            }
            return command;
        }

        protected ErrorHandlerBuilder ErrorHandlerBuilder;

        protected static readonly ErrorHandlerBuilder DefaultErrorHandlerBuilder = new DefaultErrorHandlerBuilder();

        public DbCommand Command { get => command; internal set => command = value; }

        /// <summary>
        /// Set the object that will modify command.
        /// </summary>
        /// <param name="value">Function that will modify command.</param>
        internal virtual BaseStatement SetCommandModifier(Func<DbCommand, DbCommand> value)
        {
            this.CommandModifierList.Add(value);
            return this;
        }

        /// <summary>
        /// Sets an object that will create ErrorHandler.
        /// </summary>
        /// <param name="builder">The object that will build an error handler.</param>
        /// <returns>The current instance of command.</returns>
        internal virtual BaseStatement AddErrorHandler(ErrorHandlerBuilder builder)
        {
            if (this.ErrorHandlerBuilder == null)
                this.ErrorHandlerBuilder = builder;
            else
                this.ErrorHandlerBuilder.AddErrorHandlerBuilder(builder);

            return this;
        }

        internal ErrorHandlerBuilder GetErrorHandlerBuilder()
        {
            return this.ErrorHandlerBuilder?? DefaultErrorHandlerBuilder;
        }

        internal BaseStatement SetCommand(DbCommand command)
        {
            this.Command = command;
            return this;
        }

        internal BaseStatement AddParameter(string name, DbType type, object value, int size = 0)
        {
            var p = this.Command.CreateParameter();
            p.ParameterName = name;
            p.DbType = type;
            p.Value = value;
            p.Size = size;
            if(size == 0)
            {
                if(type == DbType.AnsiString || type == DbType.String)
                {
                    p.Size = 100 * (value.ToString().Length / 100 + 1);
                }
            } else {
                p.Size = size;
            }
            this.Command.Parameters.Add(p);
            return this;
        }
    }
}
