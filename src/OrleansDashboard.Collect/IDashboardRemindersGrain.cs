using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using OrleansDashboard.Model;

namespace OrleansDashboard
{
    public interface IDashboardRemindersGrain : IGrainWithIntegerKey
    {
        Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize);
    }
}