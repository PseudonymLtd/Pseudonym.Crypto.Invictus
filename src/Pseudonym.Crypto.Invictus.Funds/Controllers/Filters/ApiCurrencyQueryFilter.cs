using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers.Filters
{
    public class ApiCurrencyQueryFilter
    {
        [FromQuery(Name = "output-currency")]
        public CurrencyCode? CurrencyCode { get; set; }
    }
}
