using System.Numerics;
using System.Text.RegularExpressions;
using Nethereum.Web3;

namespace System
{
    public static class StringExtensions
    {
        private static readonly Regex ExponentRegex = new Regex(@"^([\d.]+)[e|E]-([\d]+)$");

        public static decimal FromBigInteger(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return 0;
            }
            else
            {
                var bigInt = BigInteger.Parse(s);

                return Web3.Convert.FromWei(bigInt);
            }
        }

        public static decimal FromPythonString(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return 0;
            }
            else
            {
                if (decimal.TryParse(s, out decimal d))
                {
                    return d;
                }
                else if (ExponentRegex.IsMatch(s))
                {
                    var match = ExponentRegex.Match(s);
                    var number = double.Parse(match.Groups[1].Value);
                    var exponent = int.Parse(match.Groups[2].Value);

                    return (decimal)Math.Pow(number, exponent);
                }
                else
                {
                    throw new FormatException($"decimal `{s}` was not in the correct format");
                }
            }
        }

        public static decimal? FromOptionalPythonString(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            return FromPythonString(s);
        }
    }
}
