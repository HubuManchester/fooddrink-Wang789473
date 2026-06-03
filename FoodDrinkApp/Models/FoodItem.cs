using System.Text.Json.Serialization;

namespace FoodDrinkApp.Models;

public sealed class FoodItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("calories")]
    public int Calories { get; set; }

    [JsonPropertyName("protein")]
    public int Protein { get; set; }

    [JsonPropertyName("carbs")]
    public int Carbs { get; set; }

    [JsonPropertyName("fat")]
    public int Fat { get; set; }

    [JsonPropertyName("allergyNote")]
    public string AllergyNote { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public string Tags { get; set; } = string.Empty;

    [JsonPropertyName("isFavorite")]
    public bool IsFavorite { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public string CaloriesLabel => $"{Calories} kcal";

    [JsonIgnore]
    public string MacroSummary => $"蛋白质 {Protein}g · 碳水 {Carbs}g · 脂肪 {Fat}g";

    [JsonIgnore]
    public string AccessibleSummary => $"{Name}，{Category}，{Calories}千卡，{MacroSummary}，{AllergyNote}";

    [JsonIgnore]
    public string CategoryIcon => Category.ToLower() switch
    {
        "breakfast" or "早餐" => "🌅",
        "lunch" or "午餐" => "☀️",
        "dinner" or "晚餐" => "🌙",
        "snack" or "零食" => "🍿",
        "drink" or "饮品" => "🥤",
        _ => "🍽️"
    };

    [JsonIgnore]
    public string CategoryColor => Category.ToLower() switch
    {
        "breakfast" or "早餐" => "#F59E0B",
        "lunch" or "午餐" => "#10B981",
        "dinner" or "晚餐" => "#6366F1",
        "snack" or "零食" => "#EC4899",
        "drink" or "饮品" => "#06B6D4",
        _ => "#8B5CF6"
    };

    [JsonIgnore]
    public double ProteinPercentage => Calories > 0 ? (Protein * 4.0 / Calories) * 100 : 0;

    [JsonIgnore]
    public double CarbsPercentage => Calories > 0 ? (Carbs * 4.0 / Calories) * 100 : 0;

    [JsonIgnore]
    public double FatPercentage => Calories > 0 ? (Fat * 9.0 / Calories) * 100 : 0;
}
