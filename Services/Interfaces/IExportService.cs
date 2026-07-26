namespace StudySync.Services.Interfaces;

/// <summary>
/// Exports application data to JSON and CSV formats.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports all user data to a JSON file.
    /// </summary>
    /// <returns>The file path of the exported JSON.</returns>
    Task<string> ExportToJsonAsync(int userId);

    /// <summary>
    /// Exports all user data to CSV files (one per entity type).
    /// </summary>
    /// <returns>The directory path containing CSV files.</returns>
    Task<string> ExportToCsvAsync(int userId);

    /// <summary>
    /// Shares the exported file using the platform share dialog.
    /// </summary>
    Task ShareFileAsync(string filePath, string title);
}
