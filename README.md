# Belgrade SqlClient 

**Belgrade Sql Client** is a lightweight data access library that supports the latest features that are available in SQL Server and Azure SQL Database such as:
- JSON support
- Row-level security with SESSION_CONTEXT
- Built-in retry logic for In-memory Oltp stored procedures
- Built-in retry logic for some errors that require retrying queries (e.g. deadlock victims)

Functions in this library use standard ADO.NET classes such as `DataReader` and `SqlCommand`. Library uses these classes in **full async mode**, providing optimized concurrency. There are no constraints in the term of support of the latest SQL features. Any feature that can be used with Transact-SQL can be used with this library.

This library is used in SQL Server 2016+/Azure SQL Database Sql Server GitHub samples.

## Contents

[Setup](#setup)<br/>
[Initializing data access components](#init)<br/>
[Executing command](#command)<br/>
[Mapping results](#query-mapper)<br/>
[Streaming results](#query-pipe)<br/>

<a name="setup"></a>

## Setup

You can download source of this package and build your own version of Belgrade Sql Client, or take the library from NuGet.
To install Belgrade SqlClient using NuGet, run the following command in the Package Manager Console: 
```
Install-Package Belgrade.Sql.Client 
```

<a name="init"></a>
## Initializing data access components

The core component in this library is `Command`. `Command` is object that executes any T-SQL command or query.
In order to initialize `Command`, you can provide standard `SqlConnection` to the constructor:

```
const string ConnString = "Server=<SERVER>;Database=<DB>;Integrated Security=true";
ICommand cmd = new Command(ConnString);
```
Now you have a fully functional object that you can use to execute queries.

<a name="command"></a>

## Executing commands

Command has `Exec` method that executes a query or stored procedure that don't return any results. `Exec()` is used in update statements, for example:
```
await cmd.Sql("EXEC dbo.CalculateResults").Exec();
```
This command will open a connection, execute the procedure, and close the connection when it finishes.

Usually you would need to pass the parameters to some stored procedure when you execute it. You can set the T-SQL query text, add parameters to the command, and execute it.

```
cmd.Sql("EXEC UpdateProduct @Id, @Product");
cmd.Param("Id", DbType.Int32, id);
cmd.Param("Product", DbType.String, product);
await cmd.Exec();
```
It is just an async wrapper around standard SqlComand that handles errors and manage connection.

<a name="query-mapper"></a>

## Mapping query results

*Map()* method enables you to execute any T-SQL query and get results in callback method: 

```
await cmd
        .Sql("SELECT * FROM sys.objects")
        .Map(row => {
                    	// Populate some C# object from the row data.
                    });
```
You can provide a function that accepts `DataReader` as an argument and populates fields from `DataReader` into some object or list.
<a name="query-pipe"></a>

## Streaming results

`Stream` is a method that executes a query against database and stream the results into an output stream. 
```
await cmd
        .Sql("SELECT * FROM Product FOR JSON PATH")
        .Stream(Response.Body);
```

Method `Stream` may accept following parameters:
- First parameter is an output stream where results of query will be pushed. This can be response stream of web Http request, output stream that writes to a file, or any other output stream.
- Second (optional) parameter is a text content that should be sent to the output stream if query does not return any results. By default, nothing will be sent to output stream if there are not results form database. Usually default content that should be sent to output stream is an empty array "[]" or empty object "{}".

## More info

**Belgrade SqlClient** has some additional functionalities such as error handling, logging, retrying errors, etc. You can find more informaiton about other features in <a href="http://jocapc.github.io/Belgrade-SqlClient/">documentation</a>.