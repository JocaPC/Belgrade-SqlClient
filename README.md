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

[Initializing data access components](#init)<br/>
[Query Mapper](#query-mapper)<br/>
[Query Pipe](#query-pipe)<br/>
[Command](#command)<br/>



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
