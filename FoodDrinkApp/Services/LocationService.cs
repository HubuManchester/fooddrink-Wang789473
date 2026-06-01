using Microsoft.Maui.Devices.Sensors;

namespace FoodDrinkApp.Services;

public static class LocationService
{
    public static async Task<string> GetCurrentLocationAsync()
    {
        try
        {
            // Request permission
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    return "Location permission denied";
                }
            }

            // Get current location
            var location = await Geolocation.Default.GetLastKnownLocationAsync();

            if (location == null)
            {
                location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                });
            }

            if (location != null)
            {
                return $"Lat: {location.Latitude:F2}, Lon: {location.Longitude:F2}";
            }
            else
            {
                return "Location not available - using mock data";
            }
        }
        catch (Exception ex)
        {
            return $"Location error: {ex.Message}";
        }
    }

    // Mock location for demo (不用真实GPS)
    public static string GetMockLocation()
    {
        // 模拟一个餐厅位置
        var random = new Random();
        var mockLocations = new[]
        {
            "🍔 McDonald's - 123 Main St (Mock)",
            "🍕 Pizza Hut - 456 Oak Ave (Mock)",
            "🍜 Noodle House - 789 Pine Rd (Mock)",
            "🥗 Salad Bar - 321 Elm St (Mock)",
            "☕ Starbucks - 654 Maple Dr (Mock)"
        };

        return mockLocations[random.Next(mockLocations.Length)];
    }
}