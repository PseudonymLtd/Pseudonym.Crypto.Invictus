using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace System
{
    public static class DecimalExtensions
    {
        public static string ToCommaFormat(this decimal m)
        {
            var nonDecimalAmount = (long)m;
            var decimalAmount = m - nonDecimalAmount;

            var moneyString = nonDecimalAmount.ToString();

            for (var i = moneyString.Length - 3; i > 0; i = i - 3)
            {
                moneyString = moneyString.Insert(i, ",");
            }

            if (decimalAmount != decimal.Zero)
            {
                moneyString = $"{moneyString}.{decimalAmount.ToString().Replace("0.", string.Empty).Replace("-", string.Empty)}";
            }

            return moneyString;
        }

        public static string ToCommaFormat(this decimal m, int decimals)
        {
            var amount = Math.Round(m, decimals);

            return amount.ToCommaFormat();
        }

        public static string ToMoney(this decimal m, CurrencyCode currencyCode, int decimals)
        {
            var format = currencyCode.GetSymbol();

            return string.Format(format, m.ToCommaFormat(decimals));
        }

        public static string ToMoney(this decimal? m, CurrencyCode currencyCode, int decimals)
        {
            if (m.HasValue)
            {
                return m.Value.ToMoney(currencyCode, decimals);
            }
            else
            {
                return "-";
            }
        }
    }
}
