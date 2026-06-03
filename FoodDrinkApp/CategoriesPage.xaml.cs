using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class CategoriesPage : ContentPage
{
    private string? selectedCountry;

    public CategoriesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCountries();
    }

    private void LoadCountries()
    {
        var countries = new List<CountryInfo>
        {
            new("Chinese", "🇨🇳", "China", "#FFEBEE", "#C62828", "#C6282820"),
            new("Japanese", "🇯🇵", "Japan", "#E3F2FD", "#1565C0", "#1565C020"),
            new("Italian", "🇮🇹", "Italy", "#E8F5E9", "#2E7D32", "#2E7D3220"),
            new("Mexican", "🇲🇽", "Mexico", "#FFF3E0", "#E65100", "#E6510020")
        };

        CountrySelector.ItemsSource = countries;
    }

    private async void OnCountrySelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CountryInfo country)
        {
            selectedCountry = country.Key;
            await LoadCountryRecipesAsync(country.Key);
        }
    }

    private async Task LoadCountryRecipesAsync(string country)
    {
        var meals = await MealDbService.GetMealsByCountryAsync(country);
        CountryRecipesList.ItemsSource = meals;
    }

    private async void OnViewRecipeClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            var meal = await MealDbService.GetMealByIdAsync(id);
            if (meal != null)
            {
                await DisplayAlert(meal.Name, 
                    $"Country: {meal.Area}\nCategory: {meal.Category}\n\n{meal.Instructions.Substring(0, Math.Min(200, meal.Instructions.Length))}...", 
                    "OK");
            }
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        if (selectedCountry != null)
        {
            await LoadCountryRecipesAsync(selectedCountry);
        }
        ItemsRefreshView.IsRefreshing = false;
    }
}

public class CountryInfo
{
    public string Key { get; }
    public string Emoji { get; }
    public string Name { get; }
    public Color BackgroundColor { get; }
    public Color TextColor { get; }
    public Color ShadowColor { get; }

    public CountryInfo(string key, string emoji, string name, string bgColor, string textColor, string shadowColor)
    {
        Key = key;
        Emoji = emoji;
        Name = name;
        BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb(bgColor.Replace("#", ""));
        TextColor = Microsoft.Maui.Graphics.Color.FromArgb(textColor.Replace("#", ""));
        ShadowColor = Microsoft.Maui.Graphics.Color.FromArgb(shadowColor.Replace("#", ""));
    }
}
