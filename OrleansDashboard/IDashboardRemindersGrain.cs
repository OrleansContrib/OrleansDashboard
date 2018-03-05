using System.Threading.Tasks;
using Orleans;

namespace OrleansDashboard
{
    public sealed class ReminderResponse
    {
        public int Count { get; set; }

        public ReminderInfo[] Reminders { get; set; }
    }

    public interface IDashboardRemindersGrain : IGrainWithIntegerKey
    {
        Task<ReminderResponse> GetReminders(int pageNumber, int pageSize);
    }
}