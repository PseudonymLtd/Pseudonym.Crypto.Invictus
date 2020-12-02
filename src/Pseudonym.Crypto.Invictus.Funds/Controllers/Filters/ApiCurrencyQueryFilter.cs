using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers.Filters
{
    public class ApiCurrencyQueryFilter
    {
        [FromQuery(Name = ApiFilterNames.CurrencyQueryName)]
        public CurrencyCode CurrencyCode { get; set; } = CurrencyCode.USD;
    }
}
