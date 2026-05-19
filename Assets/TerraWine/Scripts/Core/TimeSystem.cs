using System;
using System.Globalization;

namespace TerraWine.Core
{
    public class TimeSystem : ITimeSystem
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public TimeSpan GetElapsedSince(string savedUtc)
        {
            if (!TryParseSaveTime(savedUtc, out DateTime savedTime))
            {
                return TimeSpan.Zero;
            }

            TimeSpan elapsed = UtcNow - savedTime;
            return elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
        }

        public string ToSaveString(DateTime time)
        {
            return time.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }

        public bool TryParseSaveTime(string savedUtc, out DateTime time)
        {
            return DateTime.TryParse(
                savedUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out time);
        }
    }
}
