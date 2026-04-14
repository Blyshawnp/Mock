using AppName.Core.Models.Lookup;

namespace AppName.Core.Interfaces.Services;

public interface ILookupTableService
{
    Task<LookupTableSet> GetLookupTablesAsync(CancellationToken cancellationToken = default);

    Task SaveLookupTablesAsync(LookupTableSet lookupTableSet, CancellationToken cancellationToken = default);
}
