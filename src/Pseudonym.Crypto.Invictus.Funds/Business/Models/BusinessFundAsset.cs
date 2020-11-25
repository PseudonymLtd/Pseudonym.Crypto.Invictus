using System;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessFundAsset : IFundAsset
    {
        public string Name { get; set; }

        public string Symbol { get; set; }

        public decimal Value { get; set; }

        public decimal Share { get; set; }

        public Uri Link { get; set; }
    }
}
