using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Xml;
using System.Diagnostics;
using System.Data.SqlClient;

namespace CQRS
{
    public class Scenario
    {
        public Scenario()
        {
            var command = new Command(Util.Settings.MasterConnectionString.Replace("Database=master;", "Database=ProductCatalogDemo;"));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            command.Sql("DELETE Company WHERE companyId >= 4").Exec();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        [Fact]
        public void PCRUD()
        {
            List<Task> tasks = new List<Task>();
            for (int ctr = 0; ctr <= 100; ctr++)
                tasks.Add(Task.Run(() => CRUD(ctr%2==0)));

            Task.WaitAll(tasks.ToArray());

            foreach (var t in tasks)
            {
                Assert.False(t.IsCanceled);
                Assert.True(t.IsCompleted);
                Assert.False(t.IsFaulted);
            }
        }

        static readonly object lockIdentity = new object();
        static int identity = 4;

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CRUD(bool useCommand)
        {
            List<string> errors = new List<string>();
            IQueryMapper mapper = CreateNewMapper(errors);
            var command = CreateNewCommand(errors);

            var sqlCmd = new SqlCommand();

            int ID = -1;
            lock (lockIdentity)
            {
                ID = identity++;
            }
            string NAME = "MSFT" + ID;
            string NAME2 = "MDCS" + ID;
            string NAME3 = "Microsoft" + ID;
            string NAME4 = "MS" + ID;
            int count = -1;
            mapper = CreateNewMapper(errors);
            if (useCommand)
            {
                sqlCmd = new SqlCommand();
                sqlCmd.CommandText = "select count(*) from Company where CompanyId = @ID";
                sqlCmd.Parameters.AddWithValue("ID", ID);
                await mapper.Sql(sqlCmd).Map(reader => count = reader.GetInt32(0));
            }
            else
            {
                await mapper
                    .Sql("select count(*) from Company where CompanyId = @ID")
                    .Param("ID", System.Data.DbType.Int32, ID)
                    .Map(reader => count = reader.GetInt32(0));
            }
            Assert.Equal(0, count);

            command = CreateNewCommand(errors);
            if (useCommand)
            {    
                sqlCmd.CommandText = "insert into Company(companyId, Name) values(@ID, @NAME)";
                sqlCmd.Parameters.Clear();
                sqlCmd.Parameters.AddWithValue("ID", ID);
                sqlCmd.Parameters.AddWithValue("NAME", NAME);
                await command.Sql(sqlCmd).Exec();
            }
            else
            {
                await
                    command
                    .Sql("insert into Company(companyId, Name) values(@ID, @NAME)")
                    .Param("ID", System.Data.DbType.Int32, ID)
                    .Param("NAME", System.Data.DbType.String, NAME)
                    .Exec();
            }

            mapper = CreateNewMapper(errors);
            if (useCommand)
            {
                sqlCmd.CommandText = "select count(*) from Company where CompanyId = @ID";
                sqlCmd.Parameters.Clear();
                sqlCmd.Parameters.AddWithValue("ID", ID);
                await mapper.Sql(sqlCmd).Map(reader => count = reader.GetInt32(0));
            }
            else
            {
                await
                    mapper
                    .Sql("select count(*) from Company where CompanyId = @ID")
                    .Param("ID", System.Data.DbType.Int32, ID)
                    .Map(reader => count = reader.GetInt32(0));
            }

            Assert.Equal(1, count);

            mapper = CreateNewMapper(errors);
            int? id = null;
            string name = null;
            if (useCommand)
            {
                sqlCmd.CommandText = "select CompanyId, Name from Company where CompanyId = @ID";
                sqlCmd.Parameters.Clear();
                sqlCmd.Parameters.AddWithValue("ID", ID);
                await mapper
                        .Sql(sqlCmd)
                        .Map(reader => { id = reader.GetInt32(0); name = reader.GetString(1); });
            } else
            {
                await 
                    mapper
                    .Sql("select CompanyId, Name from Company where CompanyId = @ID")
                    .Param("ID", System.Data.DbType.Int32, ID)
                    .Map(reader => { id = reader.GetInt32(0); name = reader.GetString(1); });
            }
            Assert.Equal(ID, id);
            Assert.Equal(NAME, name);

            if (useCommand)
            {
                await command
                    .Sql("update Company set Name = '" + NAME2 + "' where CompanyId = " + ID)
                    .Exec();
            } else
            {
                await command
                    .Sql("update Company set Name = '" + NAME2 + "' where CompanyId = " + ID)
                    .Exec();
            }
            mapper = CreateNewMapper(errors);
            if (useCommand)
            {
                sqlCmd.CommandText = "select Name from Company where CompanyId = @ID";
                sqlCmd.Parameters.Clear();
                sqlCmd.Parameters.AddWithValue("ID", ID);
                await mapper
                    .Sql(sqlCmd)
                    .Map( reader => { name = reader.GetString(0); });
            } else
            {
                await mapper
                    .Sql("select Name from Company where CompanyId = @ID")
                    .Param("ID", System.Data.DbType.Int32, ID)
                    .Map(reader => { name = reader.GetString(0); });
            }
            Assert.Equal(NAME2, name);


            command = CreateNewCommand(errors);
            if (useCommand)
            {
                sqlCmd.CommandText = "delete Company where CompanyId = @ID";
                sqlCmd.Parameters.Clear();
                sqlCmd.Parameters.AddWithValue("ID", ID);
                await command.Sql(sqlCmd).Exec();
            } else
            {
                await command
                    .Sql("delete Company where CompanyId = @ID")
                    .Param("ID", System.Data.DbType.Int32, ID)
                    .Exec();
            }

            mapper = CreateNewMapper(errors);
            if (useCommand)
            {
                sqlCmd.CommandText = "select count(*) from Company where CompanyId = @ID";
                sqlCmd.Parameters.Clear();
                sqlCmd.Parameters.AddWithValue("ID", ID);
                await mapper.Sql(sqlCmd).Map(reader => count = reader.GetInt32(0));
            } else
            {
                await mapper
                    .Sql("select count(*) from Company where CompanyId = @ID")
                    .Param("ID", System.Data.DbType.Int32, ID)
                    .Map(reader => count = reader.GetInt32(0));
            }
            Assert.Equal(0, count);

            id = null;
            command = CreateNewCommand(errors);
            if (useCommand)
            {
                sqlCmd.CommandText = "insert into Company(Name) output inserted.CompanyId values(@NAME)";
                sqlCmd.Parameters.Clear();
                sqlCmd.Parameters.AddWithValue("NAME", NAME3);
                await command.Sql(sqlCmd).Map(reader => id = reader.GetInt32(0));
            } else
            {
                await command
                    .Sql("insert into Company(Name) output inserted.CompanyId values(@NAME)")
                    .Param("NAME", System.Data.DbType.String, NAME3)
                    .Map(reader => id = reader.GetInt32(0));
            }

            mapper = CreateNewMapper(errors);
            if (useCommand)
            {
                sqlCmd.CommandText = "select count(*) from Company where CompanyId = @ID";
                sqlCmd.Parameters.Clear();
                sqlCmd.Parameters.AddWithValue("ID", id);
                await mapper.Sql(sqlCmd).Map(reader => count = reader.GetInt32(0));
            } else
            {
                await mapper
                    .Sql("select count(*) from Company where CompanyId = @ID")
                    .Param("ID",System.Data.DbType.Int32, id)
                    .Map(reader => count = reader.GetInt32(0));
            }
            Assert.Equal(1, count);

            mapper = CreateNewMapper(errors);
            if (useCommand)
            {
                sqlCmd.CommandText = "select Name from Company where CompanyId = @ID";
                sqlCmd.Parameters.Clear();
                sqlCmd.Parameters.AddWithValue("ID", id);
                await mapper.Sql(sqlCmd).Map(reader => name = reader.GetString(0));
            } else
            {
                await mapper
                    .Sql("select Name from Company where CompanyId = @ID")
                    .Param("ID", System.Data.DbType.Int32, id)
                    .Map(reader => name = reader.GetString(0));
            }
            Assert.Equal(NAME3, name);


            string oldname = null;
            string newname = null;
            await command.Sql("update Company SET Name = '" + NAME4 + "' output deleted.Name, inserted.Name where CompanyId = " + id).Map(reader => { oldname = reader.GetString(0); newname = reader.GetString(1); });
            Assert.Equal(NAME3, oldname);
            Assert.Equal(NAME4, newname);

            name = null;
            int deletedId = -1;
            command = CreateNewCommand(errors);
            if (useCommand)
            {
                sqlCmd.CommandText = "delete Company output deleted.CompanyId, deleted.Name where CompanyId = @ID";
                sqlCmd.Parameters.Clear();
                sqlCmd.Parameters.AddWithValue("ID", id);
                await command.Sql(sqlCmd).Map(
                    reader => { deletedId = reader.GetInt32(0); name = reader.GetString(1); });
            } else
            {
                await command
                    .Sql("delete Company output deleted.CompanyId, deleted.Name where CompanyId = @ID")
                    .Param("ID", System.Data.DbType.Int32, id)
                    .Map(reader => { deletedId = reader.GetInt32(0); name = reader.GetString(1); });

            }
            Assert.Equal(NAME4, name);
            Assert.Equal(id, deletedId);
            Assert.Empty(errors);
        }

        private static ICommand CreateNewCommand(List<string> errors)
        {
            return new Command(new SqlConnection(Util.Settings.MasterConnectionString.Replace("Database=master;", "Database=ProductCatalogDemo;")))
                                .OnError(ex => errors.Add(ex.Message));
        }

        private static IQueryMapper CreateNewMapper(List<string> errors)
        {
            return new QueryMapper(new SqlConnection(Util.Settings.MasterConnectionString.Replace("Database=master;", "Database=ProductCatalogDemo;")))
                                .OnError(ex => errors.Add(ex.Message));
        }
    }
}