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

namespace CQRS
{
    public class Scenario
    {
        IQueryMapper mapper;
        ICommand command;

        public Scenario()
        {
            mapper = new QueryMapper(Util.Settings.ConnectionString.Replace("Database=master;", "Database=ProductCatalogDemo;"));
            command = new Command(Util.Settings.ConnectionString.Replace("Database=master;", "Database=ProductCatalogDemo;"));
        }

        [Fact]
        public async Task CRUD()
        {
            int count = -1;
            await mapper.ExecuteReader("select count(*) from Company where CompanyId = 4", reader => count = reader.GetInt32(0));
            Assert.Equal(0, count);
            await command.ExecuteNonQuery("insert into Company(companyId, Name) values(4,'MSFT')");

            await mapper.ExecuteReader("select count(*) from Company where CompanyId = 4", reader => count = reader.GetInt32(0));
            Assert.Equal(1, count);

            int? id = null;
            string name = null;
            await mapper.ExecuteReader("select CompanyId, Name from Company where CompanyId = 4", 
                        reader => { id = reader.GetInt32(0); name = reader.GetString(1); });

            Assert.Equal(4, id);
            Assert.Equal("MSFT", name);

            await command.ExecuteNonQuery("update Company set Name = 'MDCS' where CompanyId = 4");

            await mapper.ExecuteReader("select Name from Company where CompanyId = 4",
                        reader => { name = reader.GetString(0); });
            Assert.Equal("MDCS", name);

            await command.ExecuteNonQuery("delete Company where CompanyId = 4");

            await mapper.ExecuteReader("select count(*) from Company where CompanyId = 4", reader => count = reader.GetInt32(0));
            Assert.Equal(0, count);

            id = null;
            await command.ExecuteReader("insert into Company(Name) output inserted.CompanyId values('Microsoft')", reader => id = reader.GetInt32(0));

            await mapper.ExecuteReader("select count(*) from Company where CompanyId = " + id, reader => count = reader.GetInt32(0));
            Assert.Equal(1, count);

            await mapper.ExecuteReader("select Name from Company where CompanyId = " + id, reader => name = reader.GetString(0));
            Assert.Equal("Microsoft", name);

            name = null;
            await command.ExecuteReader("update Company SET Name = 'MS' output deleted.Name where CompanyId = " + id, reader => name = reader.GetString(0));
            Assert.Equal("Microsoft", name);

            name = null;
            int deletedId = -1;
            await command.ExecuteReader("delete Company output deleted.CompanyId, deleted.Name where CompanyId = " + id, 
                reader => { deletedId = reader.GetInt32(0); name = reader.GetString(1); });

            Assert.Equal("MS", name);
            Assert.Equal(id, deletedId);

        }
    }
}