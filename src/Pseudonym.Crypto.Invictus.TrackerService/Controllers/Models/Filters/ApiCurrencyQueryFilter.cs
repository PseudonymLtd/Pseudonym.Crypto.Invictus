using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Controllers.Models.Filters
{
    public class ApiCurrencyQueryFilter
    {
        [FromQuery(Name = "output-currency")]
        public CurrencyCode? CurrencyCode { get; set; }
    }
}
