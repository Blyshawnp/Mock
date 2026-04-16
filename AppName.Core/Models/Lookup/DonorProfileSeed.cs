using AppName.Core.Models.Enums;

namespace AppName.Core.Models.Lookup;

public sealed class DonorProfileSeed
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DonorScenario DonorScenario { get; set; } = DonorScenario.NewDonor;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; set; }
}
