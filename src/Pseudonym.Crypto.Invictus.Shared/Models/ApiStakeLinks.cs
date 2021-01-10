using System;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiStakeLinks : ApiCollection<Uri>
    {
        [JsonIgnore]
        public Uri Self => this[nameof(Self)];

        [JsonIgnore]
        public Uri Pool => this[nameof(Pool)];

        [JsonIgnore]
        public Uri Fact => this[nameof(Fact)];

        [JsonIgnore]
        public Uri External => this[nameof(External)];
    }
}