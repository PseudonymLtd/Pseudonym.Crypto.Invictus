using System;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Models.Abstractions;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiStakeLinks : ApiCollection<Uri>, ILinks
    {
        [JsonIgnore]
        public Uri Self => this[nameof(Self)];

        [JsonIgnore]
        public Uri Detail => this["Pool"];

        [JsonIgnore]
        public Uri Fact => this[nameof(Fact)];

        [JsonIgnore]
        public Uri External => this[nameof(External)];
    }
}