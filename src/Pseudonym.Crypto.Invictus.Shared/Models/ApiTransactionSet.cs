using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiTransactionSet : ApiTransaction
    {
        [Required]
        [JsonProperty("operations")]
        public IReadOnlyList<ApiOperation> Operations { get; set; }
    }
}
