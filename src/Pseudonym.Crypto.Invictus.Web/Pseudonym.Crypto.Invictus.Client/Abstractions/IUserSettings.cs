﻿using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Web.Client.Configuration;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface IUserSettings
    {
        CurrencyCode CurrencyCode { get; }

        string WalletAddress { get; }

        IReadOnlyDictionary<Symbol, FundInfo> Funds { get; }

        bool HasValidAddress();
    }
}
