using App1.Services;
using App1.Views;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FitnessApp.UI
{

    public static class L
    {
        public static string Lang { get; set; } = "pl";

        public static string T(string key)
        {
            if (Lang == "pl")
                return _pl(key);
            else
                return _en(key);
        }

        static string _pl(string k)
        {
            switch (k)
            {
                case "LoginTitle": return "Zaloguj się";
                case "RegisterTitle": return "Zarejestruj się";
                case "Email": return "E-mail";
                case "Password": return "Hasło";
                case "ConfirmPassword": return "Powtórz hasło";
                case "Name": return "Imię";
                case "LoginButton": return "Zaloguj";
                case "GoToRegister": return "Nie masz konta? Zarejestruj się";
                case "RegisterButton": return "Utwórz konto";
                case "ForgotPassword": return "Nie pamiętasz hasła?";
                case "Or": return "LUB";
                case "InvalidEmail": return "Nieprawidłowy e-mail";
                case "PasswordTooShort": return "Hasło za krótkie (min 6 znaków)";
                default: return k;
            }
        }

        static string _en(string k)
        {
            switch (k)
            {
                case "LoginTitle": return "Sign in";
                case "RegisterTitle": return "Register";
                case "Email": return "Email";
                case "Password": return "Password";
                case "ConfirmPassword": return "Confirm password";
                case "Name": return "Name";
                case "LoginButton": return "Sign in";
                case "GoToRegister": return "Don't have an account? Register";
                case "RegisterButton": return "Create account";
                case "ForgotPassword": return "Forgot password?";
                case "Or": return "OR";
                case "InvalidEmail": return "Invalid email";
                case "PasswordTooShort": return "Password too short (min 6)";
                default: return k;
            }
        }
    }


    


    public class LoginPage : ContentPage
    {
        Entry emailEntry, passwordEntry;
        Label messageLabel;
        ApiClient api;

        public LoginPage(ApiClient apiClient)
        {
            api = apiClient;
            Title = L.T("LoginTitle");
            Padding = new Thickness(20);

            var langSwitch = new Button { Text = "EN/PL", HorizontalOptions = LayoutOptions.End };
            langSwitch.Clicked += (s, e) => {
                L.Lang = (L.Lang == "pl") ? "en" : "pl";

                Device.BeginInvokeOnMainThread(() => Application.Current.MainPage = new NavigationPage(new LoginPage(api)));
            };

            emailEntry = new Entry { Placeholder = L.T("Email"), Keyboard = Keyboard.Email };
            passwordEntry = new Entry { Placeholder = L.T("Password"), IsPassword = true };
            messageLabel = new Label { TextColor = Color.Red, IsVisible = false };

            var loginBtn = new Button { Text = L.T("LoginButton") };
            loginBtn.Clicked += async (s, e) => await OnLoginClicked();

            var registerLink = new Label { Text = L.T("GoToRegister"), HorizontalOptions = LayoutOptions.Center };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => Navigation.PushAsync(new RegisterPage(api));
            registerLink.GestureRecognizers.Add(tap);

            var forgot = new Label { Text = L.T("ForgotPassword"), HorizontalOptions = LayoutOptions.Center };
            var forgotTap = new TapGestureRecognizer();
            forgotTap.Tapped += async (s, e) =>
            {
                await Navigation.PushAsync(new ResetPasswordPage(api));
            };
            forgot.GestureRecognizers.Add(forgotTap);


            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Spacing = 16,
                    Children = {
                        new StackLayout { Orientation = StackOrientation.Horizontal, Children = { new Label { Text = "" , HorizontalOptions = LayoutOptions.StartAndExpand }, langSwitch } },
                        new Label { Text = Title, FontSize = 28, HorizontalOptions = LayoutOptions.Center },
                        emailEntry,
                        passwordEntry,
                        loginBtn,
                        messageLabel,
                        new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.Center, Children = { new BoxView { HeightRequest = 1, WidthRequest = 60, VerticalOptions = LayoutOptions.Center }, new Label { Text = L.T("Or") }, new BoxView { HeightRequest = 1, WidthRequest = 60 } } },
                        registerLink,
                        forgot
                    }
                }
            };
        }

        async Task OnLoginClicked()
        {
            messageLabel.IsVisible = false;
            var email = emailEntry.Text?.Trim();
            var pwd = passwordEntry.Text ?? string.Empty;
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                ShowError(L.T("InvalidEmail"));
                return;
            }
            if (pwd.Length < 6)
            {
                ShowError(L.T("PasswordTooShort"));
                return;
            }

            try
            {
                var (ok, msg) = await api.LoginAsync(email, pwd);
                if (ok)
                {
                    await DisplayAlert("OK", "Zalogowano (placeholder). Response: " + msg, "OK");
                    await Navigation.PushAsync(new ProfilePage(api));
                }
                else
                {
                    ShowError(msg);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        void ShowError(string text)
        {
            messageLabel.Text = text;
            messageLabel.IsVisible = true;
        }
    }


    public class RegisterPage : ContentPage
    {
        Entry nameEntry, emailEntry, passwordEntry, confirmEntry;
        Label messageLabel;
        ApiClient api;

        public RegisterPage(ApiClient apiClient)
        {
            api = apiClient;
            Title = L.T("RegisterTitle");
            Padding = new Thickness(20);

            nameEntry = new Entry { Placeholder = L.T("Name") };
            emailEntry = new Entry { Placeholder = L.T("Email"), Keyboard = Keyboard.Email };
            passwordEntry = new Entry { Placeholder = L.T("Password"), IsPassword = true };
            confirmEntry = new Entry { Placeholder = L.T("ConfirmPassword"), IsPassword = true };
            messageLabel = new Label { TextColor = Color.Red, IsVisible = false };

            var registerBtn = new Button { Text = L.T("RegisterButton") };
            registerBtn.Clicked += async (s, e) => await OnRegisterClicked();

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Spacing = 12,
                    Children = {
                        new Label { Text = Title, FontSize = 26, HorizontalOptions = LayoutOptions.Center },
                        nameEntry,
                        emailEntry,
                        passwordEntry,
                        confirmEntry,
                        registerBtn,
                        messageLabel
                    }
                }
            };
        }

        async Task OnRegisterClicked()
        {
            messageLabel.IsVisible = false;
            var name = nameEntry.Text?.Trim();
            var email = emailEntry.Text?.Trim();
            var pwd = passwordEntry.Text ?? string.Empty;
            var conf = confirmEntry.Text ?? string.Empty;

            if (string.IsNullOrEmpty(name)) { ShowError("Name required"); return; }
            if (string.IsNullOrEmpty(email) || !email.Contains("@")) { ShowError(L.T("InvalidEmail")); return; }
            if (pwd.Length < 6) { ShowError(L.T("PasswordTooShort")); return; }
            if (pwd != conf) { ShowError("Passwords do not match"); return; }

            try
            {
                var (ok, msg) = await api.RegisterAsync(name, email, pwd);
                if (ok)
                {
                    await DisplayAlert("OK", "Konto utworzone (placeholder). Response: " + msg, "OK");
                    await Navigation.PopAsync();
                    Application.Current.MainPage = new NavigationPage(new ProfilePage(api));
                }
                else
                {
                    ShowError(msg);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        void ShowError(string text)
        {
            messageLabel.Text = text;
            messageLabel.IsVisible = true;
        }
    }


    public class AppStarter
    {
        public static Page CreateMainPage()
        {
            var http = new HttpClient();
            var api = new ApiClient(http);
            var nav = new NavigationPage(new LoginPage(api));
            return nav;
        }
    }
}