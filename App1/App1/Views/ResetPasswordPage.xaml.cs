using System.Threading.Tasks;
using Xamarin.Forms;
using App1.Services;   // jeśli ApiClient tam jest

namespace App1.Views
{
    public partial class ResetPasswordPage : ContentPage
    {
        Entry emailEntry;
        Button resetBtn;
        Label messageLabel;
        ApiClient api;

        public ResetPasswordPage(ApiClient apiClient)
        {
            InitializeComponent();   

            api = apiClient;

            emailEntry = new Entry { Placeholder = "E-mail", Keyboard = Keyboard.Email };
            resetBtn = new Button { Text = "Wyślij link resetujący" };
            resetBtn.Clicked += async (s, e) => await OnResetClicked();
            messageLabel = new Label { TextColor = Color.Red, IsVisible = false };

            Content = new StackLayout
            {
                Spacing = 12,
                Children =
                {
                    new Label { Text = "Reset hasła", FontSize = 26, HorizontalOptions = LayoutOptions.Center },
                    emailEntry,
                    resetBtn,
                    messageLabel
                }
            };
        }

        async Task OnResetClicked()
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

      
        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}

