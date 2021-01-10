using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Data.Abstractions;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Data.Models
{
    public sealed class DataStakingPower : IDataAggregatable
    {
        public string Address { get; set; }

        public DateTime Date { get; set; }

        public decimal Power { get; set; }

        public IReadOnlyList<DataStakingPowerSummary> Summary { get; set; }

        public IReadOnlyList<DataStakingPowerFund> Breakdown { get; set; }
    }

    public sealed class DataStakingPowerSummary
    {
        public string ContractAddress { get; set; }

        public decimal Power { get; set; }
    }

    public sealed class DataStakingPowerFund
    {
        public string ContractAddress { get; set; }

        public decimal FundModifier { get; set; }

        public decimal PricePerToken { get; set; }

        public IReadOnlyList<DataStakingEvent> Events { get; set; }
    }

    public sealed class DataStakingEvent
    {
        public string UserAddress { get; set; }

        public DateTime StakedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public decimal Quantity { get; set; }

        public decimal TimeModifier { get; set; }
    }
}
