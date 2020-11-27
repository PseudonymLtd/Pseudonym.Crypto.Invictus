using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Shared.Models.Filters
{
    public class ApiPerformanceQueryFilter : ApiDateRangeQueryFilter
    {
        public const string ModeQueryName = "mode";

        [FromQuery(Name = ModeQueryName)]
        public PriceMode Mode { get; set; } = PriceMode.Avg;
    }
}
