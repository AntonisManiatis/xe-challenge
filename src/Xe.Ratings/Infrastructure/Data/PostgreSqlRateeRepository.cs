using Dapper;

using Xe.Ratings.Domain;

namespace Xe.Ratings.Infrastructure.Data;

sealed class PostgreSqlRateeRepository : IRateeRepository
{
    static PostgreSqlRateeRepository()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    private readonly PostgreSqlConnector connector;

    public PostgreSqlRateeRepository(PostgreSqlConnector connector) =>
        this.connector = connector;

    public async Task<Ratee?> GetRateeAsync(Guid rateeId)
    {
        var connection = await connector.GetConnectionAsync();

        const string sql = @"
            SELECT r.*, ra.*
            FROM ratee r
            LEFT JOIN rating ra ON r.id = ra.ratee_id
            WHERE r.id = @Id;
        ";

        var ratees = new Dictionary<Guid, Ratee>();

        var ratee = await connection.QueryAsync<Ratee, Rating, Ratee>(
            sql,
            (ratee, rating) =>
            {
                if (rating is null)
                {
                    return ratee;
                }

                if (!ratees.TryGetValue(ratee.Id, out Ratee? entry))
                {
                    entry = ratee;
                    ratees.Add(entry.Id, entry);
                }

                entry.AddRating(rating);

                return ratee;
            },
            new { Id = rateeId },
            splitOn: "id");

        return ratees.GetValueOrDefault(rateeId);
    }

    public async Task SaveAsync(Ratee ratee)
    {
        ArgumentNullException.ThrowIfNull(ratee);

        var connection = await connector.GetConnectionAsync();

        using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(
            @"INSERT INTO ratee (id, calculated_at, total) VALUES (@Id, @CalculatedAt, @Total)
            ON CONFLICT (id)
            DO UPDATE SET calculated_at=@CalculatedAt, total=@Total",
            ratee
        );

        foreach (var rating in ratee.Ratings)
        {
            // Has already been persisted.
            if (rating.Id > 0)
            {
                continue;
            }

            rating.Id = await connection.ExecuteScalarAsync<int>(
                "INSERT INTO rating (ratee_id, rater_id, value, posted_at) VALUES (@RateeId, @RaterId, @Value, @PostedAt) RETURNING id",
                rating
            );
        }

        await transaction.CommitAsync();
    }

    public async Task<int> CleanUpAsync()
    {
        var connection = await connector.GetConnectionAsync();

        return await connection.ExecuteAsync("DELETE FROM rating WHERE posted_at < NOW() - INTERVAL '100 days';");
    }
}