using Orleans;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class DashboardRemindersGrain : Grain, IDashboardRemindersGrain
    {
        private readonly IReminderTable _reminderTable;

        public DashboardRemindersGrain(IReminderTable reminderTable)
        {
            _reminderTable = reminderTable;
        }

        public async Task<IList<ReminderInfo>> GetReminders()
        {
            var reminderData = await _reminderTable.ReadRows(0, 0xffffffff);

            return reminderData.Reminders.Select(ToReminderInfo).ToList();
        }

        private static ReminderInfo ToReminderInfo(ReminderEntry entry)
        {
            return new ReminderInfo
            {
                GrainReference = entry.GrainRef.ToString(),
                Name = entry.ReminderName,
                StartAt = entry.StartAt,
                Period = entry.Period
            };
        }
    }
}