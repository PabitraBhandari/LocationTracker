using SQLite;

namespace LocationTracker.Data
{
    public class AppDb
    {
        private readonly SQLiteAsyncConnection _db;

        public AppDb(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<LocationEntry>().Wait();
        }

        public Task<int> InsertAsync(LocationEntry entry)
        {
            return _db.InsertAsync(entry);
        }

        public Task<List<LocationEntry>> GetAllAsync()
        {
            return _db.Table<LocationEntry>().OrderByDescending(e => e.TimestampUtc).ToListAsync();
        }
    }

    public class LocationEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateTime TimestampUtc { get; set; }
    }
}
