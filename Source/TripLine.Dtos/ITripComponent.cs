namespace TripLine.Dtos
{
    public interface ITripComponent : IDateRange
    {
        int Id { get; set; }

        string DisplayName { get; set; }

        Location Location { get; set; }
    }
}