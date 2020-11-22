using System;
using System.Collections.Generic;
using System.Linq;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessFund : IFund
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

        public Uri FactSheetUri { get; set; }

        public Uri LitepaperUri { get; set; }

        public IToken Token { get; set; }

        public decimal CirculatingSupply { get; set; }

        public decimal NetValue { get; set; }

        public decimal NetAssetValuePerToken { get; set; }

        public IMarket Market { get; set; }

        public IReadOnlyList<IAsset> Assets { get; set; } = new List<IAsset>();
    }
}
