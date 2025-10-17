using Microsoft.Maui.Devices.Sensors;

namespace LocationTracker.Services;

public class LocationService
{
    public async Task<Location?> GetOnceAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return null;

            var last = await Geolocation.GetLastKnownLocationAsync();
            if (last != null) return last;

            var req = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(8));
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(12));
            return await Geolocation.GetLocationAsync(req, cts.Token);
        }
        catch
        {
            return null;
        }
    }
}