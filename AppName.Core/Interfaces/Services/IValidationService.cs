using AppName.Core.Models.Common;
using AppName.Core.Models.Review;
using AppName.Core.Models.Sessions;

namespace AppName.Core.Interfaces.Services;

public interface IValidationService
{
    IReadOnlyList<AppWarning> ValidateCalls(IReadOnlyList<CallRecord> calls);

    IReadOnlyList<AppWarning> ValidateTransfers(IReadOnlyList<TransferRecord> transfers);

    IReadOnlyList<AppWarning> ValidateReviewReadiness(EvaluationSession session, ReviewData reviewData);
}
