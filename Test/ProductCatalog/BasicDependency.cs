using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using System.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Basic
{
    public class Dependency
    {
        public Dependency()
        {
            
        }

//#if NET46
        [Fact]
        public async Task OnChangeAction()
        {
            // Arrange
            IQuery sut = new QueryMapper(Util.Settings.ProductCatalogConnectionString);
            bool isChanged = false;
            ICommand c = new Belgrade.SqlClient.SqlDb.Command(Util.Settings.ProductCatalogConnectionString);
            await c.Sql("ALTER DATABASE ProductCatalogDemo SET ENABLE_BROKER;").Exec();
            SqlDependency.Start(Util.Settings.ProductCatalogConnectionString);
            await c.Sql("DROP TABLE IF EXISTS dbo.d1;").Exec();
            await c.Sql("SELECT TOP 10 object_id INTO dbo.d1 FROM sys.objects;").Exec();

            // Action
            sut
                    .Sql("SELECT object_id FROM dbo.d1")
                    .OnChange(e=>isChanged=true)
                    .Map(reader => {}).Wait();
            c.Sql("DELETE FROM dbo.d1;").Exec().Wait();
            await Task.Delay(5000);
            SqlDependency.Stop(Util.Settings.ProductCatalogConnectionString);

            // Assert
            Assert.True(isChanged); 
        }

        [Fact]
        public async Task OnChangeEventHandler()
        {
            // Arrange
            IQuery sut = new QueryMapper(Util.Settings.ProductCatalogConnectionString);
            bool isChanged = false;
            ICommand c = new Belgrade.SqlClient.SqlDb.Command(Util.Settings.ProductCatalogConnectionString);
            await c.Sql("ALTER DATABASE ProductCatalogDemo SET ENABLE_BROKER;").Exec();
            SqlDependency.Start(Util.Settings.ProductCatalogConnectionString);
            await c.Sql("DROP TABLE IF EXISTS dbo.d2;").Exec();
            await c.Sql("SELECT TOP 10 object_id INTO dbo.d2 FROM sys.objects;").Exec();

            // Action
            sut
                    .Sql("SELECT object_id FROM dbo.d2")
                    .OnChange((sender, e) => { if (e.Type == SqlNotificationType.Change) isChanged = true; })
                    .Map(reader => { }).Wait();
            c.Sql("DELETE FROM dbo.d2;").Exec().Wait();
            await Task.Delay(5000);
            SqlDependency.Stop(Util.Settings.ProductCatalogConnectionString);

            // Assert
            Assert.True(isChanged);
        }
//#endif

    }
}