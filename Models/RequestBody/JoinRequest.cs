public class JoinRequest
{
    public required int PlayerId { get; set; }
    public required int PartySize { get; set; }
    public required string PreferredRegion { get; set; }
    public string? AccessCode { get; set; }
}