using System;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus
{
    public sealed class InvictusPerformance
    {
        public DateTime Date { get; set; }

        public decimal CirculatingSupply { get; set; }

        public decimal NetAssetValuePerToken { get; set; }

        public decimal NetValue { get; set; }
    }
}
