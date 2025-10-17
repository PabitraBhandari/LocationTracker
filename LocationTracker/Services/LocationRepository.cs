using LocationTracker.Models;
using SQLite;

namespace LocationTracker.Services;

public class LocationRepository
{
    private readonly SQLiteAsyncConnection _db;

    public LocationRepository()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "location.db3");
        _db = new SQLiteAsyncConnection(path);
        _db.CreateTableAsync<LocationEntry>().Wait();
    }

    public Task<int> InsertAsync(LocationEntry entry) => _db.InsertAsync(entry);
    public Task<List<LocationEntry>> GetAllAsync() => _db.Table<LocationEntry>().ToListAsync();
    public Task<int> DeleteAllAsync() => _db.DeleteAllAsync<LocationEntry>();
}