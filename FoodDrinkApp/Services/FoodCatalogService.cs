using System.Net.Http.Json;
using System.Text.Json;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

public static class FoodCatalogService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly List<FoodItem> LocalFallbackItems = new()
    {
        new()
        {
            Name = "莓果酸奶碗",
            Category = "Breakfast",
            Description = "希腊酸奶配混合莓果、燕麦和少量蜂蜜，营养丰富的早餐选择",
            Calories = 340,
            Protein = 24,
            Carbs = 42,
            Fat = 8,
            AllergyNote = "含有乳制品和麸质",
            Tags = "健康 早餐 酸奶 莓果",
            IsFavorite = true,
            CreatedAt = DateTime.Now.AddDays(-5)
        },
        new()
        {
            Name = "鸡胸肉糙米饭盒",
            Category = "Lunch",
            Description = "烤鸡胸肉配糙米、菠菜、黄瓜和柠檬汁调味",
            Calories = 520,
            Protein = 38,
            Carbs = 58,
            Fat = 14,
            AllergyNote = "无常见过敏原",
            Tags = "健身 蛋白质 午餐 便当",
            IsFavorite = true,
            CreatedAt = DateTime.Now.AddDays(-3)
        },
        new()
        {
            Name = "冰抹茶拿铁",
            Category = "Drink",
            Description = "抹茶、牛奶和冰块，推荐低糖版本",
            Calories = 180,
            Protein = 8,
            Carbs = 22,
            Fat = 6,
            AllergyNote = "含乳制品，可选植物奶",
            Tags = "饮品 咖啡因 抹茶 拿铁",
            CreatedAt = DateTime.Now.AddDays(-2)
        },
        new()
        {
            Name = "番茄全麦意面",
            Category = "Dinner",
            Description = "全麦意面配番茄酱、罗勒和烤蔬菜",
            Calories = 610,
            Protein = 18,
            Carbs = 92,
            Fat = 16,
            AllergyNote = "含麸质",
            Tags = "素食 晚餐 意面 番茄",
            CreatedAt = DateTime.Now.AddDays(-1)
        },
        new()
        {
            Name = "坚果能量棒",
            Category = "Snack",
            Description = "混合坚果、燕麦和蜂蜜制成的健康零食",
            Calories = 220,
            Protein = 6,
            Carbs = 28,
            Fat = 12,
            AllergyNote = "含坚果",
            Tags = "零食 坚果 能量 健康",
            IsFavorite = true,
            CreatedAt = DateTime.Now
        },
        new()
        {
            Name = "三文鱼牛油果沙拉",
            Category = "Lunch",
            Description = "新鲜三文鱼配牛油果、混合蔬菜和柠檬汁",
            Calories = 450,
            Protein = 32,
            Carbs = 18,
            Fat = 28,
            AllergyNote = "含鱼类",
            Tags = "健康 午餐 三文鱼 牛油果",
            CreatedAt = DateTime.Now.AddHours(-6)
        }
    };

    private static List<FoodItem> cachedItems = new(LocalFallbackItems);

    public static bool LastLoadUsedMockApi { get; private set; }

    public static event EventHandler? ItemsChanged;

    public static async Task<IReadOnlyList<FoodItem>> GetAllAsync()
    {
        if (!MockApiConfig.IsConfigured)
        {
            LastLoadUsedMockApi = false;
            return cachedItems;
        }

        try
        {
            var items = await HttpClient.GetFromJsonAsync<List<FoodItem>>(MockApiConfig.EndpointUrl, JsonOptions);
            if (items is { Count: > 0 })
            {
                cachedItems = items;
                LastLoadUsedMockApi = true;
                return cachedItems;
            }
        }
        catch
        {
            // 网络不可用時使用本地數據
        }

        LastLoadUsedMockApi = false;
        return cachedItems;
    }

    public static async Task<IReadOnlyList<FoodItem>> SearchAsync(string? query)
    {
        var items = await GetAllAsync();

        if (string.IsNullOrWhiteSpace(query))
        {
            return items.OrderByDescending(item => item.CreatedAt).ToList();
        }

        var normalised = query.Trim();
        return items
            .Where(item =>
                item.Name.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Category.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Tags.Contains(normalised, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.CreatedAt)
            .ToList();
    }

    public static async Task<IReadOnlyList<FoodItem>> GetFavoritesAsync()
    {
        var items = await GetAllAsync();
        return items.Where(item => item.IsFavorite).OrderByDescending(item => item.CreatedAt).ToList();
    }

    public static async Task<IReadOnlyList<FoodItem>> GetByCategoryAsync(string category)
    {
        var items = await GetAllAsync();
        return items
            .Where(item => item.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.CreatedAt)
            .ToList();
    }

    public static async Task<IReadOnlyList<string>> GetCategoriesAsync()
    {
        var items = await GetAllAsync();
        return items.Select(item => item.Category).Distinct().OrderBy(c => c).ToList();
    }

    public static async Task<FoodItem?> GetByIdAsync(string id)
    {
        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var item = await HttpClient.GetFromJsonAsync<FoodItem>(
                    $"{MockApiConfig.EndpointUrl.TrimEnd('/')}/{Uri.EscapeDataString(id)}",
                    JsonOptions);

                if (item is not null)
                {
                    return item;
                }
            }
            catch
            {
                // 回退到本地緩存
            }
        }

        return cachedItems.FirstOrDefault(item => item.Id == id);
    }

    public static async Task<FoodItem> AddAsync(FoodItem item)
    {
        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(MockApiConfig.EndpointUrl, item, JsonOptions);
                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadFromJsonAsync<FoodItem>(JsonOptions);
                if (created is not null)
                {
                    cachedItems.Add(created);
                    ItemsChanged?.Invoke(null, EventArgs.Empty);
                    return created;
                }
            }
            catch
            {
                // 回退到本地存儲
            }
        }

        cachedItems.Add(item);
        ItemsChanged?.Invoke(null, EventArgs.Empty);
        return item;
    }

    public static async Task ToggleFavoriteAsync(string id)
    {
        var item = await GetByIdAsync(id);
        if (item is not null)
        {
            item.IsFavorite = !item.IsFavorite;
            ItemsChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static async Task DeleteAsync(string id)
    {
        var item = await GetByIdAsync(id);
        if (item is not null)
        {
            cachedItems.Remove(item);
            ItemsChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static async Task<NutritionSummary> GetNutritionSummaryAsync()
    {
        var items = await GetAllAsync();
        var summary = new NutritionSummary
        {
            TotalItems = items.Count,
            TotalCalories = items.Sum(i => i.Calories),
            TotalProtein = items.Sum(i => i.Protein),
            TotalCarbs = items.Sum(i => i.Carbs),
            TotalFat = items.Sum(i => i.Fat)
        };

        foreach (var item in items)
        {
            if (summary.CategoryCounts.ContainsKey(item.Category))
            {
                summary.CategoryCounts[item.Category]++;
                summary.CategoryCalories[item.Category] += item.Calories;
            }
            else
            {
                summary.CategoryCounts[item.Category] = 1;
                summary.CategoryCalories[item.Category] = item.Calories;
            }
        }

        return summary;
    }
}
