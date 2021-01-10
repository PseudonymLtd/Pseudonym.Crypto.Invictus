using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiStakingPower
    {
        [Required]
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [Required]
        [JsonProperty("power")]
        public decimal Power { get; set; }

        [JsonProperty("breakdown")]
        public IReadOnlyList<ApiStakingPowerFund> Breakdown { get; set; }

        public bool ShouldSerializeBreakdown() => Breakdown?.Any() ?? false;
    }
}
