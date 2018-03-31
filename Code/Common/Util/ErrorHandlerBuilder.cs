//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using Common.Logging;
using System;
using System.Data.Common;

namespace Belgrade.SqlClient.Common
{
    /// <summary>
    /// Class that builds error handlers.
    /// </summary>
    public abstract class ErrorHandlerBuilder
    {
        /// <summary>
        /// Sql command that caused an error.
        /// </summary>
        protected DbCommand Command;

        protected Action<Exception, bool> FallbackHandler = null;

        protected ILog _logger = null;

        /// <summary>
        /// Set the command that caused the error.
        /// </summary>
        /// <param name="command">The command that caused the error.</param>
        /// <returns>Current instance of ErrorHandler object.</returns>
        internal ErrorHandlerBuilder SetCommand(DbCommand command)
        {
            this.Command = command;
            return this;
        }

        internal ErrorHandlerBuilder AddErrorHandlerBuilder(ErrorHandlerBuilder errorHandlerBuilder)
        {
            this.FallbackHandler = errorHandlerBuilder.CreateErrorHandler(this._logger);
            return this;
        }

        /// <summary>
        /// Creates error handler action.
        /// </summary>
        /// <returns>The action that will be executed on error.</returns>
        internal abstract Action<Exception, bool> CreateErrorHandler(ILog logger);

        /// <summary>
        /// Function that will handle exceptions that are not handled by error handler. 
        /// </summary>
        /// <param name="ex">The unhandled exception.</param>
        internal virtual void HandleUnhandledException(Exception ex, bool isResultSentToCallback)
        {
            if (this.FallbackHandler != null)
                this.FallbackHandler(ex, isResultSentToCallback);
            else
            {
                if (_logger != null)
                    _logger.Warn("No fallback error handler for the exception.", ex);
                throw ex;
            }
        }
    }
}
