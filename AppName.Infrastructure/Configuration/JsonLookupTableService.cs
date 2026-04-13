using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Lookup;
using System.Text.Json;

namespace AppName.Infrastructure.Configuration;

public sealed class JsonLookupTableService : ILookupTableService
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public JsonLookupTableService(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "Content", "lookup-seed.json");
    }

    public async Task<LookupTableSet> GetLookupTablesAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return new LookupTableSet();
        }

        await using var stream = File.OpenRead(_filePath);
        var result = await JsonSerializer.DeserializeAsync<LookupTableSet>(stream, SerializerOptions, cancellationToken);

        return result ?? new LookupTableSet();
    }

    public async Task SaveLookupTablesAsync(LookupTableSet lookupTableSet, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, lookupTableSet, SerializerOptions, cancellationToken);
    }
}
