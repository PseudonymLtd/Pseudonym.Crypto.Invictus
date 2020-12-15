﻿using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessFundAsset : IFundAsset
    {
        public ICoin Coin { get; set; }

        public decimal Value { get; set; }

        public decimal Share { get; set; }
    }
}
