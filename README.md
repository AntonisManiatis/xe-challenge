# XE coding challenge.

Solution to Xrysi Eukeria's ratings coding challenge.

## Getting started

### Prerequisites

- [.NET 7](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/products/docker-desktop/)

To start Postgres:

```
docker-compose up
```
and to start the api:

```
dotnet run --project .\src\Xe.Ratings.API\
```

To simulate signed-in users the API is checking for the presence of a valid JWT tokens.

To generate one:

```
cd .\src\Xe.Ratings.API\ && dotnet user-jwts create
```

To run the tests (requires Docker):

```
dotnet test
```
If you're using [Thunder Client](https://www.thunderclient.com/) there's also an HTTP request collection in `./thunder-tests`

## Libraries used

- [ErrorOr](https://www.nuget.org/packages/ErrorOr): A discriminated union of an error or a result.
- [Npgsql](https://www.nuget.org/packages/Npgsql/7.0.4): Data access for PostgreSql.
- [Dapper](https://www.nuget.org/packages/Dapper): A Micro-ORM.
- [Xunit](https://www.nuget.org/packages/xunit/2.5.0-pre.26): for testing.
- [Testcontainers.PostgreSql](https://www.nuget.org/packages/Testcontainers.PostgreSql): Docker Testcontainer for PostgreSql.