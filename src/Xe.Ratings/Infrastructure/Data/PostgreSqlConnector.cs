using System.Data;

using Npgsql;

namespace Xe.Ratings.Infrastructure.Data;

sealed class PostgreSqlConnector : IDisposable, IAsyncDisposable
{
    private readonly NpgsqlConnection connection;

    public PostgreSqlConnector(string connectionString) =>
        connection = new NpgsqlConnection(connectionString);

    public async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        return connection;
    }

    public void Dispose() => connection.Dispose();

    public ValueTask DisposeAsync() => connection.DisposeAsync();
}