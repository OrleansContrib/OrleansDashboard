using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

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

        private static string PrimaryKeyAsString(GrainReference grainRef)
        {
            if (grainRef.IsPrimaryKeyBasedOnLong())
            {
                // long
                var pk = grainRef.GetPrimaryKeyLong(out var ext).ToString();
                if (null == ext) return pk;
                return $"{pk} + {ext}";
            }

            if (null != grainRef.GetPrimaryKeyString())
            {
                // string
                return grainRef.GetPrimaryKeyString();
            }
    
            // guid
            var guidPk = grainRef.GetPrimaryKey(out var guidExt).ToString();
            if (null == guidExt) return guidPk;
            return $"{guidPk} + {guidExt}";
        }

        private static ReminderInfo ToReminderInfo(ReminderEntry entry)
        {
            return new ReminderInfo
            {
                PrimaryKey = PrimaryKeyAsString(entry.GrainRef),
                GrainReference = entry.GrainRef.ToString(),
                Name = entry.ReminderName,
                StartAt = entry.StartAt,
                Period = entry.Period,
            };
        }
    }
}