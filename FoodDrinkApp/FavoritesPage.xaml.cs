using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class FavoritesPage : ContentPage
{
    private static List<MealDbMeal> favoriteMeals = new();

    public FavoritesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadFavorites();
    }

    private void LoadFavorites()
    {
        FavoritesList.ItemsSource = null;
        FavoritesList.ItemsSource = favoriteMeals;
        FavoritesCountLabel.Text = favoriteMeals.Count.ToString();
    }

    // 添加收藏（从其他页面调用）
    public static void AddToFavorites(MealDbMeal meal)
    {
        if (!favoriteMeals.Any(m => m.Id == meal.Id))
        {
            favoriteMeals.Add(meal);
        }
    }

    // 兼容RecipeDetailPage的方法名
    public static void AddFavorite(MealDbMeal meal) => AddToFavorites(meal);

    // 移除收藏
    public static void RemoveFavorite(MealDbMeal meal)
    {
        var existingMeal = favoriteMeals.FirstOrDefault(m => m.Id == meal.Id);
        if (existingMeal != null)
        {
            favoriteMeals.Remove(existingMeal);
        }
    }

    // 检查是否已收藏
    public static bool IsFavorite(string mealId)
    {
        return favoriteMeals.Any(m => m.Id == mealId);
    }

    private void OnRemoveFavoriteClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            var meal = favoriteMeals.FirstOrDefault(m => m.Id == id);
            if (meal != null)
            {
                favoriteMeals.Remove(meal);
                LoadFavorites();
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            }
        }
    }

    private async void OnExploreClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(HardwarePage));
    }

    private void OnRefreshing(object? sender, EventArgs e)
    {
        LoadFavorites();
        FavoritesRefreshView.IsRefreshing = false;
    }
}
