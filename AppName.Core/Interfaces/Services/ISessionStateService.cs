using AppName.Core.Models.Common;
using AppName.Core.Models.Review;
using AppName.Core.Models.Sessions;

namespace AppName.Core.Interfaces.Services;

public interface ISessionStateService
{
    EvaluationSession CurrentSession { get; }

    event EventHandler? SessionChanged;

    void InitializeNewSession(string evaluatorName);

    void UpdateCall(CallRecord updatedCall);

    void UpdateTransfer(TransferRecord updatedTransfer);

    void UpdateReview(ReviewData reviewData);

    IReadOnlyList<AppWarning> GetCurrentWarnings();

    double GetProgressPercent();
}
