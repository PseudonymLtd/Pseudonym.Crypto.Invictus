using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Pseudonym.Crypto.Invictus.Shared.Models.Filters
{
    public class ApiDateRangeQueryFilter : ApiCurrencyQueryFilter
    {
        public const string FromQueryName = "from";
        public const string ToQueryName = "to";

        [Required]
        [FromQuery(Name = FromQueryName)]
        public DateTime FromDate { get; set; }

        [Required]
        [FromQuery(Name = ToQueryName)]
        public DateTime ToDate { get; set; }
    }
}
