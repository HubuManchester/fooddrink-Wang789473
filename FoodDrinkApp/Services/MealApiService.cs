using FoodDrinkApp.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoodDrinkApp.Services;

public class MealApiService
{
    private readonly HttpClient _httpClient;

    // Map Chinese cuisine name to TheMealDB area parameter
    private readonly Dictionary<string, string> _areaMap = new()
    {
        { "Chinese", "Chinese" },
        { "Japanese", "Japanese" },
        { "Italian", "Italian" },
        { "Mexican", "Mexican" }
    };

    // Map for categories (fallback)
    private readonly Dictionary<string, string> _categoryMap = new()
    {
        { "Chinese", "Beef" },
        { "Japanese", "Chicken" },
        { "Italian", "Pasta" },
        { "Mexican", "Pork" }
    };

    public MealApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://www.themealdb.com/api/json/v1/1/");
    }

    // Search meals by area (cuisine)
    public async Task<List<Meal>> SearchByCuisine(string cuisine)
    {
        try
        {
            string keyword = _areaMap.ContainsKey(cuisine) ? _areaMap[cuisine] : "Chinese";
            var response = await _httpClient.GetAsync($"filter.php?a={keyword}");
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<MealSearchResponse>(json);

            var meals = new List<Meal>();
            if (result?.Meals != null)
            {
                foreach (var basicMeal in result.Meals.Take(5))
                {
                    var fullMeal = await GetMealDetails(basicMeal.Id);
                    if (fullMeal != null)
                        meals.Add(fullMeal);
                }
            }

            return meals.Count > 0 ? meals : GetMockMeals(cuisine);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
            return GetMockMeals(cuisine);
        }
    }

    // Get meal details by ID
    public async Task<Meal?> GetMealDetails(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        try
        {
            var response = await _httpClient.GetAsync($"lookup.php?i={id}");
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MealResponse>(json);
            return result?.Meals?.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    // Get random meal (for shake feature later)
    public async Task<Meal?> GetRandomMeal()
    {
        try
        {
            var response = await _httpClient.GetAsync("random.php");
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MealResponse>(json);
            return result?.Meals?.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    // Mock data when API fails
    private List<Meal> GetMockMeals(string cuisine)
    {
        var meals = new List<Meal>();

        if (cuisine == "Chinese")
        {
            meals.Add(new Meal { Name = "Beef Noodles", Instructions = "Boil noodles, add beef soup" });
            meals.Add(new Meal { Name = "Kung Pao Chicken", Instructions = "Stir fry chicken with peanuts" });
            meals.Add(new Meal { Name = "Dim Sum", Instructions = "Steam dumplings" });
        }
        else if (cuisine == "Japanese")
        {
            meals.Add(new Meal { Name = "Sushi", Instructions = "Roll rice with fish" });
            meals.Add(new Meal { Name = "Ramen", Instructions = "Boil noodles in broth" });
        }
        else if (cuisine == "Italian")
        {
            meals.Add(new Meal { Name = "Pizza", Instructions = "Bake with toppings" });
            meals.Add(new Meal { Name = "Pasta", Instructions = "Boil pasta, add sauce" });
        }
        else if (cuisine == "Mexican")
        {
            meals.Add(new Meal { Name = "Taco", Instructions = "Fill tortilla" });
            meals.Add(new Meal { Name = "Burrito", Instructions = "Wrap fillings" });
        }

        return meals;
    }
}

// Helper classes for API response
public class MealSearchResponse
{
    public List<BasicMeal>? Meals { get; set; }
}

public class BasicMeal
{
    [JsonPropertyName("idMeal")]
    public string? Id { get; set; }

    [JsonPropertyName("strMeal")]
    public string? Name { get; set; }

    [JsonPropertyName("strMealThumb")]
    public string? Thumbnail { get; set; }
}