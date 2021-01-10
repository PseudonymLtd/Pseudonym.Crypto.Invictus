using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public sealed class StakeSettings : IStakeSettings
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Symbol Symbol { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime InceptionDate { get; set; }

        public int Decimals { get; set; }

        public List<StakeTimeMultiplier> TimeMultipliers { get; set; }

        public string StakingAddress { get; set; }

        public string ContractAddress { get; set; }

        public string PoolAddress { get; set; }

        public List<string> FeeAddresses { get; set; } = new List<string>();

        public Dictionary<string, decimal> FundMultipliers { get; set; }

        public StakeLinks Links { get; set; }

        [JsonIgnore]
        IReadOnlyList<StakeTimeMultiplier> IStakeSettings.TimeMultipliers => TimeMultipliers;

        [JsonIgnore]
        EthereumAddress IStakeSettings.StakingAddress => new EthereumAddress(StakingAddress);

        [JsonIgnore]
        EthereumAddress IStakeSettings.ContractAddress => new EthereumAddress(ContractAddress);

        [JsonIgnore]
        EthereumAddress IStakeSettings.PoolAddress => new EthereumAddress(PoolAddress);

        [JsonIgnore]
        IReadOnlyList<EthereumAddress> IStakeSettings.FeeAddresses => FeeAddresses.Select(x => new EthereumAddress(x)).ToList();

        [JsonIgnore]
        IReadOnlyDictionary<Symbol, decimal> IStakeSettings.FundMultipliers => FundMultipliers
            .ToDictionary(k => Enum.Parse<Symbol>(k.Key), v => v.Value);

        public sealed class StakeTimeMultiplier
        {
            public int RangeMin { get; set; }

            public int RangeMax { get; set; }

            public decimal Multiplier { get; set; }
        }

        public sealed class StakeLinks
        {
            public Uri Fact { get; set; }

            public Uri External { get; set; }
        }
    }
}
