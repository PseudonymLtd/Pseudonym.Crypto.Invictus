using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Web.Client.Configuration
{
    public sealed class UserSettings : IUserSettings
    {
        private static readonly Regex AddressRegex = new Regex("^0x+[A-F,a-f,0-9]{40}$");

        private readonly Dictionary<Symbol, FundInfo> funds;

        public UserSettings(Dictionary<Symbol, FundInfo> funds)
        {
            this.funds = funds;
        }

        public CurrencyCode CurrencyCode { get; set; } = CurrencyCode.USD;

        public string WalletAddress { get; set; }

        public IReadOnlyDictionary<Symbol, FundInfo> Funds => funds;

        public bool HasValidAddress()
        {
            return AddressRegex.IsMatch(WalletAddress ?? string.Empty);
        }

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

        public Uri FundLink { get; set; }
    }
}
