using System.Security.Cryptography;
using System.Text;
using StudySync.Data;
using StudySync.Models;
using StudySync.Services.Interfaces;

namespace StudySync.Services;

/// <summary>
/// Handles user authentication with SHA256 password hashing and secure session storage.
/// </summary>
public class AuthenticationService : IAuthService
{
    private readonly DatabaseService _db;
    private User? _currentUser;
    private const string SessionKey = "auth_session_user_id";
    private const string RememberKey = "auth_remember_me";

    public AuthenticationService(DatabaseService db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public bool IsLoggedIn => _currentUser is not null;

    /// <inheritdoc/>
    public int CurrentUserId => _currentUser?.Id ?? 0;

    /// <inheritdoc/>
    public async Task<(bool Success, string Message)> RegisterAsync(string fullName, string email, string password, string confirmPassword)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(fullName))
            return (false, "Full name is required.");

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            return (false, "Please enter a valid email address.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters long.");

        if (password != confirmPassword)
            return (false, "Passwords do not match.");

        // Check for existing user
        var existingUser = await _db.GetUserByEmailAsync(email.Trim().ToLowerInvariant());
        if (existingUser is not null)
            return (false, "An account with this email already exists.");

        // Create user with hashed password
        var salt = GenerateSalt();
        var passwordHash = HashPassword(password, salt);

        var user = new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Salt = salt,
            CreatedAt = DateTime.UtcNow
        };

        await _db.SaveAsync(user);

        // Create default preferences
        var prefs = new UserPreference { UserId = user.Id };
        await _db.SaveAsync(prefs);

        return (true, "Account created successfully!");
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string Message)> LoginAsync(string email, string password, bool rememberMe)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Please enter your email and password.");

        var user = await _db.GetUserByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null)
            return (false, "Invalid email or password.");

        var passwordHash = HashPassword(password, user.Salt);
        if (passwordHash != user.PasswordHash)
            return (false, "Invalid email or password.");

        _currentUser = user;

        // Store session if rememberMe is requested
        if (rememberMe)
        {
            SaveSessionPreference(user.Id);
        }

        return (true, "Login successful!");
    }

    /// <inheritdoc/>
    public async Task LogoutAsync()
    {
        _currentUser = null;
        ClearSessionPreference();
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<User?> GetCurrentUserAsync()
    {
        return Task.FromResult(_currentUser);
    }

    /// <inheritdoc/>
    public async Task<bool> TryAutoLoginAsync()
    {
        try
        {
            var userIdStr = GetStoredSessionUserId();
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var userId))
            {
                var user = await _db.GetByIdAsync<User>(userId);
                if (user is not null)
                {
                    _currentUser = user;
                    return true;
                }
            }
        }
        catch
        {
            // Auto-login failed silently
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string Message)> UpdateProfileAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.FullName))
            return (false, "Full name is required.");

        await _db.SaveAsync(user);
        _currentUser = user;
        return (true, "Profile updated successfully!");
    }

    // ── Private Helpers ──────────────────────────────────────────────

    private static void SaveSessionPreference(int userId)
    {
        try
        {
            // Reflection based invocation to avoid hard compile dependency in non-MAUI test runner
            var secureType = Type.GetType("Microsoft.Maui.Storage.SecureStorage, Microsoft.Maui.Essentials");
            if (secureType != null)
            {
                var setAsync = secureType.GetMethod("SetAsync", new[] { typeof(string), typeof(string) });
                setAsync?.Invoke(null, new object[] { SessionKey, userId.ToString() });
            }
        }
        catch
        {
            // Ignore in unit tests
        }
    }

    private static void ClearSessionPreference()
    {
        try
        {
            var secureType = Type.GetType("Microsoft.Maui.Storage.SecureStorage, Microsoft.Maui.Essentials");
            if (secureType != null)
            {
                var remove = secureType.GetMethod("Remove", new[] { typeof(string) });
                remove?.Invoke(null, new object[] { SessionKey });
            }
        }
        catch
        {
            // Ignore in unit tests
        }
    }

    private static string? GetStoredSessionUserId()
    {
        try
        {
            var secureType = Type.GetType("Microsoft.Maui.Storage.SecureStorage, Microsoft.Maui.Essentials");
            if (secureType != null)
            {
                var getAsync = secureType.GetMethod("GetAsync", new[] { typeof(string) });
                var task = getAsync?.Invoke(null, new object[] { SessionKey }) as Task<string?>;
                return task?.Result;
            }
        }
        catch
        {
            // Fallback
        }
        return null;
    }

    private static string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
    {
        var combined = $"{salt}{password}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(hashBytes);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }
}
