using Xamarin.Forms;
using App1.ViewModels;
using App1.Services;
using System.Net.Http;
using FitnessApp.UI;

namespace App1.Views
{
    public partial class ActivityPage : ContentPage
    {
        readonly ActivityStore store;
        readonly ApiClient api;

        public ActivityPage(ApiClient apiClient)
        {
            InitializeComponent();
            api = apiClient;
            store = new ActivityStore();
            BindingContext = new ActivityViewModel(store, apiClient);
            Localize();
        }

        async void OnHistoryClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new ActivityHistoryPage(api, store));
        }

        void Localize()
        {
            Title = L.T("Activity");
            historyToolbar.Text = L.T("History");
            titleLabel.Text = L.T("ActivityNew");
            nameEntry.Placeholder = L.T("Name");
            typePicker.Title = L.T("Type");
            distanceLabel.Text = $"{L.T("Distance")} (km)";
            timeLabel.Text = L.T("Time");
            paceLabel.Text = L.T("Pace");
            speedLabel.Text = L.T("Speed");
            startBtn.Text = "Start";
            stopBtn.Text = "Stop";
            saveBtn.Text = L.T("Save");
            noteEditor.Placeholder = L.T("Note");
            photoBtn.Text = L.T("AddPhoto");
            photoInfo.Text = L.T("AddPhoto");
        }
    }
}

