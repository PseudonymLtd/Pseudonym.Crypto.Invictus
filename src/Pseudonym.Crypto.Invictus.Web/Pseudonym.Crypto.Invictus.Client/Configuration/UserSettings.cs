using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Configuration
{
    public sealed class UserSettings : IUserSettings
    {
        [JsonProperty("currency_code")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyCode CurrencyCode { get; set; } = CurrencyCode.USD;
    }
}
