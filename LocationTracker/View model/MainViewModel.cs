using System.ComponentModel;
using System.Windows.Input;
using LocationTracker.Models;
using LocationTracker.Services;
using LocationTracker.Views;

namespace LocationTracker.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly LocationRepository _repo;
    private readonly LocationService _loc;
    private IDispatcher _dispatcher;
    private IDispatcherTimer? _timer;

    private string _countLabel = "Points: 0";
    public string CountLabel { get => _countLabel; set { _countLabel = value; OnPropertyChanged(nameof(CountLabel)); } }

    private string _timestampLabel = "Last: —";
    public string TimestampLabel { get => _timestampLabel; set { _timestampLabel = value; OnPropertyChanged(nameof(TimestampLabel)); } }

    private string _autoToggleText = "Start Auto";
    public string AutoToggleText { get => _autoToggleText; set { _autoToggleText = value; OnPropertyChanged(nameof(AutoToggleText)); } }

    public HeatDrawable HeatDrawable { get; } = new();

    public event Action<double,double>? PointSaved;

    public ICommand AddNowCommand { get; }
    public ICommand ToggleAutoCommand { get; }
    public ICommand RefreshHeatCommand { get; }

    private int _count = 0;

    public MainViewModel(LocationRepository repo, LocationService loc, IDispatcher dispatcher)
    {
        _repo = repo;
        _loc = loc;
        _dispatcher = dispatcher;

        AddNowCommand = new Command(async () => await AddNowAsync());
        ToggleAutoCommand = new Command(async () => await ToggleAutoAsync());
        RefreshHeatCommand = new Command(async () => await RefreshHeatAsync());

        _ = RefreshHeatAsync();
    }

    private async Task ToggleAutoAsync()
    {
        if (_timer == null)
        {
            _timer = _dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(15);
            _timer.Tick += async (_, __) => await AddNowAsync();
            _timer.Start();
            AutoToggleText = "Stop Auto";
        }
        else
        {
            _timer.Stop();
            _timer = null;
            AutoToggleText = "Start Auto";
        }
    }

    private async Task AddNowAsync()
    {
        var pos = await _loc.GetOnceAsync();
        if (pos == null) return;

        await _repo.InsertAsync(new LocationEntry
        {
            Latitude = pos.Latitude,
            Longitude = pos.Longitude,
            TimestampUtc = DateTime.UtcNow
        });

        _count++;
        CountLabel = $"Points: {_count}";
        TimestampLabel = $"Last: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        PointSaved?.Invoke(pos.Latitude, pos.Longitude);
        await RefreshHeatAsync();
    }

    public async Task RefreshHeatAsync()
    {
        var rows = await _repo.GetAllAsync();
        _count = rows.Count;
        CountLabel = $"Points: {_count}";
        TimestampLabel = rows.Count == 0
            ? "Last: —"
            : $"Last: {rows.Max(r => r.TimestampUtc).ToLocalTime():yyyy-MM-dd HH:mm:ss}";

        HeatDrawable.SetPoints(rows.Select(r => (r.Latitude, r.Longitude)));
    }

    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}