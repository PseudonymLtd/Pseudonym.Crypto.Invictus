namespace System
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixSeedDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return Convert.ToInt64((dateTime - UnixSeedDate).TotalSeconds);
        }

        public static DateTime ToDateTime(this long timestamp)
        {
            return UnixSeedDate.AddSeconds(timestamp);
        }

        public static string ToISO8601String(this DateTime dataTime)
        {
            return dataTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ");
        }
    }
}
