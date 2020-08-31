using System.Text.Json.Serialization;
using Pseudonym.Crypto.Invictus.TrackerService.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Configuration
{
    public sealed class FundConfig
    {
        public string Symbol { get; set; }

        public int Decimals { get; set; }

        public string ContractAddress { get; set; }

        [JsonIgnore]
        internal EthereumAddress Address => new EthereumAddress(ContractAddress);
    }
}
