using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers.Filters
{
    public class ApiDateRangeQueryFilter : ApiCurrencyQueryFilter
    {
        [Required]
        [FromQuery(Name = ApiFilterNames.FromQueryName)]
        public DateTime FromDate { get; set; }

        [Required]
        [FromQuery(Name = ApiFilterNames.ToQueryName)]
        public DateTime ToDate { get; set; }
    }
}
