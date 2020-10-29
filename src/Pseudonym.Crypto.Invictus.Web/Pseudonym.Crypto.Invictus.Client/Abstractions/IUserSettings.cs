using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface IUserSettings
    {
        CurrencyCode CurrencyCode { get; }
    }
}
