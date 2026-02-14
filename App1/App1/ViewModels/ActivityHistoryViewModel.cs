using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using App1.Models;
using App1.Services;
using System.Linq;
using System;

namespace App1.ViewModels
{
    public class ActivityHistoryViewModel : BaseViewModel
    {
        readonly ApiClient api;
        readonly ActivityStore store;

        public ObservableCollection<Activity> Items { get; } = new ObservableCollection<Activity>();
        public Command LoadCommand { get; }
        public Command ApplyCommand { get; }

        public string FilterType
        {
            get => filterType;
            set { SetProperty(ref filterType, value); }
        }

        public string SortOption
        {
            get => sortOption;
            set { SetProperty(ref sortOption, value); }
        }

        public int TotalActivities { get => totalActivities; set => SetProperty(ref totalActivities, value); }
        public double TotalDistance { get => totalDistance; set => SetProperty(ref totalDistance, value); }
        public double AvgSpeed { get => avgSpeed; set => SetProperty(ref avgSpeed, value); }

        string filterType = "wszystkie";
        string sortOption = "data ⬇";
        int totalActivities;
        double totalDistance;
        double avgSpeed;

        System.Collections.Generic.List<Activity> allItems = new System.Collections.Generic.List<Activity>();

        public ActivityHistoryViewModel(ApiClient apiClient, ActivityStore activityStore)
        {
            api = apiClient;
            store = activityStore;
            LoadCommand = new Command(async () => await Load());
            ApplyCommand = new Command(ApplyFilters);
        }

        async Task Load()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var remote = await api.GetActivitiesAsync();
                allItems = remote.ToList();

               
                if (!allItems.Any())
                {
                    var local = await store.GetAllAsync();
                    allItems = local.ToList();
                }

                ApplyFilters();
            }
            finally
            {
                IsBusy = false;
            }
        }

        void ApplyFilters()
        {
            var query = allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterType) && FilterType != "wszystkie")
                query = query.Where(a => string.Equals(a.Type, FilterType, StringComparison.OrdinalIgnoreCase));

            switch (SortOption)
            {
                case "data ⬆":
                    query = query.OrderBy(a => a.StartTime);
                    break;
                case "dystans ⬇":
                    query = query.OrderByDescending(a => a.DistanceKm ?? 0);
                    break;
                case "dystans ⬆":
                    query = query.OrderBy(a => a.DistanceKm ?? 0);
                    break;
                default:
                    query = query.OrderByDescending(a => a.StartTime);
                    break;
            }

            Items.Clear();
            foreach (var a in query)
            {
                // DEBUG - wypisz wartości
                System.Diagnostics.Debug.WriteLine($"Aktywność: {a.Name}, Dystans: {a.DistanceKm}");
                Items.Add(a);
            }

            TotalActivities = Items.Count;
            TotalDistance = Items.Sum(a => a.DistanceKm ?? 0);
            var totalHours = Items.Where(a => a.Duration.HasValue).Sum(a => a.Duration.Value.TotalHours);
            AvgSpeed = totalHours > 0 ? TotalDistance / totalHours : 0;

            // DEBUG - podsumowanie
            System.Diagnostics.Debug.WriteLine($"Total: {TotalActivities} aktywności, {TotalDistance:0.00} km");

        }
    }
}

