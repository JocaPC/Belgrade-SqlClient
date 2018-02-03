# Belgrade SqlClient in ASP.NET applications

**Belgrade Sql Client** can be used as a data access frameworks in ASP.NET, including ASP.NET Core.

## Contents

[Initializing data access service](#init)<br/>
[Initialize controllers](#controller)<br/>
[Executing commands](#exec)<br/>

<a name="init"></a>

## Initializing data access service

To add Belgrade SqlClient to your ASP.NET project, run the following command in the Package Manager Console: 
```
Install-Package Belgrade.Sql.Client 
```

Then, you need to add Command service to your aplication using standard Dependency Injection mechanism. 
Update ConfigureService method in Startup class and add Command service:

```
using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using System.Data.SqlClient;

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string ConnString = Configuration["ConnectionStrings:MyConnectionString"];
            services.AddTransient<ICommand>(_ => new Command(new SqlConnection(ConnString)));

            // Add framework services.
            services.AddMvc();
        }

    }
```

<a name="controller"></a>
# Initialize controller

Once you add Command service using dependency injection, you can add Command service to your controllers using standard constructor injection:

```
using Belgrade.SqlClient;

    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        ICommand cmd = null;

        public ProductController(ICommand sqlCommandService)
        {
            this.cmd = sqlCommandService;
        }

    }
```

<a name="exec"></a>
# Executing commands

Once you initialize controller and associate Command service to the controller, you can execute T-SQL queries and get the data.

```

        // GET api/Product
        [HttpGet]
        public async Task Get()
        {
            await cmd.Stream(
@"select ProductID, Name, Color, Price, Quantity 
from Product
FOR JSON PATH", Response.Body);
        }

        // POST api/Product
        [HttpPost]
        public async Task Post(string Product)
        {
            var sqlCmd = new SqlCommand("InsertProduct");
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("Product", product);
            await cmd.Exec(sqlCmd);
        }
```