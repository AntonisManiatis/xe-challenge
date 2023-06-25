namespace Xe.Ratings.Domain;

public static class RatingCalculator
{
    private const int TotalRatingDays = 100;
    private const float MaxRating = 5.0f;
    private const float AnonymousRatingWeight = 0.1f;
    private const float KnownUserRatingWeight = 1;
    private const int DigitsKept = 1;

    public static float CalculateWeightFor(Rating rating, int daysAgo)
    {
        var linearFactor = rating.Value / MaxRating;
        var ageFactor = 1 - (daysAgo / 100f);
        var presenceFactor = rating.RaterId.HasValue ? KnownUserRatingWeight : AnonymousRatingWeight;

        return 100 * linearFactor * ageFactor * presenceFactor;
    }

    public static (int ratingsUsed, double overallRating) Calculate(
        IList<Rating> ratings,
        Func<DateTime>? dateTimeProvider = null)
    {
        var today = dateTimeProvider is null ? DateTime.UtcNow : dateTimeProvider();

        var latestRatings = ratings.Where(rating => (today - rating.PostedAt).TotalDays <= TotalRatingDays);
        var count = latestRatings.Count();
        if (count == 0)
        {
            return (0, 0f);
        }

        var totalWeight = latestRatings.Select(rating =>
        {
            var daysAgo = (today - rating.PostedAt).TotalDays;
            return CalculateWeightFor(rating, (int)daysAgo);
        }).Sum();

        var overall = Math.Round(totalWeight / count / 20, DigitsKept);
        return (count, overall);
    }
}