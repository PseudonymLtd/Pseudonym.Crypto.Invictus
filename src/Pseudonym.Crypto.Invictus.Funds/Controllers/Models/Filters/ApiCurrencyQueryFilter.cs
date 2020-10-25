using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers.Models.Filters
{
    public class ApiCurrencyQueryFilter
    {
        [FromQuery(Name = "output-currency")]
        public CurrencyCode? CurrencyCode { get; set; }
    }
}
