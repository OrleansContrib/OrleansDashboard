using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public interface IDashboardRemindersGrain : IGrainWithIntegerKey
    {
        Task<IList<ReminderInfo>> GetReminders();
    }
}