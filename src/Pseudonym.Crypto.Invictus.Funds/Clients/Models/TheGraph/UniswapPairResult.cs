using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.TheGraph
{
    public class UniswapPairResult
    {
        [Required]
        public EthereumAddress PoolAddress { get; set; }

        [Required]
        public IReadOnlyList<UniswapTokenResult> Tokens { get; set; }

        [Required]
        public decimal Volume { get; set; }
    }

    public sealed class UniswapTokenResult
    {
        [Required]
        public EthereumAddress ContractAddress { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Symbol { get; set; }

        [Required]
        public int Decimals { get; set; }

        [Required]
        public decimal PricePerToken { get; set; }

        [Required]
        public decimal PoolSupply { get; set; }
    }
}
