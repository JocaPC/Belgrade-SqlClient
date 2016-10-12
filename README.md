# CLR-Belgrade-SqlClient 

**Belgrade Sql Client** is a lightweight data access library that supports  the latest features that are available in SQL Server 2016 and Azure SQL Database such as:
- JSON support
- Row-level security with SESSION_CONTEXT
- Built-in retry logic for In-memory Oltp stored procedures
- Built-in retry logic for some errors that require retrying queries (e.g. deadlock victime)

Function in this library use standard ADO.NET classes such as DataReader, SqlCommand. Library uses these classes in full async mode, providing optimized concurrency. There are no constraints in the term of support of the latest SQL features. Any feature that can be used with Transact-SQL can be used with this library.

Library is build in Command-Query Responsibility Segregation (CQRS) pattern. Data access classes are divided in two classes>
- Commands that will execute SQL commands that update database.
- Queries that are used to execute SQL queries that will return results to the client. Queries are divided in two categories:
 - Query Mappers that execute SQL commands and return *DataReader* object that can be mapped to standard objects.
 - Query Pipe that execute SQL commands and stream results into output stream. SQL pipes support queries that output results in JSON or XML format.
This library is used in SQL Server 2016/Azure SQL Database samples.

## Contents

[Setup](#setup)<br/>
[Initializing data access components](#init)<br/>
[Query Mapper](#query-mapper)<br/>
[Query Pipe](#query-pipe)<br/>
[Command](#command)<br/>

<a name="setup"></a>

## Setup

You can download source of this package and build you version of Belgrade Sql Client.
To install Belgrade SqlClient using NuGet, run the following command in the Package Manager Console: 
```
Install-Package Belgrade.Sql.Client 
```

<a name="init"></a>
## Initializing data access components

In order to initialize data access components, you can provide standard *SqlConnection8 as a constructor:

```javascript
const string ConnString = "Server=<SERVER NAME>;Database=<DB NAME>;Integrated Security=true";
IQueryMapper sqlMapper = new QueryMapper(new SqlConnection(ConnString));
IQueryPipe sqlPipe = new QueryPipe(new SqlConnection(ConnString));
ICommand sqlCmd = new Command(new SqlConnection(ConnString));
```
<a name="query-mapper"></a>

## Query Mappers

*QueryMapper* is data-access component that executes a query against database and maps results using mapper function. 

```javascript
await sqlMapper.ExecuteReader(command,
                    async reader =>
                    {
                        if (reader.HasRows) {
                            // Populate object using reader.
                        }
                    });
```
You can provide function that accepts *DataReader* as an argument and populate fields from *DataReader* into some object.
<a name="query-pipe"></a>
## Query Pipes

QueryPipe is data-access component that executes a query against database and stream results into an output stream. 
```javascript
await sqlQuery.Stream("select * from Product FOR JSON PATH", Response.Body, EMPTY_PRODUCTS_ARRAY);
```
Method *Stream* in *QueryPipe* class may accept two or three parameters:
- First parameter is a T-SQL query that will be executed.
- Second parameter is an output stream where results of query will be pushed. This can be response stream of web Http request, output stream that writes to file, or any other output stream.
- Third (optional) parameter is a text content that should be sent to the output stream if query does not return any results. By default, nothing will be sent to output stream if there are not results form database. Usually default content that should be sent to output stream is an empty array "[]" or empty object "{}".

<a name="command"></a>

## Command

Command is data-access component that executes a query or stored procedure that don’t return any results. Commands are used in update statements. 
```javascript
var cmd = new SqlCommand("InsertProduct");
cmd.CommandType = System.Data.CommandType.StoredProcedure;
cmd.Parameters.AddWithValue("Product", product);
await sqlCmd.ExecuteNonQuery(cmd);
```
It is just an async wrapper around standard SqlComand that handles errors and manage connection.

## Row-level security

Row-level security is a new feature that enables you to filter some rows in the tables based on some predicate.
Condition in predicates depend on data in table rows and some informaiton that identify users (e.g. current user name, role, or some value in SESSION_CONTEXT).
If you are defining secrity rules using database roles, there you don't need any change in your app. However, if you want to put user id in SQL SESSION_CONTEXT
variable, you need to create some logic that populates this value.

First you need to define some function that will return value that identifies user.
Here is an example of C# function that get CompanyId from an ASP.NET Session:

```javascript
	services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
	Func<IServiceProvider, string> GetCompanyID = (serviceProvider =>
	{
		var session = serviceProvider.GetServices<IHttpContextAccessor>().First().HttpContext.Session;
		return session.GetString("CompanyID") ?? "-1";
	});
```
Once you define a function that will provide a value that represents identity of the user, you can create a wrapper 
that will get this value on every execution of SQL command, write value from the ASP.NET Session into SQL SESSION_CONTEXT:

```javascript
	services.AddTransient<IQueryPipe>(
		sp =>
		{
			return new QueryPipeSessionContextAdapter(
				new QueryPipe(new SqlConnection(ConnString)), "CompanyID", () => GetCompanyID(sp));
		});

	services.AddTransient<ICommand>(
		sp =>
		{
			return new CommandSessionContextAdapter(
				new Command(new SqlConnection(ConnString)), "CompanyID", () => GetCompanyID(sp));
		});
```
**QueryPipeSessionContextAdapter** and **CommandSessionContextAdapter** are wrappers around standard QueryPipe and Command classes that wput provided value in 
session context, execute underlying statement, and clean session context variable:

```
EXEC sp_set_session_context @{KEYNAME}, @{VALUE};
{command.CommandText}
EXEC sp_set_session_context @{KEYNAME}, NULL;
```

KEYNAME is second parameter provided to SessioncontextAdapter constructor, while VALUE is a value returned by function provided as third parameter.


