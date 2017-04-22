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
        IQueryMapper mapper;
        ICommand command;

        public Scenario()
        {
            var command = new Command(Util.Settings.ConnectionString.Replace("Database=master;", "Database=ProductCatalogDemo;"));
            command.ExecuteNonQuery("DELETE Company WHERE companyId >= 4");
        }

        [Fact]
        public void PCRUD()
        {
            List<Task> tasks = new List<Task>();
            for (int ctr = 0; ctr <= 100; ctr++)
                tasks.Add(Task.Run(() => CRUD()));

            Task.WaitAll(tasks.ToArray());

            foreach (var t in tasks)
            {
                Assert.Equal(false, t.IsCanceled);
                Assert.Equal(true, t.IsCompleted);
                Assert.Equal(false, t.IsFaulted);
            }
        }

        static readonly object lockIdentity = new object();
        static int identity = 4;
        [Fact]
        public async Task CRUD()
        {
            List<string> errors = new List<string>();
            var mapper = new QueryMapper(new SqlConnection(Util.Settings.ConnectionString.Replace("Database=master;", "Database=ProductCatalogDemo;")));
            mapper.OnError(ex => errors.Add(ex.Message));
            var command = new Command(new SqlConnection(Util.Settings.ConnectionString.Replace("Database=master;", "Database=ProductCatalogDemo;")));
            command.OnError(ex => errors.Add(ex.Message));
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
            sqlCmd.CommandText = "select count(*) from Company where CompanyId = @ID";
            sqlCmd.Parameters.Clear();
            sqlCmd.Parameters.AddWithValue("ID", ID);
            await mapper.ExecuteReader(sqlCmd, reader => count = reader.GetInt32(0));
            Assert.Equal(0, count);

            sqlCmd.CommandText = "insert into Company(companyId, Name) values(@ID, @NAME)";
            sqlCmd.Parameters.Clear();
            sqlCmd.Parameters.AddWithValue("ID", ID);
            sqlCmd.Parameters.AddWithValue("NAME", NAME);
            await command.ExecuteNonQuery(sqlCmd);

            sqlCmd.CommandText = "select count(*) from Company where CompanyId = @ID";
            sqlCmd.Parameters.Clear();
            sqlCmd.Parameters.AddWithValue("ID", ID);
            await mapper.ExecuteReader(sqlCmd, reader => count = reader.GetInt32(0));
            Assert.Equal(1, count);

            int? id = null;
            string name = null;
            sqlCmd.CommandText = "select CompanyId, Name from Company where CompanyId = @ID";
            sqlCmd.Parameters.Clear();
            sqlCmd.Parameters.AddWithValue("ID", ID);
            await mapper.ExecuteReader(sqlCmd, 
                        reader => { id = reader.GetInt32(0); name = reader.GetString(1); });

            Assert.Equal(ID, id);
            Assert.Equal(NAME, name);

            await command.ExecuteNonQuery("update Company set Name = '"+NAME2+"' where CompanyId = " + ID);

            sqlCmd.CommandText = "select Name from Company where CompanyId = @ID";
            sqlCmd.Parameters.Clear();
            sqlCmd.Parameters.AddWithValue("ID", ID);
            await mapper.ExecuteReader(sqlCmd,
                        reader => { name = reader.GetString(0); });
            Assert.Equal(NAME2, name);


            sqlCmd.CommandText = "delete Company where CompanyId = @ID";
            sqlCmd.Parameters.Clear();
            sqlCmd.Parameters.AddWithValue("ID", ID);
            await command.ExecuteNonQuery(sqlCmd);


            sqlCmd.CommandText = "select count(*) from Company where CompanyId = @ID";
            sqlCmd.Parameters.Clear();
            sqlCmd.Parameters.AddWithValue("ID", ID);
            await mapper.ExecuteReader(sqlCmd, reader => count = reader.GetInt32(0));
            Assert.Equal(0, count);

            id = null;
            sqlCmd.CommandText = "insert into Company(Name) output inserted.CompanyId values(@NAME)";
            sqlCmd.Parameters.Clear();
            sqlCmd.Parameters.AddWithValue("NAME", NAME3);
            await command.ExecuteReader(sqlCmd, reader => id = reader.GetInt32(0));


            sqlCmd.CommandText = "select count(*) from Company where CompanyId = @ID";
            sqlCmd.Parameters.Clear();
            sqlCmd.Parameters.AddWithValue("ID", id);
            await mapper.ExecuteReader(sqlCmd, reader => count = reader.GetInt32(0));
            Assert.Equal(1, count);


            sqlCmd.CommandText = "select Name from Company where CompanyId = @ID";
            sqlCmd.Parameters.Clear();
            sqlCmd.Parameters.AddWithValue("ID", id);
            await mapper.ExecuteReader(sqlCmd, reader => name = reader.GetString(0));
            Assert.Equal(NAME3, name);

            string oldname = null;
            string newname = null;
            await command.ExecuteReader("update Company SET Name = '" + NAME4 + "' output deleted.Name, inserted.Name where CompanyId = " + id, reader => { oldname = reader.GetString(0); newname = reader.GetString(1); });
            Assert.Equal(NAME3, oldname);
            Assert.Equal(NAME4, newname);

            name = null;
            int deletedId = -1;

            sqlCmd.CommandText = "delete Company output deleted.CompanyId, deleted.Name where CompanyId = @ID";
            sqlCmd.Parameters.Clear();
            sqlCmd.Parameters.AddWithValue("ID", id);
            await command.ExecuteReader(sqlCmd, 
                reader => { deletedId = reader.GetInt32(0); name = reader.GetString(1); });

            Assert.Equal(NAME4, name);
            Assert.Equal(id, deletedId);
            Assert.Equal(0, errors.Count);
        }
    }
}