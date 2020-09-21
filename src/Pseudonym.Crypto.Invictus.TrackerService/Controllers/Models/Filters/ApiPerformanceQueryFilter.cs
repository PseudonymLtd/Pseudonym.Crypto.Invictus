using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Pseudonym.Crypto.Invictus.TrackerService.Controllers.Models.Filters
{
    public class ApiPerformanceQueryFilter : ApiCurrencyQueryFilter
    {
        [Required]
        [FromQuery(Name = "from")]
        public DateTime FromDate { get; set; }

        [Required]
        [FromQuery(Name = "to")]
        public DateTime ToDate { get; set; }
    }
}
