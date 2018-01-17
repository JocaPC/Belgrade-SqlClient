//  Author:     Jovan Popovic. 
//  This source file is free software, available under MIT license.
//  This source file is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE. See the license files for details.
using System;
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
        public async Task Stream(DbCommand command, Stream stream, Options options = null)
        {
            command.Connection = this.Connection;
            await this.SqlResultsToStream(command, stream, options);
        }
        
        private Task SqlResultsToStream(DbCommand command, Stream stream, Options options /*byte[] defaultOutput*/)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "Stream provided to SqlResultToStream is not defined!");
            if (!stream.CanWrite)
                throw new ArgumentException("Cannot write to the stream in SqlResultToStream", "stream");

            return FlushSqlResultsToStream<Stream>(command, stream, options/*defaultOutput*/);

        }

        /// <summary>
        /// Executes SQL command and puts results into stream.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="writer">TextWriter where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into TextWriter if there are no results.</param>
        /// <returns>Task</returns>
        public Task Stream(DbCommand command, TextWriter writer, Options options /*string[] defaultOutput*/)
        {
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
            bool outputIsGenerated = false;
            bool isFirstChunk = true;
            bool isErrorDetected = false;
            try
            {
                await this.Mapper.ExecuteReader(command,
                    async reader =>
                    {
                        if (isFirstChunk && options != null && options.Prefix != null)
                        {
                            await FlushContent<TOutput>(stream, options.Prefix);
                            isFirstChunk = false;
                        }
                        if (reader.HasRows)
                        {
                            if (reader.FieldCount != 1)
                                throw new ArgumentException("SELECT query should not have " + reader.FieldCount + " columns (expected 1).", "reader");
                            string buffer = null;
                            if(reader[0].GetType().Name == "String")
                            {
                                buffer = reader.GetString(0);
                                await FlushContent<TOutput>(stream, buffer);
                                outputIsGenerated = true;
                            }
                            else if (reader[0].GetType().Name == "Byte[]")
                            {
                                byte[] binary = new byte[2048];
                                int amount = (int)reader.GetBytes(0, 0, binary, 0, 2048);
                                int pos = amount;
                                do
                                {
                                    await FlushContent<TOutput>(stream, binary, amount);
                                    outputIsGenerated = true;
                                    amount = (int)reader.GetBytes(0, pos, binary, 0, 2048);
                                    pos += amount;
                                }
                                while (amount > 0);
                            }
                            else
                            {
                                throw new ArgumentException("Return type " + reader[0].GetType().Name + " cannot be streamed.", "reader");
                            }
                        }
                        else
                        {
                            if (options != null && options.DefaultOutput != null)
                            {
                                await FlushContent<TOutput>(stream, options.DefaultOutput, ((options.DefaultOutput is byte[])? (options.DefaultOutput as byte[]).Length : (-1) ));
                                outputIsGenerated = true;
                            }
                        }
                    });
            }
            catch (Exception ex)
            {
                isErrorDetected = true;
                if (options != null)
                    options.DefaultOutput = null; // Don't generate default output if error is raised.
                try
                {
                    var errorHandler = base.GetErrorHandlerBuilder().SetCommand(command).CreateErrorHandler();
                    if (errorHandler == null)
                        throw;
                    else
                        errorHandler(ex);
                } catch {  }
            }
            finally
            {
                if ( !isErrorDetected && isFirstChunk && options != null && options.Prefix != null)
                {
                    await FlushContent<TOutput>(stream, options.Prefix);
                    isFirstChunk = false;
                }

                /// If the output is not generated by DataReader we need to generate default value.
                if (!outputIsGenerated && options != null && options.DefaultOutput != null)
                {
                    await FlushContent<TOutput>(stream, options.DefaultOutput, ((options.DefaultOutput is byte[]) ? (options.DefaultOutput as byte[]).Length : (-1)));
                }

                // Add suffix if there was no error.
                if (!isErrorDetected && options != null && options.Suffix != null)
                {
                    await FlushContent<TOutput>(stream, options.Suffix);
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
        private static async Task FlushContent<TOutput>(TOutput output, object content, int amount = -1)
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
            } else {
                if (output is TextWriter)
                {
                    var writer = output as TextWriter;
                    await writer.WriteAsync(content as string).ConfigureAwait(false);
                    await writer.FlushAsync();
                } else if (output is Stream)
                {
                    var writer = output as Stream;
                    var binary = Encoding.UTF8.GetBytes(content as string);
                    await writer.WriteAsync(binary, 0, binary.Length).ConfigureAwait(false);
                    await writer.FlushAsync();
                }
            }
        }
    }
}