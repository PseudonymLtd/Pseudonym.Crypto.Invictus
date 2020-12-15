namespace System
{
    public static class StringExtensions
    {
        public static string ToAnchorName(this string name)
        {
            return name.StartsWith("A-")
                ? name
                : $"A-{name}";
        }
    }
}
