using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Pseudonym.Crypto.Invictus.TrackerService.Ethereum.Functions
{
    [Function("balanceOf", "uint256")]
    internal sealed class BalanceOfFunction : FunctionMessage
    {
        [Parameter("address", "_owner", 1)]
        public string Owner { get; set; }
    }
}
