using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using LocationTracker.Services;
using LocationTracker.ViewModels;

namespace LocationTracker.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _vm;

    public MainPage()
    {
        InitializeComponent();

        // Try to get VM from DI; fall back to a direct construction to keep the page usable.
        var services = Application.Current?.Handler?.MauiContext?.Services;
        _vm = services?.GetService<MainViewModel>()
              ?? new MainViewModel(new LocationRepository(), new LocationService(), Dispatcher);

        BindingContext = _vm;

        if (HeatView is not null)
        {
            HeatView.Drawable = _vm.HeatDrawable;
        }

        _vm.HeatDrawable.GetProjection = GeoToPoint;
        _vm.HeatDrawable.Invalidate = () => MainThread.BeginInvokeOnMainThread(() => HeatView?.Invalidate());

        SizeChanged += (_, __) => HeatView?.Invalidate();

        if (MapControl is not null)
        {
            MapControl.PropertyChanged += OnMapPropertyChanged;
        }

        _vm.PointSaved += (lat, lng) =>
        {
            var center = new Location(lat, lng);
            var span = new MapSpan(center, 0.004, 0.004);
            MapControl?.MoveToRegion(span);
            HeatView?.Invalidate();
        };
    }

    private void OnMapPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Any map property change may alter projection -> redraw heat overlay.
        HeatView?.Invalidate();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        Location? loc = null;
        try
        {
            loc = await Geolocation.GetLastKnownLocationAsync()
                  ?? await Geolocation.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(8)),
                        new CancellationTokenSource(TimeSpan.FromSeconds(12)).Token);
        }
        catch
        {
            // Ignore and fall back below.
        }

        var center = (loc is not null)
            ? new Location(loc.Latitude, loc.Longitude)
            : new Location(37.7749, -122.4194); // SF fallback

        var span = new MapSpan(center, 0.004, 0.004);
        MapControl?.MoveToRegion(span);
        try { if (MapControl is not null) MapControl.IsShowingUser = true; } catch { /* simulator quirks */ }

        await _vm.RefreshHeatAsync();
        HeatView?.Invalidate();
    }

    private async void OnCenterOnLast(object? sender, EventArgs e)
    {
        await _vm.RefreshHeatAsync();

        var pts = _vm.HeatDrawable.Points;
        if (pts.Count == 0)
        {
            await DisplayAlert("Center on Last", "No points yet.", "OK");
            return;
        }

        var last = pts[^1];
        var center = new Location(last.lat, last.lng);
        MapControl?.MoveToRegion(new MapSpan(center, 0.004, 0.004));
        HeatView?.Invalidate();
    }

    private PointF GeoToPoint((double lat, double lng) geo)
    {
        // Ensure map and overlay are ready
        if (MapControl?.VisibleRegion is null || HeatView is null)
            return new PointF(-1, -1);

        var region = MapControl.VisibleRegion;

        // If layout not measured yet, skip
        if (HeatView.Width <= 0 || HeatView.Height <= 0)
            return new PointF(-1, -1);

        var rect = new Rect(0, 0, HeatView.Width, HeatView.Height);

        var topLeft = new Location(region.Center.Latitude + region.LatitudeDegrees / 2,
                                   region.Center.Longitude - region.LongitudeDegrees / 2);
        var bottomRight = new Location(region.Center.Latitude - region.LatitudeDegrees / 2,
                                       region.Center.Longitude + region.LongitudeDegrees / 2);

        var denomX = (bottomRight.Longitude - topLeft.Longitude);
        var denomY = (topLeft.Latitude - bottomRight.Latitude);
        if (denomX == 0 || denomY == 0)
            return new PointF(-1, -1);

        double x = (geo.lng - topLeft.Longitude) / denomX;
        double y = (topLeft.Latitude - geo.lat) / denomY;

        if (double.IsNaN(x) || double.IsNaN(y))
            return new PointF(-1, -1);

        return new PointF((float)(rect.Width * x), (float)(rect.Height * y));
    }
}