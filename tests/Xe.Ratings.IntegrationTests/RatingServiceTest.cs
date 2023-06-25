#pragma warning disable IDE1006 // Disables the capital initial letter name for methods.

using Microsoft.Extensions.DependencyInjection;

using Xe.Ratings.Service;

namespace Xe.Ratings.IntegrationTests;

[Collection("Ratings")]
public class A_rating_is_invalid_if
{
    private readonly RatingsFixture fixture;

    public A_rating_is_invalid_if(RatingsFixture fixture) =>
        this.fixture = fixture;

    [Fact]
    public async Task it_does_not_have_a_RateeId()
    {
        // Arrange
        var scope = fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRatingService>();

        var invalidRateeId = Guid.Empty;

        // Act
        var result = await service.PostRatingAsync(new RateCommand(invalidRateeId, 4.5f));

        // Assert
        Assert.Contains(Errors.InvalidRateeId, result.Errors);
    }

    [Fact]
    public async Task its_value_is_not_valid()
    {
        // Arrange
        var scope = fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRatingService>();

        const float invalidRating = 4.3f;

        // Act
        var result = await service.PostRatingAsync(new RateCommand(Guid.NewGuid(), invalidRating));

        // Assert
        Assert.Contains(Errors.InvalidRatingValue, result.Errors);
    }

    [Fact]
    public async Task its_not_in_a_valid_range()
    {
        // Arrange
        var scope = fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRatingService>();

        const float invalidRatingRange = 6f;

        // Act
        var result = await service.PostRatingAsync(new RateCommand(Guid.NewGuid(), invalidRatingRange));

        // Assert
        Assert.Contains(Errors.InvalidRatingRange, result.Errors);
    }
}

[Collection("Ratings")]
public class Adding_a_rating_successfully_returns
{
    private readonly RatingsFixture fixture;

    public Adding_a_rating_successfully_returns(RatingsFixture fixture) =>
        this.fixture = fixture;

    [Fact]
    public async Task an_updated_rating()
    {
        // Arrange
        var scope = fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRatingService>();

        var rateeId = Guid.NewGuid();
        var raterId = Guid.NewGuid();

        // Act
        var result = await service.PostRatingAsync(new RateCommand(rateeId, 4.5f, raterId));

        // Assert
        // ! Doing a full comparison is tricky, but if we get assigned an Id it is safe to assume that the entity is persisted.
        Assert.True(result.Value.Id > 0);
    }

    [Fact]
    public async Task an_overall_rating_for_the_given_ratee()
    {
        // Arrange
        var scope = fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRatingService>();

        var rateeId = Guid.NewGuid();
        var raterId = Guid.NewGuid();

        _ = await service.PostRatingAsync(new RateCommand(rateeId, 4.5f, raterId));

        // Act
        var result = await service.GetRatingForAsync(rateeId);

        // Assert
        Assert.Equal(4.5f, result.Value);
    }
}

[Collection("Ratings")]
public class Retrieving_the_overall_rating_for_a_ratee_returns_an_error_if
{
    private readonly RatingsFixture fixture;

    public Retrieving_the_overall_rating_for_a_ratee_returns_an_error_if(RatingsFixture fixture) =>
        this.fixture = fixture;

    [Fact]
    public async Task the_ratee_does_not_exist()
    {
        // Arrange
        var scope = fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRatingService>();

        var nonExistantRateeId = Guid.NewGuid();

        // Act
        var result = await service.GetRatingForAsync(nonExistantRateeId);

        // Assert
        Assert.Contains(Errors.RateeNotFound, result.Errors);
    }
}