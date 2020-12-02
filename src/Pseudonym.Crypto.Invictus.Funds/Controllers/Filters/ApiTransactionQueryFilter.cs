using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers.Filters
{
    public class ApiTransactionQueryFilter : ApiDateRangeQueryFilter, IValidatableObject
    {
        [TransactionHash]
        [FromQuery(Name = ApiFilterNames.PaginationIdQueryName)]
        public string PaginationId { get; set; }

        [FromQuery(Name = ApiFilterNames.OffsetQueryName)]
        public DateTime? Offset { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(PaginationId) && Offset.HasValue)
            {
                yield return new ValidationResult(
                    $"Cannot set field without specifying `{ApiFilterNames.PaginationIdQueryName}`.",
                    new List<string>()
                    {
                        ApiFilterNames.OffsetQueryName
                    });
            }

            if (!string.IsNullOrWhiteSpace(PaginationId) && !Offset.HasValue)
            {
                yield return new ValidationResult(
                    $"Cannot set field without specifying `{ApiFilterNames.OffsetQueryName}`.",
                    new List<string>()
                    {
                        ApiFilterNames.PaginationIdQueryName
                    });
            }
        }
    }
}
