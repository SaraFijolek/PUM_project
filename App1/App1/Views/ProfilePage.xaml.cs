﻿﻿﻿using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using App1.Services;
using FitnessApp.UI;

namespace App1.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ApiClient api;

        public ProfilePage(ApiClient apiClient)
        {
            InitializeComponent();
            api = apiClient;

            birthDatePicker.MaximumDate = DateTime.Today;
            Localize();
        }

        
        private async void SaveBtn_Clicked(object sender, EventArgs e)
        {
            await OnSaveClicked();
        }

        private async void ActivityBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ActivityPage(api));
        }

        private async void LogoutBtn_Clicked(object sender, EventArgs e)
        {
            await api.LogoutAsync();
            Application.Current.MainPage = new NavigationPage(new FitnessApp.UI.LoginPage(api));
        }

        
        private async Task OnSaveClicked()
        {
            messageLabel.IsVisible = false;

            string name = nameEntry.Text?.Trim();
            string surname = surnameEntry.Text?.Trim();
            DateTime birthDate = birthDatePicker.Date;
            string gender = genderPicker.SelectedItem?.ToString();
            string avatar = avatarEntry.Text?.Trim();

            double? height = null;
            if (!string.IsNullOrWhiteSpace(heightEntry.Text) && double.TryParse(heightEntry.Text, out var h))
                height = h;

            double? weight = null;
            if (!string.IsNullOrWhiteSpace(weightEntry.Text) && double.TryParse(weightEntry.Text, out var w))
                weight = w;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(surname))
            {
                ShowError("Imię i nazwisko są wymagane");
                return;
            }

            try
            {
                var result = await api.UpdateProfileAsync(name, surname, birthDate, gender, height, weight, avatar);
                if (result.ok)
                {
                    await DisplayAlert("OK", "Profil zaktualizowany", "OK");
                }
                else
                {
                    ShowError(result.message);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ShowError(string text)
        {
            messageLabel.Text = text;
            messageLabel.IsVisible = true;
        }

        void Localize()
        {
            Title = L.T("Profile");
            titleLabel.Text = L.T("Profile");
            nameEntry.Placeholder = L.T("FirstName");
            surnameEntry.Placeholder = L.T("LastName");
            genderPicker.Title = L.T("Gender");
            heightEntry.Placeholder = L.T("Height");
            weightEntry.Placeholder = L.T("Weight");
            saveBtn.Text = L.T("Save");
            activityBtn.Text = L.T("Activity");
            logoutBtn.Text = L.T("Logout");
        }
    }
}
