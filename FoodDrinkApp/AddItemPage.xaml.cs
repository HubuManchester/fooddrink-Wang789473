using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class AddItemPage : ContentPage
{
    public AddItemPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            var validationMessage = ValidateForm(out var calories, out var protein, out var carbs, out var fat);
            if (validationMessage is not null)
            {
                ShowValidation(validationMessage);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250));
                return;
            }

            var item = new FoodItem
            {
                Name = NameEntry.Text!.Trim(),
                Category = CategoryPicker.SelectedItem?.ToString() ?? "Snack",
                Description = DescriptionEditor.Text?.Trim() ?? string.Empty,
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                AllergyNote = string.IsNullOrWhiteSpace(AllergyEntry.Text)
                    ? "无过敏原记录"
                    : AllergyEntry.Text.Trim(),
                Tags = $"{NameEntry.Text} {CategoryPicker.SelectedItem} {DescriptionEditor.Text}",
                CreatedAt = DateTime.Now
            };

            await FoodCatalogService.AddAsync(item);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce("食物记录已保存");

            await DisplayAlert(
                "保存成功",
                MockApiConfig.IsConfigured
                    ? "记录已保存到云端"
                    : "记录已保存到本地",
                "确定");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ShowValidation($"保存失败：{ex.Message}");
        }
    }

    private string? ValidateForm(out int calories, out int protein, out int carbs, out int fat)
    {
        calories = protein = carbs = fat = 0;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            return "请输入食物名称";
        }

        if (CategoryPicker.SelectedIndex < 0)
        {
            return "请选择分类";
        }

        return TryReadNumber(CaloriesEntry.Text, "热量", out calories)
            ?? TryReadNumber(ProteinEntry.Text, "蛋白质", out protein, true)
            ?? TryReadNumber(CarbsEntry.Text, "碳水化合物", out carbs, true)
            ?? TryReadNumber(FatEntry.Text, "脂肪", out fat, true);
    }

    private static string? TryReadNumber(string? value, string fieldName, out int number, bool allowEmpty = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            number = 0;
            return allowEmpty ? null : $"请输入{fieldName}";
        }

        if (int.TryParse(value, out number) && number >= 0)
        {
            return null;
        }

        return $"{fieldName}必须是有效的非负数";
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationPanel.IsVisible = true;
        SemanticScreenReader.Announce(message);
    }
}
