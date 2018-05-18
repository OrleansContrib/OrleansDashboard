using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace OrleansDashboard
{
    public sealed class ReminderResponse
    {
        public int Count { get; set; }

        public ReminderInfo[] Reminders { get; set; }
    }

    public interface IDashboardRemindersGrain : IGrainWithIntegerKey
    {
        Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize);
    }
}