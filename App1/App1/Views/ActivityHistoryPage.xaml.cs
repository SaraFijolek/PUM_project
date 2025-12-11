using Xamarin.Forms;
using App1.ViewModels;
using App1.Services;
using App1.Models;
using FitnessApp.UI;

namespace App1.Views
{
    public partial class ActivityHistoryPage : ContentPage
    {
        readonly ApiClient api;
        readonly ActivityStore store;

        public ActivityHistoryPage(ApiClient apiClient, ActivityStore store)
        {
            InitializeComponent();
            api = apiClient;
            this.store = store;
            BindingContext = new ActivityHistoryViewModel(apiClient, store);
            Localize();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as ActivityHistoryViewModel)?.LoadCommand.Execute(null);
        }

        async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.CurrentSelection.Count > 0 ? e.CurrentSelection[0] as Activity : null;
            if (selected == null) return;
            ((CollectionView)sender).SelectedItem = null;
            await Navigation.PushAsync(new ActivityDetailPage(selected));
        }

        async void OnLeaderboardClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new LeaderboardPage(api));
        }

        void Localize()
        {
            Title = L.T("History");
            leaderboardToolbar.Text = L.T("Ranking");
            filterPicker.Title = L.T("FilterType");
            sortPicker.Title = L.T("Sort");
            applyBtn.Text = L.T("Apply");
            workoutsLabel.Text = L.T("Workouts");
            distanceLabel.Text = L.T("TotalDistance");
            avgSpeedLabel.Text = L.T("AvgSpeed");
        }
    }
}

