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
                StatusLabel.Text = "Compass ready";
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

            StatusLabel.Text = _meals.Count > 0
                ? $"Showing {_meals.Count} {cuisine} dishes"
                : $"No dishes found for {cuisine}";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
        }
    }

    // ========== Tap Gesture for Meal Selection ==========
    private async void OnMealTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var meal = frame?.BindingContext as Meal;
        if (meal != null)
        {
            StatusLabel.Text = $"Opening: {meal.Name}";
            await Navigation.PushAsync(new RecipePage(meal));
        }
    }

    // ========== Random and Menu Buttons ==========
    private async void OnRandomButtonClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "Getting random recipe...";

        var randomMeal = await _apiService.GetRandomMeal();
        if (randomMeal != null && randomMeal.Name != null)
        {
            await Navigation.PushAsync(new RecipePage(randomMeal));
            StatusLabel.Text = $"Random: {randomMeal.Name}";
        }
        else
        {
            StatusLabel.Text = "Failed to get random recipe";
            await DisplayAlert("Error", "Unable to fetch random recipe. Please check your network connection.", "OK");
        }
    }

    private void OnMenuButtonClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "Menu ready";
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
                await Navigation.PushAsync(new RecipePage(randomMeal));
                StatusLabel.Text = $"Random: {randomMeal.Name}";
            }
            else
            {
                StatusLabel.Text = "Failed to get random recipe";
                await DisplayAlert("Error", "Unable to fetch random recipe. Please check your network connection.", "OK");
            }
        });
    }

    private async void OnShakeButton(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Getting random recipe...";

        var randomMeal = await _apiService.GetRandomMeal();
        if (randomMeal != null && randomMeal.Name != null)
        {
            await Navigation.PushAsync(new RecipePage(randomMeal));
            StatusLabel.Text = $"Random: {randomMeal.Name}";
        }
        else
        {
            StatusLabel.Text = "Failed to get random recipe";
            await DisplayAlert("Error", "Unable to fetch random recipe. Please check your network connection.", "OK");
        }
    }

    // ========== Hardware 3: Text-to-Speech ==========
    private async void OnSpeakButton(object sender, EventArgs e)
    {
        try
        {
            if (_meals.Count > 0)
            {
                var firstMeal = _meals[0];
                string textToSpeak = $"{firstMeal.Name}. " +
                                     $"This is a {firstMeal.Category ?? "delicious"} dish. " +
                                     $"Enjoy your meal!";

                await TextToSpeech.Default.SpeakAsync(textToSpeak);
                StatusLabel.Text = $"Speaking: {firstMeal.Name}";
            }
            else
            {
                StatusLabel.Text = "No dishes to speak";
                await TextToSpeech.Default.SpeakAsync("No dishes available");
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"TTS error: {ex.Message}";
            await DisplayAlert("Error", "Text to speech is not available on this device.", "OK");
        }
    }

    // ========== Hardware 4: Camera ==========
    private async void OnCameraButton(object? sender, EventArgs e)
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    StatusLabel.Text = "Camera permission denied";
                    await DisplayAlert("Permission", "Camera permission is required to take photos", "OK");
                    return;
                }
            }

            StatusLabel.Text = "Opening camera...";

            var photo = await PhotoService.TakePhotoAsync();
            if (photo != null)
            {
                var fileName = $"food_photo_{DateTime.Now.Ticks}.jpg";
                var savedPath = await PhotoService.SavePhotoAsync(photo, fileName);

                if (savedPath != null)
                {
                    StatusLabel.Text = $"Photo saved!";
                    await DisplayAlert("Success", "Your food photo has been saved successfully!", "Great!");
                }
                else
                {
                    StatusLabel.Text = "Failed to save photo";
                    await DisplayAlert("Error", "Failed to save the photo. Storage may be unavailable.", "OK");
                }
            }
            else
            {
                StatusLabel.Text = "Camera cancelled or not available";
                await DisplayAlert("Info", "Camera may not be available in emulator. Please test on a physical device.", "OK");
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Camera error: {ex.Message}";
            await DisplayAlert("Camera Error", "Unable to access camera. Please check permissions.", "OK");
        }
    }

    // ========== Hardware 5: GPS / Location ==========
    private async void OnLocationButtonClicked(object sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "Getting location...";

            // 使用模拟位置（不需要真实GPS）
            var mockLocation = LocationService.GetMockLocation();

            await DisplayAlert("Nearby Restaurants",
                $"📍 Found these places near you:\n\n" +
                $"• {mockLocation}\n" +
                $"• 🍣 Sushi Restaurant\n" +
                $"• 🥘 Thai Cuisine\n" +
                $"• 🍝 Italian Bistro\n\n" +
                $"Note: Using demo mode - showing mock data",
                "OK");

            StatusLabel.Text = "Location: Demo mode";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Location error: {ex.Message}";
            await DisplayAlert("Info", "Using demo location data. No GPS required.", "OK");
        }
    }
}