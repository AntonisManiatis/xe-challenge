namespace Xe.Ratings.Domain;

/// <summary>
/// An entity that receives a rating.
/// </summary>
public class Ratee
{
    private readonly List<Rating> ratings = new();

    public Guid Id { get; set; }

    public IList<Rating> Ratings => ratings.AsReadOnly();

    public DateTime CalculatedAt { get; set; }

    public double Total { get; set; }

    public void AddRating(Rating rating)
    {
        ArgumentNullException.ThrowIfNull(rating);
        ratings.Add(rating);
    }
}