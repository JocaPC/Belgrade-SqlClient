//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using Belgrade.SqlClient.Common;
using Common.Logging;
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
        private static int RETRY_COUNT = 3;
        

        /// <summary>
        /// Enables or disables retry handler.
        /// </summary>
        /// <param name="retry">Specifies should the retry logic be enabled (<code>true</code> by default).</param>
        public static void Enable(bool retry)
        {
            RETRY_ERRORS = retry;
        }

        /// <summary>
        /// Enables or disables retry handler that performs delayed retry on transient errors such as failover.
        /// Retry handler will repeat command after 5, 10, and 15 seconds if some of the following errors occure:
        /// 4060, 40197, 40501, 40613, 49918, 49919, 49920, 11001
        /// </summary>
        /// <param name="retry">Specifies should the retry logic be enabled (<code>true</code> by default).</param>
        public static void EnableDelayedRetries(bool retry)
        {
            RETRY_TRANSIENT_ERRORS = retry;
        }

        private static bool RETRY_ERRORS = true;

        private static bool RETRY_TRANSIENT_ERRORS = true;

        /// <summary>
        /// Function that creates error handler that will implement retry logic.
        /// </summary>
        /// <returns></returns>
        internal override Action<Exception, bool> CreateErrorHandler(ILog logger)
        {
            base._logger = logger;
            if (!RETRY_ERRORS)
            {
                // if retry is disabled just re-throw the error
                return (ex, flag) => throw ex;
            }

            return async delegate (Exception ex, bool isResultSentToCallback)
            {
                /// IMPORTANT: If at least one result is sent to the client
                /// we cannot retry SQL query
                /// beacuse client might get duplicate rows.
                if (isResultSentToCallback)
                    throw ex;

                if (!RETRY_ERRORS)
                {
                    // if retry is disabled just re-throw the error
                    throw ex;
                }

                bool success = false;
                SqlException sqlex = ex as SqlException;
                if (ex == null)
                    throw new ArgumentException("Exception type must be SqlException.", ex);
                int retryIteration = 0;
                while (!success && retryIteration < RETRY_COUNT)
                {
                    bool isRetryErrorCode = false;
                    foreach(int code in RetryErrorCodes)
                    {
                        if (sqlex.Number == code)
                        {
                            isRetryErrorCode = true;
                            if(logger!=null)
                                logger.Warn("Immediate retry (" + retryIteration + ") due to error.", sqlex);

                            break;
                        }
                    }
                    if (RETRY_TRANSIENT_ERRORS)
                    {
                        foreach (int code in TransientErrorCodes)
                        {
                            if (sqlex.Number == code)
                            {
                                isRetryErrorCode = true;
                                if (logger != null)
                                    logger.Warn("Delayed retry (" + retryIteration + ") due to transient error: ", sqlex);

                                await Task.Delay(5000 + 10000 * retryIteration);
                                break;
                            }
                        }
                    }
                    if (!isRetryErrorCode)
                    {
                        if(logger!=null)
                            logger.Error("Non-transient error occured - re-throwing exception.", sqlex);
                        throw sqlex;
                    }
                    
                    try
                    {
                        await base.Command.ExecuteNonQueryAsync();
                        success = true;
                        if(logger!=null)
                            logger.Info("Retry attempt (" +  retryIteration + ") suceeded");
                        break;
                    }
                    catch (SqlException innerEx)
                    {
                        if(logger!=null)
                            logger.Error("Final retry attempt (" +  retryIteration + ") failed: " + innerEx.Message);
                        sqlex = innerEx;
                    }

                    retryIteration++;
                }

                if (!success && retryIteration == RETRY_COUNT)
                {
                    if(logger!=null)
                        logger.Error("Query failed after " +  RETRY_COUNT + " retries.", sqlex);
                    this.HandleUnhandledException(sqlex, isResultSentToCallback);
                }
            };
        }
    }
}