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
        // 设置菜品名称
        MealName.Text = _meal.Name ?? "Unknown";

        // 设置菜系和类别
        MealArea.Text = _meal.Area ?? "Unknown Cuisine";
        MealCategory.Text = _meal.Category ?? "Unknown Category";

        // 设置图片
        if (!string.IsNullOrEmpty(_meal.ImageUrl))
        {
            MealImage.Source = _meal.ImageUrl;
        }
        else
        {
            MealImage.Source = "default_food.png";
        }

        // 设置食材列表
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

        // 设置制作步骤
        InstructionsLabel.Text = _meal.Instructions ?? "No instructions available for this dish.";
    }

    // 语音朗读功能
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