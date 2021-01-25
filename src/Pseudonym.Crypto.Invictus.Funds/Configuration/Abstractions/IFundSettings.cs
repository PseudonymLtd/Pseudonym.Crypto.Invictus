using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using static Pseudonym.Crypto.Invictus.Funds.Configuration.FundSettings;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions
{
    public interface IFundSettings
    {
        string Name { get; }

        string Category { get; }

        string Description { get; }

        Symbol Symbol { get; }

        DateTimeOffset InceptionDate { get; }

        int Decimals { get; }

        bool Tradable { get; }

        bool Burnable { get; }

        EthereumAddress ContractAddress { get; }

        string CoinGeckoId { get; }

        FundLinks Links { get; }

        IReadOnlyList<FundAsset> Assets { get; }
    }
}
