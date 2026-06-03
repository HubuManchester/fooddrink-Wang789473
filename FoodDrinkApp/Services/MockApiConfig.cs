namespace FoodDrinkApp.Services;

public static class MockApiConfig
{
    public static string EndpointUrl { get; set; } = string.Empty;
    
    public static bool IsConfigured => !string.IsNullOrWhiteSpace(EndpointUrl);
}