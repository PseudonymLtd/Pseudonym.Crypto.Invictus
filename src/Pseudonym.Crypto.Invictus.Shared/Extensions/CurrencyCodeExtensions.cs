namespace Pseudonym.Crypto.Invictus.Shared.Enums
{
    public static class CurrencyCodeExtensions
    {
        public static string GetSymbol(this CurrencyCode currencyCode)
        {
            switch (currencyCode)
            {
                case CurrencyCode.USD:
                    return "$";
                case CurrencyCode.AED:
                    return "د.إ";
                case CurrencyCode.ARS:
                    return "$";
                case CurrencyCode.AUD:
                    return "$";
                case CurrencyCode.BGN:
                    return "лв";
                case CurrencyCode.BRL:
                    return "R$";
                case CurrencyCode.BSD:
                    return "B$";
                case CurrencyCode.CAD:
                    return "$";
                case CurrencyCode.CHF:
                    return "SFr.";
                case CurrencyCode.CLP:
                    return "$";
                case CurrencyCode.CNY:
                    return "¥";
                case CurrencyCode.COP:
                    return "$";
                case CurrencyCode.CZK:
                    return "Kč";
                case CurrencyCode.DKK:
                    return "kr";
                case CurrencyCode.DOP:
                    return "RD$";
                case CurrencyCode.EGP:
                    return "E£";
                case CurrencyCode.EUR:
                    return "€";
                case CurrencyCode.FJD:
                    return "FJ$";
                case CurrencyCode.GBP:
                    return "£";
                case CurrencyCode.GTQ:
                    return "Q";
                case CurrencyCode.HKD:
                    return "HK$";
                case CurrencyCode.HRK:
                    return "kn";
                case CurrencyCode.HUF:
                    return "ft";
                case CurrencyCode.IDR:
                    return "Rp";
                case CurrencyCode.ILS:
                    return "₪";
                case CurrencyCode.INR:
                    return "₹";
                case CurrencyCode.ISK:
                    return "Íkr";
                case CurrencyCode.JPY:
                    return "¥";
                case CurrencyCode.KRW:
                    return "₩";
                case CurrencyCode.KZT:
                    return "₸";
                case CurrencyCode.MXN:
                    return "$";
                case CurrencyCode.MYR:
                    return "RM";
                case CurrencyCode.NOK:
                    return "kr";
                case CurrencyCode.NZD:
                    return "$";
                case CurrencyCode.PAB:
                    return "B/.";
                case CurrencyCode.PEN:
                    return "S/.";
                case CurrencyCode.PHP:
                    return "₱";
                case CurrencyCode.PKR:
                    return "Rs";
                case CurrencyCode.PLN:
                    return "zł";
                case CurrencyCode.PYG:
                    return "₲";
                case CurrencyCode.RON:
                    return "lei";
                case CurrencyCode.RUB:
                    return "₽";
                case CurrencyCode.SAR:
                    return "﷼‎";
                case CurrencyCode.SEK:
                    return "kr";
                case CurrencyCode.SGD:
                    return "$";
                case CurrencyCode.THB:
                    return "฿";
                case CurrencyCode.TRY:
                    return "₺";
                case CurrencyCode.TWD:
                    return "NT$";
                case CurrencyCode.UAH:
                    return "₴";
                case CurrencyCode.UYU:
                    return "$U";
                case CurrencyCode.ZAR:
                    return "R";
                default:
                    return "??";
            }
        }
    }
}
