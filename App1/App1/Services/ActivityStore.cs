using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App1.Models;

namespace App1.Services
{
    
    public class ActivityStore
    {
        static readonly List<Activity> Activities = new List<Activity>();

        public Task AddAsync(Activity activity)
        {
            Activities.Add(activity);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Activity>> GetAllAsync()
        {
            return Task.FromResult((IReadOnlyList<Activity>)Activities.ToList());
        }
    }
}


