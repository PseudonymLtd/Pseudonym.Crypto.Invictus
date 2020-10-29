using System.Collections.Generic;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Web.Client.Configuration
{
    public sealed class UserSettings : IUserSettings
    {
        private readonly Dictionary<Symbol, FundInfo> funds;

        public UserSettings()
        {
            funds = new Dictionary<Symbol, FundInfo>();
        }

        [JsonProperty("currency_code")]
        public CurrencyCode CurrencyCode { get; set; } = CurrencyCode.USD;

        [JsonProperty("wallet_address")]
        public string WalletAddress { get; set; }

        [JsonProperty("funds")]
        public IReadOnlyDictionary<Symbol, FundInfo> Funds => funds;

        public void AddFund(Symbol symbol, FundInfo fund)
        {
            if (!funds.ContainsKey(symbol))
            {
                funds.Add(symbol, fund);
            }
            else
            {
                funds[symbol].Name = fund.Name;
                funds[symbol].DisplayName = fund.DisplayName;
                funds[symbol].Description = fund.Description;
                funds[symbol].Decimals = fund.Decimals;
                funds[symbol].ContractAddress = fund.ContractAddress;
            }
        }
    }

    public sealed class FundInfo
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public int Decimals { get; set; }

        public string ContractAddress { get; set; }
    }
}
