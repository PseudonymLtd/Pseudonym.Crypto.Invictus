using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum e)
        {
            var value = e.ToString();

            var t = e.GetType().GetMember(value)
                .Single();

            return t.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value;
        }
    }
}
