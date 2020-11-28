using System;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Bloxy
{
    public sealed class BloxyTokenTransfer
    {
        [JsonProperty("tx_time")]
        public DateTimeOffset ConfirmedAt { get; set; }

        [JsonProperty("tx_hash")]
        public string Hash { get; set; }

        public override bool Equals(object obj)
        {
            return obj is BloxyTokenTransfer btt &&
                btt.Hash.Equals(Hash);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hash);
        }
    }
}
