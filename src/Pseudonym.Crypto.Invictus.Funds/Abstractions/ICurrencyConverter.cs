using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface ICurrencyConverter
    {
        decimal Convert(decimal amount, CurrencyCode currencyCode);
    }
}
