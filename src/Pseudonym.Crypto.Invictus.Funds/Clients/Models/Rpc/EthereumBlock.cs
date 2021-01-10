using System;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Rpc
{
    public class EthereumBlock
    {
        public string Hash { get; set; }

        public ulong Number { get; set; }

        public DateTime MinedAt { get; set; }

        public string Miner { get; set; }
    }
}
