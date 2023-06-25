namespace Xe.Ratings.Domain;

interface IRateeRepository
{
    Task<Ratee?> GetRateeAsync(Guid rateeId);

    Task SaveAsync(Ratee ratee);

    Task<int> CleanUpAsync();
}