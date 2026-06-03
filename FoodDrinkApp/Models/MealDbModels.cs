using System.Text.Json.Serialization;

namespace FoodDrinkApp.Models;

public class MealDbResponse
{
    [JsonPropertyName("meals")]
    public List<MealDbMeal>? Meals { get; set; }
}

public class MealDbMeal
{
    [JsonPropertyName("idMeal")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("strMeal")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("strCategory")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("strArea")]
    public string Area { get; set; } = string.Empty;

    [JsonPropertyName("strInstructions")]
    public string Instructions { get; set; } = string.Empty;

    [JsonPropertyName("strMealThumb")]
    public string? ThumbUrl { get; set; }

    [JsonPropertyName("strTags")]
    public string? Tags { get; set; }

    [JsonPropertyName("strYoutube")]
    public string? YoutubeUrl { get; set; }

    [JsonPropertyName("strSource")]
    public string? SourceUrl { get; set; }

    // Ingredients (up to 20)
    [JsonPropertyName("strIngredient1")]
    public string? Ingredient1 { get; set; }
    [JsonPropertyName("strIngredient2")]
    public string? Ingredient2 { get; set; }
    [JsonPropertyName("strIngredient3")]
    public string? Ingredient3 { get; set; }
    [JsonPropertyName("strIngredient4")]
    public string? Ingredient4 { get; set; }
    [JsonPropertyName("strIngredient5")]
    public string? Ingredient5 { get; set; }
    [JsonPropertyName("strIngredient6")]
    public string? Ingredient6 { get; set; }
    [JsonPropertyName("strIngredient7")]
    public string? Ingredient7 { get; set; }
    [JsonPropertyName("strIngredient8")]
    public string? Ingredient8 { get; set; }
    [JsonPropertyName("strIngredient9")]
    public string? Ingredient9 { get; set; }
    [JsonPropertyName("strIngredient10")]
    public string? Ingredient10 { get; set; }
    [JsonPropertyName("strIngredient11")]
    public string? Ingredient11 { get; set; }
    [JsonPropertyName("strIngredient12")]
    public string? Ingredient12 { get; set; }
    [JsonPropertyName("strIngredient13")]
    public string? Ingredient13 { get; set; }
    [JsonPropertyName("strIngredient14")]
    public string? Ingredient14 { get; set; }
    [JsonPropertyName("strIngredient15")]
    public string? Ingredient15 { get; set; }
    [JsonPropertyName("strIngredient16")]
    public string? Ingredient16 { get; set; }
    [JsonPropertyName("strIngredient17")]
    public string? Ingredient17 { get; set; }
    [JsonPropertyName("strIngredient18")]
    public string? Ingredient18 { get; set; }
    [JsonPropertyName("strIngredient19")]
    public string? Ingredient19 { get; set; }
    [JsonPropertyName("strIngredient20")]
    public string? Ingredient20 { get; set; }

    // Measures
    [JsonPropertyName("strMeasure1")]
    public string? Measure1 { get; set; }
    [JsonPropertyName("strMeasure2")]
    public string? Measure2 { get; set; }
    [JsonPropertyName("strMeasure3")]
    public string? Measure3 { get; set; }
    [JsonPropertyName("strMeasure4")]
    public string? Measure4 { get; set; }
    [JsonPropertyName("strMeasure5")]
    public string? Measure5 { get; set; }
    [JsonPropertyName("strMeasure6")]
    public string? Measure6 { get; set; }
    [JsonPropertyName("strMeasure7")]
    public string? Measure7 { get; set; }
    [JsonPropertyName("strMeasure8")]
    public string? Measure8 { get; set; }
    [JsonPropertyName("strMeasure9")]
    public string? Measure9 { get; set; }
    [JsonPropertyName("strMeasure10")]
    public string? Measure10 { get; set; }
    [JsonPropertyName("strMeasure11")]
    public string? Measure11 { get; set; }
    [JsonPropertyName("strMeasure12")]
    public string? Measure12 { get; set; }
    [JsonPropertyName("strMeasure13")]
    public string? Measure13 { get; set; }
    [JsonPropertyName("strMeasure14")]
    public string? Measure14 { get; set; }
    [JsonPropertyName("strMeasure15")]
    public string? Measure15 { get; set; }
    [JsonPropertyName("strMeasure16")]
    public string? Measure16 { get; set; }
    [JsonPropertyName("strMeasure17")]
    public string? Measure17 { get; set; }
    [JsonPropertyName("strMeasure18")]
    public string? Measure18 { get; set; }
    [JsonPropertyName("strMeasure19")]
    public string? Measure19 { get; set; }
    [JsonPropertyName("strMeasure20")]
    public string? Measure20 { get; set; }

    // Country info based on Area
    public string CountryEmoji => Area.ToLower() switch
    {
        "chinese" => "🇨🇳",
        "japanese" => "🇯🇵",
        "italian" => "🇮🇹",
        "mexican" => "🇲🇽",
        _ => "🍽️"
    };

    public string CountryName => Area.ToLower() switch
    {
        "chinese" => "中国 Chinese",
        "japanese" => "日本 Japanese",
        "italian" => "意大利 Italian",
        "mexican" => "墨西哥 Mexican",
        _ => Area
    };

    public string CategoryIcon => Category.ToLower() switch
    {
        "breakfast" => "🌅",
        "beef" => "🥩",
        "chicken" => "🍗",
        "dessert" => "🍰",
        "lamb" => "🍖",
        "miscellaneous" => "🍲",
        "pasta" => "🍝",
        "pork" => "🐷",
        "seafood" => "🦐",
        "side" => "🥗",
        "starter" => "🥟",
        "vegan" => "🥬",
        "vegetarian" => "🥬",
        _ => "🍽️"
    };

    // Get all ingredients as a list
    public List<(string ingredient, string measure)> GetIngredients()
    {
        var ingredients = new List<(string, string)>();
        var ingredientProps = new[] { Ingredient1, Ingredient2, Ingredient3, Ingredient4, Ingredient5,
            Ingredient6, Ingredient7, Ingredient8, Ingredient9, Ingredient10,
            Ingredient11, Ingredient12, Ingredient13, Ingredient14, Ingredient15,
            Ingredient16, Ingredient17, Ingredient18, Ingredient19, Ingredient20 };
        var measureProps = new[] { Measure1, Measure2, Measure3, Measure4, Measure5,
            Measure6, Measure7, Measure8, Measure9, Measure10,
            Measure11, Measure12, Measure13, Measure14, Measure15,
            Measure16, Measure17, Measure18, Measure19, Measure20 };

        for (int i = 0; i < 20; i++)
        {
            if (!string.IsNullOrWhiteSpace(ingredientProps[i]))
            {
                var ingredient = ingredientProps[i]!.Trim();
                var measure = measureProps[i]?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(ingredient))
                    ingredients.Add((ingredient, measure));
            }
        }
        return ingredients;
    }

    // Create a summary text for speech
    public string GetAccessibleSummary()
    {
        var ingredients = GetIngredients().Take(5).ToList();
        var ingredientList = string.Join("、", ingredients.Select(i => i.ingredient));
        return $"{Name}，来自{CountryName}，分类：{Category}。主要食材包括：{ingredientList}。";
    }
}
