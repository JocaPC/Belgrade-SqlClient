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

namespace CQRS
{
    public class Scenario
    {
        IQueryMapper mapper;
        ICommand command;

        public Scenario()
        {
        }

        [Fact]
        public void PCRUD()
        {
            List<Task> tasks = new List<Task>();
            // Start three tasks. 
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
            var mapper = new QueryMapper(Util.Settings.ConnectionString.Replace("Database=master;", "Database=ProductCatalogDemo;"));
            var command = new Command(Util.Settings.ConnectionString.Replace("Database=master;", "Database=ProductCatalogDemo;"));

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
            await mapper.ExecuteReader("select count(*) from Company where CompanyId = "+ID, reader => count = reader.GetInt32(0));
            Assert.Equal(0, count);
            await command.ExecuteNonQuery("insert into Company(companyId, Name) values(" + ID + ", '" + NAME + "')");

            await mapper.ExecuteReader("select count(*) from Company where CompanyId = " + ID, reader => count = reader.GetInt32(0));
            Assert.Equal(1, count);

            int? id = null;
            string name = null;
            await mapper.ExecuteReader("select CompanyId, Name from Company where CompanyId = " + ID, 
                        reader => { id = reader.GetInt32(0); name = reader.GetString(1); });

            Assert.Equal(ID, id);
            Assert.Equal(NAME, name);

            await command.ExecuteNonQuery("update Company set Name = '"+NAME2+"' where CompanyId = " + ID);

            await mapper.ExecuteReader("select Name from Company where CompanyId = " + ID,
                        reader => { name = reader.GetString(0); });
            Assert.Equal(NAME2, name);

            await command.ExecuteNonQuery("delete Company where CompanyId = " + ID);

            await mapper.ExecuteReader("select count(*) from Company where CompanyId = " + ID, reader => count = reader.GetInt32(0));
            Assert.Equal(0, count);

            id = null;
            await command.ExecuteReader("insert into Company(Name) output inserted.CompanyId values('"+NAME3+"')", reader => id = reader.GetInt32(0));

            await mapper.ExecuteReader("select count(*) from Company where CompanyId = " + id, reader => count = reader.GetInt32(0));
            Assert.Equal(1, count);

            await mapper.ExecuteReader("select Name from Company where CompanyId = " + id, reader => name = reader.GetString(0));
            Assert.Equal(NAME3, name);

            string oldname = null;
            string newname = null;
            await command.ExecuteReader("update Company SET Name = '" + NAME4 + "' output deleted.Name, inserted.Name where CompanyId = " + id, reader => { oldname = reader.GetString(0); newname = reader.GetString(1); });
            Assert.Equal(NAME3, oldname);
            Assert.Equal(NAME4, newname);

            name = null;
            int deletedId = -1;
            await command.ExecuteReader("delete Company output deleted.CompanyId, deleted.Name where CompanyId = " + id, 
                reader => { deletedId = reader.GetInt32(0); name = reader.GetString(1); });

            Assert.Equal(NAME4, name);
            Assert.Equal(id, deletedId);

        }
    }
}