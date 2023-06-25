using Microsoft.Extensions.DependencyInjection;

using Xe.Ratings.Domain;
using Xe.Ratings.Infrastructure.Data;
using Xe.Ratings.Service;

namespace Xe.Ratings.Infrastructure;

public static class RatingsServiceCollectionExtensions
{
    public static IServiceCollection AddRatings(this IServiceCollection services, string connectionString)
    {
        services.AddScoped(_ => new PostgreSqlConnector(connectionString));
        services.AddScoped<IRateeRepository, PostgreSqlRateeRepository>();

        services.AddScoped<IRatingService, RatingService>();
        return services;
    }
}