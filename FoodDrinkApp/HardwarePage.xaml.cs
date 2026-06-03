using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class HardwarePage : ContentPage
{
    // Counter
    private int shakeCount;
    
    // Sensor states
    private bool isCompassActive;
    private bool isShakeActive;
    
    // Accelerometer related
    private DateTime lastShakeTime = DateTime.MinValue;
    private const double ShakeThreshold = 2.5;
    private const int ShakeCooldownMs = 800;
    
    // Current cuisine
    private string currentDirection = "north";
    private List<MealDbMeal> currentCountryMeals = new();
    private MealDbMeal? currentMeal;
    private int currentMealIndex;

    public HardwarePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    protected override void OnDisappearing()
    {
        StopCompass();
        StopShakeDetection();
        SpeechService.Stop();
        base.OnDisappearing();
    }

    #region 1. Compass Function
    private void OnCompassToggled(object? sender, EventArgs e)
    {
        if (isCompassActive)
        {
            StopCompass();
            CompassButton.Text = "Start";
            CompassButton.BackgroundColor = Color.FromArgb("#D9472B");
            SetStatus("Compass stopped.");
        }
        else
        {
            StartCompass();
            CompassButton.Text = "Stop";
            CompassButton.BackgroundColor = Color.FromArgb("#D32F2F");
            SetStatus("Compass started - rotate to explore cuisines!");
        }
    }

    private void StartCompass()
    {
        try
        {
            if (!Compass.Default.IsSupported)
            {
                SetStatus("Compass not supported - use simulation button.");
                return;
            }

            Compass.Default.ReadingChanged += OnCompassReadingChanged;
            isCompassActive = true;
        }
        catch (Exception ex)
        {
            SetStatus($"Compass error: {ex.Message}");
        }
    }

    private void StopCompass()
    {
        try
        {
            Compass.Default.ReadingChanged -= OnCompassReadingChanged;
            isCompassActive = false;
            CompassArrowImage.Rotation = 0;
            CompassHeadingLabel.Text = "Heading: --";
            CuisineRegionLabel.Text = "Point your device to discover cuisines!";
            MealNameLabel.Text = "--";
            MealImageBorder.IsVisible = false;
        }
        catch { }
    }

    private void OnCompassReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var heading = e.Reading.HeadingMagneticNorth;
            CompassHeadingLabel.Text = $"Heading: {heading:F1}";
            CompassArrowImage.Rotation = -heading;
            
            var direction = GetDirectionFromHeading(heading);
            if (direction != currentDirection)
            {
                currentDirection = direction;
                _ = LoadMealsForDirectionAsync(direction);
            }
        });
    }

    private string GetDirectionFromHeading(double heading)
    {
        if (heading >= 315 || heading < 45) return "north";
        if (heading >= 45 && heading < 135) return "east";
        if (heading >= 135 && heading < 225) return "south";
        return "west";
    }

    private async Task LoadMealsForDirectionAsync(string direction)
    {
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        
        try
        {
            CuisineRegionLabel.Text = MealDbService.GetCuisineInfo(direction);
            
            var meals = await MealDbService.GetMealsByCountryAsync(direction);
            if (meals.Count > 0)
            {
                currentCountryMeals = meals;
                currentMealIndex = Random.Shared.Next(meals.Count);
                await ShowMealAsync(meals[currentMealIndex]);
            }
            else
            {
                MealNameLabel.Text = "Loading...";
                MealImageBorder.IsVisible = false;
            }
        }
        catch
        {
            MealNameLabel.Text = "Connection error";
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async Task ShowMealAsync(MealDbMeal meal)
    {
        currentMeal = meal;
        MealNameLabel.Text = meal.Name;
        MealImageBorder.IsVisible = false;
        
        if (!string.IsNullOrEmpty(meal.ThumbUrl))
        {
            try
            {
                MealImage.Source = ImageSource.FromUri(new Uri(meal.ThumbUrl));
                MealImageBorder.IsVisible = true;
            }
            catch { }
        }
        
        SetStatus($"{meal.CountryEmoji} {meal.Name}");
        SemanticScreenReader.Announce($"Showing {meal.Name} from {meal.Area}");
    }

    // Compass simulation state
    private bool isSimulatingCompass;
    private double simulatedHeading;
    
    private async void OnSimulateCompassClicked(object? sender, EventArgs e)
    {
        if (isSimulatingCompass)
        {
            isSimulatingCompass = false;
            SimulateCompassButton.Text = "Simulate Rotation (for emulator)";
            SetStatus("Simulation stopped.");
            return;
        }
        
        isSimulatingCompass = true;
        SimulateCompassButton.Text = "Stop Simulation";
        SetStatus("Simulating compass rotation - watch cuisines change!");
        
        // Simulation loop
        while (isSimulatingCompass)
        {
            simulatedHeading += 15;
            if (simulatedHeading >= 360) simulatedHeading = 0;
            
            CompassHeadingLabel.Text = $"Heading: {simulatedHeading:F1}";
            CompassArrowImage.Rotation = -simulatedHeading;
            
            var direction = GetDirectionFromHeading(simulatedHeading);
            if (direction != currentDirection)
            {
                currentDirection = direction;
                await LoadMealsForDirectionAsync(direction);
            }
            
            await Task.Delay(800); // Delay between direction changes
        }
    }
    #endregion

    #region 2. Shake for Random Recipe
    private void OnShakeToggled(object? sender, EventArgs e)
    {
        if (isShakeActive)
        {
            StopShakeDetection();
            ShakeButton.Text = "Start";
            ShakeButton.BackgroundColor = Color.FromArgb("#7B1FA2");
            ShakeStatusLabel.Text = "Tap 'Start' then shake your device!";
            SetStatus("Shake detection stopped.");
        }
        else
        {
            StartShakeDetection();
            ShakeButton.Text = "Stop";
            ShakeButton.BackgroundColor = Color.FromArgb("#D32F2F");
            ShakeStatusLabel.Text = "Shake your device!";
            SetStatus("Shake detection started - shake for random recipe!");
        }
    }

    private void StartShakeDetection()
    {
        try
        {
            if (!Accelerometer.Default.IsSupported)
            {
                SetStatus("Accelerometer not supported on this device.");
                return;
            }

            Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
            isShakeActive = true;
            ShakeIconLabel.Text = "📱";
            ShakeStatusLabel.Text = "Shake your device!";
        }
        catch (Exception ex)
        {
            SetStatus($"Accelerometer error: {ex.Message}");
        }
    }

    private void StopShakeDetection()
    {
        try
        {
            Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
            isShakeActive = false;
            ShakeIconLabel.Text = "📱";
            ShakeStatusLabel.Text = "Detection stopped";
        }
        catch { }
    }

    private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var acceleration = e.Reading.Acceleration;
        double magnitude = Math.Sqrt(
            acceleration.X * acceleration.X + 
            acceleration.Y * acceleration.Y + 
            acceleration.Z * acceleration.Z);
        double netAcceleration = Math.Abs(magnitude - 1.0);

        if (netAcceleration > ShakeThreshold)
        {
            var now = DateTime.Now;
            if ((now - lastShakeTime).TotalMilliseconds > ShakeCooldownMs)
            {
                lastShakeTime = now;
                MainThread.BeginInvokeOnMainThread(OnShakeDetected);
            }
        }
    }

    private async void OnShakeDetected()
    {
        shakeCount++;
        ShakeCountLabel.Text = $"Shake count: {shakeCount}";
        ShakeIconLabel.Text = "📳";
        ShakeStatusLabel.Text = "Getting random recipe...";

        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
        HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);

        var meal = await MealDbService.GetRandomMealAsync();
        if (meal != null)
        {
            RandomRecipeBorder.IsVisible = true;
            RandomMealNameLabel.Text = $"{meal.CountryEmoji} {meal.Name}";
            RandomMealAreaLabel.Text = meal.CountryName;

            currentMeal = meal;

            try
            {
                await SpeechService.SpeakAsync($"Random recipe: {meal.Name} from {meal.Area}");
            }
            catch { }

            SetStatus($"Shake #{shakeCount}: {meal.Name} ({meal.Area})");
            SemanticScreenReader.Announce($"Random recipe: {meal.Name}");
        }
        else
        {
            ShakeStatusLabel.Text = "Could not load recipe, try again!";
        }

        await Task.Delay(500);
        ShakeIconLabel.Text = "📱";
    }

    // Navigate to detail page when clicking the recommended recipe
    private async void OnRandomRecipeClicked(object? sender, EventArgs e)
    {
        if (currentMeal == null)
        {
            SetStatus("No recipe to display!");
            return;
        }

        try
        {
            // Store meal data for detail page
            Preferences.Set("SelectedMealId", currentMeal.Id);
            Preferences.Set("SelectedMealName", currentMeal.Name);
            Preferences.Set("SelectedMealCategory", currentMeal.Category);
            Preferences.Set("SelectedMealArea", currentMeal.Area);
            Preferences.Set("SelectedMealThumb", currentMeal.ThumbUrl ?? "");

            // Navigate to detail page
            var detailPage = new RecipeDetailPage();
            await Shell.Current.Navigation.PushAsync(detailPage);
        }
        catch (Exception ex)
        {
            SetStatus($"Navigation error: {ex.Message}");
        }
    }

    // Simulate shake button click - jumps to detail page directly (for demo)
    private async void OnSimulateShakeClicked(object? sender, EventArgs e)
    {
        try
        {
            SetStatus("Simulating shake...");
            
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            
            shakeCount++;
            ShakeCountLabel.Text = $"Shake count: {shakeCount}";
            ShakeIconLabel.Text = "📳";
            ShakeStatusLabel.Text = "Random recipe found!";
            
            // Create mock random meal
            var mockMeal = new MealDbMeal
            {
                Id = $"shake_{shakeCount}",
                Name = "Sushi Platter",
                Category = "Seafood",
                Area = "Japanese",
                ThumbUrl = "https://www.themealdb.com/images/media/meals/1549542994.jpg"
            };
            
            RandomRecipeBorder.IsVisible = true;
            RandomMealNameLabel.Text = $"{mockMeal.CountryEmoji} {mockMeal.Name}";
            RandomMealAreaLabel.Text = mockMeal.CountryName;
            currentMeal = mockMeal;
            
            await Task.Delay(800);
            
            // Store data and navigate
            Preferences.Set("SelectedMealId", mockMeal.Id);
            Preferences.Set("SelectedMealName", mockMeal.Name);
            Preferences.Set("SelectedMealCategory", mockMeal.Category);
            Preferences.Set("SelectedMealArea", mockMeal.Area);
            Preferences.Set("SelectedMealThumb", mockMeal.ThumbUrl);
            
            var detailPage = new RecipeDetailPage();
            await Shell.Current.Navigation.PushAsync(detailPage);
            
            SetStatus($"Shake #{shakeCount}: {mockMeal.Name}");
        }
        catch (Exception ex)
        {
            SetStatus($"Shake error: {ex.Message}");
        }
    }
    #endregion

    #region 3. Camera
    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                SetStatus("Camera not supported on this device.");
                return;
            }
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                SetStatus("Photo capture cancelled.");
                return;
            }
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SetStatus("Food photo captured!");
        }
        catch (PermissionException)
        {
            SetStatus("Camera permission denied.");
        }
        catch (Exception ex)
        {
            SetStatus($"Camera error: {ex.Message}");
        }
    }
    #endregion

    #region 4. GPS Location
    private async void OnGetLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            SetStatus("Getting location...");
            
            await Task.Delay(1000); // Simulate location delay
            
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SetStatus("Location: Beijing, China (39.9042N, 116.4074E)");
            
            // Mock meal data
            var mockMeal = new MealDbMeal
            {
                Id = "gps_mock",
                Name = "Beijing Roast Duck",
                Category = "Main Course",
                Area = "Chinese",
                ThumbUrl = "https://www.themealdb.com/images/media/meals/1529442316.jpg"
            };
            
            // Store data and navigate to detail page
            Preferences.Set("SelectedMealId", mockMeal.Id);
            Preferences.Set("SelectedMealName", mockMeal.Name);
            Preferences.Set("SelectedMealCategory", mockMeal.Category);
            Preferences.Set("SelectedMealArea", mockMeal.Area);
            Preferences.Set("SelectedMealThumb", mockMeal.ThumbUrl);
            
            var detailPage = new RecipeDetailPage();
            await Shell.Current.Navigation.PushAsync(detailPage);
        }
        catch (Exception ex)
        {
            SetStatus($"GPS error: {ex.Message}");
        }
    }
    #endregion

    #region 5. Text-to-Speech
    private async void OnReadHelpClicked(object? sender, EventArgs e)
    {
        try
        {
            string helpText;
            if (currentMeal != null)
            {
                helpText = currentMeal.GetAccessibleSummary();
            }
            else
            {
                helpText = "Welcome to Smart Recipe Explorer! Rotate your device to discover cuisines from different countries. Shake your device to get random recipe recommendations.";
            }
            
            await SpeechService.SpeakAsync(helpText);
            SetStatus("Reading recipe information...");
        }
        catch (Exception ex)
        {
            SetStatus($"Speech error: {ex.Message}");
        }
    }
    #endregion

    private void SetStatus(string message)
    {
        HardwareStatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
    }
}
