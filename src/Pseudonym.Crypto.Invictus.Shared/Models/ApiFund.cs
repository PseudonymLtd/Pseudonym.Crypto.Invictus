using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Models.Abstractions;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiFund : IProduct
    {
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [Required]
        [JsonProperty("description")]
        public string Description { get; set; }

        [Required]
        [JsonProperty("token")]
        public ApiToken Token { get; set; }

        [Required]
        [JsonProperty("circulating_supply")]
        public decimal CirculatingSupply { get; set; }

        [Required]
        [JsonProperty("nav")]
        public ApiNav Nav { get; set; }

        [Required]
        [JsonProperty("market")]
        public ApiMarket Market { get; set; }

        [JsonProperty("assets")]
        public List<ApiAsset> Assets { get; set; }

        [Required]
        [JsonProperty("links")]
        public ApiFundLinks Links { get; set; }
    }
}
