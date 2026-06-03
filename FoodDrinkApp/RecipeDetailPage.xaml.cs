using FoodDrinkApp.Models;
using FoodDrinkApp.Services;
using Microsoft.Maui.Controls;

namespace FoodDrinkApp;

public partial class RecipeDetailPage : ContentPage
{
    private MealDbMeal? _meal;
    private string _mealId = string.Empty;
    private bool _isFavorite = false;

    public RecipeDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);

        try
        {
            if (Preferences.ContainsKey("SelectedMealId"))
            {
                _mealId = Preferences.Get("SelectedMealId", "");
                Preferences.Remove("SelectedMealId");

                if (!string.IsNullOrEmpty(_mealId))
                {
                    LoadMealDetail(_mealId);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
    }

    private void LoadMealDetail(string id)
    {
        var name = Preferences.Get("SelectedMealName", "Delicious Recipe");
        var category = Preferences.Get("SelectedMealCategory", "Main Course");
        var area = Preferences.Get("SelectedMealArea", "International");
        var thumbUrl = Preferences.Get("SelectedMealThumb", "");

        Preferences.Remove("SelectedMealName");
        Preferences.Remove("SelectedMealCategory");
        Preferences.Remove("SelectedMealArea");
        Preferences.Remove("SelectedMealThumb");

        _meal = new MealDbMeal
        {
            Id = id,
            Name = name,
            Category = category,
            Area = area,
            ThumbUrl = thumbUrl
        };

        DisplayMeal(_meal);
    }

    private void DisplayMeal(MealDbMeal meal)
    {
        if (!string.IsNullOrEmpty(meal.ThumbUrl))
        {
            MealImage.Source = meal.ThumbUrl;
        }

        CountryEmojiLabel.Text = meal.CountryEmoji;
        MealNameLabel.Text = meal.Name;
        CategoryLabel.Text = meal.Category;
        AreaLabel.Text = meal.Area;
        DescriptionLabel.Text = GetDescription(meal.Area);

        _isFavorite = FavoritesPage.IsFavorite(meal.Id);
        UpdateFavoriteButton();
    }

    private string GetDescription(string area)
    {
        return area.ToLower() switch
        {
            "chinese" => "Chinese cuisine - Rich cooking traditions with exquisite flavors.",
            "japanese" => "Japanese cuisine - Fresh ingredients with masterful preparation.",
            "italian" => "Italian cuisine - Classic pizza and pasta with Mediterranean flair.",
            "mexican" => "Mexican cuisine - Bold spices and vibrant flavors.",
            _ => "A delicious international recipe worth trying!"
        };
    }

    private void UpdateFavoriteButton()
    {
        if (_isFavorite)
        {
            FavoriteButton.Text = "Loved";
            FavoriteActionButton.Text = "Remove Favorite";
        }
        else
        {
            FavoriteButton.Text = "Love";
            FavoriteActionButton.Text = "Add Favorite";
        }
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        Shell.Current?.Navigation.PopAsync();
    }

    private void OnFavoriteClicked(object? sender, EventArgs e)
    {
        if (_meal == null) return;

        if (_isFavorite)
        {
            FavoritesPage.RemoveFavorite(_meal);
            _isFavorite = false;
        }
        else
        {
            FavoritesPage.AddFavorite(_meal);
            _isFavorite = true;
        }

        UpdateFavoriteButton();
        var message = _isFavorite ? "Added to favorites!" : "Removed from favorites";
        DisplayAlert(_isFavorite ? "OK" : "OK", message, "OK");
    }

    private async void OnSpeakClicked(object? sender, EventArgs e)
    {
        if (_meal == null) return;

        var textToSpeak = $"{_meal.Name}. {GetDescription(_meal.Area)}";

        try
        {
            SpeakButton.IsEnabled = false;
            SpeakButton.Text = "Speaking...";
            await TextToSpeech.Default.SpeakAsync(textToSpeak);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Speech error: {ex.Message}");
        }
        finally
        {
            SpeakButton.IsEnabled = true;
            SpeakButton.Text = "Read Aloud";
        }
    }
}
