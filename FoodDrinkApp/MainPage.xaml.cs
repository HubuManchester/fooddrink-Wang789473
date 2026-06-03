using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class MainPage : ContentPage
{
    private List<MealDbMeal> allMeals = new();
    private int exploredCount = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        await LoadDataAsync();
    }

    private async Task LoadDataAsync(string? query = null)
    {
        if (!string.IsNullOrWhiteSpace(query))
        {
            // Search mode
            allMeals = await MealDbService.SearchMealsAsync(query);
        }
        else
        {
            // Load all countries meals
            allMeals = new List<MealDbMeal>();
            var countries = new[] { "chinese", "japanese", "italian", "mexican" };
            
            foreach (var country in countries)
            {
                var meals = await MealDbService.GetMealsByCountryAsync(country);
                allMeals.AddRange(meals);
            }
        }

        RecipeCollection.ItemsSource = allMeals;
        UpdateStats();
    }

    private void UpdateStats()
    {
        TotalRecipesLabel.Text = allMeals.Count.ToString();
        ExploredLabel.Text = exploredCount.ToString();
    }

    private async void OnExploreClicked(object? sender, EventArgs e)
    {
        // Navigate to hardware page
        await Shell.Current.GoToAsync(nameof(HardwarePage));
    }

    private async void OnViewRecipeClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            exploredCount++;
            UpdateStats();

            // Find the meal from the list
            var meal = allMeals.FirstOrDefault(m => m.Id == id);
            if (meal != null)
            {
                // Store the meal data for detail page
                Preferences.Set("SelectedMealId", id);
                Preferences.Set("SelectedMealName", meal.Name);
                Preferences.Set("SelectedMealCategory", meal.Category);
                Preferences.Set("SelectedMealArea", meal.Area);
                Preferences.Set("SelectedMealThumb", meal.ThumbUrl ?? "");

                // Navigate to detail page
                var detailPage = new RecipeDetailPage();
                await Shell.Current.Navigation.PushAsync(detailPage);
            }
        }
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadDataAsync(e.NewTextValue);
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await LoadDataAsync(SearchRecipeBar.Text);
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadDataAsync(SearchRecipeBar.Text);
        RecipeRefreshView.IsRefreshing = false;
    }
}
