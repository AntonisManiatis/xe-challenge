namespace Xe.Ratings.Service;

using ErrorOr;

using Microsoft.Extensions.Logging;

using Xe.Ratings.Domain;

using RatingEntity = Domain.Rating;

public record RateCommand(
    Guid RateeId,
    float Value,
    Guid? RaterId = null
);

public record Rating(
    int Id,
    Guid RateeId,
    float Value,
    DateTime PostedAt
);

public interface IRatingService
{
    Task<ErrorOr<double>> GetRatingForAsync(Guid rateeId);

    Task<ErrorOr<Rating>> PostRatingAsync(RateCommand rateCommand);

    Task CleanUpAsync();
}

static class RatingEntityExtensions
{
    internal static Rating ToDto(this RatingEntity entity) =>
        new(entity.Id, entity.RateeId, entity.Value, entity.PostedAt);
}

sealed class RatingService : IRatingService
{
    private readonly ILogger<RatingService> logger;
    private readonly IRateeRepository rateeRepository;

    public RatingService(
        ILogger<RatingService> logger,
        IRateeRepository rateeRepository
    )
    {
        this.logger = logger;
        this.rateeRepository = rateeRepository;
    }

    public async Task<ErrorOr<double>> GetRatingForAsync(Guid rateeId)
    {
        var ratee = await rateeRepository.GetRateeAsync(rateeId);
        if (ratee is null)
        {
            return Errors.RateeNotFound;
        }

        if (ratee.Ratings.Count == 0)
        {
            return Errors.NoRatingsFound;
        }

        return ratee.Total;
    }

    public async Task<ErrorOr<Rating>> PostRatingAsync(RateCommand rateCommand)
    {
        var errors = Validate(rateCommand);
        if (errors.Any())
        {
            return errors;
        }

        var ratee = await rateeRepository.GetRateeAsync(rateCommand.RateeId)
            ?? new Ratee { Id = rateCommand.RateeId }; // Let's create it on the spot, using the given ID.

        var now = DateTime.UtcNow;
        var rating = new RatingEntity
        {
            RateeId = rateCommand.RateeId,
            RaterId = rateCommand.RaterId,
            Value = rateCommand.Value,
            PostedAt = now
        };

        ratee.AddRating(rating);

        var (count, overall) = RatingCalculator.Calculate(ratee.Ratings);

        // ? persist `count` also?
        ratee.Total = overall;
        ratee.CalculatedAt = now;

        await rateeRepository.SaveAsync(ratee);

        return rating.ToDto();
    }

    private static List<Error> Validate(RateCommand command)
    {
        var errors = new List<Error>();
        if (command.RateeId == default)
        {
            errors.Add(Errors.InvalidRateeId);
        }

        const float increment = 0.5f;
        const float tolerance = 0.00001f;

        static bool IsMultipleOfHalf(float rating)
        {
            float remainder = Math.Abs(rating % increment);
            return remainder <= tolerance || Math.Abs(remainder - increment) <= tolerance;
        }

        var rating = command.Value;
        if (!IsMultipleOfHalf(rating))
        {
            errors.Add(Errors.InvalidRatingValue);
        }

        if (rating < 0 || rating > 5)
        {
            errors.Add(Errors.InvalidRatingRange);
        }

        return errors;
    }

    public async Task CleanUpAsync()
    {
        var affected = await rateeRepository.CleanUpAsync();
        if (affected > 0)
        {
            logger.LogInformation("Cleaned up {} old ratings.", affected);
        }
        else
        {
            logger.LogDebug("No old entries to clean up.");
        }
    }
}