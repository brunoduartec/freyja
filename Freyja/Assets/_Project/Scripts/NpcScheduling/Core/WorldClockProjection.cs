using System;

namespace Freyja.NpcScheduling
{
    /// <summary>
    /// Projects the existing 0..24 island clock into the schedule core's session time.
    /// The session day starts at one and advances only across an observed midnight wrap.
    /// </summary>
    public sealed class WorldClockProjection
    {
        private const double DayHours = 24d;
        private const double EveningWrapStart = 18d;
        private const double MorningWrapEnd = 6d;

        private bool initialized;
        private int absoluteDay = 1;
        private double lastHour;
        private GameTimeSnapshot currentSnapshot;

        public int AbsoluteDay => absoluteDay;

        public bool TryProject(double hour, out GameTimeSnapshot snapshot, out string reason)
        {
            snapshot = null;
            reason = null;
            if (Double.IsNaN(hour) || Double.IsInfinity(hour) || hour < 0d || hour >= DayHours)
            {
                reason = "invalid_clock_hour";
                return false;
            }

            if (!initialized)
            {
                initialized = true;
                lastHour = hour;
            }
            else if (hour < lastHour)
            {
                if (lastHour >= EveningWrapStart && hour < MorningWrapEnd)
                {
                    if (absoluteDay == Int32.MaxValue)
                    {
                        reason = "session_day_overflow";
                        return false;
                    }

                    absoluteDay++;
                }
                else
                {
                    reason = "clock_regressed";
                    return false;
                }
            }

            lastHour = hour;
            int minuteOfDay = Math.Min(1439, (int)Math.Floor(hour * 60d));
            if (currentSnapshot == null || currentSnapshot.AbsoluteDay != absoluteDay || currentSnapshot.MinuteOfDay != minuteOfDay)
                currentSnapshot = new GameTimeSnapshot(absoluteDay, minuteOfDay, 0);
            snapshot = currentSnapshot;
            return true;
        }
    }
}
