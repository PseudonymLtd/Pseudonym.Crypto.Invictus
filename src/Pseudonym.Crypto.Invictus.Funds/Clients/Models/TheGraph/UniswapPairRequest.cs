using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.TheGraph
{
    public class UniswapPairRequest
    {
        private const string Template = @"
        {
            pair(id: ""%PAIR_ADDRESS%"")
            { 
                token0: token0 
                { 
                    id: id,
                    symbol: symbol,
                    name: name,
                    decimals: decimals
                }, 
                token1: token1 
                {
                    id: id,
                    symbol: symbol,
                    name: name,
                    decimals: decimals
                }
                price0: token0Price,
                price1: token1Price,
                supply0: reserve0,
                supply1: reserve1,
                volume: untrackedVolumeUSD
            }
        }";

        public UniswapPairRequest(string pairId)
        {
            Query = Template.Replace("%PAIR_ADDRESS%", pairId);
        }

        [JsonRequired]
        [JsonProperty("query")]
        public string Query { get; private set; }
    }
}
