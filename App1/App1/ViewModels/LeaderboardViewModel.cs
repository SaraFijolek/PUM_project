using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using App1.Models;
using App1.Services;

namespace App1.ViewModels
{
    public class LeaderboardViewModel : BaseViewModel
    {
        readonly ApiClient api;
        public ObservableCollection<LeaderboardEntry> Items { get; } = new ObservableCollection<LeaderboardEntry>();
        public string Message { get => message; set => SetProperty(ref message, value); }
        string message;

        public LeaderboardViewModel(ApiClient client)
        {
            api = client;
            _ = Load();
        }

        async Task Load()
        {
            var data = await api.GetLeaderboardAsync();
            Items.Clear();
            foreach (var e in data)
                Items.Add(e);

            Message = Items.Count == 0 ? FitnessApp.UI.L.T("NoLeaderboardData") : null;
        }
    }
}

