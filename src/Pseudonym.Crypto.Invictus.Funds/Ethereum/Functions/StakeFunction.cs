using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Pseudonym.Crypto.Invictus.Funds.Ethereum.Functions
{
    [Function("stake")]
    internal sealed class StakeFunction
    {
        [Parameter("address", "fundAddress", 1)]
        public string ContractAddress { get; set; }

        [Parameter("uint256", "amount", 2)]
        public BigInteger Quantity { get; set; }

        [Parameter("uint256", "stakeLength", 3)]
        public BigInteger Length { get; set; }

        [Parameter("bool", "autoRenewStake", 4)]
        public bool AutoRenew { get; set; }
    }
}
