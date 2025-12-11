using System.Threading.Tasks;
using Xamarin.Forms;
using App1.Services;   // jeśli ApiClient tam jest
using FitnessApp.UI;

namespace App1.Views
{
    public partial class ResetPasswordPage : ContentPage
    {
        ApiClient api;

        public ResetPasswordPage(ApiClient apiClient)
        {
            InitializeComponent();
            api = apiClient;
        }

        async void OnResetClicked(object sender, System.EventArgs e)
        {
            await HandleReset();
        }

        async Task HandleReset()
        {
            messageLabel.IsVisible = false;
            var email = emailEntry.Text?.Trim();

            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                ShowError("Nieprawidłowy e-mail");
                return;
            }

            var result = await api.ResetPasswordAsync(email);
            bool ok = result.ok;
            string msg = result.message;

            if (ok)
                await DisplayAlert("OK", "Link resetujący został wysłany na e-mail", "OK");
            else
                ShowError(msg);
        }

        void ShowError(string text)
        {
            messageLabel.Text = text;
            messageLabel.IsVisible = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Localize();
        }

        void Localize()
        {
            Title = L.T("ResetPasswordTitle");
            titleLabel.Text = L.T("ResetPasswordTitle");
            emailEntry.Placeholder = L.T("Email");
            resetBtn.Text = L.T("SendReset");
        }
    }
}

