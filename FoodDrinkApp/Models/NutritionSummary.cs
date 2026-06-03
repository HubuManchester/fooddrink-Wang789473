namespace FoodDrinkApp.Models;

public class NutritionSummary
{
    public int TotalItems { get; set; }
    public int TotalCalories { get; set; }
    public int TotalProtein { get; set; }
    public int TotalCarbs { get; set; }
    public int TotalFat { get; set; }
    
    public double AvgCalories => TotalItems > 0 ? (double)TotalCalories / TotalItems : 0;
    public double AvgProtein => TotalItems > 0 ? (double)TotalProtein / TotalItems : 0;
    public double AvgCarbs => TotalItems > 0 ? (double)TotalCarbs / TotalItems : 0;
    public double AvgFat => TotalItems > 0 ? (double)TotalFat / TotalItems : 0;

    public Dictionary<string, int> CategoryCounts { get; set; } = new();
    public Dictionary<string, int> CategoryCalories { get; set; } = new();
}

public class DailyNutritionGoal
{
    public int TargetCalories { get; set; } = 2000;
    public int TargetProtein { get; set; } = 150;
    public int TargetCarbs { get; set; } = 250;
    public int TargetFat { get; set; } = 65;

    public double GetCaloriesProgress(int current) => Math.Min(100, (double)current / TargetCalories * 100);
    public double GetProteinProgress(int current) => Math.Min(100, (double)current / TargetProtein * 100);
    public double GetCarbsProgress(int current) => Math.Min(100, (double)current / TargetCarbs * 100);
    public double GetFatProgress(int current) => Math.Min(100, (double)current / TargetFat * 100);
}
