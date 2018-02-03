# CLR-Belgrade-SqlClient 

**Belgrade Sql Client** is a lightweight data access library that supports the latest features that are available in SQL Server 2016 and Azure SQL Database such as:
- JSON support
- Row-level security with SESSION_CONTEXT
- Built-in retry logic for In-memory Oltp stored procedures
- Built-in retry logic for some errors that require retrying queries (e.g. deadlock victims)
- Read scale-out

Functions in this library use standard ADO.NET classes such as DataReader and SqlCommand guaranteeng the best performance. Library uses these classes in full async mode, providing optimized concurrency. There are no constraints in the term of support of the latest SQL features. Any feature that can be used with Transact-SQL can be used with this library.

## Contents

[Setup](#setup)<br/>
[Command](#setup)<br/>
[Initializing data access components](#init)<br/>
[Query Mapper](#map)<br/>
[Streaming results](#stream)<br/>
[Executing commands](#exec)<br/>

<a name="setup"></a>

## Setup

You can download source of this package and build your version of Belgrade Sql Client.
To install Belgrade SqlClient using NuGet, run the following command in the Package Manager Console: 
```
Install-Package Belgrade.Sql.Client 
```

<a name="command"></a>
# Command

Command object is a core component in Belgrade.Sql.Client. Every T-SQL query that you execute is one command.
Command object has three methods:
 - Map() that executed a SQL command and provides results to a callback. Use this method to map query results to a list of objects.
 - Exec() that executes a SQL command that don't returns any results (e.g. INSERT, UPDATE, DELETE)
 - Stream() that executes a SQL command that returns chunked response. Examples are queries with FOR JSON and FOR XML clauses.


<a name="init"></a>
## Initializing data access components

In order to initialize data access components, you can provide standard *SqlConnection* to a constructor of Command object:

```
const string ConnString = "Server=<SERVER NAME>;Database=<DB NAME>;Integrated Security=true";
ICommand cmd = new Command(new SqlConnection(ConnString));
```

<a name="map"></a>

## Query Mappers

*Map* method executed a T-SQL query and executes a callback for every row returned by a data reader: 

```
await cmd.Map(command, row => { /* Populate object using reader */ });
```
You can provide a callback function that accepts *DataReader* as an argument and populates fields from *DataReader* into some object or collection of objects.

<a name="stream"></a>
## Streaming results

**Stream** is method that executes a query against a database and stream results into an output stream. 
```
await cmd.Stream("SELECT * FROM Product FOR JSON PATH", Response.Body);
```
Method *Stream* may accept two or three parameters:
- First parameter is a T-SQL query that will be executed.
- Second parameter is an output stream where results of the query will be pushed. This can be response stream of web Http request, output stream that writes to file, or any other output stream.
- Third (optional) parameter is a text content that should be sent to the output stream if query does not return any results. By default, "[]" will be sent to the output stream if there are no results from database.

<a name="exec"></a>

## Executing commands

**Exec** is method that executes a query or stored procedure that don't return any results. Exec() method is used in insert, update, and delete statements. 
```
var cmd = new SqlCommand("InsertProduct");
cmd.CommandType = System.Data.CommandType.StoredProcedure;
cmd.Parameters.AddWithValue("Product", product);
await sqlCmd.Exec(cmd);
```
It is just an async wrapper around standard SqlComand that handles errors and manages connection state.
