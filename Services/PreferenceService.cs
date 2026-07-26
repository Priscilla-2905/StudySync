using StudySync.Data;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.Services;

/// <summary>
/// Manages user study preferences and app theme settings.
/// </summary>
public class PreferenceService : IPreferenceService
{
    private readonly DatabaseService _db;

    public PreferenceService(DatabaseService db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<UserPreference> GetPreferencesAsync(int userId)
    {
        return await _db.GetPreferencesAsync(userId);
    }

    /// <inheritdoc/>
    public async Task SavePreferencesAsync(UserPreference preferences)
    {
        await _db.SaveAsync(preferences);
        ApplyTheme(preferences.DarkMode);
    }

    /// <inheritdoc/>
    public void ApplyTheme(bool darkMode)
    {
        try
        {
            var appType = Type.GetType("Microsoft.Maui.Controls.Application, Microsoft.Maui.Controls");
            if (appType != null)
            {
                var currentProp = appType.GetProperty("Current");
                var currentApp = currentProp?.GetValue(null);
                if (currentApp != null)
                {
                    var themeProp = appType.GetProperty("UserAppTheme");
                    var enumType = Type.GetType("Microsoft.Maui.ApplicationModel.AppTheme, Microsoft.Maui.Essentials");
                    if (enumType != null)
                    {
                        var val = Enum.Parse(enumType, darkMode ? "Dark" : "Light");
                        themeProp?.SetValue(currentApp, val);
                    }
                }
            }
        }
        catch
        {
            // Ignore in headless test environment
        }
    }
}
