using System;
using System.Collections.Generic;
using System.Linq;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessStake : IStake
    {
        public string Name { get; set; }

        public string DisplayName =>
            string.Join(" ", Name
                .Trim()
                .Split('-')
                .Select(x =>
                {
                    var chars = x.ToCharArray();
                    chars[0] = char.ToUpperInvariant(chars[0]);
                    return new string(chars);
                }));

        public string Description { get; set; }

        public Uri InvictusUri { get; set; }

        public Uri FactSheetUri { get; set; }

        public Uri PoolUri { get; set; }

        public IToken Token { get; set; }

        public decimal CirculatingSupply { get; set; }

        public IMarket Market { get; set; }

        public EthereumAddress StakingAddress { get; set; }

        public IReadOnlyList<ITimeMultiplier> TimeMultipliers { get; set; }

        public IReadOnlyDictionary<Symbol, decimal> FundMultipliers { get; set; }

        public IStakingPower Power { get; set; }
    }
}
