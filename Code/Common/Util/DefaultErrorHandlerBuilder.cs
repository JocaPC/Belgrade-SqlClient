//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using Belgrade.SqlClient.Common;
using Common.Logging;
using System;

namespace Belgrade.SqlClient
{
    /// <summary>
    /// Class that builds a default error handler that will re-throw the error.
    /// </summary>
    public class DefaultErrorHandlerBuilder : ErrorHandlerBuilder
    {
        /// <summary>
        /// Function that creates error handler that will just re-throw exception.
        /// </summary>
        /// <returns>Action that re-throws the exception.</returns>
        public override Action<Exception, bool> CreateErrorHandler(ILog logger)
        {
            return (ex, isResultSentToCallback) => throw ex;
        }
    }
}