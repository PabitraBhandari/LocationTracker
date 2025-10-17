using LocationTracker.Views;

namespace LocationTracker;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new MainPage();
    }
}