using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Web.Client.Configuration;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface IUserSettings
    {
        CurrencyCode CurrencyCode { get; }

        DurationMode DurationMode { get; }

        string WalletAddress { get; }

        string StakingAddress { get; }

        IReadOnlyList<string> SecondaryWalletAddresses { get; }

        IReadOnlyDictionary<Symbol, FundInfo> Funds { get; }

        bool HasValidAddress();

        bool IsValidAddress(string candidateAddress);
    }
}
