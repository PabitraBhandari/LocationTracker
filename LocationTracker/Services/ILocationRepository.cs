// File: Services/ILocationRepository.cs
using LocationTracker.Models;

namespace LocationTracker.Services;

public interface ILocationRepository
{
    Task InitializeAsync();                       // create DB/table if needed
    Task InsertAsync(LocationEntry entry);        // add one row
    Task<List<LocationEntry>> GetAllAsync();      // newest first
    Task ClearAsync();                            // delete all rows
}
