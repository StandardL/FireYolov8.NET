using FireDetectionForWindows.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace FireDetectionForWindows.Views;

public sealed partial class WelcomePage : Page
{
    public WelcomeViewModel ViewModel
    {
        get;
    }

    public WelcomePage()
    {
        ViewModel = App.GetService<WelcomeViewModel>();
        InitializeComponent();
    }
}
