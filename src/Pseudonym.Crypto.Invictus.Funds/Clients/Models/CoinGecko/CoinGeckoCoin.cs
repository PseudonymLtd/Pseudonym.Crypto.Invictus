using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.CoinGecko
{
    public sealed class CoinGeckoCoin
    {
        private string symbol;

        [JsonProperty("id")]
        public string CoinId { get; set; }

        [JsonProperty("symbol")]
        public string Symbol
        {
            get => symbol;
            set => symbol = value.ToUpper();
        }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
