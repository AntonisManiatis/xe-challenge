namespace Xe.Ratings.Domain;

public class Rating
{
    public int Id { get; set; }

    public Guid? RaterId { get; set; }

    public Guid RateeId { get; set; }

    public float Value { get; set; }

    public DateTime PostedAt { get; set; }
}