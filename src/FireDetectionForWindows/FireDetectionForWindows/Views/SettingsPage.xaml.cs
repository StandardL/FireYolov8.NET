using FireDetectionForWindows.ViewModels;
using FireDetectionForWindows.Helpers;

using Microsoft.UI.Xaml.Controls;

namespace FireDetectionForWindows.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    private readonly CacheClearHelper cacheClearHelper;
    private readonly CacheCalculatingHelper cacheCalculatingHelper;

    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();

        cacheClearHelper = new();
        cacheCalculatingHelper = new();

        LoadSize();
    }

    private async void LoadSize()
    {
        double cacheSize = await cacheCalculatingHelper.GetCacheSize();
        var times = 0;
        while (cacheSize >= 1024)
        {
            cacheSize /= 1024.0;
            times++;
        }
        switch (times)
        {
            case 0:
                CacheSize_Text.Text = cacheSize.ToString("0.00") + " B";
                break;
            case 1:
                CacheSize_Text.Text = cacheSize.ToString("0.00") + " KB";
                break;
            case 2:
                CacheSize_Text.Text = cacheSize.ToString("0.00") + " MB";
                break;
            case 3:
                CacheSize_Text.Text = cacheSize.ToString("0.00") + " GB";
                break;
        }
    }

    private async void CacheClear_Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        CleaningRing.IsActive = true;

        await cacheClearHelper.Clean();
        LoadSize();
        await Task.Delay(650);

        CleaningRing.IsActive = false;
    }
}
