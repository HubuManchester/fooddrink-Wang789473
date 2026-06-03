namespace FoodDrinkApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for detail pages
            Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
        }
    }
}
