using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Shared.Models.Filters
{
    public class ApiPerformanceQueryFilter : ApiCurrencyQueryFilter
    {
        public const string FromQueryName = "from";
        public const string ToQueryName = "to";
        public const string ModeQueryName = "mode";

        [Required]
        [FromQuery(Name = FromQueryName)]
        public DateTime FromDate { get; set; }

        [Required]
        [FromQuery(Name = ToQueryName)]
        public DateTime ToDate { get; set; }

        [FromQuery(Name = ModeQueryName)]
        public PriceMode Mode { get; set; } = PriceMode.Avg;
    }
}
