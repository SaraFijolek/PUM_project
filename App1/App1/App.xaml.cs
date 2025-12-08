using System.Net.Http;
using Xamarin.Forms;
using App1.Services;
using FitnessApp.UI;

namespace App1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();

            var api = new ApiClient(new HttpClient());
            MainPage = new NavigationPage(new LoginPage(api));

        }

        protected override void OnStart() { }
        protected override void OnSleep() { }
        protected override void OnResume() { }
    }
}
