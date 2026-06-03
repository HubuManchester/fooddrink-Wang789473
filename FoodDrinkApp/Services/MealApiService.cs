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

    // Local image mapping for dishes
    private readonly Dictionary<string, string> _localImageMap = new()
    {
        { "Beef Noodles", "beef_noodles.png" },
        { "Kung Pao Chicken", "kung_pao_chicken.png" },
        { "Dim Sum", "dim_sum.jpg" },
        { "Sushi", "sushi.png" },
        { "Ramen", "ramen.jpg" },
        { "Pizza", "pizza.jpg" },
        { "Pasta", "pasta.jpg" },
        { "Taco", "taco.png" },
        { "Burrito", "burrito.png" },
        // Add more mappings as needed
        { "Golabki (cabbage roll)", "golabki.jpg" },
        { "Palidah", "palidah.jpg" },
        { "Roaf", "roaf.jpg" }
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
                    {
                        // Use local image if API image is not available
                        if (string.IsNullOrEmpty(fullMeal.ImageUrl) || fullMeal.ImageUrl.Contains("null"))
                        {
                            fullMeal.ImageUrl = GetLocalImagePath(fullMeal.Name);
                        }
                        meals.Add(fullMeal);
                    }
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
            var meal = result?.Meals?.FirstOrDefault();

            if (meal != null)
            {
                // Parse ingredients
                meal.Ingredients = ExtractIngredients(json);

                // Use local image if API image is not available
                if (string.IsNullOrEmpty(meal.ImageUrl))
                {
                    meal.ImageUrl = GetLocalImagePath(meal.Name);
                }
            }

            return meal;
        }
        catch
        {
            return null;
        }
    }

    // Get random meal (for shake feature) - uses local images
    public async Task<Meal?> GetRandomMeal()
    {
        try
        {
            var response = await _httpClient.GetAsync("random.php");
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MealResponse>(json);
            var meal = result?.Meals?.FirstOrDefault();

            if (meal != null)
            {
                // Parse ingredients
                meal.Ingredients = ExtractIngredients(json);

                // Force use local image for random meals (most reliable)
                meal.ImageUrl = GetLocalImagePath(meal.Name);
            }

            return meal;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Random meal error: {ex.Message}");
            return null;
        }
    }

    // Extract ingredients from JSON
    private List<string> ExtractIngredients(string json)
    {
        var ingredients = new List<string>();

        try
        {
            var doc = JsonDocument.Parse(json);
            var meal = doc.RootElement.GetProperty("meals")[0];

            for (int i = 1; i <= 20; i++)
            {
                var ingredient = meal.GetProperty($"strIngredient{i}").GetString();
                var measure = meal.GetProperty($"strMeasure{i}").GetString();

                if (!string.IsNullOrEmpty(ingredient) && !string.IsNullOrWhiteSpace(ingredient))
                {
                    string measureText = string.IsNullOrEmpty(measure) ? "" : measure;
                    ingredients.Add($"{measureText} {ingredient}".Trim());
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Extract ingredients error: {ex.Message}");
        }

        return ingredients;
    }

    // Get local image path for a dish
    private string GetLocalImagePath(string? dishName)
    {
        if (string.IsNullOrEmpty(dishName)) return "default_food.png";

        return _localImageMap.ContainsKey(dishName)
            ? _localImageMap[dishName]
            : "default_food.png";  // Return default image if not found
    }

    // Mock data when API fails (with local images)
    private List<Meal> GetMockMeals(string cuisine)
    {
        var meals = new List<Meal>();

        if (cuisine == "Chinese")
        {
            meals.Add(new Meal
            {
                Name = "Beef Noodles",
                Instructions = "Boil noodles, add beef soup",
                ImageUrl = GetLocalImagePath("Beef Noodles"),
                Ingredients = new List<string> { "Beef", "Noodles", "Soup base", "Vegetables" }
            });
            meals.Add(new Meal
            {
                Name = "Kung Pao Chicken",
                Instructions = "Stir fry chicken with peanuts",
                ImageUrl = GetLocalImagePath("Kung Pao Chicken"),
                Ingredients = new List<string> { "Chicken", "Peanuts", "Chili peppers", "Soy sauce" }
            });
            meals.Add(new Meal
            {
                Name = "Dim Sum",
                Instructions = "Steam dumplings",
                ImageUrl = GetLocalImagePath("Dim Sum"),
                Ingredients = new List<string> { "Dough", "Pork", "Shrimp", "Bamboo shoots" }
            });
        }
        else if (cuisine == "Japanese")
        {
            meals.Add(new Meal
            {
                Name = "Sushi",
                Instructions = "Roll rice with fish",
                ImageUrl = GetLocalImagePath("Sushi"),
                Ingredients = new List<string> { "Rice", "Nori", "Fish", "Wasabi" }
            });
            meals.Add(new Meal
            {
                Name = "Ramen",
                Instructions = "Boil noodles in broth",
                ImageUrl = GetLocalImagePath("Ramen"),
                Ingredients = new List<string> { "Noodles", "Broth", "Pork", "Egg" }
            });
        }
        else if (cuisine == "Italian")
        {
            meals.Add(new Meal
            {
                Name = "Pizza",
                Instructions = "Bake with toppings",
                ImageUrl = GetLocalImagePath("Pizza"),
                Ingredients = new List<string> { "Dough", "Tomato sauce", "Cheese", "Toppings" }
            });
            meals.Add(new Meal
            {
                Name = "Pasta",
                Instructions = "Boil pasta, add sauce",
                ImageUrl = GetLocalImagePath("Pasta"),
                Ingredients = new List<string> { "Pasta", "Tomato sauce", "Garlic", "Olive oil" }
            });
        }
        else if (cuisine == "Mexican")
        {
            meals.Add(new Meal
            {
                Name = "Taco",
                Instructions = "Fill tortilla",
                ImageUrl = GetLocalImagePath("Taco"),
                Ingredients = new List<string> { "Tortilla", "Meat", "Lettuce", "Cheese", "Salsa" }
            });
            meals.Add(new Meal
            {
                Name = "Burrito",
                Instructions = "Wrap fillings",
                ImageUrl = GetLocalImagePath("Burrito"),
                Ingredients = new List<string> { "Tortilla", "Rice", "Beans", "Meat", "Cheese" }
            });
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