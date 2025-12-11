using System;
using System.IO;
using Xamarin.Forms;
using App1.Models;
using FitnessApp.UI;

namespace App1.Views
{
    public partial class ActivityDetailPage : ContentPage
    {
        readonly Activity activity;

        public int TrackCount => activity?.Track?.Count ?? 0;

        public ActivityDetailPage(Activity act)
        {
            InitializeComponent();
            activity = act;
            BindingContext = activity;
            Localize();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!string.IsNullOrWhiteSpace(activity?.PhotoBase64))
            {
                try
                {
                    var bytes = Convert.FromBase64String(activity.PhotoBase64);
                    photoImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                    photoImage.IsVisible = true;
                }
                catch
                {
                    photoImage.IsVisible = false;
                }
            }
        }

        void Localize()
        {
            Title = L.T("Details");
            startLabel.Text = $"{L.T("Time")}: {activity.StartTime:dd.MM.yyyy HH:mm}";
            endLabel.Text = $"{L.T("Time")}: {activity.EndTime:dd.MM.yyyy HH:mm}";
            distanceLabel.Text = $"{L.T("Distance")}: {activity.DistanceKm:0.00} km";
            paceLabel.Text = $"{L.T("Pace")}: {activity.PaceText}";
            speedLabel.Text = $"{L.T("Speed")}: {activity.SpeedText}";
            typeLabel.Text = $"{L.T("Type")}: {activity.Type}";
            noteLabel.Text = $"{L.T("Note")}: {activity.Note}";
            trackLabel.Text = $"{L.T("TrackPoints")}: {activity.TrackCount}";
            trackListLabel.Text = L.T("TrackPoints");
        }
    }
}

