using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using OrleansDashboard.Client.Model;

namespace OrleansDashboard.Client
{
    public interface IDashboardRemindersGrain : IGrainWithIntegerKey
    {
        Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize);
    }
}