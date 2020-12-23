using Pseudonym.Crypto.Invictus.Shared;

namespace System
{
    public static class DateTimeExtensions
    {
        public static string ToISO8601String(this DateTimeOffset date)
        {
            return date.UtcDateTime.ToISO8601String();
        }

        public static string ToISO8601String(this DateTime date)
        {
            return date.ToString(Format.DateTimeFormat);
        }

        public static DateTimeOffset Round(this DateTimeOffset date)
        {
            return new DateTimeOffset(date.UtcDateTime.Round(), TimeSpan.Zero);
        }

        public static DateTime Round(this DateTime date)
        {
            var time = date.TimeOfDay;

            var modulus = (int)time.Add(TimeSpan.FromHours(0.5)).TotalHours;

            var roundedTimeSpan = TimeSpan.FromHours(modulus - (modulus % 1));

            var dt = new DateTime(
                date.Year,
                date.Month,
                date.Day,
                0,
                0,
                0,
                DateTimeKind.Utc);

            return dt
                .AddDays(roundedTimeSpan.Days)
                .AddHours(roundedTimeSpan.Hours);
        }
    }
}
