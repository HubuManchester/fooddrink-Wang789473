using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

public static class MealDbService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // TheMealDB API base URL (free tier)
    private const string BaseUrl = "https://www.themealdb.com/api/json/v1/1";

    // Cache for meals by country
    private static readonly Dictionary<string, List<MealDbMeal>> MealsByCountry = new();

    // Country name mapping for API
    public static string GetCountryApiName(string direction)
    {
        return direction.ToLower() switch
        {
            "north" or "chinese" => "Chinese",
            "east" or "japanese" => "Japanese",
            "south" or "italian" => "Italian",
            "west" or "mexican" => "Mexican",
            _ => "Chinese"
        };
    }

    // Get all meals from a specific country
    public static async Task<List<MealDbMeal>> GetMealsByCountryAsync(string country)
    {
        var apiCountry = GetCountryApiName(country);

        // Check cache first
        if (MealsByCountry.TryGetValue(apiCountry, out var cachedMeals))
        {
            return cachedMeals;
        }

        try
        {
            var url = $"{BaseUrl}/filter.php?a={Uri.EscapeDataString(apiCountry)}";
            var response = await HttpClient.GetFromJsonAsync<MealDbFilterResponse>(url, JsonOptions);

            if (response?.Meals == null || response.Meals.Count == 0)
            {
                return new List<MealDbMeal>();
            }

            // Get detailed info for each meal (first 10 to save API calls)
            var detailedMeals = new List<MealDbMeal>();
            var mealsToFetch = response.Meals.Take(10).ToList();

            foreach (var meal in mealsToFetch)
            {
                try
                {
                    var detail = await GetMealByIdAsync(meal.IdMeal);
                    if (detail != null)
                    {
                        detailedMeals.Add(detail);
                    }
                }
                catch
                {
                    // Continue with other meals if one fails
                }
            }

            MealsByCountry[apiCountry] = detailedMeals;
            return detailedMeals;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching meals for {country}: {ex.Message}");
            return new List<MealDbMeal>();
        }
    }

    // Get meal details by ID
    public static async Task<MealDbMeal?> GetMealByIdAsync(string id)
    {
        try
        {
            var url = $"{BaseUrl}/lookup.php?i={Uri.EscapeDataString(id)}";
            var response = await HttpClient.GetFromJsonAsync<MealDbResponse>(url, JsonOptions);
            return response?.Meals?.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    // Get a random meal from any country
    public static async Task<MealDbMeal?> GetRandomMealAsync()
    {
        try
        {
            var url = $"{BaseUrl}/random.php";
            var response = await HttpClient.GetFromJsonAsync<MealDbResponse>(url, JsonOptions);
            return response?.Meals?.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    // Get a random meal from a specific country
    public static async Task<MealDbMeal?> GetRandomMealByCountryAsync(string country)
    {
        var meals = await GetMealsByCountryAsync(country);
        return meals.Count > 0 ? meals[Random.Shared.Next(meals.Count)] : null;
    }

    // Search meals by name
    public static async Task<List<MealDbMeal>> SearchMealsAsync(string query)
    {
        try
        {
            var url = $"{BaseUrl}/search.php?s={Uri.EscapeDataString(query)}";
            var response = await HttpClient.GetFromJsonAsync<MealDbResponse>(url, JsonOptions);
            return response?.Meals ?? new List<MealDbMeal>();
        }
        catch
        {
            return new List<MealDbMeal>();
        }
    }

    // Get cuisine info based on compass direction
    public static string GetCuisineInfo(string direction)
    {
        return direction.ToLower() switch
        {
            "north" => "🇨🇳 中国菜系 - 丰富的烹饪传统，精致的调味",
            "east" => "🇯🇵 日本料理 - 新鲜的食材，独到的刀工",
            "south" => "🇮🇹 意大利菜 - 经典的披萨和意面",
            "west" => "🇲🇽 墨西哥菜 - 热情的香料，多彩的风味",
            _ => "🍽️ 探索世界美食"
        };
    }

    // Clear cache (useful for refresh)
    public static void ClearCache()
    {
        MealsByCountry.Clear();
    }
}

// Response model for filter endpoint
public class MealDbFilterResponse
{
    [JsonPropertyName("meals")]
    public List<MealDbFilterMeal>? Meals { get; set; }
}

public class MealDbFilterMeal
{
    [JsonPropertyName("idMeal")]
    public string IdMeal { get; set; } = string.Empty;

    [JsonPropertyName("strMeal")]
    public string StrMeal { get; set; } = string.Empty;

    [JsonPropertyName("strMealThumb")]
    public string? StrMealThumb { get; set; }
}
