using System.Collections.ObjectModel;
using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class MainPage : ContentPage
{
    private double _currentHeading;
    private ObservableCollection<Meal> _meals;
    private readonly MealApiService _apiService;
    private string _currentCuisine = "Chinese";

    private readonly Dictionary<string, string> _cuisineMap = new()
    {
        { "North", "Chinese" },
        { "East", "Japanese" },
        { "South", "Italian" },
        { "West", "Mexican" }
    };

    public MainPage()
    {
        InitializeComponent();
        _meals = new ObservableCollection<Meal>();
        MealsListView.ItemsSource = _meals;
        _apiService = new MealApiService();

        StartCompass();
        StartAccelerometer();

        LoadMealsByCuisine("Chinese");
    }

    // ========== Hardware 1: Compass / Magnetometer ==========
    private void StartCompass()
    {
        try
        {
            if (Compass.Default.IsSupported)
            {
                Compass.Default.ReadingChanged += OnCompassReadingChanged;
                Compass.Default.Start(SensorSpeed.UI);
            }
            else
            {
                StatusLabel.Text = "Compass not supported";
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Compass error: {ex.Message}";
        }
    }

    private void OnCompassReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        _currentHeading = e.Reading.HeadingMagneticNorth;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            CompassArrow.Rotation = _currentHeading;

            string direction = _currentHeading switch
            {
                >= 315 or < 45 => "North",
                >= 45 and < 135 => "East",
                >= 135 and < 225 => "South",
                >= 225 and < 315 => "West",
                _ => "North"
            };

            DirectionLabel.Text = $"Facing: {direction}";

            string cuisine = _cuisineMap[direction];
            CuisineLabel.Text = $"Current: {cuisine}";

            if (_currentCuisine != cuisine)
            {
                _currentCuisine = cuisine;
                LoadMealsByCuisine(cuisine);
            }
        });
    }

    // ========== API Integration ==========
    private async void LoadMealsByCuisine(string cuisine)
    {
        try
        {
            StatusLabel.Text = $"Loading {cuisine} dishes...";
            var meals = await _apiService.SearchByCuisine(cuisine);

            _meals.Clear();
            foreach (var meal in meals)
            {
                _meals.Add(meal);
            }

            StatusLabel.Text = $"Showing {_meals.Count} {cuisine} dishes";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
        }
    }

    private async void OnMealSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Meal selected)
        {
            StatusLabel.Text = $"Selected: {selected.Name}";

            if (!string.IsNullOrEmpty(selected.Instructions))
            {
                var shortInstructions = selected.Instructions.Length > 200
                    ? selected.Instructions.Substring(0, 200) + "..."
                    : selected.Instructions;
                await DisplayAlert(selected.Name, shortInstructions, "OK");
            }
        }
        ((CollectionView)sender).SelectedItem = null;
    }

    // ========== Hardware 2: Accelerometer / Shake ==========
    private void StartAccelerometer()
    {
        try
        {
            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.ShakeDetected += OnShakeDetected;
                Accelerometer.Default.Start(SensorSpeed.Game);
                StatusLabel.Text = "Ready - try shaking your phone!";
            }
            else
            {
                StatusLabel.Text = "Accelerometer not supported";
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Accelerometer error: {ex.Message}";
        }
    }

    private async void OnShakeDetected(object? sender, EventArgs e)
    {
        if (HapticFeedback.Default.IsSupported)
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            StatusLabel.Text = "Shake detected! Getting random recipe...";

            var randomMeal = await _apiService.GetRandomMeal();
            if (randomMeal != null && randomMeal.Name != null)
            {
                await DisplayAlert("Random Recipe",
                    $"Today's recommendation: {randomMeal.Name}\n\n" +
                    $"Cuisine: {randomMeal.Area ?? "Unknown"}\n" +
                    $"Category: {randomMeal.Category ?? "N/A"}",
                    "Let's Cook!");
                StatusLabel.Text = $"Random: {randomMeal.Name}";
            }
            else
            {
                StatusLabel.Text = "Failed to get random recipe";
            }
        });
    }

    private async void OnShakeButton(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Getting random recipe...";

        var randomMeal = await _apiService.GetRandomMeal();
        if (randomMeal != null && randomMeal.Name != null)
        {
            await DisplayAlert("Random Recipe",
                $"Today's recommendation: {randomMeal.Name}\n\n" +
                $"Cuisine: {randomMeal.Area ?? "Unknown"}\n" +
                $"Category: {randomMeal.Category ?? "N/A"}",
                "Yummy!");
            StatusLabel.Text = $"Random: {randomMeal.Name}";
        }
        else
        {
            StatusLabel.Text = "Failed to get random recipe";
        }
    }

    private void OnCameraButton(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Camera feature coming soon!";
    }
}