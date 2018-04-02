//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license .
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.See the license files for details.
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

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

        protected ILog _logger = null;

        /// <summary>
        /// Adds a logger that will be used by SQL client.
        /// </summary>
        /// <param name="logger">Common.Logging.ILog where log records will be written.</param>
        /// <returns>This statement.</returns>
        public BaseStatement AddLogger(ILog logger)
        {
            this._logger = logger;
            return this;
        }

        protected async Task ExecuteWithRetry(DbCommand command, object callback)
        {
            bool isResultSentToCallback = false;
            int retryIteration = 0;
            bool shouldRetry = false;
            Exception rootException = null;
            do
            {
                shouldRetry = false; // Let's assume that we should not retry execution in this iteration.
                rootException = null;
                try
                {
                    isResultSentToCallback = await ExecuteCommand(command, callback);
                }
                catch (Exception ex)
                {
                    rootException = ex;
                    // If this is transient error AND results are not already sent to the client:
                    // Retry the action.
                    if (SqlDb.RetryErrorHandler.ShouldRetry(ex) && !isResultSentToCallback)
                    {
                        shouldRetry = true;
                        retryIteration++;
                    }
                    else
                    {
                        await ExecuteCallbackWithException(callback, ex);
                    }
                }
                finally
                {
                    command.Connection.Close();
                    if (shouldRetry && !isResultSentToCallback)
                    {
                        if (SqlDb.RetryErrorHandler.ShouldWaitToRetry(rootException))
                        {
                            if (_logger != null)
                                _logger.Warn("Delayed retry (" + retryIteration + ") due to the transient error.");
                            await Task.Delay(5000 + 10000 * (retryIteration - 1)); // wait 5, 15, and 25 sec
                        }
                        else
                        {
                            if (_logger != null)
                                _logger.Warn("Retrying immediatelly (" + retryIteration + ").");
                        }
                    }
                }
            } while (shouldRetry && retryIteration < SqlDb.RetryErrorHandler.RETRY_COUNT && !isResultSentToCallback);

            if (shouldRetry && retryIteration == SqlDb.RetryErrorHandler.RETRY_COUNT)
            {
                if (this._logger != null)
                    this._logger.Error("Query failed after " + SqlDb.RetryErrorHandler.RETRY_COUNT + " retries.");

                await ExecuteCallbackWithException(callback, rootException);
            }
        }

        protected virtual async Task ExecuteCallbackWithException(object callback, Exception ex) { }
        protected virtual async Task<bool> ExecuteCommand(DbCommand command, object callback) => false;
    }
}
