using ErrorOr;

namespace Xe.Ratings.Service;

public static class Errors
{
    public static readonly Error InvalidRateeId =
        Error.Validation("InvalidRateeId", "A ratee Id is required.");

    public static readonly Error InvalidRatingValue =
        Error.Validation("InvalidRatingValue", "A rating can only be incremented by 0.5.");

    public static readonly Error InvalidRatingRange =
        Error.Validation("InvalidRatingRange", "A rating cannot be a negative or higher than 5.0 value.");

    public static readonly Error RateeNotFound =
        Error.NotFound("RateeNotFound", "Ratee is not found.");

    public static readonly Error NoRatingsFound =
        Error.NotFound("RateeNotFound", "Ratee is not found.");
}