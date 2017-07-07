namespace TripLine.Service
{
    public class PhotoSearchRequest
    {
        int? TripId { get; set; }
        int? DestId { get; set; }
        int? PlaceId { get; set; }

        string WithTag { get; set; }
        string WithoutTag { get; set; }
    }
}