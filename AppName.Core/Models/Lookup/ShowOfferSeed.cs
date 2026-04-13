namespace AppName.Core.Models.Lookup;

public sealed class ShowOfferSeed
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ShowName { get; set; } = string.Empty;

    public string OneTimeAmount { get; set; } = string.Empty;

    public string MonthlyAmount { get; set; } = string.Empty;

    public string GiftDescription { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; set; }
}
