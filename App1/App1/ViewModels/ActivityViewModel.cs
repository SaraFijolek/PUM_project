using App1.Models;
using App1.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App1.ViewModels
{
    public class ActivityViewModel : BaseViewModel
    {
        readonly ActivityStore store;
        readonly ApiClient apiClient;
        readonly Stopwatch stopwatch = new Stopwatch();

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand PickPhotoCommand { get; }
        public ICommand GoHistoryCommand { get; }

        public string Name { get => name; set => SetProperty(ref name, value); }
        public string Note { get => note; set => SetProperty(ref note, value); }
        public string Type { get => type; set => SetProperty(ref type, value); }
        public string DistanceText { get => distanceText; set => SetProperty(ref distanceText, value); }
        public string DurationText { get => durationText; set => SetProperty(ref durationText, value); }
        public bool IsRunning { get => isRunning; set => SetProperty(ref isRunning, value); }

        public ObservableCollection<Location> Track { get; } = new ObservableCollection<Location>();
        public double DistanceKm { get => distanceKm; set => SetProperty(ref distanceKm, value); }
        public string PaceText { get => paceText; set => SetProperty(ref paceText, value); }
        public string SpeedText { get => speedText; set => SetProperty(ref speedText, value); }
        public string PhotoBase64 { get => photoBase64; set => SetProperty(ref photoBase64, value); }

        double distanceKm;
        string paceText = "--";
        string speedText = "--";
        string photoBase64;
        CancellationTokenSource gpsCts;

        DateTime? startTime;
        string name;
        string note;
        string type;
        string distanceText;
        string durationText = "00:00:00";
        bool isRunning;

        string currentLocationText = "--";
        public string CurrentLocationText
        {
            get => currentLocationText;
            set => SetProperty(ref currentLocationText, value);
        }
        string distanceLiveText = "0.00 km";
        public string DistanceLiveText
        {
            get => distanceLiveText;
            set => SetProperty(ref distanceLiveText, value);
        }

        public ActivityViewModel(ActivityStore store, ApiClient apiClient)
        {
            this.store = store;
            this.apiClient = apiClient;

            StartCommand = new Command(OnStart, () => !IsRunning);
            StopCommand = new Command(OnStop, () => IsRunning);
            SaveCommand = new Command(async () => await OnSave(), () => !IsRunning && startTime.HasValue);
            PickPhotoCommand = new Command(async () => await PickPhoto());
            GoHistoryCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.ActivityHistoryPage)));
        }

        async void OnStart()
        {
            if (IsRunning) return;

            if (!await CheckGpsPermission())
                return;

            Track.Clear();
            DistanceKm = 0;

            startTime = DateTime.Now;
            stopwatch.Restart();
            IsRunning = true;

            gpsCts = new CancellationTokenSource();
            _ = StartGpsAsync(gpsCts.Token);

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                DurationText = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                UpdatePaceAndSpeed();
                return IsRunning;
            });

            UpdateCommands();
        }

        void OnStop()
        {
            if (!IsRunning) return;
            stopwatch.Stop();
            IsRunning = false;
            DurationText = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
            gpsCts?.Cancel();
            UpdatePaceAndSpeed();
            UpdateCommands();
        }

        async Task OnSave()
        {
            if (!startTime.HasValue)
                return;

            decimal? distanceKmOverride = null;
            if (double.TryParse(DistanceText, out var parsed))
                distanceKmOverride = (decimal)parsed;

            var activity = new Activity
            {
                Name = string.IsNullOrWhiteSpace(Name) ? "Aktywność" : Name.Trim(),
                StartTime = startTime.Value,
                EndTime = startTime.Value + stopwatch.Elapsed,
                DistanceKm = DistanceKm,  // override lub GPS
                Note = Note,
                Type = string.IsNullOrWhiteSpace(Type) ? "bieg" : Type,
                PaceText = PaceText,
                SpeedText = SpeedText,
                PhotoBase64 = PhotoBase64,
                Track = Track.Select(p => new GpsPoint
                {
                    Latitude = p.Latitude,
                    Longitude = p.Longitude
                }).ToList()
            };

            await store.AddAsync(activity);

            _ = apiClient.CreateActivityAsync(new
            {
                activity.Name,
                activity.StartTime,
                activity.EndTime,
                activity.DistanceKm,
                activity.Note,
                activity.Type,
                activity.PaceText,
                activity.SpeedText,
                activity.PhotoBase64,
                Track = activity.Track
            });

            await Application.Current.MainPage.DisplayAlert("Zapisano", "Aktywność zapisana lokalnie.", "OK");
            Reset();
        }

        void Reset()
        {
            Name = string.Empty;
            Note = string.Empty;
            Type = string.Empty;
            DurationText = "00:00:00";
            startTime = null;
            stopwatch.Reset();
            IsRunning = false;
            DistanceKm = 0;
            DistanceLiveText = "0.00 km";
            Track.Clear();
            UpdateCommands();
        }

        async Task StartGpsAsync(CancellationToken ct)
        {
            var request = new GeolocationRequest(
                GeolocationAccuracy.Best,
                TimeSpan.FromSeconds(5));

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var loc = await Geolocation.GetLocationAsync(request, ct);

                    if (loc == null)
                    {
                        CurrentLocationText = "Brak GPS";
                    }
                    else
                    {
                        CurrentLocationText =
                            $"{loc.Latitude:F6}, {loc.Longitude:F6}";

                        AddPoint(loc);
                    }
                }
                catch (Exception ex)
                {
                    CurrentLocationText = "GPS error";
                }

                await Task.Delay(2000, ct);
            }
        }

        void AddPoint(Location loc)
        {
            CurrentLocationText =
                $"{loc.Latitude:F6}, {loc.Longitude:F6}";

            if (Track.Count > 0)
            {
                var prev = Track[Track.Count - 1];
                DistanceKm += HaversineKm(prev, loc);
            }

            Track.Add(loc);

            DistanceLiveText = $"{DistanceKm:0.00} km";

            UpdatePaceAndSpeed();
        }

        double HaversineKm(Location a, Location b)
        {
            const double R = 6371d;
            double dLat = DegreesToRad(b.Latitude - a.Latitude);
            double dLon = DegreesToRad(b.Longitude - a.Longitude);
            double lat1 = DegreesToRad(a.Latitude);
            double lat2 = DegreesToRad(b.Latitude);

            double h = Math.Pow(Math.Sin(dLat / 2), 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);
            return 2 * R * Math.Asin(Math.Min(1, Math.Sqrt(h)));
        }

        double DegreesToRad(double deg) => deg * Math.PI / 180d;

        void UpdatePaceAndSpeed()
        {
            if (DistanceKm <= 0.0001)
            {
                PaceText = SpeedText = "--";
                return;
            }

            var elapsed = stopwatch.Elapsed;

            if (elapsed.TotalSeconds < 1)
            {
                PaceText = SpeedText = "--";
                return;
            }

            var hours = elapsed.TotalHours;

            SpeedText = (DistanceKm / hours).ToString("0.0") + " km/h";

            var minutesPerKm = elapsed.TotalMinutes / DistanceKm;
            var span = TimeSpan.FromMinutes(minutesPerKm);

            PaceText = span.ToString(@"mm\:ss") + " /km";
        }

        async Task PickPhoto()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync();
                if (result == null) return;
                using (var stream = await result.OpenReadAsync())
                using (var ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    PhotoBase64 = Convert.ToBase64String(ms.ToArray());
                }
            }
            catch (Exception)
            {
                
            }
        }
        async Task<bool> CheckGpsPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            return status == PermissionStatus.Granted;
        }

        void UpdateCommands()
        {
            ((Command)StartCommand).ChangeCanExecute();
            ((Command)StopCommand).ChangeCanExecute();
            ((Command)SaveCommand).ChangeCanExecute();
        }
    }
}


