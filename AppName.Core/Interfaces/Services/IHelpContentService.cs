using AppName.Core.Models.Help;

namespace AppName.Core.Interfaces.Services;

public interface IHelpContentService
{
    Task<IReadOnlyList<HelpArticle>> GetArticlesAsync(CancellationToken cancellationToken = default);
}
