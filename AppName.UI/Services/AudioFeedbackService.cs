using System.Media;

namespace AppName.UI.Services;

public sealed class AudioFeedbackService : IAudioFeedbackService
{
    public Task PlayErrorAsync(CancellationToken cancellationToken = default)
    {
        return PlayAsync(() => SystemSounds.Hand.Play(), cancellationToken);
    }

    public Task PlaySuccessAsync(CancellationToken cancellationToken = default)
    {
        return PlayAsync(() => SystemSounds.Asterisk.Play(), cancellationToken);
    }

    private static Task PlayAsync(Action playAction, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                playAction();
            }
            catch
            {
                // Audio feedback should never interrupt workflow.
            }
        }, cancellationToken);
    }
}
