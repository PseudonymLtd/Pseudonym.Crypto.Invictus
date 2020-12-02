using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers.Filters
{
    public class ApiPerformanceQueryFilter : ApiDateRangeQueryFilter
    {
        [FromQuery(Name = ApiFilterNames.ModeQueryName)]
        public PriceMode Mode { get; set; } = PriceMode.Avg;
    }
}
