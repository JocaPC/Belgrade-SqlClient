# Belgrade SqlClient in ASP.NET applications

**Belgrade SqlClient** can be used as a data-access framework in ASP.NET, including ASP.NET Core.

## Contents

[Initializing data access service](#init)<br/>
[Initialize controllers](#controller)<br/>
[Executing commands](#exec)<br/>
[Handling errors and logging](#error-log)<br/>

<a name="init"></a>

## Initializing data access service

To add **Belgrade SqlClient** to your **ASP.NET** project, run the following command in the **Package Manager Console**: 
```
Install-Package Belgrade.Sql.Client 
```

Then, you need to add `Command` service to your aplication using standard Dependency Injection mechanism. 
Update `ConfigureService` method in `Startup` class and add `Command` service that is initialized with connection string:

```
using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using System.Data.SqlClient;

public class Startup
{
    // This method gets called by the runtime.
    // Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        string Conn = Configuration["ConnectionStrings:MyDatabase"];
        services.AddTransient<ICommand>(_ => new Command(Conn));

        // Add framework services.
        services.AddMvc();
    }

}
```

<a name="controller"></a>
# Initialize controller

Once you add `Command` service using dependency injection, you can add `Command` service to your controllers using standard constructor injection:

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

Once you initialize controller and associate `Command` service to the controller, you can execute `T-SQL` queries and get the data.

```

// GET api/Product
[HttpGet]
public async Task Get()
{
    await cmd
        .Sql(
@"select ProductID, Name, Color, Price, Quantity 
from Product
FOR JSON PATH")
        .Stream(Response.Body);
}

// POST api/Product
[HttpPost]
public async Task Post(string product, float price)
{
    await sqlCmd
        .Proc("InsertProduct")
        .Param("Product", product)
        .Param("Price", price)
        .Exec();
}
```
**Belgrade SqlClient** is a wrapper on standard ADO.NET classes and functions, so you can either use helper methods or create and initialize classic ADO.NET Sql Command and provide to to **Belgrade SqlClient** that will execute it:
```
// POST api/Product
[HttpPost]
public async Task Post(string product)
{
    var sqlCmd = new SqlCommand("InsertProduct");
    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
    sqlCmd.Parameters.AddWithValue("Product", product);
    await cmd.Exec(sqlCmd);
}
```

<a name="error-log"></a>
## Handling errors and logging

**Belgrade SqlClient** enables you to get the potential errors that are thrown while trying to access database and to send log messages to your logging classes:

```
// POST api/Product
[HttpDelete]
public async Task Delete(int productId)
{
    await sqlCmd
        .Proc("DeleteProduct")
        .Param("ProductID", productId)
        .AddLogger(this._logger) // Works with Common.Logging.ILog
        .OnError( ex => /* do something with exception */ )
        .Exec();
}
```

Currently, **Belgrade SqlClient** works only with `Common.Logging` interface.
## See also

 - [Home](index.md)
 - [Using Belgrade SqlClient in CQRS archiectures](cqrs.md)