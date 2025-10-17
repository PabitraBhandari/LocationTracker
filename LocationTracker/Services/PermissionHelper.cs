using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace LocationTracker.Services
{
    public static class PermissionHelper
    {
        public static async Task<bool> EnsureLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return false;
            }

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                // optionally show rationale to user
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }
    }
}
