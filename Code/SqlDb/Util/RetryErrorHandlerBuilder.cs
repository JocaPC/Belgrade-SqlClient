//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using Belgrade.SqlClient.Common;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.SqlDb
{
    /// <summary>
    /// Class that builds an error handler that will handle retry logic.
    /// </summary>
    public class RetryErrorHandlerBuilder : ErrorHandlerBuilder
    {
        /// <summary>
        /// Sql error codes that indicate that retry acton is required.
        /// </summary>
        static readonly int [] RetryErrorCodes =
            new int[5]{
                //Transaction was deadlocked on lock resources with another process and has been chosen as the deadlock victim.
                1205,
                //The current transaction attempted to update a record that has been updated since the transaction started.
                41302,
                //The current transaction failed to commit due to a repeatable read validation failure.
                41305,
                //The current transaction failed to commit due to a serializable validation failure.
                41325,
                //A previous transaction that the current transaction took a dependency on has aborted, and the current transaction can no longer commit
                41301
        };

        /// <summary>
        /// Sql error codes that indicate that retry acton is required.
        /// </summary>
        static readonly int[] TransientErrorCodes =
            new int[8]{
                4060, 40197, 40501, 40613, 49918, 49919, 49920, 11001
        };

        /// <summary>
        /// Default number of retry attempts.
        /// </summary>
        public static int RETRY_COUNT = 3;

        /// <summary>
        /// Function that creates error handler that will implement retry logic.
        /// </summary>
        /// <returns></returns>
        public override Action<Exception> CreateErrorHandler(
#if NETCOREAPP2_0
            Microsoft.Extensions.Logging.ILogger logger
#endif
            )
        {
#if NETCOREAPP2_0
            base._logger = logger;
#endif

            return async delegate (Exception ex)
            {
                bool success = false;
                SqlException sqlex = ex as SqlException;
                if (ex == null)
                    throw new ArgumentException("Exception type must be SqlException.");
                int retryIteration = 0;
                while (!success && retryIteration < RETRY_COUNT)
                {
                    bool isRetryErrorCode = false;
                    foreach(int code in RetryErrorCodes)
                    {
                        if (sqlex.Number == code)
                        {
                            isRetryErrorCode = true;
#if NETCOREAPP2_0
                            if(logger!=null)
                                logger.Log<string>(
                                    Microsoft.Extensions.Logging.LogLevel.Warning,
                                    code, "Retry (" + retryIteration + ") due to error: " + sqlex.Number, ex, (str, exc) => str + exc.Message);
#endif
                            break;
                        }
                    }
                    foreach (int code in TransientErrorCodes)
                    {
                        if (sqlex.Number == code)
                        {
                            isRetryErrorCode = true;
#if NETCOREAPP2_0
                            if(logger!=null)
                                logger.Log<string>(
                                    Microsoft.Extensions.Logging.LogLevel.Warning,
                                    code, "Retry (" + retryIteration + ") due to transient error: " + sqlex.Number, ex, (str, exc) => str + exc.Message);
#endif
                            await Task.Delay(5000 + 10000 * retryIteration);
                            break;
                        }
                    }
                    if (!isRetryErrorCode)
                    {
#if NETCOREAPP2_0
                        if(logger!=null)
                            logger.Log<string>(
                                    Microsoft.Extensions.Logging.LogLevel.Error,
                                    sqlex.Number, "Non-transient error occured: " + sqlex.Number, ex, (str, exc) => str + exc.Message);
#endif
                        throw sqlex;
                    }
                    
                    try
                    {
                        await base.Command.ExecuteNonQueryAsync();
                        success = true;
#if NETCOREAPP2_0
                        if(logger!=null)
                            logger.Log<string>(
                                    Microsoft.Extensions.Logging.LogLevel.Information,
                                    0, "Retry attempt (" +  retryIteration + ") suceeded", null, (str, exc) => str);
#endif
                        break;
                    }
                    catch (SqlException innerEx)
                    {
#if NETCOREAPP2_0
                        if(logger!=null)
                            logger.Log<string>(
                                    Microsoft.Extensions.Logging.LogLevel.Error,
                                    innerEx.Number, "Retry attempt (" +  retryIteration + ") failed: " + innerEx.Number, ex, (str, exc) => str + exc.Message);
#endif
                        sqlex = innerEx;
                    }

                    retryIteration++;
                }

                if (!success && retryIteration == RETRY_COUNT)
                {
#if NETCOREAPP2_0
                        if(logger!=null)
                            logger.Log<string>(
                                    Microsoft.Extensions.Logging.LogLevel.Error,
                                    -1, "Query failed after " +  RETRY_COUNT + " retries.", null, (str, exc) => str);
#endif
                    this.HandleUnhandledException(sqlex);
                }
            };
        }
    }
}