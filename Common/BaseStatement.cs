//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Data.Common;

namespace Belgrade.SqlClient.Common
{
    public class BaseStatement
    {
        /// <summary>
        /// Connection to Sql Database.
        /// </summary>
        protected DbConnection Connection;

        protected Func<DbCommand, DbCommand> CommandModifier = c => c;

        protected ErrorHandlerBuilder ErrorHandlerBuilder;

        protected static readonly ErrorHandlerBuilder DefaultErrorHandlerBuilder = new DefaultErrorHandlerBuilder();

        /// <summary>
        /// Set the object that will modify command.
        /// </summary>
        /// <param name="value">Function that will modify command.</param>
        internal BaseStatement SetCommandModifier(Func<DbCommand, DbCommand> value)
        {
            this.CommandModifier = value;
            return this;
        }

        /// <summary>
        /// Sets an object that will create ErrorHandler.
        /// </summary>
        /// <param name="builder">The object that will build an error handler.</param>
        /// <returns>The current instance of command.</returns>
        public BaseStatement AddErrorHandlerBuilder(ErrorHandlerBuilder builder)
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
    }
}
