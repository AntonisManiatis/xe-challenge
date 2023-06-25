using System.Security.Claims;
namespace Xe.Ratings.API;

using System;

using ErrorOr;

using Xe.Ratings.Service;

using static Results;

record RatingRequest(
    Guid RateeId,
    float Value
);

static class Ratings
{
    internal static void MapRatings(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/ratings")
            .AllowAnonymous();

        group.MapGet("/{id}", GetRating); //.CacheOutput("Ratings"); // Uncomment to enable caching on the HTTP level.
        group.MapPost("/", PostRating);
    }

    internal static async Task<IResult> GetRating(Guid id, IRatingService ratingService)
    {
        var rating = await ratingService.GetRatingForAsync(id);

        static IResult ToResult(Error error) => error.Type switch
        {
            ErrorType.NotFound => NotFound(),
            _ => StatusCode(500)
        };

        return rating.MatchFirst(
            rating => Ok(rating),
            ToResult
        );
    }

    internal static async Task<IResult> PostRating(
        RatingRequest req,
        IRatingService ratingService,
        ClaimsPrincipal user
    )
    {
        var command = new RateCommand(
            req.RateeId,
            req.Value,
            user.Identity!.IsAuthenticated ? Guid.NewGuid() : null
        );

        var result = await ratingService.PostRatingAsync(command);

        static IResult ToResult(Error error) => error.Type switch
        {
            ErrorType.NotFound => NotFound(),
            ErrorType.Validation => BadRequest(error.Description),
            _ => StatusCode(500)
        };

        return result.MatchFirst(
            rating => Created($"/ratings/{req.RateeId}", rating),
            ToResult
        );
    }
}