using StudySync.Models;

namespace StudySync.Services.Interfaces;

/// <summary>
/// Provides access to and management of user study preferences.
/// </summary>
public interface IPreferenceService
{
    /// <summary>
    /// Gets user preferences, creating defaults if none exist.
    /// </summary>
    Task<UserPreference> GetPreferencesAsync(int userId);

    /// <summary>
    /// Saves user preferences.
    /// </summary>
    Task SavePreferencesAsync(UserPreference preferences);

    /// <summary>
    /// Applies the dark mode preference to the app theme.
    /// </summary>
    void ApplyTheme(bool darkMode);
}
