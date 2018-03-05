using Orleans;
using Orleans.Runtime;

namespace OrleansDashboard
{
    public static class Extensions
    {
        public static string PrimaryKeyAsString(this GrainReference grainRef)
        {
            if (grainRef.IsPrimaryKeyBasedOnLong()) // Long
            {
                var longKey = grainRef.GetPrimaryKeyLong(out var longExt);

                return longExt != null ? $"{longKey} + {longExt}" : longKey.ToString();
            }

            var stringKey = grainRef.GetPrimaryKeyString();

            if (stringKey == null) // Guid
            {
                var guidKey = grainRef.GetPrimaryKey(out var guidExt).ToString();

                return guidExt != null ? $"{guidKey} + {guidExt}" : guidKey.ToString();
            }

            return stringKey;
        }
    }
}
