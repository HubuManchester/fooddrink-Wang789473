using System.Collections.ObjectModel;
using FoodDrinkApp.Models;

namespace FoodDrinkApp;

public partial class RecipePage : ContentPage
{
    private Meal _meal;

    public RecipePage(Meal meal)
    {
        InitializeComponent();
        _meal = meal;

        LoadMealData();
    }

    private void LoadMealData()
    {
        MealName.Text = _meal.Name ?? "Unknown";
        MealArea.Text = _meal.Area ?? "Unknown Cuisine";
        MealCategory.Text = _meal.Category ?? "Unknown Category";

        // Set image - use local image path
        if (!string.IsNullOrEmpty(_meal.ImageUrl))
        {
            MealImage.Source = _meal.ImageUrl;
        }
        else
        {
            // Hide image if no URL available
            MealImage.IsVisible = false;
        }

        // Set ingredients
        if (_meal.Ingredients != null && _meal.Ingredients.Count > 0)
        {
            IngredientsView.ItemsSource = new ObservableCollection<string>(_meal.Ingredients);
        }
        else
        {
            IngredientsView.ItemsSource = new ObservableCollection<string>
            {
                "No ingredient list available"
            };
        }

        // Set instructions
        InstructionsLabel.Text = _meal.Instructions ?? "No instructions available for this dish.";
    }

    private async void OnReadAloudClicked(object sender, EventArgs e)
    {
        try
        {
            string textToSpeak = $"{_meal.Name}. " +
                                 $"Cuisine: {_meal.Area ?? "Unknown"}. " +
                                 $"Category: {_meal.Category ?? "Unknown"}. " +
                                 $"Ingredients: {GetIngredientsText()}. " +
                                 $"Instructions: {_meal.Instructions ?? "No instructions available"}";

            await TextToSpeech.Default.SpeakAsync(textToSpeak);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Speech failed: {ex.Message}", "OK");
        }
    }

    private string GetIngredientsText()
    {
        if (_meal.Ingredients == null || _meal.Ingredients.Count == 0)
            return "No ingredients listed";

        return string.Join(", ", _meal.Ingredients.Take(8));
    }
}