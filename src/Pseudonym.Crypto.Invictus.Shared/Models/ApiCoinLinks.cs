using System;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiCoinLinks : ApiCollection<Uri>
    {
        [JsonIgnore]
        public Uri Link => this[nameof(Link)];

        [JsonIgnore]
        public Uri ImageLink => this[nameof(ImageLink)];

        [JsonIgnore]
        public Uri MarketLink => this[nameof(MarketLink)];
    }
}
