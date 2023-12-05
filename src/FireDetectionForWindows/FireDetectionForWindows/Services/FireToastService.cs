using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
namespace FireDetectionForWindows.Services;
public class FireToastService
{
    private readonly AppNotificationBuilder builder;

    public FireToastService()
    {
        builder = new AppNotificationBuilder().AddText("Detected Fire!").AddText("Please cheak it out!");
        Show();
    }

    private void Show()
    {
        var notificationManager = AppNotificationManager.Default;
        notificationManager.Show(builder.BuildNotification());
    }
}
