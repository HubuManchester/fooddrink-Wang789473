namespace FoodDrinkApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnRecommendClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "Today's Recommendation: 🍜 Beef Noodles | 🍛 Chicken Curry | 🥗 Garden Salad";
    }

    private void OnCameraClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "Camera ready. Photo feature coming soon.";
    }

    private void OnSpeakClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "Voice assistant ready. TTS feature coming soon.";
    }
}