namespace FoodDrinkApp.Services;

public static class PhotoService
{
    public static async Task<string?> SavePhotoAsync(FileResult photo, string fileName)
    {
        try
        {
            if (photo == null) return null;

            var savedPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using var stream = await photo.OpenReadAsync();
            using var fileStream = File.OpenWrite(savedPath);
            await stream.CopyToAsync(fileStream);

            return savedPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Save photo error: {ex.Message}");
            return null;
        }
    }

    public static async Task<FileResult?> TakePhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            return photo;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Camera error: {ex.Message}");
            return null;
        }
    }
}