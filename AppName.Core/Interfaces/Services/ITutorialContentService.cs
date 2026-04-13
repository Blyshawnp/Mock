using AppName.Core.Models.Help;

namespace AppName.Core.Interfaces.Services;

public interface ITutorialContentService
{
    Task<IReadOnlyList<TutorialTopic>> GetTopicsAsync(CancellationToken cancellationToken = default);
}
