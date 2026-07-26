using SQLite;
using StudySync.Models;

namespace StudySync.Data;

/// <summary>
/// Manages the SQLite database connection and table creation.
/// Provides a singleton async connection to the local database.
/// </summary>
public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public DatabaseService(string? customDbPath = null)
    {
        if (!string.IsNullOrEmpty(customDbPath))
        {
            _dbPath = customDbPath;
        }
        else
        {
            try
            {
                var fileSystemType = Type.GetType("Microsoft.Maui.Storage.FileSystem, Microsoft.Maui.Essentials");
                var appDataProp = fileSystemType?.GetProperty("AppDataDirectory");
                var appData = appDataProp?.GetValue(null) as string;

                if (!string.IsNullOrEmpty(appData))
                {
                    _dbPath = Path.Combine(appData, "studysync.db3");
                }
                else
                {
                    var tempDir = Path.Combine(Path.GetTempPath(), "StudySync");
                    Directory.CreateDirectory(tempDir);
                    _dbPath = Path.Combine(tempDir, "studysync.db3");
                }
            }
            catch
            {
                // Fallback for unit testing environments without MAUI Essentials initialized
                var tempDir = Path.Combine(Path.GetTempPath(), "StudySync");
                Directory.CreateDirectory(tempDir);
                _dbPath = Path.Combine(tempDir, "studysync.db3");
            }
        }
    }

    /// <summary>
    /// Gets or creates the database connection and ensures all tables exist.
    /// </summary>
    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_database is not null)
            return _database;

        _database = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);

        // Create all tables
        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<Course>();
        await _database.CreateTableAsync<TimetableEntry>();
        await _database.CreateTableAsync<Assignment>();
        await _database.CreateTableAsync<Exam>();
        await _database.CreateTableAsync<StudySession>();
        await _database.CreateTableAsync<UserPreference>();
        await _database.CreateTableAsync<TodoItem>();
        await _database.CreateTableAsync<PomodoroSession>();

        return _database;
    }

    /// <summary>
    /// Gets the async database connection.
    /// </summary>
    public Task<SQLiteAsyncConnection> GetDatabaseAsync() => GetConnectionAsync();

    // ── Generic CRUD Operations ──────────────────────────────────────

    /// <summary>
    /// Retrieves all records of type T.
    /// </summary>
    public async Task<List<T>> GetAllAsync<T>() where T : new()
    {
        var db = await GetConnectionAsync();
        return await db.Table<T>().ToListAsync();
    }

    /// <summary>
    /// Retrieves a single record by its primary key.
    /// </summary>
    public async Task<T?> GetByIdAsync<T>(int id) where T : new()
    {
        var db = await GetConnectionAsync();
        try
        {
            return await db.GetAsync<T>(id);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Inserts or updates a record. Returns the number of rows affected.
    /// </summary>
    public async Task<int> SaveAsync<T>(T item) where T : new()
    {
        var db = await GetConnectionAsync();
        var idProp = typeof(T).GetProperty("Id");
        if (idProp is not null)
        {
            var id = (int)(idProp.GetValue(item) ?? 0);
            if (id != 0)
                return await db.UpdateAsync(item);
        }
        return await db.InsertAsync(item);
    }

    /// <summary>
    /// Deletes a record. Returns the number of rows affected.
    /// </summary>
    public async Task<int> DeleteAsync<T>(T item) where T : new()
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync(item);
    }

    // ── User-scoped Query Helpers ────────────────────────────────────

    /// <summary>
    /// Gets all courses for a specific user.
    /// </summary>
    public async Task<List<Course>> GetCoursesAsync(int userId)
    {
        var db = await GetConnectionAsync();
        return await db.Table<Course>().Where(c => c.UserId == userId).ToListAsync();
    }

    /// <summary>
    /// Gets all timetable entries for a specific user.
    /// </summary>
    public async Task<List<TimetableEntry>> GetTimetableAsync(int userId)
    {
        var db = await GetConnectionAsync();
        return await db.Table<TimetableEntry>().Where(t => t.UserId == userId).ToListAsync();
    }

    /// <summary>
    /// Gets timetable entries for a specific day.
    /// </summary>
    public async Task<List<TimetableEntry>> GetTimetableForDayAsync(int userId, StudyDay day)
    {
        var db = await GetConnectionAsync();
        return await db.Table<TimetableEntry>()
            .Where(t => t.UserId == userId && t.Day == day)
            .OrderBy(t => t.StartTimeTicks)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all assignments for a specific user.
    /// </summary>
    public async Task<List<Assignment>> GetAssignmentsAsync(int userId)
    {
        var db = await GetConnectionAsync();
        return await db.Table<Assignment>().Where(a => a.UserId == userId).ToListAsync();
    }

    /// <summary>
    /// Gets pending assignments for a specific user, ordered by deadline.
    /// </summary>
    public async Task<List<Assignment>> GetPendingAssignmentsAsync(int userId)
    {
        var db = await GetConnectionAsync();
        return await db.Table<Assignment>()
            .Where(a => a.UserId == userId && a.Status != AssignmentStatus.Completed)
            .OrderBy(a => a.Deadline)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all exams for a specific user.
    /// </summary>
    public async Task<List<Exam>> GetExamsAsync(int userId)
    {
        var db = await GetConnectionAsync();
        return await db.Table<Exam>().Where(e => e.UserId == userId).ToListAsync();
    }

    /// <summary>
    /// Gets upcoming exams for a specific user.
    /// </summary>
    public async Task<List<Exam>> GetUpcomingExamsAsync(int userId)
    {
        var db = await GetConnectionAsync();
        var today = DateTime.Now.Date;
        return await db.Table<Exam>()
            .Where(e => e.UserId == userId && e.ExamDate >= today)
            .OrderBy(e => e.ExamDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets study sessions for a specific user and date.
    /// </summary>
    public async Task<List<StudySession>> GetStudySessionsAsync(int userId, DateTime date)
    {
        var db = await GetConnectionAsync();
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);
        return await db.Table<StudySession>()
            .Where(s => s.UserId == userId && s.Date >= startOfDay && s.Date < endOfDay)
            .OrderBy(s => s.StartTimeTicks)
            .ToListAsync();
    }

    /// <summary>
    /// Gets study sessions for a date range.
    /// </summary>
    public async Task<List<StudySession>> GetStudySessionsRangeAsync(int userId, DateTime start, DateTime end)
    {
        var db = await GetConnectionAsync();
        return await db.Table<StudySession>()
            .Where(s => s.UserId == userId && s.Date >= start.Date && s.Date <= end.Date)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTimeTicks)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all study sessions for a user.
    /// </summary>
    public async Task<List<StudySession>> GetAllStudySessionsAsync(int userId)
    {
        var db = await GetConnectionAsync();
        return await db.Table<StudySession>()
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTimeTicks)
            .ToListAsync();
    }

    /// <summary>
    /// Gets user preferences, creating defaults if none exist.
    /// </summary>
    public async Task<UserPreference> GetPreferencesAsync(int userId)
    {
        var db = await GetConnectionAsync();
        var prefs = await db.Table<UserPreference>().FirstOrDefaultAsync(p => p.UserId == userId);
        if (prefs is null)
        {
            prefs = new UserPreference { UserId = userId };
            await db.InsertAsync(prefs);
        }
        return prefs;
    }

    /// <summary>
    /// Gets todo items for a specific date.
    /// </summary>
    public async Task<List<TodoItem>> GetTodoItemsAsync(int userId, DateTime date)
    {
        var db = await GetConnectionAsync();
        var targetDate = date.Date;
        return await db.Table<TodoItem>()
            .Where(t => t.UserId == userId && t.Date == targetDate)
            .OrderBy(t => t.Order)
            .ToListAsync();
    }

    /// <summary>
    /// Gets Pomodoro sessions for a date range.
    /// </summary>
    public async Task<List<PomodoroSession>> GetPomodoroSessionsAsync(int userId, DateTime start, DateTime end)
    {
        var db = await GetConnectionAsync();
        return await db.Table<PomodoroSession>()
            .Where(p => p.UserId == userId && p.Date >= start.Date && p.Date <= end.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var db = await GetConnectionAsync();
        return await db.Table<User>().FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Deletes all study sessions for a user on or after a specific date.
    /// Used when regenerating the schedule.
    /// </summary>
    public async Task DeleteFutureStudySessionsAsync(int userId, DateTime fromDate)
    {
        var db = await GetConnectionAsync();
        var sessions = await db.Table<StudySession>()
            .Where(s => s.UserId == userId && s.Date >= fromDate.Date && !s.Completed)
            .ToListAsync();

        foreach (var session in sessions)
            await db.DeleteAsync(session);
    }

    /// <summary>
    /// Deletes auto-generated todo items for a date.
    /// </summary>
    public async Task DeleteAutoGeneratedTodosAsync(int userId, DateTime date)
    {
        var db = await GetConnectionAsync();
        var items = await db.Table<TodoItem>()
            .Where(t => t.UserId == userId && t.Date == date.Date && t.IsAutoGenerated)
            .ToListAsync();

        foreach (var item in items)
            await db.DeleteAsync(item);
    }

    /// <summary>
    /// Executes a raw SQL query. Use with caution.
    /// </summary>
    public async Task<int> ExecuteAsync(string query, params object[] args)
    {
        var db = await GetConnectionAsync();
        return await db.ExecuteAsync(query, args);
    }
}
