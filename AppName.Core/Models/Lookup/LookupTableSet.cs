namespace AppName.Core.Models.Lookup;

public sealed class LookupTableSet
{
    public List<LookupItem> CallTypes { get; set; } = [];

    public List<LookupItem> SupervisorReasons { get; set; } = [];

    public List<ShowOfferSeed> ShowOffers { get; set; } = [];

    public List<DonorProfileSeed> DonorProfiles { get; set; } = [];

    public List<LookupItem> FailReasons { get; set; } = [];

    public List<LookupItem> CoachingCategories { get; set; } = [];
}
