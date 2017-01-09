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
        /// Executes SQL query and put results into stream.
        /// </summary>
        /// <param name="sql">SQL query that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into stream if there are no results.</param>
        /// <returns>Task</returns>
        public async Task Stream(string sql, Stream stream)
        {
            using (DbCommand command = new T())
            {
                command.CommandText = sql;
                await this.Stream(command, stream);
            }
        }

        /// <summary>
        /// Executes SQL command and put results into stream.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        public async Task Stream(DbCommand command, Stream stream)
        {
            command.Connection = this.Connection;
            await this.SqlResultsToStream(command, stream, null);
        }


        /// <summary>
        /// Executes SQL query and puts results into stream.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <returns>Task</returns>
        public async Task Stream<T1>(string sql, Stream stream, T1 defaultOutput)
        {
            using (DbCommand command = new T())
            {
                command.CommandText = sql;
                await this.Stream<T1>(command, stream, defaultOutput);
            }
        }

        /// <summary>
        /// Executes SQL command and puts results into stream.
        /// </summary>
        /// <param name="command">SQL command that will be executed.</param>
        /// <param name="stream">Output stream where results will be written.</param>
        /// <param name="defaultOutput">Default content that will be written into stream if there are no results.</param>
        /// <returns>Task</returns>
        public async Task Stream<T1>(DbCommand command, Stream stream, T1 defaultOutput)
        {
            if (defaultOutput is byte[])
            {
                await SqlResultsToStream(command, stream, defaultOutput as byte[]).ConfigureAwait(false);
            }
            else if (defaultOutput is string)
            {
                await SqlResultsToStream(command, stream, Encoding.UTF8.GetBytes(defaultOutput as string)).ConfigureAwait(false);
            }
            else
                throw new ArgumentException();
        }

        private async Task SqlResultsToStream(DbCommand command, Stream stream, byte[] defaultOutput)
        {
            bool outputIsGenerated = false;
            try
            {
                await this.Mapper.ExecuteReader(command,
                    async reader =>
                    {
                        if (reader.HasRows)
                        {
                            byte[] buffer = null;
                            if(reader[0].GetType().Name == "String")
                            {
                                buffer = Encoding.UTF8.GetBytes(reader.GetString(0));
                            } else if (reader[0].GetType().Name == "Byte[]")
                            {
                                buffer = (byte[])reader[0];
                            }
                            await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                            await stream.FlushAsync();
                            outputIsGenerated = true;
                        }
                        else
                        {
                            if (defaultOutput != null)
                            {
                                stream.Write(defaultOutput, 0, defaultOutput.Length);
                                outputIsGenerated = true;
                            }
                        }
                    });
            }
            catch (Exception ex)
            {
                try
                {
                    base.GetErrorHandlerBuilder().SetCommand(command).CreateErrorHandler()(ex);
                } catch {
                    defaultOutput = null; // Don't generate default output if error is raised.
                }
            }
            finally
            {
                /// If the output is not generated by DataReader we need to generate default value.
                if (!outputIsGenerated && defaultOutput != null)
                {
                    stream.Write(defaultOutput, 0, defaultOutput.Length);
                }
                command.Connection.Close();
            }
        }
    }
}