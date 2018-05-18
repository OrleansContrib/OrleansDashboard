using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace OrleansDashboard
{
    public class DashboardRemindersGrain : Grain, IDashboardRemindersGrain
    {
        private readonly IReminderTable _reminderTable;

        public DashboardRemindersGrain(IReminderTable reminderTable)
        {
            _reminderTable = reminderTable;
        }

        public async Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize)
        {
            var reminderData = await _reminderTable.ReadRows(0, 0xffffffff);

            return new ReminderResponse
            {
                Reminders = reminderData
                    .Reminders
                    .OrderBy(x => x.StartAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ToReminderInfo)
                    .ToArray(),

                Count = reminderData.Reminders.Count
            }.AsImmutable();
        }

        private static ReminderInfo ToReminderInfo(ReminderEntry entry)
        {
            return new ReminderInfo
            {
                PrimaryKey = entry.GrainRef.PrimaryKeyAsString(),
                GrainReference = entry.GrainRef.ToString(),
                Name = entry.ReminderName,
                StartAt = entry.StartAt,
                Period = entry.Period,
            };
        }
    }
}