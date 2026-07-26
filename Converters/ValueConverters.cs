using System.Globalization;
using StudySync.Models;

namespace StudySync.Converters;

public class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Colors.Green;
    public Color FalseColor { get; set; } = Colors.Gray;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? TrueColor : FalseColor;
        }
        return FalseColor;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AssignmentStatus status)
        {
            return status switch
            {
                AssignmentStatus.Pending => Color.FromArgb("#FFA000"),    // Amber
                AssignmentStatus.InProgress => Color.FromArgb("#2196F3"), // Blue
                AssignmentStatus.Completed => Color.FromArgb("#4CAF50"),  // Green
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PriorityToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Priority priority)
        {
            return priority switch
            {
                Priority.Low => Color.FromArgb("#8BC34A"),      // Light Green
                Priority.Medium => Color.FromArgb("#FF9800"),   // Orange
                Priority.High => Color.FromArgb("#F44336"),     // Red
                Priority.Critical => Color.FromArgb("#D32F2F"), // Dark Red
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DateToCountdownConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            var days = (int)(date.Date - DateTime.Now.Date).TotalDays;
            return days switch
            {
                < 0 => "Overdue",
                0 => "Due Today",
                1 => "Due Tomorrow",
                _ => $"{days} days left"
            };
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }
}

public class TimeSpanToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            var dateTime = DateTime.Today.Add(ts);
            return dateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && DateTime.TryParse(s, out var dt))
        {
            return dt.TimeOfDay;
        }
        return TimeSpan.Zero;
    }
}
