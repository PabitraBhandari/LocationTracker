using LocationTracker.Models;
using SQLite;

namespace LocationTracker.Services;

public sealed class SqliteLocationRepository : ILocationRepository
{
    private readonly string _dbPath;
    private SQLiteAsyncConnection? _conn;
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public SqliteLocationRepository()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "location.db3");
    }

    private async Task<SQLiteAsyncConnection> GetConnAsync()
    {
        if (_conn != null) return _conn;

        await _mutex.WaitAsync();
        try
        {
            if (_conn == null)
            {
                _conn = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);
                await _conn.CreateTableAsync<LocationEntry>();
            }
            return _conn;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task InitializeAsync() => await GetConnAsync();

    public async Task InsertAsync(LocationEntry entry)
    {
        var db = await GetConnAsync();
        await db.InsertAsync(entry);
    }

    public async Task<List<LocationEntry>> GetAllAsync()
    {
        var db = await GetConnAsync();
        // newest first
        return await db.Table<LocationEntry>()
                       .OrderByDescending(x => x.Id)
                       .ToListAsync();
    }

    public async Task ClearAsync()
    {
        var db = await GetConnAsync();
        await db.DeleteAllAsync<LocationEntry>();
    }
}
