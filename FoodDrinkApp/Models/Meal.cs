using System.Text.Json.Serialization;

namespace FoodDrinkApp.Models;

public class MealResponse
{
    [JsonPropertyName("meals")]
    public List<Meal>? Meals { get; set; }
}

public class Meal
{
    [JsonPropertyName("idMeal")]
    public string? Id { get; set; }

    [JsonPropertyName("strMeal")]
    public string? Name { get; set; }

    [JsonPropertyName("strCategory")]
    public string? Category { get; set; }

    [JsonPropertyName("strArea")]
    public string? Area { get; set; }

    [JsonPropertyName("strInstructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("strMealThumb")]
    public string? ImageUrl { get; set; }

    // 食材列表（从 API 解析）
    public List<string>? Ingredients { get; set; }
}