using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Shared.Models.Filters
{
    public class ApiCurrencyQueryFilter
    {
        public const string CurrencyQueryName = "currency";

        [FromQuery(Name = CurrencyQueryName)]
        public CurrencyCode CurrencyCode { get; set; } = CurrencyCode.USD;
    }
}
