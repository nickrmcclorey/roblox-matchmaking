public class JoinRequest
{
    public required List<int> PlayerIds { get; set; }
    public required string PreferredRegion { get; set; }
    public string? AccessCode { get; set; }
}