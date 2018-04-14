//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license.
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE. See the license files for details.
using Common.Logging;
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Belgrade.SqlClient.Common
{
    /// <summary>
    /// Component that streams results of SQL query into an output stream.
    /// </summary>
    public class GenericQueryPipe<T> : BaseStatement, IQueryPipe
        where T : DbCommand, new()
    {
        /// <summary>
        /// Query mapper used to stream results.
        /// </summary>
        private GenericQueryMapper<T> Mapper;

        internal override BaseStatement SetCommandModifier(Func<DbCommand, DbCommand> value)
        {
            this.Mapper.SetCommandModifier(value);
            return this;
        }

        internal override BaseStatement AddErrorHandler(ErrorHandlerBuilder builder)
        {
            // IMPORTANT: Secondary Mapper should NOT have error handler!!!!
            // If Mapper handles the error, Pipe will not know that error is thrown and it will generate default output.
            // Mapper should handle all errors.
            //this.Mapper.AddErrorHandler(builder);

            return base.AddErrorHandler(builder);
        }

        /// <summary>
        /// Adds a logger that will be used by SQL Command.
        /// </summary>
        /// <param name="logger">Common.Logging.ILog where log records will be written.</param>
        /// <returns>This statement.</returns>
        public override BaseStatement AddLogger(ILog logger)
        {
            if (this.Mapper != null) this.Mapper.AddLogger(logger);

            return base.AddLogger(logger);
        }

        /// <summary>
        /// Creates QueryPipe object.
        /// </summary>
        /// <param name="connection">Connection to Sql Database.</param>
        public GenericQueryPipe(DbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("Connection is not defined.");

            if (string.IsNullOrWhiteSpace(connection.ConnectionString))
                throw new ArgumentNullException("Connection string is not set.");

            this.Connection = connection;
            this.Mapper = new GenericQueryMapper<T>(connection);
        }

        /// <summary>
        /// Executes SQL command and put results into stream.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        public async Task Stream(Stream stream, Options options = null)
        {
            DbCommand command = base.Command;
            if(command == null)
            {
                throw new InvalidOperationException("Cannot stream results while command is not defined.");
            }
            command.Connection = this.Connection;
            await this.SqlResultsToStream(command, stream, options);
        }
        
        private Task SqlResultsToStream(DbCommand command, Stream stream, Options options)
        {
            if (command == null)
            {
                throw new InvalidOperationException("Cannot stream results while command is not defined.");
            }
            if (stream == null)
                throw new ArgumentNullException("stream", "Stream provided to SqlResultToStream is not defined!");
            if (!stream.CanWrite)
                throw new ArgumentException("Cannot write to the stream in SqlResultToStream", "stream");

            return FlushSqlResultsToStream<Stream>(command, stream, options);
        }

        /// <summary>
        /// Executes SQL command and puts results into stream.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="writer">TextWriter where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into TextWriter if there are no results.</param>
        /// <returns>Task</returns>
        public Task Stream(TextWriter writer, Options options)
        {
            DbCommand command = base.Command;
            if (command == null)
            {
                throw new InvalidOperationException("Cannot stream results while command is not defined.");
            }
            return FlushSqlResultsToStream<TextWriter>(command, writer, options);
        }

        /// <summary>
        /// Helper function that flushes results of SQL query into the stream.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="command"></param>
        /// <param name="stream"></param>
        /// <param name="defaultOutput"></param>
        /// <returns></returns>
        private async Task FlushSqlResultsToStream<TOutput>(DbCommand command, TOutput stream, Options options /*byte[] defaultOutput*/)
        {
            if (command == null)
            {
                throw new InvalidOperationException("Cannot stream results while command is not defined.");
            }
            bool outputIsGenerated = false;
            bool isFirstChunk = true;
            bool isErrorDetected = false;
            try
            {
                await this.Mapper.Sql(command).Map(
                    async reader =>
                    {
                        try
                        {
                            if (isFirstChunk && options != null && options.Prefix != null)
                            {
                                await FlushContent<TOutput>(stream, options.Prefix, _logger);
                                isFirstChunk = false;
                            }
                            if (reader.HasRows)
                            {
                                if (reader.FieldCount != 1)
                                    throw new ArgumentException("SELECT query should not have " + reader.FieldCount + " columns (expected 1).", "reader");
                                string buffer = null;
                                if (reader[0].GetType().Name == "String")
                                {
                                    buffer = reader.GetString(0);
                                    await FlushContent<TOutput>(stream, buffer, _logger);
                                    outputIsGenerated = true;
                                }
                                else if (reader[0].GetType().Name == "Byte[]")
                                {
                                    byte[] binary = new byte[2048];
                                    int amount = (int)reader.GetBytes(0, 0, binary, 0, 2048);
                                    int pos = amount;
                                    do
                                    {
                                        await FlushContent<TOutput>(stream, binary, _logger, amount);
                                        outputIsGenerated = true;
                                        amount = (int)reader.GetBytes(0, pos, binary, 0, 2048);
                                        pos += amount;
                                    }
                                    while (amount > 0);
                                }
                                else
                                {
                                    if (_logger != null)
                                        _logger.Fatal("Column type " + reader[0].GetType().Name + " cannot be sent to the streamed.");

                                    throw new ArgumentException("The column type returned by the query cannot be sent to the stream.", reader[0].GetType().Name);
                                }
                            }
                            else
                            {
                                if (options != null && options.DefaultOutput != null)
                                {
                                    await FlushContent<TOutput>(stream, options.DefaultOutput, _logger, ((options.DefaultOutput is byte[]) ? (options.DefaultOutput as byte[]).Length : (-1)));
                                    outputIsGenerated = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_logger != null)
                                _logger.ErrorFormat("Error {error} occured while trying to flush results of SQL query to the stream.\n{exception}", ex.Message, ex);
                            isErrorDetected = true;
                            if (options != null)
                                options.DefaultOutput = null; // Don't generate default output if error is raised.
                            try
                            {
                                var errorHandler = base.GetErrorHandlerBuilder().CreateErrorHandler(base._logger);
                                errorHandler(ex);
                            }
                            catch (Exception ex2){
                                if (_logger != null)
                                    _logger.ErrorFormat("Error {error} occured while trying to handle error in query pipe error handler on {source}.", ex2.Message, ex2.Source);
                                throw;
                            }
                        }
                    });
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.Error("Error occured while trying to map results to output.", ex);

                isErrorDetected = true;
                if (options != null)
                    options.DefaultOutput = null; // Don't generate default output if error is raised.
                try
                {
                    var errorHandler = base.GetErrorHandlerBuilder().CreateErrorHandler(base._logger);
                    errorHandler(ex);
                }
                catch (Exception ex2)
                {
                    if (_logger != null)
                        _logger.Warn("Error occured while trying to handle error in query pipe error handler(2).", ex2);
                    throw;
                }
            }
            finally
            {
                if ( !isErrorDetected && isFirstChunk && options != null && options.Prefix != null)
                {
                    await FlushContent<TOutput>(stream, options.Prefix, _logger);
                    isFirstChunk = false;
                }

                /// If the output is not generated by DataReader we need to generate default value.
                if (!outputIsGenerated && options != null && options.DefaultOutput != null)
                {
                    await FlushContent<TOutput>(stream, options.DefaultOutput, _logger, ((options.DefaultOutput is byte[]) ? (options.DefaultOutput as byte[]).Length : (-1)));
                }

                // Add suffix if there was no error.
                if (!isErrorDetected && options != null && options.Suffix != null)
                {
                    await FlushContent<TOutput>(stream, options.Suffix, _logger);
                }
                command.Connection.Close();
            }
        }

        /// <summary>
        /// Helper function that flushes content into stream or writer.
        /// </summary>
        /// <typeparam name="TOutput">Type of the output - Stream or TextWriter</typeparam>
        /// <param name="output">Stream or Output object.</param>
        /// <param name="content">Content that will be flushed into stream or text writer.</param>
        /// <param name="amount">Lengt of the bytes to be writted (-1 for string).</param>
        /// <returns>Task</returns>
        private static async Task FlushContent<TOutput>(TOutput output, object content, ILog _logger, int amount = -1)
        {
            try
            {
                if (amount > -1)
                {
                    if (output is TextWriter)
                    {
                        var writer = output as TextWriter;
                        await writer.WriteAsync(Encoding.UTF8.GetString((content as byte[]), 0, amount)).ConfigureAwait(false);
                        await writer.FlushAsync();
                    }
                    else
                    {
                        // Since default value is not changed,we are writing binary.
                        var writer = output as Stream;
                        await writer.WriteAsync((content as byte[]), 0, amount).ConfigureAwait(false);
                        await writer.FlushAsync();
                    }
                }
                else
                {
                    if (output is TextWriter)
                    {
                        var writer = output as TextWriter;
                        await writer.WriteAsync(content as string).ConfigureAwait(false);
                        await writer.FlushAsync();
                    }
                    else if (output is Stream)
                    {
                        var writer = output as Stream;
                        var binary = Encoding.UTF8.GetBytes(content as string);
                        await writer.WriteAsync(binary, 0, binary.Length).ConfigureAwait(false);
                        await writer.FlushAsync();
                    }
                }
            } catch(Exception ex)
            {
                if (_logger != null)
                    _logger.Warn("Error occured while trying to flush content " + content + " to output " + output.GetType().Name, ex);
                throw;
            }
        }

        /// <summary>
        /// Set T-SQL query that should be executed.
        /// </summary>
        /// <param name="cmd">DbCommand with the query text.</param>
        /// <returns>Query initialized with query text that will be executed.</returns>
        public IQueryPipe Sql(DbCommand cmd)
        {
            return base.SetCommand(cmd) as IQueryPipe;
        }

        /// <summary>
        /// Adds a parameter to the mapper.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="type">Parameter type.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <param name="size">Size of the parameter.</param>
        /// <returns>Mapper with new parameter.</returns>
        public IQueryPipe Param(string name, DbType type, object value, int size = 0)
        {
            return base.AddParameter(name, type, value, size) as IQueryPipe;
        }
    }
}