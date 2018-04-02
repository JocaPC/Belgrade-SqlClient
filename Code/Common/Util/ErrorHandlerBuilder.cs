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
        protected Action<Exception> FallbackHandler = null;

        protected ILog _logger = null;
        
        internal ErrorHandlerBuilder AddErrorHandlerBuilder(ErrorHandlerBuilder errorHandlerBuilder)
        {
            this.FallbackHandler = errorHandlerBuilder.CreateErrorHandler(this._logger);
            return this;
        }

        /// <summary>
        /// Creates error handler action.
        /// </summary>
        /// <returns>The action that will be executed on error.</returns>
        internal abstract Action<Exception> CreateErrorHandler(ILog logger);

        /// <summary>
        /// Function that will handle exceptions that are not handled by error handler. 
        /// </summary>
        /// <param name="ex">The unhandled exception.</param>
        internal virtual void HandleUnhandledException(Exception ex)
        {
            if (this.FallbackHandler != null)
                this.FallbackHandler(ex);
            else
            {
                if (_logger != null)
                    _logger.Warn("No fallback error handler for the exception.", ex);
                throw ex;
            }
        }
    }
}
