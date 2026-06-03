using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        ThemePicker.SelectedIndex = 0;
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
        ApplyLargeTextState();
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Application.Current!.UserAppTheme = ThemePicker.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        Announce("主题已更新");
    }

    private void OnLargeTextToggled(object? sender, ToggledEventArgs e)
    {
        AccessibilityService.LargeTextEnabled = e.Value;
        ApplyLargeTextState();
        Announce(e.Value
            ? "大字体模式已开启"
            : "大字体模式已关闭");
    }

    private void ApplyLargeTextState()
    {
        AccessibilityService.ApplyFontScale(this);

        PreviewTitle.Text = AccessibilityService.LargeTextEnabled
            ? "字体预览（已放大）"
            : "字体预览";
        PreviewBody.Text = AccessibilityService.LargeTextEnabled
            ? "开启后，应用中的文字会变得更大，方便阅读。"
            : "开启大字体模式后，这段文字会变得更大。";
    }

    private void Announce(string message)
    {
        SettingsStatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
    }
}