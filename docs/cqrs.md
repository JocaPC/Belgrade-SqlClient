# Command-Query Responsibility Segregation

**Belgrade Sql Client** is follows a Command-Query Responsibility Segregation (CQRS) pattern. Data access classes are divided in two classes:
- Commands that will execute SQL commands that update database.
- Queries that are used to execute SQL queries that will return results to the client. Queries are divided in two categories:
 - Query Mappers that execute SQL commands and return *DataReader* object that can be mapped to standard objects.
 - Query Pipe that execute SQL commands and stream results into output stream. SQL pipes support queries that output results in JSON or XML format.
This library is used in SQL Server 2016/Azure SQL Database samples.

## Contents

[Query Mapper](#query-mapper)<br/>
[Query Pipe](#query-pipe)<br/>
[Command](#command)<br/>

<a name="init"></a>
## Initializing data access components

In order to initialize data access components, you can provide standard *SqlConnection* as a constructor:

```javascript
const string ConnString = "Server=<SERVER NAME>;Database=<DB NAME>;Integrated Security=true";
IQueryMapper sqlMapper = new QueryMapper(new SqlConnection(ConnString));
IQueryPipe sqlPipe = new QueryPipe(new SqlConnection(ConnString));
ICommand sqlCmd = new Command(new SqlConnection(ConnString));
```
<a name="query-mapper"></a>

## Query Mappers

*QueryMapper* is data-access component that executes a query against database and maps results using mapper function. 

```
await sqlMapper.ExecuteReader(command, row => { /* Populate an object from the row */ });
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

Command is data-access component that executes a query or stored procedure that don't return any results. Commands are used in update statements. 
```javascript
var cmd = new SqlCommand("InsertProduct");
cmd.CommandType = System.Data.CommandType.StoredProcedure;
cmd.Parameters.AddWithValue("Product", product);
await sqlCmd.ExecuteNonQuery(cmd);
```
It is just an async wrapper around standard SqlComand that handles errors and manage connection.
