//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using System;
using System.Data.SqlClient;

namespace Belgrade.SqlClient.SqlDb
{
    /// <summary>
    /// Class that builds an error handler that will handle retry logic.
    /// </summary>
    public class RetryErrorHandler
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
        /// See - https://docs.microsoft.com/azure/sql-database/sql-database-develop-error-messages#transient-fault-error-codes
        /// </summary>
        static readonly int[] TransientErrorCodes =
            new int[9]{
                4060, 40197, 40501, 40613, 49918, 49919, 49920, 4221, 11001
        };

        /// <summary>
        /// Default number of retry attempts.
        /// </summary>
        internal static int RETRY_COUNT = 3;
        
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
        /// - 4060, 40197, 40501, 40613, 49918, 49919, 49920, 4221 - https://docs.microsoft.com/azure/sql-database/sql-database-develop-error-messages#transient-fault-error-codes
        /// - 11001 - An error has occurred while establishing a connection to the server. When connecting to SQL Server, this failure may be caused by the fact that under the default settings SQL Server does not allow remote connections. (provider: TCP Provider, error: 0 - No such host is known.)
        /// </summary>
        /// <param name="retry">Specifies should the retry logic be enabled (<code>true</code> by default).</param>
        public static void EnableDelayedRetries(bool retry)
        {
            RETRY_TRANSIENT_ERRORS = retry;
        }

        internal static bool RETRY_ERRORS = true;

        internal static bool RETRY_TRANSIENT_ERRORS = true;
        
        internal static bool ShouldWaitToRetry(Exception ex)
        {
            var sqlex = (ex as SqlException);
            if (sqlex == null)
                return false;
            if (RETRY_TRANSIENT_ERRORS)
            {
                foreach (int code in TransientErrorCodes)
                {
                    if (sqlex.Number == code)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool ShouldRetry(Exception ex)
        {
            var sqlex = (ex as SqlException);
            if (sqlex == null)
                return false;
            foreach (int code in RetryErrorCodes)
            {
                if (sqlex.Number == code)
                {
                    return true;
                }
            }
            if (RETRY_TRANSIENT_ERRORS)
            {
                foreach (int code in TransientErrorCodes)
                {
                    if (sqlex.Number == code)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}