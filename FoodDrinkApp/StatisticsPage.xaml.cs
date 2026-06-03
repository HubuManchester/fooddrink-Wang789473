using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class StatisticsPage : ContentPage
{
    // 统计数据（可以从其他地方更新）
    public static int CompassUses { get; set; } = 0;
    public static int ShakeCount { get; set; } = 0;
    public static int CountriesExplored { get; set; } = 0;
    public static int FavoritesCount { get; set; } = 0;
    public static int ChineseCount { get; set; } = 0;
    public static int JapaneseCount { get; set; } = 0;
    public static int ItalianCount { get; set; } = 0;
    public static int MexicanCount { get; set; } = 0;

    public StatisticsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateStats();
    }

    private void UpdateStats()
    {
        CompassUsesLabel.Text = CompassUses.ToString();
        ShakeCountLabel.Text = ShakeCount.ToString();
        CountriesExploredLabel.Text = CountriesExplored.ToString();
        FavoritesCountLabel.Text = FavoritesCount.ToString();
        ChineseCountLabel.Text = ChineseCount.ToString();
        JapaneseCountLabel.Text = JapaneseCount.ToString();
        ItalianCountLabel.Text = ItalianCount.ToString();
        MexicanCountLabel.Text = MexicanCount.ToString();
    }

    private async void OnSpeakStatsClicked(object? sender, EventArgs e)
    {
        var text = $"Your exploration statistics: Compass used {CompassUses} times, " +
                   $"Shake triggered {ShakeCount} times, " +
                   $"Explored {CountriesExplored} countries, " +
                   $"Saved {FavoritesCount} favorites. " +
                   $"Cuisines explored: {ChineseCount} Chinese, {JapaneseCount} Japanese, " +
                   $"{ItalianCount} Italian, {MexicanCount} Mexican recipes.";

        try
        {
            await SpeechService.SpeakAsync(text);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Speech Error", ex.Message, "OK");
        }
    }
}
