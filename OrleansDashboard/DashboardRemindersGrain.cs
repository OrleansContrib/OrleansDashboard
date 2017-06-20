using System.Linq;
using System.Threading.Tasks;
using Orleans;

namespace OrleansDashboard
{
    public class DashboardRemindersGrain : Grain, IDashboardRemindersGrain
    {
        private readonly IReminderTable _reminderTable;

        public DashboardRemindersGrain(IReminderTable reminderTable)
        {
            _reminderTable = reminderTable;
        }

        public async Task<ReminderResponse> GetReminders(int pageNumber, int pageSize)
        {
            var pageStart = (pageNumber * pageSize) - pageSize;

            var reminderData = await _reminderTable.ReadRows(0, 0xffffffff);

            return new ReminderResponse {

                Reminders = reminderData
                    .Reminders
                    .OrderBy(x => x.StartAt)
                    .Skip((pageNumber -1) * pageSize)
                    .Take(pageSize)
                    .Select(ToReminderInfo)
                    .ToArray(),

                Count = reminderData.Reminders.Count
            }; 
        }

        private static ReminderInfo ToReminderInfo(ReminderEntry entry)
        {
            return new ReminderInfo
            {
                GrainReference = entry.GrainRef.ToString(),
                Name = entry.ReminderName,
                StartAt = entry.StartAt,
                Period = entry.Period,
            };
        }
    }
}