using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

[QueryProperty(nameof(ItemId), "id")]
public partial class FoodDetailPage : ContentPage
{
    private FoodItem? currentItem;

    public FoodDetailPage()
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
        SpeechService.Stop();
        base.OnDisappearing();
    }

    public string ItemId
    {
        set => _ = LoadItemAsync(value);
    }

    private async Task LoadItemAsync(string id)
    {
        currentItem = await FoodCatalogService.GetByIdAsync(id);
        BindingContext = currentItem;
        RenderItem();
    }

    private void RenderItem()
    {
        if (currentItem is null)
        {
            NameLabel.Text = "记录未找到";
            DescriptionLabel.Text = "所选食物记录无法加载";
            AllergyBorder.IsVisible = false;
            return;
        }

        CategoryIconLabel.Text = currentItem.CategoryIcon;
        NameLabel.Text = currentItem.Name;
        CategoryLabel.Text = currentItem.Category;
        CaloriesLabel.Text = currentItem.Calories.ToString("N0");
        DescriptionLabel.Text = currentItem.Description;
        AllergyLabel.Text = currentItem.AllergyNote;

        // 宏量营养素
        ProteinLabel.Text = $"{currentItem.Protein}g";
        CarbsLabel.Text = $"{currentItem.Carbs}g";
        FatLabel.Text = $"{currentItem.Fat}g";

        // 进度条
        ProteinProgress.Progress = currentItem.ProteinPercentage / 100;
        CarbsProgress.Progress = currentItem.CarbsPercentage / 100;
        FatProgress.Progress = currentItem.FatPercentage / 100;

        // 过敏提示
        AllergyBorder.IsVisible = !string.IsNullOrWhiteSpace(currentItem.AllergyNote) && 
                                   !currentItem.AllergyNote.Contains("无");

        SemanticProperties.SetDescription(NameLabel, currentItem.AccessibleSummary);
    }

    private async void OnSpeakClicked(object? sender, EventArgs e)
    {
        if (currentItem is null)
        {
            await DisplayAlert("提示", "没有可朗读的内容", "确定");
            return;
        }

        try
        {
            await SpeechService.SpeakAsync(currentItem.AccessibleSummary);
        }
        catch (Exception ex)
        {
            await DisplayAlert("语音播报失败", ex.Message, "确定");
        }
    }

    private void OnStopSpeechClicked(object? sender, EventArgs e)
    {
        SpeechService.Stop();
        SemanticScreenReader.Announce("已停止朗读");
    }

    private async void OnVibrateClicked(object? sender, EventArgs e)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            await DisplayAlert("提醒", "已触发震动反馈", "确定");
        }
        catch (Exception ex)
        {
            await DisplayAlert("震动不可用", ex.Message, "确定");
        }
    }
}
