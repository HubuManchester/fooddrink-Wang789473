namespace FoodDrinkApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // 注册页面路由
        Routing.RegisterRoute(nameof(FoodDetailPage), typeof(FoodDetailPage));
        Routing.RegisterRoute(nameof(AddItemPage), typeof(AddItemPage));
        Routing.RegisterRoute(nameof(CategoriesPage), typeof(CategoriesPage));
        Routing.RegisterRoute(nameof(FavoritesPage), typeof(FavoritesPage));
        Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));
        Routing.RegisterRoute(nameof(HardwarePage), typeof(HardwarePage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

        MainPage = new AppShell();
    }
}
