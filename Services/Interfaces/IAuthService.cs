using StudySync.Models;

namespace StudySync.Services.Interfaces;

/// <summary>
/// Provides authentication operations including registration, login, and session management.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Gets whether a user is currently logged in.
    /// </summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// Gets the currently logged-in user's ID.
    /// </summary>
    int CurrentUserId { get; }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    Task<(bool Success, string Message)> RegisterAsync(string fullName, string email, string password, string confirmPassword);

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    Task<(bool Success, string Message)> LoginAsync(string email, string password, bool rememberMe);

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Gets the currently logged-in user.
    /// </summary>
    Task<User?> GetCurrentUserAsync();

    /// <summary>
    /// Attempts to restore a previous session from secure storage.
    /// </summary>
    Task<bool> TryAutoLoginAsync();

    /// <summary>
    /// Updates user profile information.
    /// </summary>
    Task<(bool Success, string Message)> UpdateProfileAsync(User user);
}
