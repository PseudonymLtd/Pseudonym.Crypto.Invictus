using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;

namespace Pseudonym.Crypto.Invictus.TrackerService.Controllers.Models
{
    public sealed class ApiCurrencyQueryFilter
    {
        [FromQuery(Name = "output-currency")]
        public CurrencyCode? CurrencyCode { get; set; }
    }
}
