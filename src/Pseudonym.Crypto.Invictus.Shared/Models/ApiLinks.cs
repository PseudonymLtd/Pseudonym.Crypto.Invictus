using System;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiLinks : ApiCollection<Uri>
    {
        [JsonIgnore]
        public Uri Self => this[nameof(Self)];

        [JsonIgnore]
        public Uri Lite => this[nameof(Lite)];

        [JsonIgnore]
        public Uri Fact => this[nameof(Fact)];
    }
}
