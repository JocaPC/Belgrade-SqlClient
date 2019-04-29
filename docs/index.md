# Belgrade SqlClient Data Access library

**Belgrade SqlClient** is a lightweight data access library that wraps standard ADO.NET classes. It enables you to write .Net code where
you need to write **one C# statement** to execute **one T-SQL statement** without need to deal with try/catch blocks, opening closing connections, etc. You can see how easily you can execute a T-SQL command in the following example: 
```
await sqlCmd.Sql("UPDATE Products SET Price = Price * 1.1;").Exec();
```
You just need to specify what T-SQL query you want to execute, and then just execute it. Also, note the `await` keyword - all methods in **Belgrade SqlClient** are async because there is no need to block your app while waiting for the T-SQL query to execute. This best practice is built-in into this library.

If you ever wanted to execute T-SQL queries from the C# code the same way as you execute LINQ queries, this might be the library for you. It wraps all complexity of connection state management, and enables you to write one line of code to execute the query.

> This libary is a utility library that uses the same ADO.NET classes that you always use. The additional value that is brings is automatic opening/closing connections, closing connection when the query is completed, etc. Also, it uses async methods for data access such as `OpenAsync`, `ExecuteNonQueryAsync`, `ExecuteReaderAsync`, etc. providing the best concurrency in the .Net client code. This library also solves some common developer data access mistakes that could happen in your data access code. 

**Why would you use this library?**

This library is designed for **.Net application developers** who extensively use **T-SQL language** to access database, but need to have some utility/helper classes to execute queries. It can be good choice for developers who use other frameworks for data access but need to quickly execute some query without writing custom code that opens/closes connections and handles the errors.
This library enables you to use all T-SQL language elements (for example window aggregates) including the latest features that are available in SQL Server 2016+ and Azure SQL Database such as:
- JSON support
- Temporal querying syntax
- Row-level security with SESSION_CONTEXT
- Graph support
- New T-SQL language features such as STRING_AGG
- Read scale-out

Functions in this library use standard ADO.NET classes such as `DataReader` and `SqlCommand` guaranteeing the best performance. Library uses these classes in **full async mode**, providing optimized concurrency. There are no constraints in the term of support of the latest SQL features. Any feature that can be used with Transact-SQL can be used with this library.

## Contents

[Setup](#setup)<br/>
[Command](#command)<br/>
[Initializing data access components](#init)<br/>
[Query Mapper](#map)<br/>
[Streaming results](#stream)<br/>
[Executing commands](#exec)<br/>
[Logging and error handling](#error-log)<br/>

<a name="setup"></a>

## Setup

You can download source of this package and build your version of **Belgrade SqlClient** <a href="https://github.com/JocaPC/Belgrade-SqlClient">from GitHub</a>.
To install **Belgrade SqlClient** using <a href="https://www.nuget.org/packages/Belgrade.Sql.Client/>">NuGet</a>, run the following command in the Package Manager Console: 
```
Install-Package Belgrade.Sql.Client 
```

<a name="command"></a>
# Command

`Command` object is the core component in **Belgrade SqlClient**. Every T-SQL query that you execute is one command.
`Command` object has three methods:
 - Map() that executed a SQL command and provides results to a callback. Use this method to map query results to a list of objects.
 - Exec() that executes a SQL command that don't returns any results (e.g. INSERT, UPDATE, DELETE)
 - Stream() that executes a SQL command that returns chunked response. Examples are queries with FOR JSON and FOR XML clauses.


<a name="init"></a>
## Initializing data access components

In order to initialize data access components, you need to provide connection string to the constructor of `Command` object:

```
var ConnString = "Server=<SERVER>;Database=<DB>;Integrated Security=true";
ICommand cmd = new Command(ConnString);
```

Once you provide the connection string to the `Command`, you can execute any query. As an alternative, you can provide `SqlConnection` object to the constructor, something like `new Command(new SqlConnection(ConnString))`.

<a name="map"></a>

## Query Mappers

`Map` method executes a T-SQL query and executes a callback for every row returned by a data reader: 

```
await cmd
        .Sql(command)
        .Map(row => { /* Populate object using reader */ });
```
You can provide a callback function that accepts `DataReader` as an argument and populates fields from `DataReader` into some object or collection of objects. in order to access values returned by query, you can use indexer with the column names (for example, `row["ID"]`, `row["Name"]`) in the body of callback.

<a name="stream"></a>
## Streaming results

`Stream` is a method that executes a query against a database and streams results into an output stream. 
```
await cmd
        .Sql("SELECT * FROM Product FOR JSON PATH")
        .Stream(Response.Body);
```
Method `Stream` may accept following parameters:
- First parameter is  an output stream where results of the query will be pushed. This can be response stream of web Http request, output stream that writes to file, or any other output stream.
- Second (optional) parameter is a text content that should be sent to the output stream if query does not return any results. By default, "[]" will be sent to the output stream if there are no results from the database.

<a name="exec"></a>

## Executing commands

`Exec` is method that executes a query or stored procedure that don't return any results. `Exec` method can be used in insert, update, and delete statements. 
```
await sqlCmd
        .Proc("InsertProduct")
        .Param("Product", product)
        .Exec();
```
It is just an async wrapper around standard `SqlCommand` that handles errors and manages connection state.

<a name="error-log"></a>
## Handling errors and logging

**Belgrade SqlClient** enables you to get the potential errors that are thrown while trying to access database and to send log messages to your logging classes:

```
ILog logger = logManaged.GetLogger<MyClass>();
await sqlCmd
    .Proc("DeleteProduct")
    .Param("ProductID", productId)
    .AddLogger( logger ) // Works with Common.Logging.ILog
    .OnError( ex => /* do something with exception */ )
    .Exec();
```
Currently, **Belgrade SqlClient** works only with `Common.Logging` interface.

## Using the library
You can use this library without any restriction since it is licenses under <a href="https://github.com/JocaPC/Belgrade-SqlClient/blob/master/license.txt">MIT license</a>.

This library is used in several <a href="https://github.com/Microsoft/sql-server-samples/tree/master/samples/features/json">SQL Server GitHub Samples</a> so there you can find how to use it. You can also find different examples of usage in <a href="https://github.com/JocaPC/Belgrade-SqlClient/tree/master/Test">Test</a> project. There are 7500 test cases used to test this library, so you can find various usage scenarios in tests.
Feel free to report any issue on <a href="https://github.com/JocaPC/Belgrade-SqlClient/issues">GitHub Issues</a> or send a Pull Request if you want to correct some issue or update documentation or tests.

## See also

 - [Using Belgrade SqlClient in ASP.NET](aspnet.md)
 - [Using Belgrade SqlClient in CQRS archiectures](cqrs.md)
