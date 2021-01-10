using System.Numerics;
using System.Text.RegularExpressions;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.Funds.Business.Json;

namespace System
{
    public static class StringExtensions
    {
        private static readonly Regex ExponentRegex = new Regex(@"^([\d.]+)[e|E]-([\d]+)$");

        public static decimal FromBigInteger(this string s, int decimals = 18)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return decimal.Zero;
            }
            else if (s.Contains('.'))
            {
                var parts = s.Split('.', StringSplitOptions.RemoveEmptyEntries);

                return decimal.Parse($"{parts[0].Trim()}.{parts[1].Trim().Substring(0, Math.Min(parts[1].Trim().Length, 18))}");
            }
            else
            {
                var bigInt = BigInteger.Parse(s);

                return Web3.Convert.FromWei(bigInt, decimals);
            }
        }

        public static decimal FromPythonString(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return decimal.Zero;
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

        public static string Serialize<T>(this T item)
        {
            return Json.Serialize(item);
        }

        public static T Deserialize<T>(this string json)
        {
            return Json.Deserialize<T>(json);
        }
    }
}
