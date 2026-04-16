namespace AppName.UI.Services;

public interface IAudioFeedbackService
{
    Task PlayErrorAsync(CancellationToken cancellationToken = default);

    Task PlaySuccessAsync(CancellationToken cancellationToken = default);
}
