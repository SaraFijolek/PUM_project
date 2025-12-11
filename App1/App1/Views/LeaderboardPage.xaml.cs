using Xamarin.Forms;
using App1.ViewModels;
using App1.Services;
using FitnessApp.UI;

namespace App1.Views
{
    public partial class LeaderboardPage : ContentPage
    {
        public LeaderboardPage(ApiClient api)
        {
            InitializeComponent();
            BindingContext = new LeaderboardViewModel(api);
            Title = L.T("Ranking");
        }
    }
}

