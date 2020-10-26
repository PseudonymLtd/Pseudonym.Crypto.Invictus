using System.Collections.Generic;
using System.Text.Json.Serialization;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public sealed class FundSettings
    {
        public string FundName { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Symbol Symbol { get; set; }

        public int Decimals { get; set; }

        public string ContractAddress { get; set; }

        public List<FundAsset> Assets { get; set; } = new List<FundAsset>();

        [JsonIgnore]
        internal EthereumAddress Address => new EthereumAddress(ContractAddress);

        public sealed class FundAsset
        {
            public string Name { get; set; }

            public string Symbol { get; set; }

            public decimal Value { get; set; }

            public decimal Share { get; set; }
        }
    }
}
