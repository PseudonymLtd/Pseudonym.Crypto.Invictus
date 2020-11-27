using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Pseudonym.Crypto.Invictus.Shared.Models.Filters
{
    public class ApiTransactionQueryFilter : ApiDateRangeQueryFilter, IValidatableObject
    {
        public const string OffsetQueryName = "offset";
        public const string PaginationIdQueryName = "pagination_id";

        [TransactionHash]
        [FromQuery(Name = PaginationIdQueryName)]
        public string PaginationId { get; set; }

        [FromQuery(Name = OffsetQueryName)]
        public DateTime? Offset { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(PaginationId) && Offset.HasValue)
            {
                yield return new ValidationResult(
                    $"Cannot set field without specifying `{PaginationIdQueryName}`.",
                    new List<string>()
                    {
                        OffsetQueryName
                    });
            }

            if (!string.IsNullOrWhiteSpace(PaginationId) && !Offset.HasValue)
            {
                yield return new ValidationResult(
                    $"Cannot set field without specifying `{OffsetQueryName}`.",
                    new List<string>()
                    {
                        PaginationIdQueryName
                    });
            }
        }
    }
}
