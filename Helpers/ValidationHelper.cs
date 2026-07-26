using System.Text.RegularExpressions;

namespace StudySync.Helpers;

public static class ValidationHelper
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return EmailRegex.IsMatch(email.Trim());
    }

    public static bool IsValidPassword(string password, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            errorMessage = "Password is required.";
            return false;
        }

        if (password.Length < 6)
        {
            errorMessage = "Password must be at least 6 characters long.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public static bool DoTimesOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
    {
        return start1 < end2 && start2 < end1;
    }
}
