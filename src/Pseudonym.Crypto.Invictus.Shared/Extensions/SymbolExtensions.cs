using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Pseudonym.Crypto.Invictus.Shared.Enums
{
    public static class SymbolExtensions
    {
        public static bool IsFund(this Symbol s)
        {
            var t = typeof(Symbol).GetMember(s.ToString())
                .Single();

            return t.GetCustomAttribute<FundAttribute>() != null;
        }

        public static bool IsStake(this Symbol s)
        {
            var t = typeof(Symbol).GetMember(s.ToString())
                .Single();

            return t.GetCustomAttribute<StakeAttribute>() != null;
        }
    }
}
