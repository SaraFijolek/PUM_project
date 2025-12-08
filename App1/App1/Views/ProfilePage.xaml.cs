using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using App1.Services;

namespace App1.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ApiClient api;

        public ProfilePage(ApiClient apiClient)
        {
            InitializeComponent(); // Niezbędne, aby x:Name działało
            api = apiClient;

            // Opcjonalnie ustawienie MaximumDate, jeśli nie ustawiono w XAML
            birthDatePicker.MaximumDate = DateTime.Today;
        }

        // Handler powiązany z XAML: Clicked="SaveBtn_Clicked"
        private async void SaveBtn_Clicked(object sender, EventArgs e)
        {
            await OnSaveClicked();
        }

        // Logika aktualizacji profilu
        private async Task OnSaveClicked()
        {
            messageLabel.IsVisible = false;

            string name = nameEntry.Text?.Trim();
            string surname = surnameEntry.Text?.Trim();
            DateTime birthDate = birthDatePicker.Date;
            string gender = genderPicker.SelectedItem?.ToString();

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
                var result = await api.UpdateProfileAsync(name, surname, birthDate, gender, height, weight, null);
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
    }
}
