#pragma warning disable IDE1006 // Disables the capital initial letter name for methods.

namespace Xe.Ratings.UnitTests;

using Xe.Ratings.Domain;

public class A_rating_is_excluded_from_the_calculation_if
{
    [Fact]
    public void it_is_older_than_100_days_old()
    {
        // Arrange
        var ratings = new List<Rating>
        {
            new Rating { Id = 1, Value = 4.5f, PostedAt = DateTime.UtcNow, RateeId = Guid.NewGuid() },
            new Rating { Id = 2, Value = 4.5f, PostedAt = DateTime.UtcNow.AddDays(-100), RateeId = Guid.NewGuid() }
        };

        // Act
        var (ratingsUsed, _) = RatingCalculator.Calculate(ratings);

        // Assert
        Assert.Equal(1, ratingsUsed);
    }
}

public class An_individual_ratings_weight_is_affected_by
{
    public static IEnumerable<object[]> RatingData =>
        new List<object[]>
        {
            new object[] { 4.5f, 9 },
            new object[] { 3.5f, 7 },
            new object[] { 2.0f, 4 },
        };

    [Theory]
    [MemberData(nameof(RatingData))]
    public void the_actual_value_of_the_rating(float val, int expectedWeight)
    {
        // Arrange
        var rating = new Rating { Value = val };
        const int daysAgo = 0; // all posted today.

        // Act
        var weightedRating = RatingCalculator.CalculateWeightFor(rating, daysAgo);

        // Assert
        Assert.Equal(expectedWeight, Math.Round(weightedRating, 1));
    }

    public static IEnumerable<object[]> AgeData =>
        new List<object[]>
        {
            new object[] { 5f, DateTime.UtcNow.AddDays(-10), 9.0 },
            new object[] { 5f, DateTime.UtcNow.AddDays(-25), 7.5 },
            new object[] { 5f, DateTime.UtcNow.AddDays(-50), 5.0 },
        };

    [Theory]
    [MemberData(nameof(AgeData))]
    public void the_age_of_the_rating(float ratingVal, DateTime postedAt, double expectedWeight)
    {
        // Arrange
        var rating = new Rating { Value = ratingVal, PostedAt = postedAt };
        var today = DateTime.UtcNow;
        var daysAgo = (today - rating.PostedAt).TotalDays;

        // Act
        var weightedRating = RatingCalculator.CalculateWeightFor(rating, (int)daysAgo);

        // Assert
        Assert.Equal(expectedWeight, Math.Round(weightedRating, 1));
    }

    public static IEnumerable<object[]> PresenceData =>
        new List<object[]>
        {
            new object[] { 4.5f, true, 9 },
            new object[] { 4.5f, false, 90 },
        };

    [Theory]
    [MemberData(nameof(PresenceData))]
    public void if_the_rating_is_posted_by_an_anonymous_user(float val, bool anonymous, int expectedWeight)
    {
        // Arrange
        var rating = new Rating { Value = val, RaterId = anonymous ? null : Guid.NewGuid() };
        const int daysAgo = 0;

        // Act
        var weightedRating = RatingCalculator.CalculateWeightFor(rating, daysAgo);

        // Assert
        Assert.Equal(expectedWeight, Math.Round(weightedRating, 1));
    }
}

public class A_calculated_overall_rating
{
    [Fact]
    public void is_zero_if_no_ratings_are_provided()
    {
        // Arrange
        var ratings = new List<Rating>();

        // Act
        var (ratingsUsed, overall) = RatingCalculator.Calculate(ratings);

        // Assert
        Assert.Equal(0, ratingsUsed);
        Assert.Equal(0, overall);
    }

    [Fact]
    public void is_expected_to_be_a_single_digit_number() // TODO: Name
    {
        // Arrange
        var ratings = new List<Rating>
        {
            new Rating { Value = 4.5f, PostedAt = DateTime.Parse("2020/11/04 11:20:00"), RaterId = null },
            new Rating { Value = 5f, PostedAt = DateTime.Parse("2020/10/10 23:10:00"), RaterId = Guid.NewGuid() },
            new Rating { Value = 1.5f, PostedAt = DateTime.Parse("2020/09/25 09:50:00"), RaterId = Guid.NewGuid() },
            new Rating { Value = 3.5f, PostedAt = DateTime.Parse("2020/07/15 09:20:00"), RateeId = Guid.NewGuid() }
        };

        // Act
        var (ratingsUsed, overall) = RatingCalculator.Calculate(ratings, () => DateTime.Parse("2020/11/04"));

        // Assert
        Assert.Equal(3, ratingsUsed);
        Assert.Equal(1.7, overall);
    }

    // TODO: Add edge case test to see if it can exceed 5.0?
}